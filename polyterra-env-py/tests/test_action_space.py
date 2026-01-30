"""
Comprehensive test of the new padded action space implementation.
"""
import sys
import numpy as np
from polyterra_env import PolyterraEnv

def test_action_space():
    print("=" * 60)
    print("Testing PolyterraEnv Action Space")
    print("=" * 60)

    # Create environment
    print("\n1. Creating environment...")
    env = PolyterraEnv(num_players=2)
    print(f"   MAX_ACTIONS = {env.MAX_ACTIONS}")

    # Check action space
    print("\n2. Checking action_space...")
    action_space = env.action_space("player_0")
    print(f"   action_space = {action_space}")
    print(f"   action_space.n = {action_space.n}")
    assert action_space.n == env.MAX_ACTIONS, f"Expected {env.MAX_ACTIONS}, got {action_space.n}"
    print("   ✓ Action space is Discrete(512)")

    # Reset environment
    print("\n3. Resetting environment...")
    env.reset(seed=42)
    print(f"   Current agent: {env.agent_selection}")
    print(f"   Agents: {env.agents}")

    # Get observation
    print("\n4. Checking observation structure...")
    obs = env.observe(env.agent_selection)

    required_fields = [
        "turn", "currency", "score", "tiles", "units", "cities",
        "valid_actions", "valid_actions_list", "valid_actions_mask"
    ]
    for field in required_fields:
        assert field in obs, f"Missing field: {field}"
        print(f"   ✓ '{field}' present")

    # Check valid_actions_list
    print("\n5. Checking valid_actions_list...")
    valid_list = obs["valid_actions_list"]
    valid_mask = obs["valid_actions_mask"]

    print(f"   valid_actions_list length: {len(valid_list)}")
    print(f"   valid_actions_mask shape: {valid_mask.shape}")

    assert len(valid_list) == env.MAX_ACTIONS, f"List should be {env.MAX_ACTIONS}, got {len(valid_list)}"
    assert valid_mask.shape == (env.MAX_ACTIONS,), f"Mask shape wrong: {valid_mask.shape}"
    print("   ✓ Correct sizes")

    # Count valid actions
    num_valid = int(np.sum(valid_mask))
    print(f"\n6. Valid actions count: {num_valid}")

    # Show first few valid actions
    print("\n7. Sample valid actions:")
    for i in range(min(10, num_valid)):
        action = valid_list[i]
        print(f"   [{i}] type={action.get('type')}, {action}")

    # Verify mask matches list
    print("\n8. Verifying mask matches list...")
    for i in range(env.MAX_ACTIONS):
        is_valid = valid_mask[i] == 1
        is_padding = valid_list[i].get("type") == "invalid"
        if is_valid and is_padding:
            print(f"   ERROR: Index {i} is marked valid but is padding!")
            return False
        if not is_valid and not is_padding:
            print(f"   ERROR: Index {i} is marked invalid but has action: {valid_list[i]}")
            return False
    print("   ✓ Mask correctly matches list")

    # Test sampling with mask
    print("\n9. Testing action sampling with mask...")
    valid_indices = np.where(valid_mask == 1)[0]
    print(f"   Valid indices: {valid_indices[:10]}...")

    if len(valid_indices) == 0:
        print("   ERROR: No valid actions!")
        return False

    # Pick a random valid action
    action_idx = int(np.random.choice(valid_indices))
    action = valid_list[action_idx]
    print(f"   Selected action index: {action_idx}")
    print(f"   Action: {action}")

    # Execute the action
    print("\n10. Executing action...")
    prev_agent = env.agent_selection
    env.step(action_idx)

    reward = env.rewards.get(prev_agent, 0)
    info = env.infos.get(prev_agent, {})

    print(f"   Reward: {reward}")
    print(f"   Info: {info}")

    if info.get("invalid_action"):
        print(f"   WARNING: Action was marked invalid: {info.get('error')}")
    else:
        print("   ✓ Action executed successfully")

    # Test a few more steps
    print("\n11. Testing multiple steps...")
    for step in range(5):
        agent = env.agent_selection
        if agent not in env.agents:
            print(f"   Step {step}: No more agents")
            break

        obs = env.observe(agent)
        valid_mask = obs["valid_actions_mask"]
        valid_list = obs["valid_actions_list"]
        valid_indices = np.where(valid_mask == 1)[0]

        if len(valid_indices) == 0:
            print(f"   Step {step}: No valid actions for {agent}")
            break

        # Prefer end_turn to keep things simple
        end_turn_idx = None
        for idx in valid_indices:
            if valid_list[idx].get("type") == "end_turn":
                end_turn_idx = idx
                break

        action_idx = end_turn_idx if end_turn_idx is not None else int(valid_indices[0])
        action = valid_list[action_idx]

        print(f"   Step {step}: {agent} -> {action.get('type')}")
        env.step(action_idx)

        if env.infos.get(agent, {}).get("invalid_action"):
            print(f"   ERROR: Invalid action!")
            return False

    print("\n" + "=" * 60)
    print("ALL TESTS PASSED!")
    print("=" * 60)

    env.close()
    return True

if __name__ == "__main__":
    success = test_action_space()
    sys.exit(0 if success else 1)
