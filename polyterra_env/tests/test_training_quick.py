"""
Quick training test to verify:
1. Rewards are score-based
2. Actions execute properly
3. Environment works for multiple episodes
"""

import numpy as np
from polyterra_env import PolyterraEnv


def test_basic_training():
    """Test basic training loop with random actions"""
    print("="*70)
    print("QUICK TRAINING TEST")
    print("="*70)

    env = PolyterraEnv(
        num_players=4,
        game_mode="perfection",
        max_turns=10,
        render_mode=None
    )

    num_episodes = 3
    all_rewards = []

    for episode in range(num_episodes):
        print(f"\nEpisode {episode + 1}/{num_episodes}")

        env.reset(seed=42 + episode)

        episode_rewards = {agent: 0.0 for agent in env.agents}
        step_count = 0
        max_steps = 200

        for agent in env.agent_iter(max_iter=max_steps):
            obs = env.observe(agent)

            # Check if agent is already terminated
            if env.terminations.get(agent) or env.truncations.get(agent):
                action = None
            else:
                # Random action (mostly END_TURN for now)
                if np.random.random() < 0.9:
                    action = np.array([0, 0, 0, 0, 0, 0])  # END_TURN
                else:
                    action = env.action_space(agent).sample()

            env.step(action)

            # Track rewards
            reward = env.rewards.get(agent, 0)
            episode_rewards[agent] += reward

            step_count += 1

        # Print episode summary
        print(f"  Steps: {step_count}")
        print(f"  Rewards: {episode_rewards}")
        print(f"  Final scores: {[obs.get('score', 0) for obs in [env.observe(a) for a in env.agents]]}")

        all_rewards.append(episode_rewards)

    env.close()

    print("\n" + "="*70)
    print("TEST RESULTS")
    print("="*70)
    print(f"Completed {num_episodes} episodes")
    print("\nReward summary:")
    for i, rewards in enumerate(all_rewards):
        print(f"  Episode {i}: {rewards}")

    # Verify rewards are non-zero (score-based)
    total_reward = sum(sum(r.values()) for r in all_rewards)
    print(f"\nTotal reward across all episodes: {total_reward}")

    if total_reward > 0:
        print("SUCCESS: Rewards are score-based!")
    else:
        print("WARNING: All rewards are zero - scores not changing")

    return all_rewards


def test_specific_actions():
    """Test specific action types"""
    print("\n" + "="*70)
    print("ACTION EXECUTION TEST")
    print("="*70)

    env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10)
    env.reset(seed=42)

    agent = env.agent_selection
    obs = env.observe(agent)

    print(f"\nInitial state:")
    print(f"  Agent: {agent}")
    print(f"  Currency: {obs.get('currency')} stars")
    print(f"  Score: {obs.get('score')}")
    print(f"  Cities: {len(obs.get('cities', []))}")
    print(f"  Units: {len(obs.get('units', []))}")

    # Test END_TURN
    print(f"\nTest 1: END_TURN action")
    action = np.array([0, 0, 0, 0, 0, 0])
    env.step(action)
    print(f"  Result: {'SUCCESS' if not env.terminations.get(agent) else 'FAILED'}")

    # Cycle through all players to get back to player 0
    for _ in range(3):
        agent = env.agent_selection
        action = np.array([0, 0, 0, 0, 0, 0])
        env.step(action)

    # Test RESEARCH action (if we have currency)
    agent = env.agent_selection
    obs = env.observe(agent)
    print(f"\nTest 2: RESEARCH action")
    print(f"  Currency: {obs.get('currency')} stars")

    # Try to research riding (tech_idx=1)
    action = np.array([5, 0, 0, 0, 1, 0])  # ACTION_RESEARCH, riding
    env.step(action)

    if env.terminations.get(agent):
        print(f"  Result: Action rejected (likely insufficient currency)")
    else:
        print(f"  Result: Action accepted")

    env.close()

    print("\n" + "="*70)
    print("ACTION TEST COMPLETE")
    print("="*70)


if __name__ == "__main__":
    # Test 1: Basic training loop
    rewards = test_basic_training()

    # Test 2: Specific actions
    test_specific_actions()

    print("\n" + "="*70)
    print("ALL TESTS COMPLETE")
    print("="*70)
    print("\nNext steps:")
    print("1. Implement action masking for valid actions")
    print("2. Add reward shaping for interesting behaviors")
    print("3. Train with proper RL algorithm (Stable-Baselines3, RLlib, etc.)")
