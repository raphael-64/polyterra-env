"""Test that each player has a different observation space (fog of war)"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

print("="*70)
print("TESTING PLAYER OBSERVATIONS - Each player should see different tiles")
print("="*70)

# Collect observations for all players at the start
observations = {}
for player in ['player_0', 'player_1', 'player_2', 'player_3']:
    observations[player] = env._observations.get(player, {})

# Display each player's view
for player_name, obs in observations.items():
    tiles = obs.get('tiles', [])
    visible_tiles = [t for t in tiles if t.get('visible')]

    print(f"\n{'='*70}")
    print(f"{player_name.upper()} ({obs.get('tribe')}) - Turn {obs.get('turn')}")
    print(f"Currency: {obs.get('currency')} | Score: {obs.get('score')} | Cities: {obs.get('cities')}")
    print(f"Visible tiles: {len(visible_tiles)}/{len(tiles)}")
    print('='*70)

    # Find capital location
    capital = next((t for t in visible_tiles if t.get('improvement', {}).get('is_capital')), None)
    if capital:
        print(f"Capital at: ({capital.get('x')}, {capital.get('y')})")

    # Show map from this player's perspective
    width = obs.get('map_width', 16)
    height = obs.get('map_height', 16)

    grid = {}
    for tile in tiles:
        x, y = tile.get('x', 0), tile.get('y', 0)
        grid[(x, y)] = tile

    print("\nMap (player's view):")
    for y in range(height):
        row = []
        for x in range(width):
            tile = grid.get((x, y), {})
            if not tile.get('visible', False):
                row.append(' ')  # Fog of war
            else:
                # Show capital
                if tile.get('improvement', {}).get('is_capital'):
                    row.append('*')
                # Show city
                elif tile.get('improvement', {}).get('type') == 'City':
                    row.append('C')
                # Show unit
                elif tile.get('unit'):
                    row.append('U')
                # Show terrain
                else:
                    terrain = tile.get('terrain', '')
                    if terrain in ['Ocean', 'Water']:
                        row.append('.')
                    elif terrain == 'Mountain':
                        row.append('^')
                    elif terrain == 'Forest':
                        row.append('T')
                    else:
                        row.append(',')
        print(''.join(row))

    print(f"\nLegend: * = capital, C = city, U = unit, . = water, ^ = mountain")
    print(f"        T = forest, , = plains, (space) = unexplored (fog of war)")

print("\n" + "="*70)
print("VERIFICATION: Each player should have different visible regions")
print("="*70)

# Show statistics comparison
for player_name, obs in observations.items():
    tiles = obs.get('tiles', [])
    visible_count = sum(1 for t in tiles if t.get('visible'))
    print(f"{player_name}: {visible_count} visible tiles")

env.close()
