"""Test that action mask changes as game state progresses"""
import numpy as np
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=30)
env.reset(seed=42)

print("="*70)
print("ACTION MASK PROGRESSION TEST")
print("="*70)

# Track first player's masks over time
player_0_observations = []

for i in range(40):  # Run ~10 turns (4 players per turn)
    agent = env.agent_selection
    obs = env.observe(agent)

    # Only track player_0
    if agent == "player_0":
        action_mask = obs.get('action_mask')
        valid_actions = np.where(action_mask[0] == 1)[0]

        player_0_observations.append({
            'step': i // 4 + 1,
            'currency': obs.get('currency'),
            'score': obs.get('score'),
            'units': len(obs.get('units', [])),
            'cities': len(obs.get('cities', [])),
            'valid_action_types': valid_actions.tolist(),
            'num_valid': len(valid_actions)
        })

        print(f"\nTurn {i // 4 + 1} - {agent}:")
        print(f"  Currency: {obs.get('currency')} stars")
        print(f"  Score: {obs.get('score')}")
        print(f"  Units: {len(obs.get('units', []))}")
        print(f"  Cities: {len(obs.get('cities', []))}")
        print(f"  Valid action types: {valid_actions.tolist()}")
        print(f"    (0=END_TURN, 1=MOVE, 2=ATTACK, 3=BUILD, 4=TRAIN, 5=RESEARCH, ...)")

    # Take END_TURN action
    action = np.array([0, 0, 0, 0, 0, 0])
    env.step(action)

env.close()

print("\n" + "="*70)
print("SUMMARY: ACTION MASK CHANGES")
print("="*70)

for i, obs_data in enumerate(player_0_observations):
    print(f"Turn {obs_data['step']}: "
          f"Currency={obs_data['currency']}, "
          f"Valid actions={obs_data['num_valid']} types {obs_data['valid_action_types']}")

# Check if mask changed
unique_masks = set(tuple(obs['valid_action_types']) for obs in player_0_observations)
print(f"\n{len(unique_masks)} unique action masks observed")

if len(unique_masks) == 1:
    print("WARNING: Action mask never changed!")
elif len(unique_masks) > 1:
    print("SUCCESS: Action mask changes based on game state!")
    print(f"Different masks: {[list(m) for m in unique_masks]}")
