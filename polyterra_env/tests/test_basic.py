"""Basic test to verify environment functionality"""
import sys
from pathlib import Path
sys.path.insert(0, str(Path(__file__).parent.parent))

from polyterra_env import PolyterraEnv
import numpy as np


def test_basic_functionality():
    """Test basic environment creation and stepping"""
    print("="*70)
    print("BASIC FUNCTIONALITY TEST")
    print("="*70)

    # Create environment
    print("\n1. Creating environment...")
    env = PolyterraEnv(
        num_players=4,
        game_mode="perfection",
        max_turns=10,
        render_mode=None
    )
    print("   SUCCESS: Environment created")

    # Reset
    print("\n2. Resetting environment...")
    env.reset(seed=42)
    print(f"   SUCCESS: Environment reset")
    print(f"   Agents: {env.agents}")
    print(f"   Current agent: {env.agent_selection}")

    # Get observation
    print("\n3. Getting observation...")
    agent = env.agent_selection
    obs = env.observe(agent)
    print(f"   SUCCESS: Observation retrieved for {agent}")
    print(f"   Turn: {obs.get('turn')}")
    print(f"   Currency: {obs.get('currency')}")
    print(f"   Score: {obs.get('score')}")
    print(f"   Cities: {len(obs.get('cities', []))}")
    print(f"   Visible tiles: {len([t for t in obs.get('tiles', []) if t.get('visible')])}")

    # Take action
    print("\n4. Taking END_TURN action...")
    action = np.array([0, 0, 0, 0, 0, 0])  # END_TURN
    env.step(action)
    print(f"   SUCCESS: Action executed")

    # Run a few turns
    print("\n5. Running 10 more steps...")
    for i in range(10):
        agent = env.agent_selection
        action = np.array([0, 0, 0, 0, 0, 0])
        env.step(action)

        if env.terminations.get(agent) or env.truncations.get(agent):
            print(f"   Game ended at step {i+1}")
            break
    else:
        print(f"   SUCCESS: Completed 10 steps")

    # Close
    print("\n6. Closing environment...")
    env.close()
    print("   SUCCESS: Environment closed")

    print("\n" + "="*70)
    print("ALL TESTS PASSED")
    print("="*70)


if __name__ == "__main__":
    test_basic_functionality()
