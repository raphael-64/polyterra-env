"""Verify the environment is actually working"""
from polyterra_env import PolyterraEnv
import json

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

print("=== AFTER RESET ===")
print(f"Current agent: {env.agent_selection}")
print(f"All agents: {env.agents}")

# Get observation for player_0
obs = env._observations.get('player_0', {})
print(f"\nPlayer 0 observation:")
print(f"  Turn: {obs.get('turn')}")
print(f"  Tribe: {obs.get('tribe')}")
print(f"  Currency: {obs.get('currency')}")
print(f"  Cities: {obs.get('cities')}")
print(f"  Total tiles: {len(obs.get('tiles', []))}")

# Count visible tiles
tiles = obs.get('tiles', [])
visible_count = sum(1 for t in tiles if t.get('visible'))
has_city_count = sum(1 for t in tiles if t.get('improvement', {}).get('type') == 'City')
print(f"  Visible tiles: {visible_count}")
print(f"  Tiles with cities: {has_city_count}")

# Take 4 steps so all players act once
print("\n=== TAKING 4 STEPS ===")
for i in range(4):
    agent = env.agent_selection
    print(f"Step {i+1}: {agent} acts")
    env.step(0)

print(f"\nAfter 4 steps:")
print(f"  Current agent: {env.agent_selection}")
print(f"  Current turn: {env._observations[env.agent_selection].get('turn')}")

# Get observation for current agent
obs = env._observations.get(env.agent_selection, {})
print(f"\n{env.agent_selection} observation:")
print(f"  Tribe: {obs.get('tribe')}")
print(f"  Currency: {obs.get('currency')}")
tiles = obs.get('tiles', [])
visible_count = sum(1 for t in tiles if t.get('visible'))
print(f"  Visible tiles: {visible_count}")

# Show first few visible tiles
print("\nFirst 5 visible tiles:")
visible_tiles = [t for t in tiles if t.get('visible')][:5]
for t in visible_tiles:
    print(f"  ({t.get('x')}, {t.get('y')}): terrain={t.get('terrain')}, "
          f"owner={t.get('owner')}, has_city={t.get('improvement', {}).get('type') == 'City'}")

env.close()
