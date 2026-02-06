"""
RLlib Self-Play PPO training for Polyterra.
Both players share the same policy - as it improves, so does the opponent.
"""
import os

# Remove RAY_RUNTIME_ENV_HOOK if set (can cause issues with uv-managed environments)
if "RAY_RUNTIME_ENV_HOOK" in os.environ:
    del os.environ["RAY_RUNTIME_ENV_HOOK"]
import numpy as np
from datetime import datetime

from polyterra_env import PolyterraEnv

import ray
from ray.rllib.algorithms.ppo import PPOConfig
from ray.rllib.env.multi_agent_env import MultiAgentEnv
from ray.tune.registry import register_env
from ray.rllib.algorithms.callbacks import DefaultCallbacks

from gymnasium import spaces

# Optional: W&B integration
try:
    import wandb
    HAS_WANDB = True
except ImportError:
    HAS_WANDB = False
    print("wandb not available, logging to console only")


_env_instance_count = 0

class FlattenedPolyterraEnv(MultiAgentEnv):
    """
    RLlib MultiAgentEnv wrapper around PolyterraEnv.

    Handles:
    - Observation flattening (Dict -> flat array for neural net)
    - Action masking via infos
    - Proper multi-agent step semantics
    """

    def __init__(self, config=None):
        global _env_instance_count
        _env_instance_count += 1
        self._instance_id = _env_instance_count
        print(f"[ENV] Creating FlattenedPolyterraEnv instance #{self._instance_id}")
        super().__init__()
        config = config or {}
        self.num_players = config.get("num_players", 2)
        self.map_size = config.get("map_size", 16)
        self.max_steps = config.get("max_steps", 2000)

        # Create the underlying PettingZoo env
        self.env = PolyterraEnv(
            num_players=self.num_players,
            map_size=self.map_size,
        )

        self.steps = 0

        # Observation dimensionality: 10 scalars + map_size^2 * 4 tile features
        self._obs_dim = 10 + self.map_size * self.map_size * 4

        # Agent IDs
        self._agent_ids = set(f"player_{i}" for i in range(self.num_players))
        self.possible_agents = [f"player_{i}" for i in range(self.num_players)]
        self.agents = []  # Set in reset()

        # Define spaces (shared by all agents)
        self._obs_space = spaces.Box(-1.0, 1.0, shape=(self._obs_dim,), dtype=np.float32)
        self._action_space = spaces.Discrete(512)

    @property
    def observation_space(self):
        # Return dict mapping agent_id -> space for RLlib multi-agent
        return spaces.Dict({
            agent: self._obs_space for agent in self.possible_agents
        })

    @property
    def action_space(self):
        # Return dict mapping agent_id -> space for RLlib multi-agent
        return spaces.Dict({
            agent: self._action_space for agent in self.possible_agents
        })

    def get_observation_space(self, agent_id):
        return self._obs_space

    def get_action_space(self, agent_id):
        return self._action_space

    def _flatten_obs(self, obs_dict):
        """Convert dict observation to flat numpy array."""
        if isinstance(obs_dict, np.ndarray):
            return obs_dict

        # Scalar features (normalized)
        flat = [
            obs_dict.get('currency', 0) / 1000.0,
            obs_dict.get('score', 0) / 10000.0,
            obs_dict.get('turn', 0) / 100.0,
            obs_dict.get('num_cities', 0) / 10.0,
            obs_dict.get('num_kills', 0) / 100.0,
            obs_dict.get('num_casualties', 0) / 100.0,
            obs_dict.get('current_player_idx', 0) / float(self.num_players),
            obs_dict.get('player_id', 0) / float(self.num_players + 1),
            len(obs_dict.get('units', [])) / 50.0,
            len(obs_dict.get('cities', [])) / 20.0,
        ]

        # Tile grid features
        tile_grid = np.zeros((self.map_size, self.map_size, 4), dtype=np.float32)
        for tile in obs_dict.get('tiles', []):
            x, y = tile.get('x', 0), tile.get('y', 0)
            if 0 <= x < self.map_size and 0 <= y < self.map_size:
                tile_grid[x, y] = [
                    tile.get('explored', 0),
                    tile.get('owner', 0) / float(self.num_players + 1),
                    tile.get('has_unit', 0),
                    tile.get('terrain', 0) / 7.0,
                ]

        return np.array(flat + list(tile_grid.flatten()), dtype=np.float32)

    def reset(self, *, seed=None, options=None):
        if not hasattr(self, '_reset_count'):
            self._reset_count = 0
        self._reset_count += 1
        # Log every 50 resets to reduce spam
        if self._reset_count % 50 == 0:
            print(f"[ENV #{self._instance_id}] Reset #{self._reset_count}")
        self.env.reset(seed=seed)
        self.steps = 0

        # Update agents list from underlying env
        self.agents = list(self.env.agents)

        obs = {}
        infos = {}
        for agent in self.agents:
            raw_obs = self.env.observe(agent)
            obs[agent] = self._flatten_obs(raw_obs)
            mask = raw_obs.get('valid_actions_mask', np.ones(512, dtype=np.int8))
            infos[agent] = {"action_mask": np.array(mask, dtype=np.float32)[:512]}

        return obs, infos

    def step(self, action_dict):
        """Execute actions for all agents."""
        self.steps += 1

        obs = {}
        rewards = {}
        terminateds = {"__all__": False}
        truncateds = {"__all__": False}
        infos = {}

        # PettingZoo AEC: process one agent at a time
        # RLlib may pass actions for multiple agents, but we step through in order
        for agent in list(self.env.agents):
            if agent not in action_dict:
                continue
            if self.env.agent_selection != agent:
                continue

            action = int(action_dict[agent])
            self.env.step(action)

        # Update agents list from underlying env
        self.agents = list(self.env.agents)

        # Get observations and rewards for all agents
        # RLlib requires all agents in obs to have rewards
        for agent in self.agents:
            raw_obs = self.env.observe(agent)
            obs[agent] = self._flatten_obs(raw_obs)
            mask = raw_obs.get('valid_actions_mask', np.ones(512, dtype=np.int8))
            infos[agent] = {"action_mask": np.array(mask, dtype=np.float32)[:512]}
            rewards[agent] = self.env.rewards.get(agent, 0.0)
            terminateds[agent] = self.env.terminations.get(agent, False)
            truncateds[agent] = self.env.truncations.get(agent, False)

        # Check termination
        all_done = all(
            self.env.terminations.get(a, False) or self.env.truncations.get(a, False)
            for a in self.env.possible_agents
        )
        truncated_by_steps = self.steps >= self.max_steps

        terminateds["__all__"] = all_done
        truncateds["__all__"] = truncated_by_steps

        return obs, rewards, terminateds, truncateds, infos

    def close(self):
        self.env.close()

    def __del__(self):
        """Ensure cleanup on garbage collection"""
        try:
            self.close()
        except:
            pass

    def render(self):
        pass


def env_creator(config):
    """Create Polyterra environment for RLlib."""
    return FlattenedPolyterraEnv(config)


class SelfPlayCallback(DefaultCallbacks):
    """Callback to log self-play specific metrics."""
    pass  # Using default callbacks for now - new API stack metrics work differently


def main():
    print("=" * 60)
    print("POLYTERRA SELF-PLAY TRAINING")
    print("=" * 60)

    # Initialize Ray
    ray.init(ignore_reinit_error=True)

    # Initialize W&B if available
    if HAS_WANDB:
        # Get training dir for logs regardless of where we run from
        training_dir = os.path.dirname(os.path.abspath(__file__))
        wandb.init(
            project="polyterra-rl",
            name=f"selfplay_{datetime.now().strftime('%Y%m%d_%H%M%S')}",
            dir=training_dir,  # Logs go to training/ not cwd
            config={
                "algorithm": "PPO",
                "framework": "torch",
                "num_players": 2,
                "map_size": 16,
                "max_steps": 500,
                "train_batch_size": 2048,
                "training_iterations": 500,
            }
        )
        print("W&B initialized")

    # Register environment
    register_env("polyterra", env_creator)

    # Create test env to verify spaces
    test_env = env_creator({"num_players": 2})
    print(f"\nObservation space: {test_env.observation_space}")
    print(f"Action space: {test_env.action_space}")
    test_env.close()

    # Training config
    training_iterations = 500  # Full training run
    checkpoint_freq = 50

    # Configure PPO for self-play
    config = (
        PPOConfig()
        .environment(
            env="polyterra",
            env_config={
                "num_players": 2,
                "map_size": 16,  # Match PolyterraEnv default
                "max_steps": 500,  # Longer games for strategic learning
            },
        )
        .framework("torch")
        .env_runners(
            num_env_runners=0,  # Local mode - Ray+uv remote workers are broken
            rollout_fragment_length=256,
        )
        .training(
            train_batch_size=2048,
            minibatch_size=128,
            num_epochs=4,
            lr=3e-4,
            gamma=0.99,
            lambda_=0.95,  # GAE lambda
            entropy_coeff=0.01,  # Encourage exploration
            vf_loss_coeff=0.5,
            clip_param=0.2,
            grad_clip=0.5,
        )
        .multi_agent(
            policies={"shared_policy"},
            # Both players use the same policy (self-play)
            policy_mapping_fn=lambda agent_id, episode, **kwargs: "shared_policy",
        )
        .resources(num_gpus=0)
        .fault_tolerance(
            restart_failed_sub_environments=True,  # Auto-restart crashed envs
            num_consecutive_env_runner_failures_tolerance=100,
        )
        .callbacks(SelfPlayCallback)
        .reporting(
            min_sample_timesteps_per_iteration=1000,
        )
    )

    # Build algorithm
    print("\nBuilding PPO algorithm...")
    algo = config.build()

    # Create checkpoint directory (in training/ regardless of cwd)
    training_dir = os.path.dirname(os.path.abspath(__file__))
    checkpoint_dir = os.path.join(training_dir, f"checkpoints/selfplay_{datetime.now().strftime('%Y%m%d_%H%M%S')}")
    os.makedirs(checkpoint_dir, exist_ok=True)

    # Training loop
    print(f"\nTraining for {training_iterations} iterations...")
    print("-" * 60)

    best_reward = float('-inf')

    for i in range(training_iterations):
        result = algo.train()

        # Extract metrics (new API uses episode_return_mean)
        env_runners = result.get("env_runners", {})
        mean_reward = env_runners.get("episode_return_mean", 0)  # New API
        if mean_reward == 0 or mean_reward is None:
            mean_reward = env_runners.get("episode_reward_mean", 0)  # Old API fallback
        episodes = env_runners.get("num_episodes", 0)
        timesteps = result.get("num_env_steps_sampled_lifetime",
                              env_runners.get("num_env_steps_sampled_lifetime", 0))

        # Learner metrics (new API: result['learners']['shared_policy'])
        learners = result.get("learners", {})
        policy_stats = learners.get("shared_policy", {})
        policy_loss = policy_stats.get("policy_loss", 0)
        vf_loss = policy_stats.get("vf_loss", 0)
        entropy = policy_stats.get("entropy", 0)
        vf_explained_var = policy_stats.get("vf_explained_var", 0)
        total_loss = policy_stats.get("total_loss", 0)
        kl_loss = policy_stats.get("mean_kl_loss", 0)

        print(f"Iter {i+1:3d} | reward: {mean_reward:7.2f} | episodes: {int(episodes):3d} | "
              f"timesteps: {int(timesteps):6d} | expl_var: {vf_explained_var:.3f} | "
              f"entropy: {entropy:.3f}")

        # Log to W&B
        if HAS_WANDB:
            wandb.log({
                "reward/mean": mean_reward,
                "reward/min": env_runners.get("episode_return_min", 0),
                "reward/max": env_runners.get("episode_return_max", 0),
                "episodes": episodes,
                "timesteps": timesteps,
                "train/policy_loss": policy_loss,
                "train/vf_loss": vf_loss,
                "train/total_loss": total_loss,
                "train/entropy": entropy,
                "train/kl_loss": kl_loss,
                "train/vf_explained_var": vf_explained_var,
                "iteration": i + 1,
            })

        # Save checkpoint periodically
        if (i + 1) % checkpoint_freq == 0:
            path = algo.save(checkpoint_dir)
            print(f"  -> Saved checkpoint: {path}")

        # Track best
        if mean_reward > best_reward:
            best_reward = mean_reward
            best_path = algo.save(f"{checkpoint_dir}/best")
            print(f"  -> New best! reward={best_reward:.2f}")

    # Final save
    final_path = algo.save(f"{checkpoint_dir}/final")
    print(f"\nFinal checkpoint: {final_path}")

    algo.stop()
    ray.shutdown()

    # Close W&B
    if HAS_WANDB:
        wandb.finish()

    print("\nDone!")


if __name__ == "__main__":
    main()
