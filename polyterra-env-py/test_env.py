"""
Test script for Polyterra PettingZoo environment
"""

import polyterra_env

def test_basic():
    """Test basic environment functionality"""
    print("Creating environment...")
    env = polyterra_env.env(
        num_players=4,
        game_mode="perfection",
        max_turns=10,
        render_mode="human"
    )

    print("Resetting environment with seed=42...")
    env.reset(seed=42)

    print(f"\nPossible agents: {env.possible_agents}")
    print(f"Active agents: {env.agents}")
    print(f"Current agent: {env.agent_selection}")

    print("\nRunning 5 steps...")
    for i in range(5):
        for agent in env.agent_iter(max_iter=20):
            obs, reward, termination, truncation, info = env.last()

            if termination or truncation:
                action = None
            else:
                # Random action (just end_turn for now)
                action = 0

            print(f"\nStep {i+1}, Agent: {agent}")
            print(f"  Observation keys: {list(obs.keys()) if obs else 'None'}")
            print(f"  Reward: {reward}")
            print(f"  Termination: {termination}, Truncation: {truncation}")

            env.step(action)

            if all(env.terminations.values()) or all(env.truncations.values()):
                break

        env.render()

        if all(env.terminations.values()) or all(env.truncations.values()):
            print("\nGame ended!")
            break

    print("\nClosing environment...")
    env.close()
    print("Done!")


if __name__ == "__main__":
    test_basic()
