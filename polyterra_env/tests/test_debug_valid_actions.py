"""Debug what valid_actions backend is returning"""
from polyterra_env import PolyterraEnv
import json

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=30)
env.reset(seed=42)

# Get first observation
agent = env.agent_selection
raw_obs = env._raw_observations[agent]

print("="*70)
print("RAW VALID_ACTIONS FROM BACKEND")
print("="*70)

valid_actions = raw_obs.get('valid_actions', {})

print(f"\ncan_end_turn: {valid_actions.get('can_end_turn')}")
print(f"\nvalid_research: {valid_actions.get('valid_research', [])}")
print(f"\nvalid_moves (count): {len(valid_actions.get('valid_moves', []))}")
if valid_actions.get('valid_moves'):
    print("Sample moves:")
    for move in valid_actions.get('valid_moves', [])[:5]:
        print(f"  {move}")

print(f"\nvalid_attacks (count): {len(valid_actions.get('valid_attacks', []))}")
print(f"\nvalid_builds (count): {len(valid_actions.get('valid_builds', []))}")
print(f"\nvalid_trains (count): {len(valid_actions.get('valid_trains', []))}")

# Check unit info
units = raw_obs.get('tiles', [])
player_units = [t for t in units if t.get('unit') and t.get('unit', {}).get('owner') == raw_obs.get('player_id')]

print(f"\n\nPlayer units:")
for tile in player_units[:3]:
    unit = tile.get('unit', {})
    print(f"  Unit at ({tile.get('x')}, {tile.get('y')}): "
          f"type={unit.get('type')}, "
          f"moved={unit.get('moved')}, "
          f"attacked={unit.get('attacked')}")

env.close()
