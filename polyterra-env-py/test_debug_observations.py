"""Debug test to see what the C# backend is actually returning"""
from polyterra_env import PolyterraEnv
import json

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10)
env.reset(seed=42)

print("="*70)
print("RAW OBSERVATIONS FROM C# BACKEND")
print("="*70)

# Get raw observations
for agent in env.agents:
    print(f"\n{agent}:")
    raw_obs = env._observations.get(agent, {})

    # Print in formatted JSON
    print(json.dumps(raw_obs, indent=2, default=str))

    # Break after first agent to avoid too much output
    break

env.close()
