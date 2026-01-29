"""
PPO training for Polyterra with FULL action space.
Properly handles MultiDiscrete actions with action masking.
"""
import sys
sys.path.insert(0, '../polyterra-env-py')

import numpy as np
from polyterra_env import PolyterraEnv
from sb3_contrib import MaskablePPO
from sb3_contrib.common.wrappers import ActionMasker
from sb3_contrib.common.maskable.policies import MaskableMultiInputActorCriticPolicy
import gymnasium as gym
from gymnasium import spaces


class PolyterraGymWrapper(gym.Env):
    """
    Wraps Polyterra AEC env for SB3 with FULL action space.
    - Flattens observations for MLP compatibility
    - Keeps full MultiDiscrete action space
    - Proper action masking for all action components
    """

    def __init__(self, num_players=2, max_steps=200, map_size=15):
        super().__init__()
        self.aec_env = PolyterraEnv(num_players=num_players)
        self.max_steps = max_steps
        self.map_size = map_size
        self.steps = 0

        # Initialize env
        self.aec_env.reset()

        # FULL action space: [action_type, target_x, target_y, unit_idx, param1, param2]
        # This matches the env's MultiDiscrete([37, 50, 50, 100, 47, 10])
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
            # Dead agent - only END_TURN is valid
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
                # Pad or truncate to expected size
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
                m[0] = 1  # Only END_TURN
            else:
                m[0] = 1  # Allow index 0 for other components
            masks.append(m)
        return tuple(masks)

    def _make_all_valid_mask(self):
        """Create mask where all actions are valid."""
        sizes = [37, 50, 50, 100, 47, 10]
        return tuple(np.ones(s, dtype=np.int8) for s in sizes)

    def reset(self, seed=None, options=None):
        if seed is not None:
            self.aec_env.reset(seed=seed)
        else:
            self.aec_env.reset()
        self.steps = 0

        self._update_mask()
        agent = self.aec_env.agent_selection
        obs = self.aec_env.observe(agent)
        return self._flatten_obs(obs), {}

    def step(self, action):
        self.steps += 1
        agent = self.aec_env.agent_selection

        # Check if agent is dead
        if agent not in self.aec_env.agents:
            action = None
        else:
            # Convert numpy array to tuple for the env
            action = tuple(int(a) for a in action)

        self.aec_env.step(action)

        # Get reward
        reward = self.aec_env.rewards.get(agent, 0)

        # Check termination
        all_done = all(
            self.aec_env.terminations.get(a, False) or self.aec_env.truncations.get(a, False)
            for a in self.aec_env.possible_agents
        )
        truncated = self.steps >= self.max_steps
        terminated = all_done

        # Update mask and get observation
        self._update_mask()
        next_agent = self.aec_env.agent_selection
        if next_agent in self.aec_env.agents:
            obs = self.aec_env.observe(next_agent)
            flat_obs = self._flatten_obs(obs)
        else:
            flat_obs = np.zeros(self.observation_space.shape, dtype=np.float32)

        return flat_obs, reward, terminated, truncated, {}

    def action_masks(self):
        """Return flattened action mask for MaskablePPO with MultiDiscrete."""
        # For MultiDiscrete, we need to concatenate all masks
        if self._current_mask is None:
            self._update_mask()
        return np.concatenate(self._current_mask)

    def close(self):
        self.aec_env.close()


def mask_fn(env):
    return env.action_masks()


def main():
    print("=" * 60)
    print("POLYTERRA PPO TRAINING (FULL ACTION SPACE)")
    print("=" * 60)

    # Create environment
    print("\nCreating environment...")
    env = PolyterraGymWrapper(num_players=2, max_steps=100)
    env = ActionMasker(env, mask_fn)

    print(f"Observation space: {env.observation_space}")
    print(f"Action space: {env.action_space}")

    # Create model
    print("\nCreating MaskablePPO model...")
    model = MaskablePPO(
        "MlpPolicy",
        env,
        verbose=1,
        learning_rate=3e-4,
        n_steps=256,
        batch_size=64,
        n_epochs=4,
        gamma=0.99,
        ent_coef=0.01,
    )

    # Train
    print("\nTraining for 20k steps...")
    print("-" * 60)
    model.learn(total_timesteps=20_000)

    # Save model
    model.save("polyterra_ppo_full")
    print("\nModel saved to polyterra_ppo_full.zip")

    # Test the trained model
    print("\n" + "=" * 60)
    print("TESTING TRAINED MODEL")
    print("=" * 60)

    test_env = PolyterraGymWrapper(num_players=2, max_steps=50)
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

    print(f"\nTest episode: {steps} steps, total reward: {total_reward}")

    test_env.close()
    env.close()

    print("\nDone!")


if __name__ == "__main__":
    main()
