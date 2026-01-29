#!/bin/bash
# Quick test of the interactive game
python3 << 'PYTHON'
import sys
sys.path.insert(0, '/Users/raphael/Documents/GitHub/polyterra-env/polyterra-env-py')

from play_game import *
import numpy as np

print("Quick test of interactive game wrapper...")
print()

e = env(render_mode="human", use_action_masking=True)
e.reset()
e_unwrapped = e.unwrapped

agent = e_unwrapped.agent_selection
print_game_state(e_unwrapped, agent)

obs = e_unwrapped._raw_observations[agent]
va = print_valid_actions(obs)

print("\n✓ Interactive game wrapper working!")
print("\nTo play the full game, run: python play_game.py")
PYTHON
