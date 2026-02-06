#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

# Check if setup has been run
if [ ! -d ".venv" ]; then
    echo "Run ./setup.sh first!"
    exit 1
fi

# Check for backend DLL in bundled location or dev location
if [ ! -f "src/polyterra_env/backend/PolyterraBackend.dll" ] && \
   [ ! -f "csharp-backend/bin/Debug/net8.0/PolyterraBackend.dll" ]; then
    echo "C# backend not built. Run ./setup.sh first!"
    exit 1
fi

source .venv/bin/activate

echo "Starting game server on http://localhost:5001"
echo "Open web/polytopia_playable.html in your browser"
echo ""

cd web
python game_server.py
