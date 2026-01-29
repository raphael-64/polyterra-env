"""
Record a game with proper agent cycling and full map state.
"""
import sys
sys.path.insert(0, '../polyterra-env-py')

import numpy as np
from polyterra_env import PolyterraEnv
from sb3_contrib import MaskablePPO
from replay_system import ReplayRecorder
import argparse


def record_game(model_path=None, max_steps=200, save_as="latest_game.json"):
    """Record a game and save replay with full multi-agent support."""

    recorder = ReplayRecorder()
    recorder.start_recording({
        "model": model_path or "random",
        "max_steps": max_steps,
    })

    # Load model if provided
    model = None
    if model_path:
        try:
            model = MaskablePPO.load(model_path)
            print(f"Loaded model from {model_path}")
        except Exception as e:
            print(f"Could not load model: {e}")
            print("Using random actions instead")

    # Create environment
    env = PolyterraEnv(num_players=2)
    env.reset(seed=42)

    print(f"Recording game (max {max_steps} steps)...")
    print("=" * 60)

    step_count = 0
    turn_count = 0
    last_turn = 0

    while step_count < max_steps:
        agent = env.agent_selection

        # Check if done
        if agent not in env.agents:
            print(f"Agent {agent} not in active agents")
            break

        all_done = all(
            env.terminations.get(a, False) or env.truncations.get(a, False)
            for a in env.possible_agents
        )
        if all_done:
            print("All agents terminated/truncated")
            break

        # Get observation
        obs = env.observe(agent)
        current_turn = obs.get('turn', 0)

        # Track turn changes
        if current_turn != last_turn:
            turn_count = current_turn
            last_turn = current_turn

        # Get action mask
        action_mask = obs.get('action_mask', None)
        if action_mask and isinstance(action_mask, tuple) and len(action_mask) > 0:
            type_mask = np.array(action_mask[0], dtype=np.int8)
        else:
            type_mask = np.ones(37, dtype=np.int8)

        # Select action
        if model:
            # Flatten observation for model
            flat_obs = flatten_obs(obs)
            # Use model to predict action type
            action_type, _ = model.predict(flat_obs, action_masks=type_mask, deterministic=False)
            action_type = int(action_type)
        else:
            # Random action from valid actions
            valid_actions = np.where(type_mask == 1)[0]
            action_type = np.random.choice(valid_actions) if len(valid_actions) > 0 else 0

        # Create full action tuple
        action = (action_type, 0, 0, 0, 0, 0)

        # Step environment
        env.step(action)

        # Get reward
        reward = env.rewards.get(agent, 0)

        # Record step
        recorder.record_step(agent, obs, action, reward)

        step_count += 1

        # Print progress every agent cycle (2 steps)
        if step_count % 10 == 0:
            print(f"Step {step_count}: Turn {turn_count} | {agent} | Action: {get_action_name(action_type)}")

    # Save replay
    filepath = recorder.save_replay(save_as, compress=False)
    print("\n" + "=" * 60)
    print(f"Replay saved to: {filepath}")
    print(f"Total steps: {step_count}, Total turns: {turn_count}")
    print(f"Open replay_viewer.html in a browser and load this file")

    env.close()
    return filepath


def flatten_obs(obs, map_size=16):
    """Flatten observation for model (same as training)."""
    flat = []
    flat.append(obs.get('currency', 0) / 1000.0)
    flat.append(obs.get('score', 0) / 10000.0)
    flat.append(obs.get('turn', 0) / 30.0)
    flat.append(obs.get('num_cities', 0) / 10.0)
    flat.append(obs.get('num_kills', 0) / 100.0)
    flat.append(obs.get('num_casualties', 0) / 100.0)
    flat.append(obs.get('current_player_idx', 0) / 2.0)
    flat.append(obs.get('player_id', 0) / 3.0)
    flat.append(len(obs.get('units', [])) / 20.0)
    flat.append(len(obs.get('cities', [])) / 10.0)

    tiles = obs.get('tiles', [])
    tile_grid = np.zeros((map_size, map_size, 4), dtype=np.float32)
    for tile in tiles:
        x, y = tile.get('x', 0), tile.get('y', 0)
        if 0 <= x < map_size and 0 <= y < map_size:
            tile_grid[x, y, 0] = tile.get('explored', 0)
            tile_grid[x, y, 1] = tile.get('owner', 0) / 4.0
            tile_grid[x, y, 2] = tile.get('has_unit', 0)
            tile_grid[x, y, 3] = tile.get('terrain', 0) / 7.0
    flat.extend(tile_grid.flatten())

    return np.array(flat, dtype=np.float32)


def get_action_name(action_type):
    """Get readable action name."""
    names = {
        0: "END_TURN", 1: "MOVE", 2: "ATTACK", 3: "CAPTURE", 4: "BUILD",
        5: "RESEARCH", 6: "TRAIN", 7: "UPGRADE_UNIT", 8: "DISBAND",
        9: "EXAMINE_RUINS", 10: "HEAL", 11: "CONVERT", 12: "MIND_BEND",
        13: "GROW_FOREST", 14: "BURN_FOREST", 15: "DESTROY", 16: "CLEAR_FOREST",
    }
    return names.get(action_type, f"ACTION_{action_type}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--model", type=str, help="Path to trained model (optional)")
    parser.add_argument("--steps", type=int, default=200, help="Max steps")
    parser.add_argument("--output", type=str, default="game.json", help="Output filename")
    args = parser.parse_args()

    record_game(
        model_path=args.model,
        max_steps=args.steps,
        save_as=args.output
    )
