"""
Replay recording and viewing system for Polyterra games.
Saves full game states for later playback and analysis.
"""
import json
import gzip
import time
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Any, Optional

# Action type names for display - must match polyterra_env.py
ACTION_NAMES = {
    0: "END_TURN",
    1: "MOVE",
    2: "ATTACK",
    3: "BUILD",
    4: "TRAIN",
    5: "RESEARCH",
    6: "UPGRADE",
    7: "RECOVER",
    8: "HEAL_OTHERS",
    9: "PROMOTE",
    10: "EXAMINE_RUINS",
    11: "DISBAND",
    12: "DESTROY",
    13: "CAPTURE",
}


class ReplayRecorder:
    """Records game sessions for later playback."""

    def __init__(self, save_dir: str = "replays"):
        self.save_dir = Path(save_dir)
        self.save_dir.mkdir(exist_ok=True)
        self.current_replay: Optional[Dict] = None
        self.step_count = 0

    def start_recording(self, metadata: Dict[str, Any] = None):
        """Start recording a new game."""
        self.current_replay = {
            "metadata": {
                "start_time": datetime.now().isoformat(),
                "version": "1.0",
                **(metadata or {})
            },
            "steps": []
        }
        self.step_count = 0

    def record_step(self,
                   agent: str,
                   observation: Dict,
                   action: tuple,
                   reward: float,
                   info: Dict = None):
        """Record a single step."""
        if self.current_replay is None:
            return

        # Convert action to Python native types
        action = tuple(int(a) for a in action)

        step_data = {
            "step": self.step_count,
            "agent": str(agent),
            "observation": self._serialize_obs(observation),
            "action": {
                "raw": list(action),
                "type": ACTION_NAMES.get(action[0], f"ACTION_{action[0]}"),
                "target": (int(action[1]), int(action[2])) if len(action) > 2 else None,
                "unit_idx": int(action[3]) if len(action) > 3 else None,
                "params": [int(p) for p in action[4:]] if len(action) > 4 else []
            },
            "reward": float(reward),
            "info": info or {}
        }

        self.current_replay["steps"].append(step_data)
        self.step_count += 1

    def _serialize_obs(self, obs: Dict) -> Dict:
        """Serialize observation to JSON-compatible format."""
        return {
            "turn": int(obs.get("turn", 0)),
            "currency": int(obs.get("currency", 0)),
            "score": int(obs.get("score", 0)),
            "num_cities": int(obs.get("num_cities", 0)),
            "num_kills": int(obs.get("num_kills", 0)),
            "num_casualties": int(obs.get("num_casualties", 0)),
            "num_units": int(len(obs.get("units", []))),
            "units": [self._serialize_unit(u) for u in obs.get("units", [])[:20]],  # Limit for size
            "cities": [self._serialize_city(c) for c in obs.get("cities", [])[:10]],
            "tiles": [self._serialize_tile(t) for t in obs.get("tiles", [])],  # Full map
        }

    def _serialize_unit(self, unit: Dict) -> Dict:
        """Serialize unit data."""
        return {
            "type": str(unit.get("type", "")),
            "position": (int(unit.get("x", 0)), int(unit.get("y", 0))),
            "health": float(unit.get("health", 0)),
            "moved": bool(unit.get("moved", False)),
            "attacked": bool(unit.get("attacked", False)),
        }

    def _serialize_city(self, city: Dict) -> Dict:
        """Serialize city data."""
        return {
            "name": str(city.get("name", "")),
            "position": (int(city.get("x", 0)), int(city.get("y", 0))),
            "level": int(city.get("level", 0)),
            "population": int(city.get("population", 0)),
        }

    def _serialize_tile(self, tile: Dict) -> Dict:
        """Serialize tile data."""
        return {
            "x": int(tile.get("x", 0)),
            "y": int(tile.get("y", 0)),
            "terrain": int(tile.get("terrain", 0)),
            "owner": int(tile.get("owner", 0)),
            "explored": int(tile.get("explored", 0)),
            "visible": int(tile.get("visible", 0)),
            "has_unit": int(tile.get("has_unit", 0)),
            "unit_type": int(tile.get("unit_type", 0)),
            "unit_owner": int(tile.get("unit_owner", 0)),
            "unit_health": float(tile.get("unit_health", 0)),
            "improvement_type": int(tile.get("improvement_type", 0)),
            "is_capital": int(tile.get("is_capital", 0)),
        }

    def save_replay(self, filename: str = None, compress: bool = True) -> Path:
        """Save the current replay to disk."""
        if self.current_replay is None:
            raise ValueError("No replay to save")

        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"replay_{timestamp}.json"

        if compress and not filename.endswith(".gz"):
            filename += ".gz"

        filepath = self.save_dir / filename

        # Add final metadata
        self.current_replay["metadata"]["end_time"] = datetime.now().isoformat()
        self.current_replay["metadata"]["total_steps"] = self.step_count

        # Save
        data = json.dumps(self.current_replay, indent=2)
        if compress:
            with gzip.open(filepath, "wt", encoding="utf-8") as f:
                f.write(data)
        else:
            with open(filepath, "w") as f:
                f.write(data)

        return filepath


class ReplayViewer:
    """View recorded game replays."""

    def __init__(self, replay_file: str):
        self.replay_file = Path(replay_file)
        self.replay_data = self._load_replay()
        self.current_step = 0

    def _load_replay(self) -> Dict:
        """Load replay from file."""
        if self.replay_file.suffix == ".gz":
            with gzip.open(self.replay_file, "rt", encoding="utf-8") as f:
                return json.load(f)
        else:
            with open(self.replay_file, "r") as f:
                return json.load(f)

    def get_metadata(self) -> Dict:
        """Get replay metadata."""
        return self.replay_data["metadata"]

    def get_total_steps(self) -> int:
        """Get total number of steps in replay."""
        return len(self.replay_data["steps"])

    def get_step(self, step_num: int) -> Optional[Dict]:
        """Get data for a specific step."""
        if 0 <= step_num < len(self.replay_data["steps"]):
            return self.replay_data["steps"][step_num]
        return None

    def print_summary(self):
        """Print a summary of the replay."""
        meta = self.get_metadata()
        steps = self.replay_data["steps"]

        print("=" * 70)
        print("REPLAY SUMMARY")
        print("=" * 70)
        print(f"Recorded: {meta.get('start_time', 'Unknown')}")
        print(f"Total steps: {len(steps)}")

        # Count actions
        action_counts = {}
        total_reward = {}

        for step in steps:
            action_type = step["action"]["type"]
            action_counts[action_type] = action_counts.get(action_type, 0) + 1

            agent = step["agent"]
            total_reward[agent] = total_reward.get(agent, 0) + step["reward"]

        print(f"\nAgents: {', '.join(total_reward.keys())}")
        for agent, reward in total_reward.items():
            print(f"  {agent}: {reward:,.0f} total reward")

        print(f"\nAction distribution:")
        for action, count in sorted(action_counts.items(), key=lambda x: -x[1])[:10]:
            print(f"  {action}: {count}")

        print("=" * 70)

    def watch(self, speed: float = 0.5, start_step: int = 0, max_steps: int = None):
        """Watch the replay with formatted output."""
        print("=" * 70)
        print("WATCHING REPLAY")
        print("=" * 70)

        steps = self.replay_data["steps"]
        if max_steps:
            steps = steps[start_step:start_step + max_steps]
        else:
            steps = steps[start_step:]

        for step_data in steps:
            self._print_step(step_data)
            if speed > 0:
                time.sleep(speed)

    def _print_step(self, step_data: Dict):
        """Print a single step."""
        obs = step_data["observation"]
        action = step_data["action"]

        print(f"\n--- Step {step_data['step']} ---")
        print(f"Agent: {step_data['agent']} | Turn: {obs['turn']} | "
              f"Currency: {obs['currency']} | Score: {obs['score']:,}")
        print(f"Units: {obs['num_units']} | Cities: {obs['num_cities']} | "
              f"Kills: {obs['num_kills']} | Casualties: {obs['num_casualties']}")

        # Show unit positions
        if obs.get('units'):
            unit_info = []
            for u in obs['units'][:5]:  # Show first 5
                pos = u['position']
                hp = u['health']
                moved = "M" if u['moved'] else ""
                attacked = "A" if u['attacked'] else ""
                flags = f"{moved}{attacked}".ljust(2)
                unit_info.append(f"{u['type']}@({pos[0]},{pos[1]})[{hp}hp]{flags}")
            print(f"Units: {', '.join(unit_info)}")

        # Show action
        action_str = f">> {action['type']}"
        if action.get('target'):
            action_str += f" to {action['target']}"
        if action.get('unit_idx') is not None:
            action_str += f" (unit {action['unit_idx']})"
        print(action_str)

        # Show reward
        if step_data['reward'] != 0:
            print(f"   Reward: {step_data['reward']:+,.1f}")

    def analyze_agent_behavior(self, agent_name: str):
        """Analyze behavior patterns for a specific agent."""
        steps = [s for s in self.replay_data["steps"] if s["agent"] == agent_name]

        if not steps:
            print(f"No steps found for agent {agent_name}")
            return

        print(f"\n{'=' * 70}")
        print(f"AGENT ANALYSIS: {agent_name}")
        print(f"{'=' * 70}")

        # Action distribution
        actions = {}
        for step in steps:
            action_type = step["action"]["type"]
            actions[action_type] = actions.get(action_type, 0) + 1

        print(f"\nTotal steps: {len(steps)}")
        print(f"Action distribution:")
        for action, count in sorted(actions.items(), key=lambda x: -x[1]):
            pct = 100 * count / len(steps)
            print(f"  {action}: {count} ({pct:.1f}%)")

        # Reward analysis
        rewards = [s["reward"] for s in steps]
        total_reward = sum(rewards)
        avg_reward = total_reward / len(rewards) if rewards else 0

        print(f"\nReward analysis:")
        print(f"  Total: {total_reward:,.1f}")
        print(f"  Average per step: {avg_reward:.2f}")
        print(f"  Max single reward: {max(rewards) if rewards else 0:.1f}")
        print(f"  Min single reward: {min(rewards) if rewards else 0:.1f}")

        # State progression
        first_obs = steps[0]["observation"]
        last_obs = steps[-1]["observation"]

        print(f"\nState progression:")
        print(f"  Currency: {first_obs['currency']} -> {last_obs['currency']}")
        print(f"  Score: {first_obs['score']:,} -> {last_obs['score']:,}")
        print(f"  Units: {first_obs['num_units']} -> {last_obs['num_units']}")
        print(f"  Cities: {first_obs['num_cities']} -> {last_obs['num_cities']}")
        print(f"  Kills: {first_obs['num_kills']} -> {last_obs['num_kills']}")


def should_record_replay(episode_num: int, interval: int = 10) -> bool:
    """Determine if we should record this episode (1 in N)."""
    return episode_num % interval == 0
