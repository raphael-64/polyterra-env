"""Test to see debug output"""
import subprocess
import json
import time

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

time.sleep(0.5)

# Reset
cmd = {"command": "reset", "seed": 42, "num_players": 4, "game_mode": "perfection"}
proc.stdin.write(json.dumps(cmd) + "\n")
proc.stdin.flush()

# Read response
resp = json.loads(proc.stdout.readline())

# Read stderr to get debug output
import threading
def read_stderr():
    for line in iter(proc.stderr.readline, ''):
        if line:
            print(f"STDERR: {line.rstrip()}")

stderr_thread = threading.Thread(target=read_stderr, daemon=True)
stderr_thread.start()

time.sleep(1)

proc.terminate()
