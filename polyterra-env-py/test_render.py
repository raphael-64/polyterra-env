"""Test rendering"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

# Take a few steps
for _ in range(3):
    agent = env.agent_selection
    obs, reward, term, trunc, info = env.last()
    env.step(0)

# Render
print("\n\n=== RENDERING ===\n")
env.render()

env.close()
