"""
PPO training for Polyterra with FULL action space.
Properly handles MultiDiscrete actions with action masking.
Includes reward shaping and replay logging.
"""
import sys
sys.path.insert(0, '../polyterra-env-py')

import os
import json
import numpy as np
from polyterra_env import PolyterraEnv
from sb3_contrib import MaskablePPO
from sb3_contrib.common.wrappers import ActionMasker
from stable_baselines3.common.callbacks import BaseCallback
import gymnasium as gym
from gymnasium import spaces

# W&B for experiment tracking
import wandb
from wandb.integration.sb3 import WandbCallback

# Action type names for logging
ACTION_NAMES = {
    0: "END_TURN", 1: "MOVE", 2: "ATTACK", 3: "BUILD", 4: "TRAIN",
    5: "RESEARCH", 6: "UPGRADE", 7: "RECOVER", 8: "HEAL_OTHERS",
    9: "PROMOTE", 10: "EXAMINE_RUINS", 11: "DISBAND", 12: "DESTROY",
    13: "CAPTURE", 14: "HARVEST", 15: "EXAMINE", 16: "GROW_FOREST",
}


class PolyterraGymWrapper(gym.Env):
    """
    Wraps Polyterra AEC env for SB3 with FULL action space.
    - Flattens observations for MLP compatibility
    - Keeps full MultiDiscrete action space
    - Proper action masking for all action components
    - Tracks stats for debugging
    """

    def __init__(self, num_players=2, max_steps=200, map_size=15, reward_shaping=True):
        super().__init__()
        self.aec_env = PolyterraEnv(num_players=num_players)
        self.max_steps = max_steps
        self.map_size = map_size
        self.steps = 0
        self.reward_shaping = reward_shaping

        # Stats tracking
        self.episode_stats = {
            "invalid_actions": 0,
            "end_turns": 0,
            "moves": 0,
            "attacks": 0,
            "builds": 0,
            "trains": 0,
            "research": 0,
            "other": 0,
            "total_actions": 0,
        }
        self.prev_state = {}  # For reward shaping

        # Initialize env
        self.aec_env.reset()

        # FULL action space: [action_type, target_x, target_y, unit_idx, param1, param2]
        self.action_space = spaces.MultiDiscrete([37, 50, 50, 100, 47, 10])

        # Flattened observation space
        obs_dim = 10 + map_size * map_size * 4
        self.observation_space = spaces.Box(
            low=-1.0, high=1.0, shape=(obs_dim,), dtype=np.float32
        )

        # Current action mask (tuple of 6 masks)
        self._current_mask = None
        self._update_mask()

    def _flatten_obs(self, obs):
        """Convert complex observation to flat vector."""
        flat = []

        # Basic player stats (normalized)
        flat.append(obs.get('currency', 0) / 1000.0)
        flat.append(obs.get('score', 0) / 10000.0)
        flat.append(obs.get('turn', 0) / 30.0)
        flat.append(obs.get('num_cities', 0) / 10.0)
        flat.append(obs.get('num_kills', 0) / 100.0)
        flat.append(obs.get('num_casualties', 0) / 100.0)
        flat.append(obs.get('current_player_idx', 0) / 2.0)
        flat.append(obs.get('player_id', 0) / 3.0)
        flat.append(len(obs.get('units', [])) / 20.0)
        flat.append(len(obs.get('cities', [])) / 10.0)

        # Simple tile grid
        tiles = obs.get('tiles', [])
        tile_grid = np.zeros((self.map_size, self.map_size, 4), dtype=np.float32)

        for tile in tiles:
            x, y = tile.get('x', 0), tile.get('y', 0)
            if 0 <= x < self.map_size and 0 <= y < self.map_size:
                tile_grid[x, y, 0] = tile.get('explored', 0)
                tile_grid[x, y, 1] = tile.get('owner', 0) / 4.0
                tile_grid[x, y, 2] = tile.get('has_unit', 0)
                tile_grid[x, y, 3] = tile.get('terrain', 0) / 7.0

        flat.extend(tile_grid.flatten())
        return np.array(flat, dtype=np.float32)

    def _update_mask(self):
        """Update action mask from current observation."""
        agent = self.aec_env.agent_selection
        if agent not in self.aec_env.agents:
            self._current_mask = self._make_end_turn_only_mask()
            return

        obs = self.aec_env.observe(agent)
        action_mask = obs.get('action_mask', None)

        if action_mask is None or not isinstance(action_mask, tuple):
            self._current_mask = self._make_all_valid_mask()
            return

        # Convert each component to proper size
        masks = []
        expected_sizes = [37, 50, 50, 100, 47, 10]

        for i, size in enumerate(expected_sizes):
            if i < len(action_mask):
                m = np.array(action_mask[i], dtype=np.int8)
                if len(m) < size:
                    m = np.pad(m, (0, size - len(m)))
                m = m[:size]
            else:
                m = np.ones(size, dtype=np.int8)
            masks.append(m)

        self._current_mask = tuple(masks)

    def _make_end_turn_only_mask(self):
        """Create mask where only END_TURN (action 0) is valid."""
        masks = []
        sizes = [37, 50, 50, 100, 47, 10]
        for i, size in enumerate(sizes):
            m = np.zeros(size, dtype=np.int8)
            if i == 0:
                m[0] = 1
            else:
                m[0] = 1
            masks.append(m)
        return tuple(masks)

    def _make_all_valid_mask(self):
        """Create mask where all actions are valid."""
        sizes = [37, 50, 50, 100, 47, 10]
        return tuple(np.ones(s, dtype=np.int8) for s in sizes)

    def _get_state_for_shaping(self, obs):
        """Extract state values for reward shaping."""
        return {
            "score": obs.get("score", 0),
            "currency": obs.get("currency", 0),
            "num_cities": obs.get("num_cities", 0),
            "num_units": len(obs.get("units", [])),
            "explored_tiles": sum(1 for t in obs.get("tiles", []) if t.get("explored", False)),
            "num_kills": obs.get("num_kills", 0),
        }

    def _compute_shaped_reward(self, reward, obs, action_type):
        """Add dense reward shaping."""
        if not self.reward_shaping:
            return reward

        shaped = reward
        curr_state = self._get_state_for_shaping(obs)

        if self.prev_state:
            # Reward for exploration
            explored_delta = curr_state["explored_tiles"] - self.prev_state["explored_tiles"]
            shaped += explored_delta * 0.1

            # Reward for economy growth
            currency_delta = curr_state["currency"] - self.prev_state["currency"]
            shaped += currency_delta * 0.01

            # Reward for army growth
            unit_delta = curr_state["num_units"] - self.prev_state["num_units"]
            shaped += unit_delta * 0.5

            # Reward for city growth
            city_delta = curr_state["num_cities"] - self.prev_state["num_cities"]
            shaped += city_delta * 2.0

            # Reward for kills
            kill_delta = curr_state["num_kills"] - self.prev_state["num_kills"]
            shaped += kill_delta * 1.0

        # Small reward for taking meaningful actions (not just END_TURN)
        if action_type not in [0, None]:  # Not END_TURN
            shaped += 0.01

        self.prev_state = curr_state
        return shaped

    def _track_action(self, action, reward):
        """Track action stats."""
        self.episode_stats["total_actions"] += 1

        if reward == -1.0:  # Invalid action penalty
            self.episode_stats["invalid_actions"] += 1
            return

        if action is None:
            return

        action_type = action[0]
        if action_type == 0:
            self.episode_stats["end_turns"] += 1
        elif action_type == 1:
            self.episode_stats["moves"] += 1
        elif action_type == 2:
            self.episode_stats["attacks"] += 1
        elif action_type == 3:
            self.episode_stats["builds"] += 1
        elif action_type == 4:
            self.episode_stats["trains"] += 1
        elif action_type == 5:
            self.episode_stats["research"] += 1
        else:
            self.episode_stats["other"] += 1

    def reset(self, seed=None, options=None):
        if seed is not None:
            self.aec_env.reset(seed=seed)
        else:
            self.aec_env.reset()
        self.steps = 0
        self.prev_state = {}
        self.episode_stats = {k: 0 for k in self.episode_stats}

        self._update_mask()
        agent = self.aec_env.agent_selection
        obs = self.aec_env.observe(agent)
        self.prev_state = self._get_state_for_shaping(obs)
        return self._flatten_obs(obs), {}

    def step(self, action):
        self.steps += 1
        agent = self.aec_env.agent_selection

        if agent not in self.aec_env.agents:
            action_tuple = None
        else:
            action_tuple = tuple(int(a) for a in action)

        self.aec_env.step(action_tuple)

        # Get base reward
        base_reward = self.aec_env.rewards.get(agent, 0)
        self._track_action(action_tuple, base_reward)

        # Get observation for shaping
        next_agent = self.aec_env.agent_selection
        if next_agent in self.aec_env.agents:
            obs = self.aec_env.observe(next_agent)
            flat_obs = self._flatten_obs(obs)
        else:
            obs = {}
            flat_obs = np.zeros(self.observation_space.shape, dtype=np.float32)

        # Apply reward shaping
        action_type = action_tuple[0] if action_tuple else None
        shaped_reward = self._compute_shaped_reward(base_reward, obs, action_type)

        # Check termination
        all_done = all(
            self.aec_env.terminations.get(a, False) or self.aec_env.truncations.get(a, False)
            for a in self.aec_env.possible_agents
        )
        truncated = self.steps >= self.max_steps
        terminated = all_done

        self._update_mask()

        return flat_obs, shaped_reward, terminated, truncated, {"stats": self.episode_stats.copy()}

    def action_masks(self):
        """Return flattened action mask for MaskablePPO with MultiDiscrete."""
        if self._current_mask is None:
            self._update_mask()
        return np.concatenate(self._current_mask)

    def get_episode_stats(self):
        """Get current episode stats."""
        return self.episode_stats.copy()

    def close(self):
        self.aec_env.close()


class ReplayAndStatsCallback(BaseCallback):
    """Callback to log stats and save replays during training."""

    def __init__(self, save_freq=10000, replay_freq=50000, replay_dir="replays", verbose=0):
        super().__init__(verbose)
        self.save_freq = save_freq
        self.replay_freq = replay_freq
        self.replay_dir = replay_dir
        self.episode_count = 0
        self.cumulative_stats = {
            "invalid_actions": 0,
            "end_turns": 0,
            "moves": 0,
            "attacks": 0,
            "builds": 0,
            "trains": 0,
            "research": 0,
            "other": 0,
            "total_actions": 0,
        }
        os.makedirs(replay_dir, exist_ok=True)

    def _on_step(self) -> bool:
        # Check for episode end via info
        infos = self.locals.get("infos", [])
        for info in infos:
            if "stats" in info:
                stats = info["stats"]
                for k, v in stats.items():
                    self.cumulative_stats[k] += v
                self.episode_count += 1

        # Log stats periodically
        if self.n_calls % self.save_freq == 0 and self.cumulative_stats["total_actions"] > 0:
            total = self.cumulative_stats["total_actions"]
            invalid_rate = self.cumulative_stats["invalid_actions"] / total
            end_turn_rate = self.cumulative_stats["end_turns"] / total

            wandb.log({
                "custom/invalid_action_rate": invalid_rate,
                "custom/end_turn_rate": end_turn_rate,
                "custom/move_rate": self.cumulative_stats["moves"] / total,
                "custom/attack_rate": self.cumulative_stats["attacks"] / total,
                "custom/build_rate": self.cumulative_stats["builds"] / total,
                "custom/train_rate": self.cumulative_stats["trains"] / total,
                "custom/research_rate": self.cumulative_stats["research"] / total,
                "custom/episodes": self.episode_count,
            }, step=self.n_calls)

            # Reset stats
            self.cumulative_stats = {k: 0 for k in self.cumulative_stats}

        # Save replay periodically
        if self.n_calls % self.replay_freq == 0:
            self._save_replay()

        return True

    def _save_replay(self):
        """Play one episode and save as JSON replay."""
        env = self.training_env.envs[0]
        unwrapped = env.env  # Get through ActionMasker wrapper

        replay_data = {
            "metadata": {"step": self.n_calls, "type": "training_checkpoint"},
            "steps": []
        }

        obs, _ = unwrapped.reset()
        done = False
        step_count = 0

        while not done and step_count < 100:
            action_masks = unwrapped.action_masks()
            action, _ = self.model.predict(obs, action_masks=action_masks, deterministic=True)

            # Record step
            replay_data["steps"].append({
                "step": step_count,
                "action_type": int(action[0]),
                "action_name": ACTION_NAMES.get(int(action[0]), f"ACTION_{action[0]}"),
                "action": [int(a) for a in action],
            })

            obs, reward, terminated, truncated, info = unwrapped.step(action)
            done = terminated or truncated
            step_count += 1

        # Save replay
        filepath = os.path.join(self.replay_dir, f"replay_step_{self.n_calls}.json")
        with open(filepath, "w") as f:
            json.dump(replay_data, f, indent=2)

        wandb.save(filepath)


def mask_fn(env):
    return env.action_masks()


def main():
    # Training config
    config = {
        "algorithm": "MaskablePPO",
        "policy": "MlpPolicy",
        "total_timesteps": 1_000_000,
        "learning_rate": 3e-4,
        "n_steps": 512,
        "batch_size": 64,
        "n_epochs": 4,
        "gamma": 0.99,
        "ent_coef": 0.05,  # Higher entropy for more exploration
        "vf_coef": 0.5,
        "max_grad_norm": 0.5,
        "num_players": 2,
        "max_steps": 200,
        "map_size": 15,
        "reward_shaping": True,
    }

    # Initialize W&B
    run = wandb.init(
        project="polyterra",
        config=config,
        save_code=True,
        sync_tensorboard=True,
    )

    # Create environment with reward shaping
    env = PolyterraGymWrapper(
        num_players=config["num_players"],
        max_steps=config["max_steps"],
        map_size=config["map_size"],
        reward_shaping=config["reward_shaping"],
    )
    env = ActionMasker(env, mask_fn)

    # Create model
    model = MaskablePPO(
        config["policy"],
        env,
        verbose=0,
        learning_rate=config["learning_rate"],
        n_steps=config["n_steps"],
        batch_size=config["batch_size"],
        n_epochs=config["n_epochs"],
        gamma=config["gamma"],
        ent_coef=config["ent_coef"],
        vf_coef=config["vf_coef"],
        max_grad_norm=config["max_grad_norm"],
        tensorboard_log=f"runs/{run.id}",
    )

    # Callbacks
    callbacks = [
        WandbCallback(
            model_save_path=f"models/{run.id}",
            model_save_freq=50_000,
            verbose=0,
        ),
        ReplayAndStatsCallback(
            save_freq=5000,
            replay_freq=50_000,
            replay_dir=f"replays/{run.id}",
            verbose=0,
        ),
    ]

    # Train
    model.learn(
        total_timesteps=config["total_timesteps"],
        callback=callbacks,
    )

    # Save final model
    model.save(f"models/{run.id}/final_model")

    # Final test episode
    test_env = PolyterraGymWrapper(num_players=2, max_steps=100, reward_shaping=False)
    test_env = ActionMasker(test_env, mask_fn)
    obs, _ = test_env.reset()
    total_reward = 0
    steps = 0

    while True:
        action_masks = test_env.action_masks()
        action, _ = model.predict(obs, action_masks=action_masks, deterministic=True)
        obs, reward, terminated, truncated, _ = test_env.step(action)
        total_reward += reward
        steps += 1
        if terminated or truncated:
            break

    wandb.log({"test/episode_reward": total_reward, "test/episode_length": steps})

    test_env.close()
    env.close()
    wandb.finish()


if __name__ == "__main__":
    main()
