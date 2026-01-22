"""Show game progression for player_0 across multiple turns"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=30, render_mode="human")
env.reset(seed=42)

print("="*70)
print("GAME PROGRESSION TEST - Following player_0 across 5 rounds")
print("="*70)

# Track player_0's turns
player_0_turn_count = 0
max_player_0_turns = 5

while player_0_turn_count < max_player_0_turns:
    agent = env.agent_selection
    obs, reward, term, trunc, info = env.last()

    if agent == "player_0":
        player_0_turn_count += 1

        print(f"\n{'='*70}")
        print(f"PLAYER_0's TURN #{player_0_turn_count}")
        print(f"Game Turn: {obs.get('turn')}")
        print(f"Currency: {obs.get('currency')} | Score: {obs.get('score')} | Cities: {obs.get('cities')}")
        print(f"Available Techs: {obs.get('available_techs', [])}")
        print(f"Kills: {obs.get('kills')}")
        print('='*70)

        # Show map
        tiles = obs.get('tiles', [])
        width = obs.get('map_width', 16)
        height = obs.get('map_height', 16)

        # Create grid
        grid = {}
        for tile in tiles:
            x, y = tile.get('x', 0), tile.get('y', 0)
            grid[(x, y)] = tile

        print("\nMap:")
        for y in range(height):
            row = []
            for x in range(width):
                tile = grid.get((x, y), {})
                if not tile.get('visible', False):
                    row.append(' ')
                else:
                    # Check for city first
                    if tile.get('improvement'):
                        imp = tile['improvement']
                        if imp.get('type') == 'City':
                            if imp.get('is_capital', False):
                                row.append('*')
                            else:
                                row.append('C')
                        else:
                            row.append('I')  # Other improvement
                    # Check for unit
                    elif tile.get('unit'):
                        row.append('U')
                    # Terrain
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

        # Count visible tiles and cities
        visible_count = sum(1 for t in tiles if t.get('visible'))
        owned_tiles = sum(1 for t in tiles if t.get('visible') and t.get('owner') == obs.get('player_id'))
        cities_count = sum(1 for t in tiles if t.get('improvement', {}).get('type') == 'City' and t.get('owner') == obs.get('player_id'))
        units_count = sum(1 for t in tiles if t.get('unit', {}).get('owner') == obs.get('player_id'))

        print(f"\nStats: Visible={visible_count} | Owned={owned_tiles} | Cities={cities_count} | Units={units_count}")
        print(f"Reward this turn: {reward}")

        input("\nPress Enter to continue to next player_0 turn...")

    # Take action (just end turn)
    env.step(0)

    if term or trunc:
        print("\nGame ended!")
        break

env.close()
print("\n" + "="*70)
print("Test complete!")
