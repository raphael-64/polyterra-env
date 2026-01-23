"""
Example: Training with game state logging for later visualization

This demonstrates saving interesting game states during training for
post-hoc analysis with a separate viewer.
"""

import json
import numpy as np
from polyterra_env import PolyterraEnv
from pathlib import Path


def should_save_state(episode_reward, turn, last_save_turn):
    """
    Determine if current state is interesting enough to save.

    You can customize this based on:
    - High/low rewards
    - Novel strategies (entropy, diversity metrics)
    - Critical moments (captures, tech discoveries)
    - Periodic snapshots
    """
    # Save every 10 turns as baseline
    if turn - last_save_turn >= 10:
        return True

    # Save high-reward moments
    if abs(episode_reward) > 100:
        return True

    return False


def train_with_logging(num_episodes=5, log_dir="game_logs"):
    """Run training episodes and log interesting states"""

    # Create log directory
    Path(log_dir).mkdir(exist_ok=True)

    # Training environment - no rendering overhead
    env = PolyterraEnv(
        num_players=4,
        game_mode="perfection",
        max_turns=30,
        render_mode=None  # No rendering during training
    )

    episode_logs = []

    for episode in range(num_episodes):
        print(f"\nEpisode {episode + 1}/{num_episodes}")

        env.reset(seed=42 + episode)

        episode_data = {
            "episode": episode,
            "seed": 42 + episode,
            "states": [],
            "total_reward": 0,
            "num_turns": 0,
        }

        last_save_turn = -10
        episode_reward = 0

        # Run episode
        step = 0
        while True:
            agent = env.agent_selection
            obs = env.observe(agent)

            # Simple random policy (replace with your RL agent)
            action = np.array([0, 0, 0, 0, 0, 0])  # END_TURN for now

            env.step(action)

            # Track rewards
            reward = env.rewards.get(agent, 0)
            episode_reward += reward

            current_turn = obs.get("turn", 0)

            # Save interesting states
            if should_save_state(episode_reward, current_turn, last_save_turn):
                state_snapshot = env.get_state_snapshot()
                episode_data["states"].append(state_snapshot)
                last_save_turn = current_turn
                print(f"  Saved state at turn {current_turn}, reward={episode_reward:.1f}")

            # Check termination
            if env.terminations.get(agent) or env.truncations.get(agent):
                episode_data["num_turns"] = current_turn
                episode_data["total_reward"] = episode_reward
                print(f"  Episode finished: {episode_data['num_turns']} turns, "
                      f"reward={episode_data['total_reward']:.1f}, "
                      f"{len(episode_data['states'])} states saved")
                break

            step += 1
            if step > 1000:  # Safety limit
                break

        # Save episode log
        episode_logs.append(episode_data)

        # Save to file
        log_file = Path(log_dir) / f"episode_{episode:04d}.json"
        with open(log_file, "w") as f:
            json.dump(episode_data, f, indent=2)

    env.close()

    # Save summary
    summary = {
        "num_episodes": num_episodes,
        "episodes": [
            {
                "episode": ep["episode"],
                "seed": ep["seed"],
                "turns": ep["num_turns"],
                "reward": ep["total_reward"],
                "states_saved": len(ep["states"]),
            }
            for ep in episode_logs
        ]
    }

    with open(Path(log_dir) / "summary.json", "w") as f:
        json.dump(summary, f, indent=2)

    print(f"\n\nLogged {num_episodes} episodes to {log_dir}/")
    print("Use a separate viewer to visualize saved states.")

    return episode_logs


if __name__ == "__main__":
    logs = train_with_logging(num_episodes=5, log_dir="game_logs")

    print("\n" + "="*70)
    print("TRAINING COMPLETE")
    print("="*70)
    print("\nSummary:")
    for log in logs:
        print(f"  Episode {log['episode']}: "
              f"{log['num_turns']} turns, "
              f"reward={log['total_reward']:.1f}, "
              f"{len(log['states'])} states saved")

    print(f"\nLogs saved to: game_logs/")
    print("Next: Build a viewer to visualize game_logs/episode_*.json")
