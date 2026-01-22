"""Check where visible tiles are for each player"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

for player in ['player_0', 'player_1', 'player_2', 'player_3']:
    obs = env._observations.get(player, {})
    tiles = obs.get('tiles', [])
    visible_tiles = [t for t in tiles if t.get('visible')]

    # Find capital
    capital = next((t for t in visible_tiles if t.get('improvement', {}).get('is_capital')), None)

    if capital:
        print(f"{player} ({obs.get('tribe')}): capital at ({capital.get('x')}, {capital.get('y')})")

    # Show range of visible tiles
    xs = [t.get('x') for t in visible_tiles]
    ys = [t.get('y') for t in visible_tiles]
    if xs and ys:
        print(f"  Visible tiles: x=[{min(xs)}-{max(xs)}], y=[{min(ys)}-{max(ys)}]")

env.close()
