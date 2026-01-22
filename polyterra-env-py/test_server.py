"""Test C# server directly"""
import subprocess
import json

# Start process
proc = subprocess.Popen(
    ["dotnet", "run", "--env-server"],
    cwd="/Users/raphael/Documents/GitHub/polyterra-env/polyterra-test/PolyterraTest",
    stdin=subprocess.PIPE,
    stdout=subprocess.PIPE,
    stderr=subprocess.PIPE,
    text=True,
    bufsize=1
)

import time
time.sleep(0.5)

# Reset
cmd = {"command": "reset", "seed": 42, "num_players": 4, "game_mode": "perfection"}
proc.stdin.write(json.dumps(cmd) + "\n")
proc.stdin.flush()
resp = json.loads(proc.stdout.readline())
print(f"Reset: agent_selection={resp.get('agent_selection')}")

# Step 8 times
for i in range(8):
    cmd = {"command": "step", "action_type": "end_turn"}
    proc.stdin.write(json.dumps(cmd) + "\n")
    proc.stdin.flush()
    resp = json.loads(proc.stdout.readline())
    info = resp.get('info', {})
    print(f"Step {i+1}: agent_selection={resp.get('agent_selection')}, "
          f"idx={info.get('debug_current_player_index')}, id={info.get('debug_current_player_id')}, "
          f"stack={info.get('debug_action_stack_count')}, processing={info.get('debug_is_processing')}")

proc.terminate()
