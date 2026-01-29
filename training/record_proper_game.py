"""Record a proper game with smart action selection using valid_actions."""
import sys
sys.path.insert(0, '../polyterra-env-py')

import numpy as np
from polyterra_env import PolyterraEnv
from replay_system import ReplayRecorder

env = PolyterraEnv(num_players=2)
env.reset(seed=42)

recorder = ReplayRecorder()
recorder.start_recording({"type": "smart_valid_actions"})

print("Recording game with valid action selection...")
print("=" * 60)

step_count = 0
max_steps = 300

while step_count < max_steps:
    agent = env.agent_selection

    if agent not in env.agents:
        break

    if all(env.terminations.get(a, False) for a in env.possible_agents):
        break

    # Get raw observation for valid_actions
    raw_obs = env._raw_observations.get(agent, {})
    valid_actions = raw_obs.get("valid_actions", {})
    obs = env.observe(agent)

    # Decide action based on what's valid
    action = None
    action_name = ""

    # Get available action types
    valid_moves = valid_actions.get("valid_moves", [])
    valid_attacks = valid_actions.get("valid_attacks", [])
    valid_research = valid_actions.get("valid_research", [])
    valid_builds = valid_actions.get("valid_builds", [])
    valid_trains = valid_actions.get("valid_trains", [])

    # Build list of possible choices
    choices = []
    if valid_moves:
        choices.append(("move", valid_moves))
    if valid_attacks:
        choices.append(("attack", valid_attacks))
    if valid_research:
        choices.append(("research", valid_research))
    if valid_builds:
        choices.append(("build", valid_builds))
    if valid_trains:
        choices.append(("train", valid_trains))

    # 80% chance to do something other than END_TURN
    if choices and np.random.random() < 0.8:
        choice_type, choice_list = choices[np.random.randint(len(choices))]
        selected = choice_list[np.random.randint(len(choice_list))]

        if choice_type == "move":
            unit_id = selected["unit_id"]
            units = obs.get("units", [])
            unit_idx = 0
            for i, u in enumerate(units):
                if u.get("id") == unit_id:
                    unit_idx = i
                    break
            action = (1, selected["to_x"], selected["to_y"], unit_idx, 0, 0)
            action_name = f"MOVE to ({selected['to_x']},{selected['to_y']})"

        elif choice_type == "attack":
            unit_id = selected["unit_id"]
            units = obs.get("units", [])
            unit_idx = 0
            for i, u in enumerate(units):
                if u.get("id") == unit_id:
                    unit_idx = i
                    break
            action = (2, selected["target_x"], selected["target_y"], unit_idx, 0, 0)
            action_name = f"ATTACK at ({selected['target_x']},{selected['target_y']})"

        elif choice_type == "research":
            tech_name = selected  # This is the tech name string like "Climbing"
            # Look up the actual tech type index from mapping
            from game_data_mappings import TECH_NAME_TO_IDX
            tech_idx = TECH_NAME_TO_IDX.get(tech_name.lower(), 0)
            action = (5, 0, 0, 0, tech_idx, 0)
            action_name = f"RESEARCH {tech_name}"

        elif choice_type == "build":
            # ACTION_BUILD = 3, param1 = improvement type index
            from game_data_mappings import IMPROVEMENT_NAME_TO_IDX
            imp_name = selected.get('improvement_type', 'Farm')
            imp_idx = IMPROVEMENT_NAME_TO_IDX.get(imp_name.lower(), 5)  # 5 = Farm
            action = (3, selected["x"], selected["y"], 0, imp_idx, 0)
            action_name = f"BUILD {imp_name} at ({selected['x']},{selected['y']})"

        elif choice_type == "train":
            # ACTION_TRAIN = 4, need to pass unit_type in param1
            # For now just train the first unit type available (Warrior=2, Scout=1)
            unit_type_str = selected.get("unit_type", "Warrior")
            unit_type_idx = 2 if unit_type_str == "Warrior" else 1  # Warrior=2, Scout=1
            action = (4, selected.get("city_x", 0), selected.get("city_y", 0), 0, unit_type_idx, 0)
            action_name = f"TRAIN {unit_type_str}"

    # Default to END_TURN
    if action is None:
        action = (0, 0, 0, 0, 0, 0)
        action_name = "END_TURN"

    # Execute action
    env.step(action)
    reward = env.rewards.get(agent, 0)
    recorder.record_step(agent, obs, action, reward)

    step_count += 1

    if step_count % 30 == 0:
        turn = obs.get("turn", 0)
        print(f"Step {step_count}: Turn {turn} | {agent} | {action_name}")

# Save
filepath = recorder.save_replay("good_game.json", compress=False)
print(f"\n{'='*60}")
print(f"Saved to: {filepath}")
print(f"Total steps: {step_count}")

# Analyze
import json
with open(filepath) as f:
    data = json.load(f)

agents = {}
turns = set()
moves = 0
for step in data['steps']:
    agents[step['agent']] = agents.get(step['agent'], 0) + 1
    turns.add(step['observation']['turn'])
    if step['action']['type'] == 'MOVE':
        moves += 1

print(f"Agents: {agents}")
print(f"Turns reached: {len(turns)} (0-{max(turns) if turns else 0})")
print(f"Move actions: {moves}")

env.close()
