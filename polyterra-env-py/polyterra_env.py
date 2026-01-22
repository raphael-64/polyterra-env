"""
PettingZoo environment for Polytopia game
Communicates with C# game engine via JSON over subprocess
"""

import json
import subprocess
import functools
from typing import Dict, List, Any, Optional
import numpy as np
from gymnasium import spaces
from pettingzoo import AECEnv
from pettingzoo.utils.agent_selector import agent_selector
from pettingzoo.utils import wrappers


class PolyterraEnv(AECEnv):
    """
    PettingZoo AEC environment for Polytopia                           

    Multi-agent turn-based strategy game with 4 players
    """

    metadata = {
        "render_modes": ["human", "ansi"],
        "name": "polyterra_v0",
        "is_parallelizable": False,
    }

    def __init__(
        self,
        num_players: int = 4,
        game_mode: str = "perfection",
        max_turns: int = 30,
        map_size: int = 16,
        render_mode: Optional[str] = None,
        dotnet_path: str = "dotnet",
        dll_path: Optional[str] = None,
    ):
        """
        Initialize Polytopia environment

        Args:
            num_players: Number of players (2-4)
            game_mode: "perfection" or "domination"
            max_turns: Maximum turns before truncation
            map_size: Map dimension (auto-determined by game mode)
            render_mode: "human" or "ansi"
            dotnet_path: Path to dotnet executable
            dll_path: Path to PolyterraTest.dll (auto-detected if None)
        """
        super().__init__()

        self.num_players = num_players
        self.game_mode = game_mode
        self.max_turns = max_turns
        self.map_size = map_size
        self.render_mode = render_mode
        self.dotnet_path = dotnet_path

        # Auto-detect DLL path if not provided
        if dll_path is None:
            import os
            base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            dll_path = os.path.join(
                base_dir,
                "polyterra-test/PolyterraTest/bin/Debug/net8.0/PolyterraTest.dll"
            )
        self.dll_path = dll_path

        # Agent setup
        self.possible_agents = [f"player_{i}" for i in range(num_players)]
        self.agent_name_mapping = {name: i for i, name in enumerate(self.possible_agents)}

        # Will be set in reset()
        self.agents = []
        self._agent_selector = None
        self.rewards = {}
        self.terminations = {}
        self.truncations = {}
        self.infos = {}
        self._cumulative_rewards = {}

        # C# subprocess
        self.process = None

    @functools.lru_cache(maxsize=None)
    def observation_space(self, agent):
        """
        Observation space for each agent

        Returns a Dict space containing:
        - turn: Current turn number
        - current_player_idx: Index of current player
        - currency: Stars (money)
        - score: Victory points
        - tribe: Tribe name (encoded as string)
        - map_width, map_height: Map dimensions
        - tiles: List of tile observations (256 for 16x16)
        """
        return spaces.Dict({
            "turn": spaces.Discrete(self.max_turns + 1),
            "current_player_idx": spaces.Discrete(self.num_players),
            "currency": spaces.Box(0, 10000, shape=(), dtype=np.int32),
            "score": spaces.Box(0, 1000000, shape=(), dtype=np.int32),
            "tribe": spaces.Text(20),
            "map_width": spaces.Discrete(50),
            "map_height": spaces.Discrete(50),
            # Simplified tile representation
            # Each tile: [x, y, terrain, owner, visible, has_unit, has_city]
            "tiles": spaces.Sequence(
                spaces.Dict({
                    "x": spaces.Discrete(50),
                    "y": spaces.Discrete(50),
                    "terrain": spaces.Discrete(6),  # 6 terrain types
                    "owner": spaces.Discrete(6),    # 0-4 players + neutral
                    "visible": spaces.Discrete(2),  # 0/1
                    "has_unit": spaces.Discrete(2),
                    "has_city": spaces.Discrete(2),
                })
            )
        })

    @functools.lru_cache(maxsize=None)
    def action_space(self, agent):
        """
        Action space for each agent

        For now, simplified to just "end_turn"
        TODO: Expand to full action space (move, attack, build, etc.)
        """
        return spaces.Discrete(1)  # Only end_turn for now

    def reset(self, seed: Optional[int] = None, options: Optional[dict] = None):
        """Reset the environment"""
        # Start C# subprocess if not running
        if self.process is None:
            self._start_process()

        # Send reset command
        command = {
            "command": "reset",
            "seed": seed,
            "num_players": self.num_players,
            "game_mode": self.game_mode
        }

        response = self._send_command(command)

        if not response.get("success"):
            raise RuntimeError(f"Reset failed: {response.get('error')}")

        # Initialize agent tracking
        self.agents = response["agents"].copy()
        self._agent_selector = agent_selector(self.agents)
        self.agent_selection = self._agent_selector.next()

        # Initialize rewards and terminations
        self.rewards = {agent: 0.0 for agent in self.agents}
        self._cumulative_rewards = {agent: 0.0 for agent in self.agents}
        self.terminations = {agent: False for agent in self.agents}
        self.truncations = {agent: False for agent in self.agents}
        self.infos = {agent: {} for agent in self.agents}

        # Store observations
        self._observations = response["observations"]

    def step(self, action):
        """Execute one step in the environment"""
        if (
            self.terminations[self.agent_selection]
            or self.truncations[self.agent_selection]
        ):
            # Agent already done, just advance
            return self._was_dead_step(action)

        agent = self.agent_selection

        # Convert action to command
        # For now, only support end_turn (action=0)
        command = {
            "command": "step",
            "action_type": "end_turn",
            "action_params": {}
        }

        response = self._send_command(command)

        if not response.get("success"):
            # Invalid action - penalize
            self.rewards[agent] = -10.0
            self.terminations[agent] = True
            self.infos[agent] = {"error": response.get("error")}
        else:
            # Update state from response
            self.agents = response["agents"]
            self._observations = response["observations"]

            # Update rewards (delta from previous)
            new_rewards = response["rewards"]
            for ag in self.agents:
                reward_delta = new_rewards[ag] - self._cumulative_rewards.get(ag, 0)
                self.rewards[ag] = reward_delta
                self._cumulative_rewards[ag] = new_rewards[ag]

            # Update terminations and truncations
            self.terminations = response["terminations"]
            self.truncations = response["truncations"]

            # Update agent selection
            self.agent_selection = response["agent_selection"]

            # Update info
            for ag in self.agents:
                self.infos[ag] = response.get("info", {})

        # Clear reward for next agent
        self._clear_rewards()

        # Select next agent if current is done
        if self.agent_selection not in self.agents:
            self.agent_selection = self._agent_selector.next()

    def observe(self, agent):
        """Get observation for specific agent"""
        return self._observations.get(agent, {})

    def render(self):
        """Render the environment"""
        if self.render_mode is None:
            return

        if self.render_mode == "ansi":
            return self._render_ansi()
        elif self.render_mode == "human":
            print(self._render_ansi())

    def _render_ansi(self):
        """Render as ASCII text with map visualization"""
        # Get current observation for rendering
        current_agent = self.agent_selection
        obs = self._observations.get(current_agent, {})

        if not obs:
            return "No observation available"

        output = []
        output.append("=" * 60)
        output.append(f"POLYTOPIA - Turn {obs.get('turn', 0)}")
        output.append(f"Current Player: {current_agent} ({obs.get('tribe', 'Unknown')})")
        output.append(f"Currency: {obs.get('currency', 0)} | Score: {obs.get('score', 0)} | Cities: {obs.get('cities', 0)}")
        output.append("=" * 60)

        # Render mini-map
        tiles = obs.get('tiles', [])
        width = obs.get('map_width', 16)
        height = obs.get('map_height', 16)

        # Create grid
        grid = {}
        for tile in tiles:
            x, y = tile.get('x', 0), tile.get('y', 0)
            grid[(x, y)] = tile

        output.append("\nMap:")
        # Show full map
        for y in range(height):
            row = []
            for x in range(width):
                tile = grid.get((x, y), {})
                char = self._get_tile_char(tile)
                row.append(char)
            output.append(''.join(row))

        output.append("\nLegend: . water  , land  C city  U unit  * capital  (space = unexplored)")
        output.append("=" * 60)

        return "\n".join(output)

    def _get_tile_char(self, tile):
        """Get ASCII character for a tile"""
        if not tile.get('visible', False):
            return ' '  # Unknown

        # Check for city first
        if tile.get('improvement'):
            imp = tile['improvement']
            if imp.get('type') == 'City':
                return '*' if imp.get('is_capital', False) else 'C'

        # Check for unit
        if tile.get('unit'):
            return 'U'

        # Terrain
        terrain = tile.get('terrain', '')
        if terrain in ['Ocean', 'Water']:
            return '.'
        elif terrain in ['Mountain']:
            return '^'
        elif terrain in ['Forest']:
            return 'T'
        else:
            return ','  # Field/plain

    def close(self):
        """Clean up resources"""
        if self.process is not None:
            command = {"command": "close"}
            self._send_command(command)
            self.process.terminate()
            self.process.wait()
            self.process = None

    def _start_process(self):
        """Start the C# game process"""
        cmd = [self.dotnet_path, self.dll_path, "--env-server"]

        self.process = subprocess.Popen(
            cmd,
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            bufsize=1
        )

        # Wait for startup message
        import time
        time.sleep(0.5)

    def _send_command(self, command: dict) -> dict:
        """Send JSON command to C# process and get response"""
        if self.process is None:
            raise RuntimeError("Process not started")

        # Send command
        json_str = json.dumps(command)
        self.process.stdin.write(json_str + "\n")
        self.process.stdin.flush()

        # Read response
        response_str = self.process.stdout.readline()

        if not response_str:
            raise RuntimeError("No response from game process")

        return json.loads(response_str)


def env(**kwargs):
    """
    Create environment with standard wrappers
    """
    env = PolyterraEnv(**kwargs)

    # Apply standard wrappers
    env = wrappers.CaptureStdoutWrapper(env)
    env = wrappers.AssertOutOfBoundsWrapper(env)
    env = wrappers.OrderEnforcingWrapper(env)

    return env


# For direct import
__all__ = ["PolyterraEnv", "env"]
