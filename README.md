# Polyterra Environment

A PettingZoo-compatible multi-agent reinforcement learning environment for The Battle of Polytopia.

## Installation

```bash
pip install polyterra-env
```

That's it. The C# game engine is bundled — no .NET SDK or extra build steps required.

> **Note:** Currently only supports macOS ARM (Apple Silicon). Linux/x64 support coming soon.

## Quick Start

```python
from polyterra_env import PolyterraEnv
import numpy as np

env = PolyterraEnv(num_players=2)
env.reset(seed=42)

# Game loop
while env.agents:
    agent = env.agent_selection
    obs = env.observe(agent)

    # Pick a random valid action
    mask = obs["valid_actions_mask"]
    valid_indices = np.where(mask == 1)[0]
    action = int(np.random.choice(valid_indices))

    env.step(action)

    if all(env.terminations.get(a, False) for a in env.possible_agents):
        break

env.close()
```

## Observation Space

The observation is a `Dict` containing:

| Key | Description |
|-----|-------------|
| `turn` | Current game turn |
| `currency` | Stars (in-game currency) |
| `score` | Current score |
| `tiles` | Full map state — terrain, improvements, units, visibility |
| `units` | List of own units with type, health, position, status |
| `cities` | List of own cities with level, population, production |
| `opponents` | Partial info on other players (fog of war) |
| `available_techs` | One-hot vector of researchable technologies |
| `valid_actions_list` | Padded list of 512 action dicts |
| `valid_actions_mask` | Binary mask over the 512 action slots |

## Action Space

`Discrete(512)` — the agent picks an index into `valid_actions_list`.

Each valid action is a dict with a `type` field:

- `end_turn` — end the current turn
- `move` — move a unit
- `attack` — attack an enemy
- `build` — build an improvement (farm, mine, etc.)
- `train` — train a unit at a city
- `research` — research a technology
- `capture` — capture a village/city
- `harvest` — harvest a resource
- `city_reward` — choose a city level-up reward

Use `valid_actions_mask` for action masking during training.

## Training

For training scripts (PPO, RLlib self-play, replay recording), see [polyterra-training](https://github.com/raphael-64/polyterra-training).

## Web UI

A playable web interface is included for testing:

```bash
./run_playable.sh
# Then open web/polytopia_playable.html in your browser
```

## Game Modes

- **Perfection** — score-based, fixed number of turns (default 30)
- **Domination** — last player standing

## Development Setup

To modify the C# backend or Python environment:

```bash
git clone https://github.com/yourusername/polyterra-env.git
cd polyterra-env
./setup.sh   # Builds C# backend + installs Python deps
```

Requires .NET 8.0 SDK for development only. End users don't need it.

### Running Tests

```bash
pytest tests/
```

### Project Structure

```
polyterra-env/
├── src/polyterra_env/        # Python package
│   ├── env.py                # PettingZoo AEC environment
│   ├── game_data_mappings.py # Entity name/index mappings
│   ├── _backend.py           # C# binary discovery
│   └── backend/              # Bundled C# binary (gitignored)
├── csharp-backend/           # C# game engine source
├── polytopia-game-logic/     # Game logic (C# dependency)
├── web/                      # Playable web UI & game server
├── tests/                    # Test suite
└── scripts/build_backend.py  # Build script for C# backend
```

## License

This project uses decompiled game logic from The Battle of Polytopia for educational and research purposes. All game assets and logic remain property of Midjiwan AB.
