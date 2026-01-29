"""Debug unit state at different points in turn"""
from polyterra_env import PolyterraEnv
import numpy as np

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=30)
env.reset(seed=42)

print("="*70)
print("UNIT STATE AND VALID ACTIONS DEBUGGING")
print("="*70)

# Check player_0 at start
agent = env.agent_selection
raw_obs = env._raw_observations[agent]
valid_actions = raw_obs.get('valid_actions', {})

print(f"\n[START OF TURN 1 - {agent}]")
print(f"Valid moves: {len(valid_actions.get('valid_moves', []))}")

# Find player unit
tiles = raw_obs.get('tiles', [])
for tile in tiles:
    unit = tile.get('unit')
    if unit and unit.get('owner') == raw_obs.get('player_id'):
        print(f"Unit: type={unit.get('type')}, "
              f"moved={unit.get('moved')}, "
              f"attacked={unit.get('attacked')}, "
              f"at ({tile.get('x')}, {tile.get('y')})")

# End turn for all players to advance to turn 2
for i in range(4):
    agent = env.agent_selection
    action = np.array([0, 0, 0, 0, 0, 0])  # END_TURN
    env.step(action)

# Check player_0 at turn 2 start
agent = 'player_0'
raw_obs = env._raw_observations.get(agent, {})
valid_actions = raw_obs.get('valid_actions', {})

print(f"\n[START OF TURN 2 - {agent}]")
print(f"Valid moves: {len(valid_actions.get('valid_moves', []))}")

# Find player unit
tiles = raw_obs.get('tiles', [])
for tile in tiles:
    unit = tile.get('unit')
    if unit and unit.get('owner') == raw_obs.get('player_id'):
        print(f"Unit: type={unit.get('type')}, "
              f"moved={unit.get('moved')}, "
              f"attacked={unit.get('attacked')}, "
              f"at ({tile.get('x')}, {tile.get('y')})")

if valid_actions.get('valid_moves'):
    print(f"\nSample valid moves:")
    for move in valid_actions.get('valid_moves', [])[:3]:
        print(f"  From ({move.get('from_x')}, {move.get('from_y')}) "
              f"to ({move.get('to_x')}, {move.get('to_y')})")

env.close()
