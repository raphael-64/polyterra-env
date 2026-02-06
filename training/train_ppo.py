"""
Simple PPO training for Polyterra.
"""
import os
import json
import numpy as np
from polyterra_env import PolyterraEnv
from sb3_contrib import MaskablePPO
from sb3_contrib.common.wrappers import ActionMasker
from stable_baselines3.common.callbacks import BaseCallback
import gymnasium as gym
from gymnasium import spaces

import wandb
from wandb.integration.sb3 import WandbCallback


class SimplePolyterraWrapper(gym.Env):
    """
    Wrapper: AEC -> Gym with simplified action space.

    Instead of 512 raw action indices, we use 8 ACTION TYPES:
    0: END_TURN, 1: MOVE, 2: ATTACK, 3: BUILD, 4: TRAIN, 5: RESEARCH, 6: HARVEST, 7: OTHER

    The env picks the best/random target for that action type.
    This makes the action space consistent and learnable.
    """

    # Simplified action types
    ACTION_TYPES = ["end_turn", "move", "attack", "build", "train", "research", "harvest", "other"]
    NUM_ACTION_TYPES = 8

    def __init__(self, num_players=2, max_steps=200, map_size=15, reward_shaping=True):
        super().__init__()
        self.aec_env = PolyterraEnv(num_players=num_players)
        self.max_steps = max_steps
        self.map_size = map_size
        self.steps = 0
        self.our_agent = None
        self.reward_shaping = reward_shaping

        # State tracking for reward shaping
        self.prev_state = {}

        self.aec_env.reset()

        # Simplified action space: just pick action TYPE
        self.action_space = spaces.Discrete(self.NUM_ACTION_TYPES)
        obs_dim = 10 + map_size * map_size * 4 + self.NUM_ACTION_TYPES  # +8 for action availability
        self.observation_space = spaces.Box(-1.0, 1.0, shape=(obs_dim,), dtype=np.float32)

        self._action_mask = np.ones(self.NUM_ACTION_TYPES, dtype=np.int8)
        self._valid_actions_by_type = {}  # Cache: type -> list of action indices
        self._last_action_type = None

    def _flatten_obs(self, obs):
        """Dict obs -> flat vector."""
        flat = [
            obs.get('currency', 0) / 1000.0,
            obs.get('score', 0) / 10000.0,
            obs.get('turn', 0) / 30.0,
            obs.get('num_cities', 0) / 10.0,
            obs.get('num_kills', 0) / 100.0,
            obs.get('num_casualties', 0) / 100.0,
            obs.get('current_player_idx', 0) / 2.0,
            obs.get('player_id', 0) / 3.0,
            len(obs.get('units', [])) / 20.0,
            len(obs.get('cities', [])) / 10.0,
        ]

        tile_grid = np.zeros((self.map_size, self.map_size, 4), dtype=np.float32)
        for tile in obs.get('tiles', []):
            x, y = tile.get('x', 0), tile.get('y', 0)
            if 0 <= x < self.map_size and 0 <= y < self.map_size:
                tile_grid[x, y] = [
                    tile.get('explored', 0),
                    tile.get('owner', 0) / 4.0,
                    tile.get('has_unit', 0),
                    tile.get('terrain', 0) / 7.0,
                ]

        return np.array(flat + list(tile_grid.flatten()), dtype=np.float32)

    def _update_mask(self):
        """Get action mask from current agent's observation."""
        agent = self.aec_env.agent_selection
        if agent in self.aec_env.agents:
            obs = self.aec_env.observe(agent)
            mask = obs.get('action_mask')
            if mask is not None:
                self._action_mask = np.array(mask, dtype=np.int8)[:self.MAX_ACTIONS]
                if len(self._action_mask) < self.MAX_ACTIONS:
                    self._action_mask = np.pad(self._action_mask, (0, self.MAX_ACTIONS - len(self._action_mask)))

    def _extract_state(self, obs):
        """Extract state values for reward shaping comparison."""
        return {
            "score": obs.get("score", 0),
            "currency": obs.get("currency", 0),
            "num_cities": obs.get("num_cities", 0),
            "num_units": len(obs.get("units", [])),
            "num_kills": obs.get("num_kills", 0),
            "explored_tiles": sum(1 for t in obs.get("tiles", []) if t.get("explored")),
        }

    def _compute_shaped_reward(self, base_reward, prev_obs, curr_obs, action_type):
        """Add dense reward shaping on top of base game reward."""
        if not self.reward_shaping:
            return base_reward

        shaped = base_reward
        prev = self._extract_state(prev_obs) if prev_obs else {}
        curr = self._extract_state(curr_obs)

        if prev:
            # Exploration reward: +0.1 per new tile
            explored_delta = curr["explored_tiles"] - prev.get("explored_tiles", 0)
            shaped += explored_delta * 0.1

            # Economy reward: +0.02 per currency gained
            currency_delta = curr["currency"] - prev.get("currency", 0)
            shaped += max(0, currency_delta) * 0.02  # Only reward gains

            # Army reward: +0.5 per new unit
            unit_delta = curr["num_units"] - prev.get("num_units", 0)
            shaped += unit_delta * 0.5

            # City reward: +2.0 per new city
            city_delta = curr["num_cities"] - prev.get("num_cities", 0)
            shaped += city_delta * 2.0

            # Kill reward: +1.0 per kill
            kill_delta = curr["num_kills"] - prev.get("num_kills", 0)
            shaped += kill_delta * 1.0

        # Action type bonuses (encourage diverse actions)
        if action_type and action_type != "end_turn" and action_type != "invalid":
            shaped += 0.01  # Small bonus for doing something

        return shaped

    def reset(self, seed=None, options=None):
        self.aec_env.reset(seed=seed)
        self.steps = 0
        self.our_agent = self.aec_env.agent_selection
        self._last_action_type = None

        self._update_mask()
        obs = self.aec_env.observe(self.our_agent)
        self.prev_state = obs  # Store for reward shaping
        return self._flatten_obs(obs), {}

    def step(self, action):
        self.steps += 1
        action_idx = int(action)

        # Get action type before stepping (for reward shaping)
        pre_obs = self.aec_env.observe(self.our_agent) if self.our_agent in self.aec_env.agents else {}
        valid_actions_list = pre_obs.get('valid_actions_list', [])
        action_dict = valid_actions_list[action_idx] if action_idx < len(valid_actions_list) else {}
        action_type = action_dict.get('type', 'invalid') if isinstance(action_dict, dict) else 'invalid'
        self._last_action_type = action_type

        # Our turn
        self.aec_env.step(action_idx)
        base_reward = self.aec_env.rewards.get(self.our_agent, 0)

        # Let opponent play (random valid action)
        while self.aec_env.agent_selection != self.our_agent and self.aec_env.agents:
            opp = self.aec_env.agent_selection
            if opp in self.aec_env.agents:
                opp_obs = self.aec_env.observe(opp)
                opp_mask = opp_obs.get('action_mask', [])
                valid_indices = np.where(np.array(opp_mask) == 1)[0]
                opp_action = np.random.choice(valid_indices) if len(valid_indices) > 0 else 0
                self.aec_env.step(int(opp_action))

            # Check if game ended
            if all(self.aec_env.terminations.get(a, False) or self.aec_env.truncations.get(a, False)
                   for a in self.aec_env.possible_agents):
                break

        # Get our new observation
        self._update_mask()
        terminated = self.aec_env.terminations.get(self.our_agent, False)
        truncated = self.aec_env.truncations.get(self.our_agent, False) or self.steps >= self.max_steps

        if self.our_agent in self.aec_env.agents:
            curr_obs = self.aec_env.observe(self.our_agent)
            flat_obs = self._flatten_obs(curr_obs)
        else:
            curr_obs = {}
            flat_obs = np.zeros(self.observation_space.shape, dtype=np.float32)
            terminated = True

        # Apply reward shaping
        shaped_reward = self._compute_shaped_reward(base_reward, self.prev_state, curr_obs, action_type)
        self.prev_state = curr_obs  # Update for next step

        return flat_obs, shaped_reward, terminated, truncated, {"action_type": action_type}

    def action_masks(self):
        return self._action_mask

    def close(self):
        self.aec_env.close()


class StatsCallback(BaseCallback):
    """Callback for logging and rich replays (viewable in polytopia_playable.html)."""

    def __init__(self, log_freq=1000, replay_freq=25000, replay_dir="replays"):
        super().__init__()
        self.log_freq = log_freq
        self.replay_freq = replay_freq  # Less frequent due to file size
        self.replay_dir = replay_dir
        self.ep_rewards = []
        os.makedirs(replay_dir, exist_ok=True)

    def _on_step(self):
        # Track episode rewards
        if self.locals.get("dones", [False])[0]:
            ep_info = self.locals.get("infos", [{}])[0].get("episode")
            if ep_info:
                self.ep_rewards.append(ep_info["r"])

        # Log periodically
        if self.n_calls % self.log_freq == 0:
            if self.ep_rewards:
                wandb.log({
                    "custom/mean_reward": np.mean(self.ep_rewards[-10:]),
                    "custom/episodes": len(self.ep_rewards),
                })
            print(f"[{self.n_calls:,} steps] episodes={len(self.ep_rewards)}")

        # Save replay (less frequent - files are big)
        if self.n_calls % self.replay_freq == 0:
            self._save_replay()

        return True

    def _save_replay(self):
        """Save replay in good_game.json format for viewing in polytopia_playable.html."""
        from datetime import datetime
        try:
            # Create fresh env for replay
            aec_env = PolyterraEnv(num_players=2)
            aec_env.reset()

            replay = {
                "metadata": {
                    "start_time": datetime.now().isoformat(),
                    "version": "1.0",
                    "type": "training_checkpoint",
                    "training_step": self.n_calls,
                },
                "steps": []
            }

            our_agent = aec_env.agent_selection
            step_count = 0
            max_steps = 150  # Cap replay length

            while step_count < max_steps and aec_env.agents:
                agent = aec_env.agent_selection
                if agent not in aec_env.agents:
                    break

                # Get full observation
                full_obs = aec_env.observe(agent)

                # Prepare observation for replay (matching good_game.json format)
                obs_for_replay = {
                    "turn": full_obs.get("turn", 0),
                    "currency": full_obs.get("currency", 0),
                    "score": full_obs.get("score", 0),
                    "num_cities": full_obs.get("num_cities", 0),
                    "num_kills": full_obs.get("num_kills", 0),
                    "num_casualties": full_obs.get("num_casualties", 0),
                    "num_units": len(full_obs.get("units", [])),
                    "units": [
                        {
                            "type": str(u.get("type", 0)),
                            "position": [u.get("x", 0), u.get("y", 0)],
                            "health": u.get("health", 10.0),
                            "moved": u.get("moved", False),
                            "attacked": u.get("attacked", False),
                        }
                        for u in full_obs.get("units", [])
                    ],
                    "cities": [
                        {
                            "name": "",
                            "position": [c.get("x", 0), c.get("y", 0)],
                            "level": c.get("level", 1),
                            "population": c.get("population", 0),
                        }
                        for c in full_obs.get("cities", [])
                    ],
                    "tiles": full_obs.get("tiles", []),
                }

                # Get action
                mask = full_obs.get("action_mask", [])
                valid_actions_list = full_obs.get("valid_actions_list", [])

                if agent == our_agent:
                    # Use trained model
                    flat_obs = self._flatten_obs_for_model(full_obs)
                    mask_arr = np.array(mask, dtype=np.int8)
                    if len(mask_arr) < 512:
                        mask_arr = np.pad(mask_arr, (0, 512 - len(mask_arr)))
                    action_idx, _ = self.model.predict(flat_obs, action_masks=mask_arr, deterministic=True)
                    action_idx = int(action_idx)
                else:
                    # Random valid action for opponent
                    valid_indices = np.where(np.array(mask) == 1)[0]
                    action_idx = int(np.random.choice(valid_indices)) if len(valid_indices) > 0 else 0

                # Get action details for replay
                action_dict = valid_actions_list[action_idx] if action_idx < len(valid_actions_list) else {"type": "invalid"}
                action_type = action_dict.get("type", "unknown").upper()

                action_for_replay = {
                    "raw": [action_idx, 0, 0, 0, 0, 0],
                    "type": action_type,
                    "target": [action_dict.get("x", action_dict.get("to_x", 0)),
                               action_dict.get("y", action_dict.get("to_y", 0))],
                    "unit_idx": 0,
                    "params": [0, 0],
                }

                # Record step
                replay["steps"].append({
                    "step": step_count,
                    "agent": agent,
                    "observation": obs_for_replay,
                    "action": action_for_replay,
                    "reward": aec_env.rewards.get(agent, 0),
                    "info": {},
                })

                # Execute action
                aec_env.step(action_idx)
                step_count += 1

                # Check if game ended
                if all(aec_env.terminations.get(a, False) or aec_env.truncations.get(a, False)
                       for a in aec_env.possible_agents):
                    break

            replay["metadata"]["end_time"] = datetime.now().isoformat()
            replay["metadata"]["total_steps"] = step_count

            path = f"{self.replay_dir}/replay_{self.n_calls}.json"
            with open(path, "w") as f:
                json.dump(replay, f, indent=2)

            aec_env.close()
            print(f"Saved replay: {path} ({step_count} steps)")

        except Exception as e:
            print(f"Replay failed: {e}")
            import traceback
            traceback.print_exc()

    def _flatten_obs_for_model(self, obs, map_size=15):
        """Flatten observation for model prediction."""
        flat = [
            obs.get('currency', 0) / 1000.0,
            obs.get('score', 0) / 10000.0,
            obs.get('turn', 0) / 30.0,
            obs.get('num_cities', 0) / 10.0,
            obs.get('num_kills', 0) / 100.0,
            obs.get('num_casualties', 0) / 100.0,
            obs.get('current_player_idx', 0) / 2.0,
            obs.get('player_id', 0) / 3.0,
            len(obs.get('units', [])) / 20.0,
            len(obs.get('cities', [])) / 10.0,
        ]
        tile_grid = np.zeros((map_size, map_size, 4), dtype=np.float32)
        for tile in obs.get('tiles', []):
            x, y = tile.get('x', 0), tile.get('y', 0)
            if 0 <= x < map_size and 0 <= y < map_size:
                tile_grid[x, y] = [
                    tile.get('explored', 0),
                    tile.get('owner', 0) / 4.0,
                    tile.get('has_unit', 0),
                    tile.get('terrain', 0) / 7.0,
                ]
        return np.array(flat + list(tile_grid.flatten()), dtype=np.float32)


def mask_fn(env):
    return env.action_masks()


def main():
    config = {
        "total_timesteps": 100_000,
        "learning_rate": 3e-4,
        "n_steps": 256,
        "batch_size": 64,
        "ent_coef": 0.05,
    }

    run = wandb.init(project="polyterra", config=config, sync_tensorboard=True)

    env = SimplePolyterraWrapper()
    env = ActionMasker(env, mask_fn)

    model = MaskablePPO(
        "MlpPolicy", env, verbose=1,  # verbose=1 to see progress
        learning_rate=config["learning_rate"],
        n_steps=config["n_steps"],
        batch_size=config["batch_size"],
        ent_coef=config["ent_coef"],
        tensorboard_log=f"runs/{run.id}",
    )

    model.learn(
        total_timesteps=config["total_timesteps"],
        callback=[
            WandbCallback(model_save_path=f"models/{run.id}", model_save_freq=25000, verbose=0),
            StatsCallback(log_freq=1000, replay_freq=25000, replay_dir=f"replays/{run.id}"),
        ],
    )

    model.save(f"models/{run.id}/final")
    env.close()
    wandb.finish()


if __name__ == "__main__":
    main()
