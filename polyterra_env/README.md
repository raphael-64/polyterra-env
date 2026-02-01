# Polyterra PettingZoo Environment

Multi-agent reinforcement learning environment for the Polytopia strategy game.

## Installation

```bash
# Install Python package
cd polyterra-env-py
pip install -e .
```

## Requirements

- Python 3.8+
- .NET 8.0 SDK
- PettingZoo
- Gymnasium
- NumPy

## Quick Start

```python
import polyterra_env

# Create environment
env = polyterra_env.env(
    num_players=4,
    game_mode="perfection",  # or "domination"
    max_turns=100,
    render_mode="human"
)

# Reset with seed for reproducibility
env.reset(seed=42)

# Run episode
for agent in env.agent_iter():
    obs, reward, termination, truncation, info = env.last()

    if termination or truncation:
        action = None
    else:
        # Your policy here
        action = env.action_space(agent).sample()

    env.step(action)

env.close()
```

## Environment Details

### Observation Space

Dictionary containing:
- `turn`: Current turn number
- `current_player_idx`: Index of active player
- `currency`: Stars (money)
- `score`: Victory points
- `tribe`: Tribe name
- `map_width`, `map_height`: Map dimensions
- `tiles`: List of tile data (terrain, owner, units, cities, etc.)

### Action Space

Currently simplified to:
- `0`: End turn

TODO: Expand to full action space (move, attack, build, research, etc.)

### Agents

- 4 players: `player_0`, `player_1`, `player_2`, `player_3`
- Turn-based (AEC API)
- Each agent acts sequentially

### Game Modes

- **Perfection**: 16x16 map, score-based, 30-turn limit
- **Domination**: Variable map (16x16 for 4 players), elimination-based

### Rewards

Currently: Delta in score between turns

TODO: Implement shaped rewards (territory, kills, tech, etc.)

## Testing

```bash
python test_env.py
```

## Architecture

```
Python (PettingZoo) <-> JSON <-> C# (Game Logic)
    polyterra_env.py         PolyterraEnvBridge.cs
```

The environment communicates with the C# game engine via JSON over stdin/stdout subprocess communication.

## Development

The C# game engine must be built before using the environment:

```bash
cd ../polyterra-test/PolyterraTest
dotnet build
```

The DLL path is auto-detected, but can be overridden:

```python
env = polyterra_env.env(
    dll_path="/path/to/PolyterraTest.dll"
)
```

## Next Steps

- [ ] Implement full action space (move, attack, build, etc.)
- [ ] Add action masking for invalid moves
- [ ] Implement reward shaping
- [ ] Add more observation features
- [ ] Create parallel API wrapper
- [ ] Add rendering modes (pygame, etc.)
- [ ] Benchmark performance
- [ ] Train baseline agents (PPO, DQN, etc.)

## License

See parent project for license information.
