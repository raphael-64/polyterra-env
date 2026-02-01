# Polyterra Environment

A PettingZoo-compatible reinforcement learning environment for The Battle of Polytopia.

## Overview

This project provides a Python RL environment that communicates with a C# game engine via JSON subprocess. It supports multi-agent training with full game state observation and comprehensive action spaces.

## Project Structure

```
polyterra-entity-encoder/
├── polyterra_env/             # Python PettingZoo environment (package)
│   ├── __init__.py            # Package exports
│   ├── polyterra_env.py       # Main environment class
│   ├── game_data_mappings.py  # Game data index mappings
│   └── tests/                 # Test suite
├── training/                  # RL training scripts
│   ├── train_rllib.py         # RLlib self-play training
│   ├── train_entity_encoder.py # Entity encoder (attention-based) training
│   └── train_ppo.py           # Stable-Baselines3 training
├── csharp-backend/            # C# game engine bridge
│   ├── PolyterraEnvBridge.cs  # RL environment bridge
│   ├── Program.cs             # Server entry point
│   └── *.cs                   # Supporting classes
├── polytopia-game-logic/      # Decompiled game logic (dependencies)
│   ├── GameLogicAssembly/     # Core game logic
│   └── PolytopiaBackendBase/  # Backend helpers
├── pyproject.toml             # Python project config (uv/pip)
└── README.md                  # Documentation
```

## Installation

### Prerequisites

- Python 3.12+
- .NET 8.0 SDK
- [uv](https://github.com/astral-sh/uv) (recommended) or pip

### Setup

1. **Build C# backend (required first!):**
```bash
cd csharp-backend
dotnet build
```

Verify the DLL exists:
```bash
ls bin/Debug/net8.0/PolyterraBackend.dll
```

2. **Install Python dependencies:**

Using uv (recommended):
```bash
uv sync
```

Or using pip:
```bash
pip install -e .
```

3. **Verify installation:**
```bash
uv run python -c "from polyterra_env import PolyterraEnv; print('OK')"
```

### Troubleshooting

**BrokenPipeError**: The C# backend isn't running. Rebuild it:
```bash
cd csharp-backend && dotnet build
```

**ModuleNotFoundError**: Run `uv sync` to install dependencies.

## Usage

### Basic Example

```python
from polyterra_env import PolyterraEnv
import numpy as np

# Create environment
env = PolyterraEnv(
    num_players=4,
    game_mode="perfection",
    max_turns=30,
    render_mode="human"
)

# Reset environment
env.reset(seed=42)

# Game loop
for agent in env.agent_iter():
    obs = env.observe(agent)

    # Simple policy: end turn
    action = np.array([0, 0, 0, 0, 0, 0])  # END_TURN

    env.step(action)

    if env.terminations[agent] or env.truncations[agent]:
        break

env.close()
```

### Logging Game States for Visualization

The environment doesn't include rendering to keep training fast. Instead, save interesting game states during training:

```python
from polyterra_env import PolyterraEnv

env = PolyterraEnv(render_mode=None)  # No rendering overhead
env.reset(seed=42)

# During training, save interesting moments
for agent in env.agent_iter():
    obs = env.observe(agent)
    action = policy(obs)
    env.step(action)

    # Save state when something interesting happens
    if high_reward or novel_strategy:
        state = env.get_state_snapshot()
        save_to_file(state)  # For later visualization

env.close()
```

See `example_training_with_logging.py` for a complete example.

### Running Tests

```bash
cd polyterra-env-py/tests
python test_integration.py
python test_comprehensive_spaces.py
```

## Environment Details

### Observation Space

The observation is a Dict containing:

- **Global State**: turn, current_player_idx
- **Player State**: currency, score, tribe, cities, kills, technologies
- **Map State**: 256 tiles with terrain, improvements, units, visibility
- **Units**: List of own units with health, position, status
- **Cities**: List of own cities with level, population, production
- **Opponents**: Partial information (fog of war)
- **Action Mask**: Valid actions (10,000 possible)

### Action Space

MultiDiscrete space with 6 components:
```
[action_type, target_x, target_y, unit_id_idx, param1, param2]
```

Supports 37 command types:
- 0: END_TURN
- 1: MOVE
- 2: ATTACK
- 3: BUILD
- 4: TRAIN
- 5: RESEARCH
- 6: UPGRADE
- 7-36: RECOVER, HEAL, PROMOTE, DISBAND, DESTROY, etc.

### Game Modes

- **Perfection**: Score-based, 30 turns
- **Domination**: Last player standing

## Features

- Multi-agent support (2-4 players)
- Fog of war (partial observability)
- Full game state access
- Parameterized action space
- Comprehensive observation space
- Turn-based gameplay
- Compatible with RL libraries (Stable-Baselines3, RLlib)

## Architecture

The environment uses a client-server architecture:

1. **Python Environment** (`polyterra_env.py`) - PettingZoo interface
2. **C# Backend** (`PolyterraBackend`) - Game engine and logic
3. **JSON Protocol** - Communication via subprocess stdin/stdout

The C# backend handles:
- Game state management
- Action validation
- Map generation
- Game rules enforcement

The Python environment handles:
- RL interface (observation/action spaces)
- Agent coordination
- Reward calculation
- Episode management

## Development

### Adding New Actions

1. Add action type constant in `polyterra_env.py` (line 62-77)
2. Implement conversion in `_action_to_command()` (line 522+)
3. Update C# backend `HandleStep()` to process new command

### Modifying Observations

1. Update `observation_space()` definition (line 134+)
2. Modify `_parse_observation()` to extract new fields (line 319+)
3. Update C# backend `GetObservation()` to include new data

## License

This project uses decompiled game logic from The Battle of Polytopia for educational and research purposes. All game assets and logic remain property of Midjiwan AB.

## Credits

- Game: The Battle of Polytopia by Midjiwan AB
- Environment: PettingZoo framework
- Backend: .NET 8.0 with decompiled game logic
