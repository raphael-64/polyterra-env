"""
RLlib PPO training for Polyterra environment.
Proper multi-agent support with complex observation spaces.
"""
import sys
sys.path.insert(0, '../polyterra-env-py')

import numpy as np
from polyterra_env import PolyterraEnv

from ray import tune
from ray.rllib.algorithms.ppo import PPOConfig
from ray.rllib.env import PettingZooEnv
from ray.tune.registry import register_env


def env_creator(config):
    """Create Polyterra environment wrapped for RLlib."""
    env = PolyterraEnv(num_players=config.get("num_players", 2))
    return PettingZooEnv(env)


def main():
    print("=" * 60)
    print("POLYTERRA RLLIB PPO TRAINING")
    print("=" * 60)

    # Register environment
    register_env("polyterra", env_creator)

    # Create a test env to get observation/action spaces
    test_env = env_creator({"num_players": 2})
    obs_space = test_env.observation_space
    act_space = test_env.action_space
    test_env.close()

    print(f"\nObservation space: {obs_space}")
    print(f"Action space: {act_space}")

    # Configure PPO
    config = (
        PPOConfig()
        .environment(
            env="polyterra",
            env_config={"num_players": 2},
        )
        .framework("torch")
        .env_runners(
            num_env_runners=0,  # Use local worker only for simplicity
            rollout_fragment_length=128,
        )
        .training(
            train_batch_size_per_learner=512,
            minibatch_size=64,
            num_epochs=4,
            lr=3e-4,
            gamma=0.99,
            entropy_coeff=0.01,
        )
        .multi_agent(
            policies={"shared_policy"},
            policy_mapping_fn=lambda agent_id, episode, worker, **kwargs: "shared_policy",
        )
        .resources(num_gpus=0)
    )

    # Build algorithm
    print("\nBuilding PPO algorithm...")
    algo = config.build()

    # Train
    print("\nTraining for 10 iterations...")
    print("-" * 60)

    for i in range(10):
        result = algo.train()
        mean_reward = result.get("env_runners", {}).get("episode_reward_mean", 0)
        episodes = result.get("env_runners", {}).get("num_episodes", 0)
        print(f"Iteration {i+1}: reward={mean_reward:.2f}, episodes={episodes}")

    # Save checkpoint
    checkpoint_dir = algo.save("./checkpoints")
    print(f"\nCheckpoint saved to: {checkpoint_dir}")

    # Test
    print("\n" + "=" * 60)
    print("TESTING TRAINED MODEL")
    print("=" * 60)

    env = env_creator({"num_players": 2})
    obs, info = env.reset()

    total_rewards = {agent: 0 for agent in env.possible_agents}
    steps = 0

    while True:
        actions = {}
        for agent_id, agent_obs in obs.items():
            action = algo.compute_single_action(agent_obs, policy_id="shared_policy")
            actions[agent_id] = action

        obs, rewards, terminateds, truncateds, infos = env.step(actions)

        for agent, r in rewards.items():
            total_rewards[agent] += r
        steps += 1

        if terminateds.get("__all__", False) or truncateds.get("__all__", False):
            break

        if steps >= 100:
            break

    print(f"\nTest episode: {steps} steps")
    for agent, reward in total_rewards.items():
        print(f"  {agent}: {reward:.2f}")

    env.close()
    algo.stop()

    print("\nDone!")


if __name__ == "__main__":
    main()
