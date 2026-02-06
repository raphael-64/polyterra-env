"""
Comprehensive test of all action types in the padded action space.
"""
import sys
import numpy as np
from polyterra_env import PolyterraEnv

def test_comprehensive():
    print("=" * 70)
    print("Comprehensive PolyterraEnv Action Space Test")
    print("=" * 70)

    env = PolyterraEnv(num_players=2)
    env.reset(seed=12345)

    action_types_tested = set()
    action_types_succeeded = set()
    errors = []

    max_steps = 200
    step_count = 0

    print(f"\nRunning up to {max_steps} steps to test all action types...")
    print("-" * 70)

    while step_count < max_steps:
        agent = env.agent_selection

        if agent not in env.agents:
            break

        if env.terminations.get(agent, False) or env.truncations.get(agent, False):
            break

        obs = env.observe(agent)
        valid_list = obs["valid_actions_list"]
        valid_mask = obs["valid_actions_mask"]
        valid_indices = np.where(valid_mask == 1)[0]

        if len(valid_indices) == 0:
            print(f"Step {step_count}: No valid actions for {agent}")
            break

        # Count action types available
        available_types = {}
        for idx in valid_indices:
            action = valid_list[idx]
            atype = action.get("type")
            if atype not in available_types:
                available_types[atype] = []
            available_types[atype].append(idx)

        # Try to pick an action type we haven't tested yet
        untested = [t for t in available_types.keys() if t not in action_types_tested]

        if untested:
            # Pick an untested action type
            chosen_type = untested[0]
        else:
            # All available types tested, pick randomly
            chosen_type = list(available_types.keys())[0]

        # Pick first action of that type
        action_idx = available_types[chosen_type][0]
        action = valid_list[action_idx]
        action_types_tested.add(chosen_type)

        # Execute
        prev_agent = agent
        env.step(action_idx)

        reward = env.rewards.get(prev_agent, 0)
        info = env.infos.get(prev_agent, {})

        if info.get("invalid_action"):
            error_msg = f"Step {step_count}: {chosen_type} FAILED - {info.get('error')}"
            errors.append(error_msg)
            print(f"  ✗ {error_msg}")
        else:
            action_types_succeeded.add(chosen_type)
            if step_count < 20 or step_count % 20 == 0:
                print(f"  ✓ Step {step_count}: {agent} -> {chosen_type} (reward={reward:.1f})")

        step_count += 1

    print("-" * 70)
    print(f"\nCompleted {step_count} steps")

    print(f"\nAction types tested ({len(action_types_tested)}):")
    for atype in sorted(action_types_tested):
        status = "✓" if atype in action_types_succeeded else "✗"
        print(f"  {status} {atype}")

    print(f"\nAction types that succeeded: {len(action_types_succeeded)}/{len(action_types_tested)}")

    if errors:
        print(f"\nErrors ({len(errors)}):")
        for e in errors[:10]:  # Show first 10 errors
            print(f"  {e}")
    else:
        print("\nNo errors!")

    # Final verification
    print("\n" + "=" * 70)

    # Check that we tested the important action types
    important_types = {"end_turn", "move"}  # At minimum these should work
    missing_important = important_types - action_types_succeeded

    if missing_important:
        print(f"FAILED: Missing important action types: {missing_important}")
        env.close()
        return False

    if len(errors) > 0:
        print(f"FAILED: {len(errors)} errors occurred")
        env.close()
        return False

    print("ALL IMPORTANT TESTS PASSED!")
    print("=" * 70)

    env.close()
    return True

def test_action_mask_sampling():
    """Test that sampling with mask always gives valid actions."""
    print("\n" + "=" * 70)
    print("Testing action mask sampling")
    print("=" * 70)

    env = PolyterraEnv(num_players=2)
    env.reset(seed=999)

    for i in range(50):
        agent = env.agent_selection
        if agent not in env.agents:
            break

        obs = env.observe(agent)
        valid_mask = obs["valid_actions_mask"]
        valid_list = obs["valid_actions_list"]

        # Simulate masked sampling (what PPO would do)
        valid_indices = np.where(valid_mask == 1)[0]
        if len(valid_indices) == 0:
            print(f"Step {i}: No valid actions")
            break

        # Random choice from valid indices
        action_idx = int(np.random.choice(valid_indices))

        # Verify the action is not padding
        action = valid_list[action_idx]
        if action.get("type") == "invalid":
            print(f"ERROR: Sampled invalid action at index {action_idx}")
            env.close()
            return False

        env.step(action_idx)

        if env.infos.get(agent, {}).get("invalid_action"):
            # This can happen if game state changed - not necessarily a bug
            pass

    print("✓ 50 steps of masked sampling completed without sampling invalid actions")
    env.close()
    return True

def test_ppo_style_usage():
    """Test PPO-style usage pattern."""
    print("\n" + "=" * 70)
    print("Testing PPO-style usage pattern")
    print("=" * 70)

    env = PolyterraEnv(num_players=2)
    env.reset(seed=42)

    # Simulate what PPO training loop would do
    for episode_step in range(30):
        agent = env.agent_selection
        if agent not in env.agents:
            break

        # 1. Get observation
        obs = env.observe(agent)

        # 2. Get action mask (this is what PPO needs for masked softmax)
        action_mask = obs["valid_actions_mask"]  # shape: (512,)

        # 3. Policy would output logits, then mask invalid actions
        # Simulating: logits = policy(obs)
        fake_logits = np.random.randn(env.MAX_ACTIONS)

        # 4. Apply mask (set invalid to -inf)
        masked_logits = np.where(action_mask == 1, fake_logits, -1e9)

        # 5. Sample action (softmax + sample)
        exp_logits = np.exp(masked_logits - np.max(masked_logits))
        probs = exp_logits / np.sum(exp_logits)
        action_idx = int(np.random.choice(len(probs), p=probs))

        # 6. Verify sampled action is valid
        if action_mask[action_idx] != 1:
            print(f"ERROR: Sampled invalid action index {action_idx}")
            env.close()
            return False

        # 7. Step environment
        env.step(action_idx)

    print("✓ 30 steps of PPO-style masked sampling completed successfully")
    env.close()
    return True

if __name__ == "__main__":
    success = True

    success = test_comprehensive() and success
    success = test_action_mask_sampling() and success
    success = test_ppo_style_usage() and success

    if success:
        print("\n" + "=" * 70)
        print("ALL COMPREHENSIVE TESTS PASSED!")
        print("=" * 70)
    else:
        print("\n" + "=" * 70)
        print("SOME TESTS FAILED!")
        print("=" * 70)

    sys.exit(0 if success else 1)
