"""Test player_3 specifically"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

# Check all players after reset
for player in ['player_0', 'player_1', 'player_2', 'player_3']:
    obs = env._observations.get(player, {})
    tiles = obs.get('tiles', [])
    visible = sum(1 for t in tiles if t.get('visible'))
    cities = sum(1 for t in tiles if t.get('improvement', {}).get('type') == 'City')
    print(f"{player}: turn={obs.get('turn')}, tribe={obs.get('tribe')}, "
          f"visible={visible}, cities={cities}")

# Take 3 steps to get to player_3
for i in range(3):
    agent = env.agent_selection
    env.step(0)
    print(f"Step {i+1}: {agent} ended turn")

print(f"\nCurrent agent after 3 steps: {env.agent_selection}")

# Now render
print("\n=== RENDERING ===")
env.render()

env.close()
