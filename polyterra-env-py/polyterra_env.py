"""
PettingZoo environment for Polytopia game - Version 2
Comprehensive observation and action spaces based on full game state
"""

import json
import subprocess
import functools
from typing import Dict, List, Any, Optional, Tuple
import numpy as np
from gymnasium import spaces
from pettingzoo import AECEnv
from pettingzoo.utils.agent_selector import agent_selector
from pettingzoo.utils import wrappers

# Import game data mappings
from game_data_mappings import (
    TERRAIN_NAME_TO_IDX, RESOURCE_NAME_TO_IDX,
    UNIT_NAME_TO_IDX, IMPROVEMENT_NAME_TO_IDX,
    TECH_NAME_TO_IDX, TRIBE_NAME_TO_IDX
)


class PolyterraEnv(AECEnv):
    """
    PettingZoo AEC environment for Polytopia with full observation/action spaces

    Observation space includes:
    - Global state (turn, current player)
    - Player state (currency, score, techs, units, cities)
    - Full map state (tiles, terrain, improvements, units, resources)
    - Diplomatic relations
    - Action masks for valid actions

    Action space supports:
    - 36 command types (Move, Attack, Build, Train, Research, etc.)
    - Parameterized actions with coordinates, unit IDs, types
    - Action masking for validity
    """

    metadata = {
        "render_modes": ["human", "ansi"],
        "name": "polyterra_v0",
        "is_parallelizable": False,
    }

    # Game constants from C# backend
    NUM_TERRAINS = 7  # None, Water, Ocean, Field, Mountain, Forest, Ice
    NUM_RESOURCES = 8  # None, Game, Crop, Fish, Whale, Metal, Fruit, Spores
    NUM_UNITS = 40  # 39 unit types + None
    NUM_TECHS = 38
    NUM_IMPROVEMENTS = 47  # 46 improvement types + None
    NUM_TRIBES = 17
    NUM_COMMAND_TYPES = 37  # 36 commands + EndTurn

    # Action type enum indices
    ACTION_END_TURN = 0
    ACTION_MOVE = 1
    ACTION_ATTACK = 2
    ACTION_BUILD = 3
    ACTION_TRAIN = 4
    ACTION_RESEARCH = 5
    ACTION_UPGRADE = 6
    ACTION_RECOVER = 7
    ACTION_HEAL_OTHERS = 8
    ACTION_PROMOTE = 9
    ACTION_EXAMINE_RUINS = 10
    ACTION_DISBAND = 11
    ACTION_DESTROY = 12
    ACTION_CAPTURE = 13
    ACTION_HARVEST = 14
    ACTION_CITY_REWARD = 15
    # ... more action types

    def __init__(
        self,
        num_players: int = 4,
        game_mode: str = "perfection",
        max_turns: int = 30,
        map_size: int = 16,
        render_mode: Optional[str] = None,
        dotnet_path: str = "dotnet",
        dll_path: Optional[str] = None,
        use_action_masking: bool = True,
    ):
        """
        Initialize Polytopia environment with comprehensive spaces

        Args:
            num_players: Number of players (2-4)
            game_mode: "perfection" or "domination"
            max_turns: Maximum turns before truncation
            map_size: Map dimension (typically 16)
            render_mode: "human" or "ansi"
            dotnet_path: Path to dotnet executable
            dll_path: Path to PolyterraTest.dll
            use_action_masking: Enable action masking in observation
        """
        super().__init__()

        self.num_players = num_players
        self.game_mode = game_mode
        self.max_turns = max_turns
        self.map_size = map_size
        self.render_mode = render_mode
        self.dotnet_path = dotnet_path
        self.use_action_masking = use_action_masking

        # Auto-detect DLL path if not provided
        if dll_path is None:
            import os
            base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            dll_path = os.path.join(
                base_dir,
                "csharp-backend/bin/Debug/net8.0/PolyterraBackend.dll"
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
        Comprehensive observation space including:
        - Global game state
        - Player-specific state (currency, score, techs)
        - Full map state (tiles with terrain, improvements, units)
        - Unit details (health, position, status)
        - City details (level, population, production)
        - Diplomatic relations
        - Action mask for valid actions
        """
        max_tiles = self.map_size * self.map_size
        max_units = 100  # Reasonable upper bound
        max_cities = 50  # Reasonable upper bound

        obs_space = {
            # ===== GLOBAL STATE =====
            "turn": spaces.Box(0, self.max_turns, shape=(), dtype=np.int32),
            "current_player_idx": spaces.Discrete(self.num_players),

            # ===== PLAYER STATE (current agent) =====
            "player_id": spaces.Discrete(self.num_players + 1),  # 1-indexed player IDs
            "currency": spaces.Box(0, 100000, shape=(), dtype=np.int32),
            "score": spaces.Box(0, 10000000, shape=(), dtype=np.int32),
            "tribe": spaces.Discrete(self.NUM_TRIBES),
            "num_cities": spaces.Box(0, 100, shape=(), dtype=np.int32),
            "num_kills": spaces.Box(0, 10000, shape=(), dtype=np.int32),
            "num_casualties": spaces.Box(0, 10000, shape=(), dtype=np.int32),

            # Available technologies (one-hot encoding)
            "available_techs": spaces.MultiBinary(self.NUM_TECHS),

            # ===== MAP STATE =====
            # Flattened tile array (map_size x map_size)
            "tiles": spaces.Sequence(
                spaces.Dict({
                    "x": spaces.Discrete(50),
                    "y": spaces.Discrete(50),
                    "terrain": spaces.Discrete(self.NUM_TERRAINS),
                    "owner": spaces.Discrete(self.num_players + 2),  # +1 neutral, +1 unknown
                    "visible": spaces.Discrete(2),
                    "explored": spaces.Discrete(2),
                    "has_road": spaces.Discrete(2),
                    "resource": spaces.Discrete(self.NUM_RESOURCES),

                    # Improvement details
                    "improvement_type": spaces.Discrete(self.NUM_IMPROVEMENTS),
                    "improvement_level": spaces.Box(0, 20, shape=(), dtype=np.int32),
                    "is_capital": spaces.Discrete(2),
                    "city_population": spaces.Box(0, 100, shape=(), dtype=np.int32),

                    # Unit details (if present)
                    "has_unit": spaces.Discrete(2),
                    "unit_type": spaces.Discrete(self.NUM_UNITS),
                    "unit_owner": spaces.Discrete(self.num_players + 2),  # +1 neutral, +1 unknown
                    "unit_health": spaces.Box(0, 100, shape=(), dtype=np.float32),
                    "unit_promotion": spaces.Discrete(4),
                    "unit_moved": spaces.Discrete(2),
                    "unit_attacked": spaces.Discrete(2),
                })
            ),

            # ===== UNITS (easier indexing) =====
            "units": spaces.Sequence(
                spaces.Dict({
                    "id": spaces.Box(0, 100000, shape=(), dtype=np.int32),
                    "type": spaces.Discrete(self.NUM_UNITS),
                    "owner": spaces.Discrete(self.num_players + 1),  # 1-indexed player IDs
                    "x": spaces.Discrete(50),
                    "y": spaces.Discrete(50),
                    "health": spaces.Box(0, 100, shape=(), dtype=np.float32),
                    "max_health": spaces.Box(0, 100, shape=(), dtype=np.float32),
                    "promotion": spaces.Discrete(4),
                    "moved": spaces.Discrete(2),
                    "attacked": spaces.Discrete(2),
                })
            ),

            # ===== CITIES (easier indexing) =====
            "cities": spaces.Sequence(
                spaces.Dict({
                    "x": spaces.Discrete(50),
                    "y": spaces.Discrete(50),
                    "owner": spaces.Discrete(self.num_players + 1),  # 1-indexed player IDs
                    "level": spaces.Box(0, 20, shape=(), dtype=np.int32),
                    "population": spaces.Box(0, 100, shape=(), dtype=np.int32),
                    "production": spaces.Box(0, 100, shape=(), dtype=np.int32),
                    "is_capital": spaces.Discrete(2),
                })
            ),

            # ===== OPPONENT INFO (partial observability) =====
            "opponents": spaces.Sequence(
                spaces.Dict({
                    "id": spaces.Discrete(self.num_players + 1),  # 1-indexed player IDs
                    "tribe": spaces.Discrete(self.NUM_TRIBES),
                    "score": spaces.Box(0, 10000000, shape=(), dtype=np.int32),
                    "num_cities": spaces.Box(0, 100, shape=(), dtype=np.int32),
                    "is_alive": spaces.Discrete(2),
                })
            ),
        }

        # Only include action mask if enabled
        if self.use_action_masking:
            obs_space["action_mask"] = spaces.Tuple((
                spaces.MultiBinary(self.NUM_COMMAND_TYPES),  # action_type
                spaces.MultiBinary(50),  # target_x
                spaces.MultiBinary(50),  # target_y
                spaces.MultiBinary(100),  # unit_id_idx
                spaces.MultiBinary(max(self.NUM_UNITS, self.NUM_IMPROVEMENTS, self.NUM_TECHS)),  # param1
                spaces.MultiBinary(10),  # param2
            ))

        return spaces.Dict(obs_space)

    @functools.lru_cache(maxsize=None)
    def action_space(self, agent):
        """
        Parameterized action space using MultiDiscrete

        Action structure:
        [action_type, target_x, target_y, unit_id_idx, param1, param2]

        - action_type: Command type (0-36)
        - target_x, target_y: Target coordinates (0 to map_size-1)
        - unit_id_idx: Index into visible units list (0-99)
        - param1: Context-dependent (unit_type, improvement_type, tech_type)
        - param2: Reserved for future use

        Action types:
        0  = END_TURN
        1  = MOVE (unit_id_idx, target_x, target_y)
        2  = ATTACK (unit_id_idx, target_x, target_y)
        3  = BUILD (target_x, target_y, improvement_type=param1)
        4  = TRAIN (target_x, target_y, unit_type=param1)
        5  = RESEARCH (tech_type=param1)
        6  = UPGRADE (target_x, target_y, unit_type=param1)
        7  = RECOVER (unit_id_idx)
        8  = HEAL_OTHERS (unit_id_idx, target_x, target_y)
        9  = PROMOTE (unit_id_idx)
        10 = EXAMINE_RUINS (target_x, target_y)
        11 = DISBAND (unit_id_idx)
        12 = DESTROY (target_x, target_y)
        13 = CAPTURE (target_x, target_y)
        ... (more action types)
        """
        return spaces.MultiDiscrete([
            self.NUM_COMMAND_TYPES,  # action_type
            50,  # target_x
            50,  # target_y
            100,  # unit_id_idx
            max(self.NUM_UNITS, self.NUM_IMPROVEMENTS, self.NUM_TECHS),  # param1
            10,  # param2 (reserved)
        ])

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

        # Store raw observations from C#
        self._raw_observations = response["observations"]

        # Parse into structured observations
        self._observations = {
            agent: self._parse_observation(self._raw_observations[agent])
            for agent in self.agents
        }

    def _parse_observation(self, raw_obs: dict) -> dict:
        """
        Parse raw JSON observation from C# into structured Gymnasium space format

        Args:
            raw_obs: Raw observation dict from C# backend

        Returns:
            Structured observation matching observation_space
        """
        # Extract basic info
        player_id = raw_obs.get("player_id", 0)

        # Parse available techs into one-hot array
        available_techs = np.zeros(self.NUM_TECHS, dtype=np.int8)
        for tech_name in raw_obs.get("available_techs", []):
            # Map tech name to index (TODO: create proper mapping)
            tech_idx = self._tech_name_to_idx(tech_name)
            if 0 <= tech_idx < self.NUM_TECHS:
                available_techs[tech_idx] = 1

        # Parse tiles
        tiles = []
        raw_tiles = raw_obs.get("tiles", [])
        for tile in raw_tiles:
            tiles.append({
                "x": tile.get("x", 0),
                "y": tile.get("y", 0),
                "terrain": self._terrain_name_to_idx(tile.get("terrain", "None")),
                "owner": tile.get("owner", self.num_players + 1),  # Unknown owner
                "visible": int(tile.get("visible", False)),
                "explored": int(tile.get("explored", False)),
                "has_road": int(tile.get("has_road", False)),
                "resource": self._resource_name_to_idx(tile.get("resource", "None")),

                # Improvement
                "improvement_type": self._improvement_name_to_idx(
                    tile.get("improvement", {}).get("type", "None")
                ),
                "improvement_level": tile.get("improvement", {}).get("level", 0),
                "is_capital": int(tile.get("improvement", {}).get("is_capital", False)),
                "city_population": tile.get("improvement", {}).get("population", 0),
                "city_production": tile.get("improvement", {}).get("production", 0),

                # Unit
                "has_unit": int(tile.get("unit") is not None),
                "unit_type": self._unit_name_to_idx(
                    tile.get("unit", {}).get("type", "None")
                ),
                "unit_owner": tile.get("unit", {}).get("owner", self.num_players),
                "unit_health": float(tile.get("unit", {}).get("health", 0)),
                "unit_promotion": tile.get("unit", {}).get("promotion", 0),
                "unit_moved": int(tile.get("unit", {}).get("moved", False)),
                "unit_attacked": int(tile.get("unit", {}).get("attacked", False)),
            })

        # Parse units (for easier access)
        units = []
        for tile in raw_tiles:
            unit = tile.get("unit")
            if unit and unit.get("owner") == player_id:
                units.append({
                    "id": unit.get("id", 0),
                    "type": self._unit_name_to_idx(unit.get("type", "None")),
                    "owner": unit.get("owner", 0),
                    "x": tile.get("x", 0),
                    "y": tile.get("y", 0),
                    "health": float(unit.get("health", 0)),
                    "max_health": float(unit.get("max_health", 10)),
                    "promotion": unit.get("promotion", 0),
                    "moved": int(unit.get("moved", False)),
                    "attacked": int(unit.get("attacked", False)),
                })

        # Parse cities
        cities = []
        for tile in raw_tiles:
            improvement = tile.get("improvement")
            if improvement and improvement.get("type") == "City":
                if tile.get("owner") == player_id:
                    cities.append({
                        "x": tile.get("x", 0),
                        "y": tile.get("y", 0),
                        "owner": tile.get("owner", 0),
                        "level": improvement.get("level", 0),
                        "population": improvement.get("population", 0),
                        "production": improvement.get("production", 0),
                        "is_capital": int(improvement.get("is_capital", False)),
                    })

        # Parse opponents (minimal info due to fog of war)
        opponents = []
        for i in range(self.num_players):
            if i != player_id:
                opponents.append({
                    "id": i,
                    "tribe": 0,  # Unknown initially
                    "score": 0,  # Hidden in fog of war
                    "num_cities": 0,  # Unknown
                    "is_alive": 1,  # Assume alive
                })

        # Construct structured observation
        obs = {
            "turn": raw_obs.get("turn", 0),
            "current_player_idx": raw_obs.get("current_player_idx", 0),
            "player_id": player_id,
            "currency": raw_obs.get("currency", 0),
            "score": raw_obs.get("score", 0),
            "tribe": self._tribe_name_to_idx(raw_obs.get("tribe", "Imperius")),
            "num_cities": raw_obs.get("cities", 0),
            "num_kills": raw_obs.get("kills", 0),
            "num_casualties": raw_obs.get("casualties", 0),  # TODO: add to C# backend
            "map_width": raw_obs.get("map_width", 16),
            "map_height": raw_obs.get("map_height", 16),
            "available_techs": available_techs,
            "tiles": tiles,
            "units": units,
            "cities": cities,
            "opponents": opponents,
            # Include valid actions so agent knows what it can do
            "valid_actions": raw_obs.get("valid_actions", {}),
        }

        # Only include action mask if enabled
        if self.use_action_masking:
            obs["action_mask"] = self._compute_action_mask(raw_obs)

        return obs

    def _compute_action_mask(self, raw_obs: dict) -> tuple:
        """
        Compute valid action mask based on game state using C# backend valid_actions

        Returns:
            Tuple of masks for each action dimension matching MultiDiscrete nvec
        """
        # Get valid actions from C# backend
        valid_actions = raw_obs.get('valid_actions', {})

        # Initialize masks - all zeros (invalid) by default for action_type
        # MultiDiscrete nvec = [NUM_COMMAND_TYPES, 50, 50, 100, max(units/improvements/techs), 10]
        action_type_mask = np.zeros(self.NUM_COMMAND_TYPES, dtype=np.int8)
        target_x_mask = np.ones(50, dtype=np.int8)  # Allow all coordinates by default
        target_y_mask = np.ones(50, dtype=np.int8)
        unit_idx_mask = np.ones(100, dtype=np.int8)
        param1_mask = np.ones(max(self.NUM_UNITS, self.NUM_IMPROVEMENTS, self.NUM_TECHS), dtype=np.int8)
        param2_mask = np.ones(10, dtype=np.int8)

        # Enable END_TURN if valid
        if valid_actions.get('can_end_turn', False):
            action_type_mask[0] = 1  # END_TURN is action type 0

        # Enable MOVE if there are valid moves
        if len(valid_actions.get('valid_moves', [])) > 0:
            action_type_mask[1] = 1  # MOVE is action type 1

        # Enable ATTACK if there are valid attacks
        if len(valid_actions.get('valid_attacks', [])) > 0:
            action_type_mask[2] = 1  # ATTACK is action type 2

        # Enable RESEARCH if there are valid techs
        if len(valid_actions.get('valid_research', [])) > 0:
            action_type_mask[5] = 1  # RESEARCH is action type 5

        # Enable BUILD if there are valid builds
        if len(valid_actions.get('valid_builds', [])) > 0:
            action_type_mask[3] = 1  # BUILD is action type 3

        # Enable TRAIN if there are valid trains
        if len(valid_actions.get('valid_trains', [])) > 0:
            action_type_mask[4] = 1  # TRAIN is action type 4

        # Enable CAPTURE if there are valid captures
        if len(valid_actions.get('valid_captures', [])) > 0:
            action_type_mask[13] = 1  # CAPTURE is action type 13

        # Enable HARVEST if there are valid harvests
        if len(valid_actions.get('valid_harvests', [])) > 0:
            action_type_mask[14] = 1  # HARVEST is action type 14

        # Enable CITY_REWARD if there are pending city level up rewards to choose
        if len(valid_actions.get('valid_city_rewards', [])) > 0:
            action_type_mask[15] = 1  # CITY_REWARD is action type 15

        # If no actions are valid, allow END_TURN as fallback
        if np.sum(action_type_mask) == 0:
            action_type_mask[0] = 1

        mask = (
            action_type_mask,
            target_x_mask,
            target_y_mask,
            unit_idx_mask,
            param1_mask,
            param2_mask,
        )
        return mask

    def step(self, action: np.ndarray):
        """
        Execute one step in the environment

        Args:
            action: MultiDiscrete action array [action_type, target_x, target_y, unit_idx, param1, param2]
        """
        # Handle already-terminated agents (or agents not in active list)
        agent = self.agent_selection
        if agent not in self.agents or self.terminations.get(agent, False) or self.truncations.get(agent, False):
            # Agent is dead - just advance to next agent without doing anything
            self._clear_rewards()
            # Find next live agent
            if self.agents:
                self.agent_selection = self.agents[0]
            return

        agent = self.agent_selection

        # Parse action
        action_type = int(action[0])
        target_x = int(action[1])
        target_y = int(action[2])
        unit_idx = int(action[3])
        param1 = int(action[4])
        param2 = int(action[5])

        # Convert to command for C# backend
        command = self._action_to_command(action_type, target_x, target_y, unit_idx, param1, param2)

        response = self._send_command(command)

        if not response.get("success"):
            # Invalid action - penalize but DON'T terminate agent
            # Terminating on invalid action would end episode too quickly during training
            self._clear_rewards()
            self.rewards[agent] = -1.0  # Small negative reward for invalid action
            self.infos[agent] = {"error": response.get("error"), "invalid_action": True}
            # The agent still has the turn - they need to pick a valid action or END_TURN
            # Don't change agent_selection - let them try again
            return

        # Action succeeded - update state
        else:
            # Update state from response
            self.agents = response["agents"]
            self._raw_observations = response["observations"]
            self._observations = {
                ag: self._parse_observation(self._raw_observations[ag])
                for ag in self.agents
            }

            # Update rewards - only for current agent
            new_rewards = response["rewards"]
            reward_delta = new_rewards[agent] - self._cumulative_rewards.get(agent, 0)
            self._cumulative_rewards[agent] = new_rewards[agent]

            # Set reward for current agent only
            self._clear_rewards()  # Clear all first
            self.rewards[agent] = reward_delta  # Set current agent's reward

            # Update terminations and truncations
            self.terminations = response["terminations"]
            self.truncations = response["truncations"]

            # Update agent selection from C# backend
            self.agent_selection = response["agent_selection"]

            # Update info
            for ag in self.agents:
                self.infos[ag] = response.get("info", {})

        # Note: We trust the C# backend's agent_selection completely.
        # Do NOT use _agent_selector here as it will cause double advancement.

    def _action_to_command(self, action_type: int, target_x: int, target_y: int,
                           unit_idx: int, param1: int, param2: int) -> dict:
        """
        Convert action array to C# command JSON

        Args:
            action_type: Command type index
            target_x, target_y: Target coordinates
            unit_idx: Unit index in units list
            param1, param2: Additional parameters

        Returns:
            Command dict for C# backend
        """
        # Get current agent's observation to access units list
        current_obs = self._observations.get(self.agent_selection, {})
        units = current_obs.get("units", [])

        # Get unit ID if needed
        unit_id = None
        if unit_idx < len(units):
            unit_id = units[unit_idx].get("id")

        # END_TURN - No parameters
        if action_type == self.ACTION_END_TURN:
            return {
                "command": "step",
                "action_type": "end_turn",
                "action_params": {}
            }

        # MOVE - Requires unit_id, from coordinates, to coordinates
        elif action_type == self.ACTION_MOVE:
            if unit_id is None:
                return self._invalid_action("Move requires valid unit")

            unit = units[unit_idx]
            return {
                "command": "step",
                "action_type": "move",
                "action_params": {
                    "unit_id": unit_id,
                    "from_x": unit.get("x"),
                    "from_y": unit.get("y"),
                    "to_x": target_x,
                    "to_y": target_y,
                }
            }

        # ATTACK - Requires unit_id, origin, target
        elif action_type == self.ACTION_ATTACK:
            if unit_id is None:
                return self._invalid_action("Attack requires valid unit")

            unit = units[unit_idx]
            return {
                "command": "step",
                "action_type": "attack",
                "action_params": {
                    "unit_id": unit_id,
                    "from_x": unit.get("x"),
                    "from_y": unit.get("y"),
                    "target_x": target_x,
                    "target_y": target_y,
                }
            }

        # BUILD - Requires improvement type, coordinates
        elif action_type == self.ACTION_BUILD:
            from game_data_mappings import IMPROVEMENT_IDX_TO_NAME
            improvement_name = IMPROVEMENT_IDX_TO_NAME.get(param1, "farm")

            return {
                "command": "step",
                "action_type": "build",
                "action_params": {
                    "improvement_type": improvement_name,
                    "x": target_x,
                    "y": target_y,
                }
            }

        # TRAIN - Requires unit type, coordinates (city location)
        elif action_type == self.ACTION_TRAIN:
            from game_data_mappings import UNIT_IDX_TO_NAME
            unit_type_name = UNIT_IDX_TO_NAME.get(param1, "warrior")

            return {
                "command": "step",
                "action_type": "train",
                "action_params": {
                    "unit_type": unit_type_name,
                    "city_x": target_x,
                    "city_y": target_y,
                }
            }

        # RESEARCH - Requires tech type
        elif action_type == self.ACTION_RESEARCH:
            from game_data_mappings import TECH_IDX_TO_NAME
            tech_name = TECH_IDX_TO_NAME.get(param1, "riding")

            return {
                "command": "step",
                "action_type": "research",
                "action_params": {
                    "tech_type": tech_name,
                }
            }

        # UPGRADE - Requires unit type, coordinates
        elif action_type == self.ACTION_UPGRADE:
            from game_data_mappings import UNIT_IDX_TO_NAME
            unit_type_name = UNIT_IDX_TO_NAME.get(param1, "knight")

            return {
                "command": "step",
                "action_type": "upgrade",
                "action_params": {
                    "unit_type": unit_type_name,
                    "x": target_x,
                    "y": target_y,
                }
            }

        # RECOVER - Heal own unit
        elif action_type == self.ACTION_RECOVER:
            if unit_id is None:
                return self._invalid_action("Recover requires valid unit")

            unit = units[unit_idx]
            return {
                "command": "step",
                "action_type": "recover",
                "action_params": {
                    "unit_id": unit_id,
                    "x": unit.get("x"),
                    "y": unit.get("y"),
                }
            }

        # HEAL_OTHERS - MindBender healing allies
        elif action_type == self.ACTION_HEAL_OTHERS:
            if unit_id is None:
                return self._invalid_action("Heal requires valid unit")

            return {
                "command": "step",
                "action_type": "heal_others",
                "action_params": {
                    "unit_id": unit_id,
                    "target_x": target_x,
                    "target_y": target_y,
                }
            }

        # PROMOTE - Promote unit
        elif action_type == self.ACTION_PROMOTE:
            if unit_id is None:
                return self._invalid_action("Promote requires valid unit")

            unit = units[unit_idx]
            return {
                "command": "step",
                "action_type": "promote",
                "action_params": {
                    "unit_id": unit_id,
                    "x": unit.get("x"),
                    "y": unit.get("y"),
                }
            }

        # EXAMINE_RUINS - Explore ruins
        elif action_type == self.ACTION_EXAMINE_RUINS:
            return {
                "command": "step",
                "action_type": "examine_ruins",
                "action_params": {
                    "x": target_x,
                    "y": target_y,
                }
            }

        # DISBAND - Remove own unit
        elif action_type == self.ACTION_DISBAND:
            if unit_id is None:
                return self._invalid_action("Disband requires valid unit")

            unit = units[unit_idx]
            return {
                "command": "step",
                "action_type": "disband",
                "action_params": {
                    "unit_id": unit_id,
                    "x": unit.get("x"),
                    "y": unit.get("y"),
                }
            }

        # DESTROY - Destroy improvement
        elif action_type == self.ACTION_DESTROY:
            return {
                "command": "step",
                "action_type": "destroy",
                "action_params": {
                    "x": target_x,
                    "y": target_y,
                }
            }

        # CAPTURE - Capture enemy city/village
        elif action_type == self.ACTION_CAPTURE:
            return {
                "command": "step",
                "action_type": "capture",
                "action_params": {
                    "target_x": target_x,
                    "target_y": target_y,
                    "unit_id": unit_id if unit_id is not None else 0,
                }
            }

        # HARVEST - Harvest resources (creates hidden improvements like hunting, fishing)
        elif action_type == self.ACTION_HARVEST:
            from game_data_mappings import IMPROVEMENT_IDX_TO_NAME
            improvement_name = IMPROVEMENT_IDX_TO_NAME.get(param1, "Hunting")
            return {
                "command": "step",
                "action_type": "harvest",
                "action_params": {
                    "x": target_x,
                    "y": target_y,
                    "improvement_type": improvement_name,
                }
            }

        # CITY_REWARD - Choose reward when city levels up
        elif action_type == self.ACTION_CITY_REWARD:
            # param1 = reward index (0 or 1 for the two choices at current level)
            # Look up the actual reward name from valid_city_rewards in RAW observation
            reward_name = "Workshop"  # Default fallback

            # Get current agent's RAW observation to find valid rewards
            # (valid_actions is in raw obs, not structured obs)
            if self.agent_selection:
                raw_obs = self._raw_observations.get(self.agent_selection, {})
                valid_actions = raw_obs.get('valid_actions', {})
                city_rewards = valid_actions.get('valid_city_rewards', [])

                # Find the city reward at target coordinates
                for cr in city_rewards:
                    if cr.get('x') == target_x and cr.get('y') == target_y:
                        rewards = cr.get('rewards', [])
                        print(f"[DEBUG] City reward at ({target_x},{target_y}): rewards={rewards}, param1={param1}")
                        if param1 < len(rewards):
                            reward_name = rewards[param1]
                            print(f"[DEBUG] Selected reward: {reward_name}")
                        break

            return {
                "command": "step",
                "action_type": "city_reward",
                "action_params": {
                    "x": target_x,
                    "y": target_y,
                    "reward": reward_name,
                }
            }

        # Unknown action - default to end turn
        else:
            return {
                "command": "step",
                "action_type": "end_turn",
                "action_params": {}
            }

    def _invalid_action(self, reason: str) -> dict:
        """Return end_turn command for invalid actions"""
        return {
            "command": "step",
            "action_type": "end_turn",
            "action_params": {},
            "error": reason
        }

    def observe(self, agent):
        """Get observation for specific agent"""
        return self._observations.get(agent, {})

    def render(self):
        """No-op render method for PettingZoo compatibility.

        For visualization, save game states during training and use a separate viewer.
        """
        pass

    def get_state_snapshot(self):
        """Get complete game state for logging/visualization.

        Returns:
            dict: Complete game state including all observations, rewards, and metadata
        """
        return {
            "turn": self._raw_observations.get(self.agent_selection, {}).get("turn", 0),
            "agent_selection": self.agent_selection,
            "agents": self.agents.copy(),
            "observations": self._raw_observations.copy(),
            "rewards": self.rewards.copy(),
            "cumulative_rewards": self._cumulative_rewards.copy(),
            "terminations": self.terminations.copy(),
            "truncations": self.truncations.copy(),
        }

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

        # Wait for startup
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

    # ===== NAME TO INDEX MAPPINGS =====
    # Using imported game data mappings

    def _terrain_name_to_idx(self, name: str) -> int:
        """Map terrain name to index"""
        return TERRAIN_NAME_TO_IDX.get(name.lower(), 0)

    def _resource_name_to_idx(self, name: str) -> int:
        """Map resource name to index"""
        if name == "None" or name is None:
            return 0
        return RESOURCE_NAME_TO_IDX.get(name.lower(), 0)

    def _unit_name_to_idx(self, name: str) -> int:
        """Map unit name to index"""
        if name == "None" or name is None:
            return 0
        return UNIT_NAME_TO_IDX.get(name.lower(), 0)

    def _improvement_name_to_idx(self, name: str) -> int:
        """Map improvement name to index"""
        if name == "None" or name is None:
            return 0
        return IMPROVEMENT_NAME_TO_IDX.get(name.lower(), 0)

    def _tech_name_to_idx(self, name: str) -> int:
        """Map tech name to index"""
        if name == "None" or name is None:
            return 0
        return TECH_NAME_TO_IDX.get(name.lower(), 0)

    def _tribe_name_to_idx(self, name: str) -> int:
        """Map tribe name to index"""
        return TRIBE_NAME_TO_IDX.get(name.lower(), 0)


def env(**kwargs):
    """Create environment with standard wrappers"""
    env = PolyterraEnv(**kwargs)
    env = wrappers.CaptureStdoutWrapper(env)
    env = wrappers.AssertOutOfBoundsWrapper(env)
    env = wrappers.OrderEnforcingWrapper(env)
    return env


__all__ = ["PolyterraEnv", "env"]
