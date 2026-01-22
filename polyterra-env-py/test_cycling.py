"""Test agent cycling"""
import polyterra_env

env = polyterra_env.env(num_players=4, game_mode="perfection", max_turns=10, render_mode="human")
env.reset(seed=42)

print("Initial agent:", env.agent_selection)

# Take 20 steps and print which agent acts
for i in range(20):
    agent = env.agent_selection
    obs, reward, term, trunc, info = env.last()

    print(f"Step {i+1}: Agent {agent} acts")

    env.step(0)  # end_turn

    if all(env.terminations.values()) or all(env.truncations.values()):
        break

env.close()
