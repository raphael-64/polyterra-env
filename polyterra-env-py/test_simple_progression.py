"""Show game progression for player_0 across multiple turns"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=30, render_mode="human")
env.reset(seed=42)

print("GAME PROGRESSION - Following player_0 across 5 rounds\n")

# Track player_0's turns
player_0_turn_count = 0
max_player_0_turns = 5

while player_0_turn_count < max_player_0_turns:
    agent = env.agent_selection
    obs, reward, term, trunc, info = env.last()

    if agent == "player_0":
        player_0_turn_count += 1

        print(f"\n{'='*70}")
        print(f"PLAYER_0 TURN #{player_0_turn_count} | Game Turn: {obs.get('turn')} | Reward: {reward:.0f}")
        print(f"Currency: {obs.get('currency')} | Score: {obs.get('score')} | Cities: {obs.get('cities')} | Kills: {obs.get('kills')}")

        # Count stats
        tiles = obs.get('tiles', [])
        visible_count = sum(1 for t in tiles if t.get('visible'))
        owned_tiles = sum(1 for t in tiles if t.get('visible') and t.get('owner') == obs.get('player_id'))
        units_count = sum(1 for t in tiles if t.get('unit', {}).get('owner') == obs.get('player_id'))

        print(f"Visible tiles: {visible_count} | Owned: {owned_tiles} | Units: {units_count}")
        print('='*70)

        # Show compact map (just the visible region)
        width = obs.get('map_width', 16)
        height = obs.get('map_height', 16)
        grid = {}
        for tile in tiles:
            x, y = tile.get('x', 0), tile.get('y', 0)
            grid[(x, y)] = tile

        for y in range(height):
            row = []
            for x in range(width):
                tile = grid.get((x, y), {})
                if not tile.get('visible', False):
                    row.append(' ')
                else:
                    owner = tile.get('owner', 0)
                    # Color code by owner (1-4)
                    if owner == 1:  # player_0
                        prefix = ''
                    else:
                        prefix = str(owner) if owner > 0 else ' '

                    # Check for city/improvement
                    if tile.get('improvement'):
                        imp = tile['improvement']
                        if imp.get('type') == 'City':
                            row.append('*' if imp.get('is_capital', False) else 'C')
                        else:
                            row.append('i')
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

    # Take action (just end turn)
    env.step(0)

    if term or trunc:
        print("\nGame ended!")
        break

env.close()
