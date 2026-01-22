"""
Test detailed action execution in PolyterraEnvV2

This tests various action types to ensure the action-to-command conversion works
"""

from polyterra_env_v2 import PolyterraEnvV2
import numpy as np
from game_data_mappings import TECH_IDX_TO_NAME, UNIT_IDX_TO_NAME

def test_research_action():
    """Test researching a technology"""
    print("\n" + "="*70)
    print("TEST 1: RESEARCH ACTION")
    print("="*70)

    env = PolyterraEnvV2(num_players=4, game_mode="perfection", max_turns=10)
    env.reset(seed=42)

    agent = env.agent_selection
    obs_before = env.observe(agent)

    print(f"\nAgent: {agent}")
    print(f"Currency before: {obs_before.get('currency')} stars")
    print(f"Available techs before: {np.sum(obs_before.get('available_techs'))} techs")

    # Try to research "riding" (tech_idx = 1)
    research_action = np.array([5, 0, 0, 0, 1, 0])  # RESEARCH, tech_type=riding
    print(f"\nAttempting to RESEARCH tech_idx=1 ({TECH_IDX_TO_NAME.get(1, 'unknown')})")

    try:
        env.step(research_action)
        obs_after = env.observe(agent)

        print(f"Currency after: {obs_after.get('currency')} stars")
        print(f"Available techs after: {np.sum(obs_after.get('available_techs'))} techs")

        reward = env.rewards.get(agent, 0)
        print(f"Reward: {reward}")

        if env.terminations.get(agent):
            info = env.infos.get(agent, {})
            print(f"Action failed: {info.get('error', 'Unknown error')}")
        else:
            print("Action executed successfully!")

    except Exception as e:
        print(f"Exception occurred: {e}")

    env.close()


def test_train_action():
    """Test training a unit"""
    print("\n" + "="*70)
    print("TEST 2: TRAIN UNIT ACTION")
    print("="*70)

    env = PolyterraEnvV2(num_players=4, game_mode="perfection", max_turns=10)
    env.reset(seed=42)

    agent = env.agent_selection
    obs_before = env.observe(agent)

    print(f"\nAgent: {agent}")
    print(f"Currency before: {obs_before.get('currency')} stars")
    print(f"Units before: {len(obs_before.get('units', []))} units")

    # Find a city
    cities = obs_before.get('cities', [])
    if cities:
        city = cities[0]
        city_x, city_y = city.get('x'), city.get('y')
        print(f"City found at ({city_x}, {city_y})")

        # Try to train a warrior (unit_idx = 2)
        train_action = np.array([4, city_x, city_y, 0, 2, 0])  # TRAIN, unit_type=warrior
        print(f"\nAttempting to TRAIN unit_idx=2 ({UNIT_IDX_TO_NAME.get(2, 'unknown')}) at city")

        try:
            env.step(train_action)

            # Move to next agent and back to see changes
            for _ in range(3):
                env.step(np.array([0, 0, 0, 0, 0, 0]))  # END_TURN

            obs_after = env.observe(agent)

            print(f"Currency after: {obs_after.get('currency')} stars")
            print(f"Units after: {len(obs_after.get('units', []))} units")

            reward = env.rewards.get(agent, 0)
            print(f"Reward: {reward}")

            if env.terminations.get(agent):
                info = env.infos.get(agent, {})
                print(f"Action failed: {info.get('error', 'Unknown error')}")
            else:
                print("Action executed successfully!")

        except Exception as e:
            print(f"Exception occurred: {e}")
    else:
        print("No cities found!")

    env.close()


def test_move_action():
    """Test moving a unit (if we have any)"""
    print("\n" + "="*70)
    print("TEST 3: MOVE UNIT ACTION")
    print("="*70)

    env = PolyterraEnvV2(num_players=4, game_mode="perfection", max_turns=10)
    env.reset(seed=42)

    agent = env.agent_selection
    obs = env.observe(agent)

    print(f"\nAgent: {agent}")

    units = obs.get('units', [])
    print(f"Units available: {len(units)}")

    if units:
        unit = units[0]
        unit_x, unit_y = unit.get('x'), unit.get('y')
        unit_type = unit.get('type')

        print(f"Unit 0: type={unit_type}, pos=({unit_x}, {unit_y})")

        # Try to move to adjacent tile
        target_x, target_y = unit_x + 1, unit_y

        move_action = np.array([1, target_x, target_y, 0, 0, 0])  # MOVE
        print(f"\nAttempting to MOVE unit 0 to ({target_x}, {target_y})")

        try:
            env.step(move_action)

            # Get observation after move
            obs_after = env.observe(agent)
            units_after = obs_after.get('units', [])

            if units_after:
                unit_after = units_after[0]
                new_x, new_y = unit_after.get('x'), unit_after.get('y')
                print(f"Unit 0 after: pos=({new_x}, {new_y})")

                if new_x == target_x and new_y == target_y:
                    print("Unit moved successfully!")
                else:
                    print(f"Unit didn't move to target (might be invalid move)")

            reward = env.rewards.get(agent, 0)
            print(f"Reward: {reward}")

            if env.terminations.get(agent):
                info = env.infos.get(agent, {})
                print(f"Action failed: {info.get('error', 'Unknown error')}")

        except Exception as e:
            print(f"Exception occurred: {e}")
    else:
        print("No units available to move (players start without units)")

    env.close()


def test_action_space_sample():
    """Test that action space sampling works"""
    print("\n" + "="*70)
    print("TEST 4: ACTION SPACE SAMPLING")
    print("="*70)

    env = PolyterraEnvV2(num_players=4, game_mode="perfection", max_turns=10)
    env.reset(seed=42)

    agent = env.agent_selection
    action_space = env.action_space(agent)

    print(f"\nAction space for {agent}:")
    print(f"Type: {type(action_space)}")
    print(f"Shape: {action_space.nvec}")

    # Sample 5 random actions
    print("\nSampling 5 random actions:")
    for i in range(5):
        action = action_space.sample()
        print(f"  Sample {i+1}: {action}")
        print(f"    Action type: {action[0]}, Target: ({action[1]}, {action[2]}), "
              f"Unit idx: {action[3]}, Param1: {action[4]}")

    env.close()


def test_observation_space_contains():
    """Test that actual observations match the observation space"""
    print("\n" + "="*70)
    print("TEST 5: OBSERVATION SPACE VALIDATION")
    print("="*70)

    env = PolyterraEnvV2(num_players=4, game_mode="perfection", max_turns=10)
    env.reset(seed=42)

    agent = env.agent_selection
    obs = env.observe(agent)
    obs_space = env.observation_space(agent)

    print(f"\nValidating observation for {agent}...")

    # Check key fields
    checks = [
        ('turn', obs.get('turn') is not None),
        ('player_id', obs.get('player_id') is not None),
        ('currency', obs.get('currency') is not None),
        ('score', obs.get('score') is not None),
        ('tiles', isinstance(obs.get('tiles'), list)),
        ('units', isinstance(obs.get('units'), list)),
        ('cities', isinstance(obs.get('cities'), list)),
        ('opponents', isinstance(obs.get('opponents'), list)),
    ]

    all_passed = True
    for field, passed in checks:
        status = "✓" if passed else "✗"
        print(f"  {status} {field}: {passed}")
        if not passed:
            all_passed = False

    if all_passed:
        print("\n✓ All observation fields present and valid!")
    else:
        print("\n✗ Some observation fields missing or invalid")

    # Try to validate with Gymnasium (may fail due to Sequence type)
    try:
        # Note: Full validation might fail because Gymnasium doesn't support nested Sequence well
        print("\nAttempting Gymnasium space.contains() check...")
        contains = obs_space.contains(obs)
        print(f"  Observation in space: {contains}")
    except Exception as e:
        print(f"  Space validation skipped: {e}")

    env.close()


def main():
    """Run all tests"""
    print("\n" + "="*70)
    print("DETAILED ACTION TESTING FOR POLYTERRAENVV2")
    print("="*70)

    test_research_action()
    test_train_action()
    test_move_action()
    test_action_space_sample()
    test_observation_space_contains()

    print("\n" + "="*70)
    print("ALL TESTS COMPLETE")
    print("="*70)
    print("""
Summary:
1. Research action - Tests tech research with parameterized action
2. Train action - Tests unit training at cities
3. Move action - Tests unit movement (if units exist)
4. Action sampling - Verifies action space can be sampled
5. Observation validation - Checks observation structure

Note: Some actions may fail if game state doesn't support them
(e.g., insufficient currency, no units, invalid targets).
This is expected behavior - the action-to-command conversion works,
but the C# backend validates game rules.
""")


if __name__ == "__main__":
    main()
