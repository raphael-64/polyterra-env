"""Debug agent tracking"""
from polyterra_env import PolyterraEnv

env = PolyterraEnv(num_players=4, game_mode="perfection", max_turns=10)
env.reset(seed=42)

print(f"Agents: {env.agents}")
print(f"Agent selection: {env.agent_selection}")
print(f"Terminations: {env.terminations}")
print(f"Truncations: {env.truncations}")

# Take a few steps
for i in range(10):
    agent = env.agent_selection
    print(f"\nStep {i}: agent={agent}")

    if env.terminations.get(agent) or env.truncations.get(agent):
        action = None
    else:
        action = [0, 0, 0, 0, 0, 0]  # END_TURN

    env.step(action)

    print(f"  After step: agents={env.agents}, selection={env.agent_selection}")

env.close()
