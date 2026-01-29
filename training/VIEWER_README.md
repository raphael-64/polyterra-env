# Polyterra Replay Viewer

## Quick Start

1. Record a game:
```bash
cd training
uv run python record_game.py --steps 100 --output my_game.json
```

2. Open `replay_viewer.html` in your browser

3. Click "Load Replay File" and select the JSON file from `replays/` directory

4. Use the controls to step through the game:
   - **First/Last**: Jump to start/end
   - **Previous/Next**: Step through turns
   - **Arrow keys**: Left/Right to navigate

## Recording Games

### Random agent:
```bash
uv run python record_game.py --steps 100 --output random_game.json
```

### Trained model:
```bash
uv run python record_game.py --model polyterra_ppo.zip --steps 100 --output trained_game.json
```

### During training:
Integrate `ReplayRecorder` into your training loop to automatically save games.

## What You'll See

- **Game View**: Units and cities with their positions and stats
- **Stats Panel**: Turn, currency, score, unit count, etc.
- **Action Display**: What action was taken and why
- **Reward Display**: Rewards received (when non-zero)

## Files

- `replay_viewer.html` - Web-based viewer (open in browser)
- `replay_system.py` - Recording and analysis tools
- `record_game.py` - Record single games
- `replays/` - Saved game files

## Tips

- Games are saved as JSON (not compressed by default)
- The viewer works offline - no server needed
- Use keyboard arrows for quick navigation
- Units show "Moved" (orange) and "Attacked" (red) status
