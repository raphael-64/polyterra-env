"""Test rendering for all players"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

# Render for each player
for i in range(4):
    print(f"\n{'='*60}")
    print(f"RENDERING FOR PLAYER {i}")
    print('='*60)
    env.render()
    env.step(0)  # Move to next player

env.close()
