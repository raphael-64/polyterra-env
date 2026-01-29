"""
Watch a trained agent play Polyterra with readable output.
"""
import sys
sys.path.insert(0, '../polyterra-env-py')

import numpy as np
from polyterra_env import PolyterraEnv
from sb3_contrib import MaskablePPO
from sb3_contrib.common.wrappers import ActionMasker
import time

# Action type names
ACTION_NAMES = {
    0: "END_TURN",
    1: "MOVE",
    2: "ATTACK",
    3: "CAPTURE",
    4: "BUILD",
    5: "RESEARCH",
    6: "TRAIN",
    7: "UPGRADE_UNIT",
    8: "DISBAND",
    9: "EXAMINE_RUINS",
    10: "HEAL",
    11: "CONVERT",
    12: "MIND_BEND",
    13: "GROW_FOREST",
    14: "BURN_FOREST",
    15: "DESTROY",
    16: "CLEAR_FOREST",
}


class WatchableEnv:
    """Simple wrapper to watch games."""

    def __init__(self, num_players=2):
        self.env = PolyterraEnv(num_players=num_players)
        self.map_size = 15

    def reset(self, seed=None):
        self.env.reset(seed=seed)
        return self._get_obs()

    def _get_obs(self):
        agent = self.env.agent_selection
        if agent in self.env.agents:
            return self.env.observe(agent)
        return None

    def _get_action_mask(self):
        obs = self._get_obs()
        if obs is None:
            return np.ones(37, dtype=np.int8)
        mask = obs.get('action_mask', None)
        if mask and isinstance(mask, tuple) and len(mask) > 0:
            type_mask = np.array(mask[0], dtype=np.int8)
            if len(type_mask) < 37:
                type_mask = np.pad(type_mask, (0, 37 - len(type_mask)))
            return type_mask[:37]
        return np.ones(37, dtype=np.int8)

    def step(self, action_type):
        """Take action and return info."""
        agent = self.env.agent_selection
        obs_before = self._get_obs()

        # Convert to full action tuple
        action = (int(action_type), 0, 0, 0, 0, 0)
        self.env.step(action)

        obs_after = self._get_obs()
        reward = self.env.rewards.get(agent, 0)

        done = all(
            self.env.terminations.get(a, False) or self.env.truncations.get(a, False)
            for a in self.env.possible_agents
        )

        return obs_after, reward, done, agent, obs_before

    def get_game_state(self):
        """Get readable game state."""
        obs = self._get_obs()
        if obs is None:
            return None
        return {
            'agent': self.env.agent_selection,
            'turn': obs.get('turn', 0),
            'currency': obs.get('currency', 0),
            'score': obs.get('score', 0),
            'num_units': len(obs.get('units', [])),
            'num_cities': obs.get('num_cities', 0),
            'valid_actions': [ACTION_NAMES.get(i, f"ACTION_{i}")
                            for i, v in enumerate(self._get_action_mask()) if v],
        }

    def close(self):
        self.env.close()


def flatten_obs(obs, map_size=15):
    """Flatten observation for model."""
    flat = []
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

    tiles = obs.get('tiles', [])
    tile_grid = np.zeros((map_size, map_size, 4), dtype=np.float32)
    for tile in tiles:
        x, y = tile.get('x', 0), tile.get('y', 0)
        if 0 <= x < map_size and 0 <= y < map_size:
            tile_grid[x, y, 0] = tile.get('explored', 0)
            tile_grid[x, y, 1] = tile.get('owner', 0) / 4.0
            tile_grid[x, y, 2] = tile.get('has_unit', 0)
            tile_grid[x, y, 3] = tile.get('terrain', 0) / 7.0
    flat.extend(tile_grid.flatten())

    return np.array(flat, dtype=np.float32)


def watch_random_game(max_steps=30):
    """Watch a game with random actions."""
    print("=" * 60)
    print("WATCHING RANDOM GAME")
    print("=" * 60)

    env = WatchableEnv(num_players=2)
    env.reset(seed=42)

    for step in range(max_steps):
        state = env.get_game_state()
        if state is None:
            break

        print(f"\n--- Step {step + 1} ---")
        print(f"Agent: {state['agent']} | Turn: {state['turn']} | Currency: {state['currency']} | Score: {state['score']}")
        print(f"Units: {state['num_units']} | Cities: {state['num_cities']}")
        print(f"Valid: {', '.join(state['valid_actions'][:5])}...")

        # Pick random valid action
        mask = env._get_action_mask()
        valid_actions = np.where(mask == 1)[0]
        if len(valid_actions) == 0:
            action = 0
        else:
            action = np.random.choice(valid_actions)

        action_name = ACTION_NAMES.get(action, f"ACTION_{action}")
        print(f">> Taking action: {action_name}")

        obs, reward, done, agent, _ = env.step(action)
        print(f"   Reward: {reward}")

        if done:
            print("\n*** GAME OVER ***")
            break

        time.sleep(0.1)

    env.close()


def watch_trained_game(model_path="polyterra_ppo.zip", max_steps=30):
    """Watch trained model play."""
    print("=" * 60)
    print("WATCHING TRAINED MODEL")
    print("=" * 60)

    # Load model
    try:
        model = MaskablePPO.load(model_path)
        print(f"Loaded model from {model_path}")
    except:
        print(f"Could not load {model_path}, using random actions")
        model = None

    env = WatchableEnv(num_players=2)
    env.reset(seed=123)

    total_reward = 0

    for step in range(max_steps):
        state = env.get_game_state()
        if state is None:
            break

        print(f"\n--- Step {step + 1} ---")
        print(f"Agent: {state['agent']} | Turn: {state['turn']} | Currency: {state['currency']} | Score: {state['score']}")
        print(f"Units: {state['num_units']} | Cities: {state['num_cities']}")
        print(f"Valid: {', '.join(state['valid_actions'][:6])}")

        mask = env._get_action_mask()

        if model:
            obs = env._get_obs()
            flat_obs = flatten_obs(obs)
            action, _ = model.predict(flat_obs, action_masks=mask, deterministic=True)
            action = int(action)
        else:
            valid_actions = np.where(mask == 1)[0]
            action = np.random.choice(valid_actions) if len(valid_actions) > 0 else 0

        action_name = ACTION_NAMES.get(action, f"ACTION_{action}")
        print(f">> Action: {action_name}")

        obs, reward, done, agent, _ = env.step(action)
        total_reward += reward

        if reward != 0:
            print(f"   Reward: {reward:+.0f} (Total: {total_reward:.0f})")

        if done:
            print("\n*** GAME OVER ***")
            break

        time.sleep(0.2)

    print(f"\n{'=' * 60}")
    print(f"Final total reward: {total_reward}")
    env.close()


if __name__ == "__main__":
    import argparse
    parser = argparse.ArgumentParser()
    parser.add_argument("--random", action="store_true", help="Watch random game")
    parser.add_argument("--steps", type=int, default=30, help="Max steps")
    args = parser.parse_args()

    if args.random:
        watch_random_game(max_steps=args.steps)
    else:
        watch_trained_game(max_steps=args.steps)
