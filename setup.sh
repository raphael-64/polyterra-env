#!/bin/bash
set -e

echo "=== Polyterra Environment Setup ==="
echo ""

# Check prerequisites
echo "Checking prerequisites..."

if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found. Install from https://dotnet.microsoft.com/download"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "  dotnet: $DOTNET_VERSION"

if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python 3 not found."
    exit 1
fi

PYTHON_VERSION=$(python3 --version)
echo "  python: $PYTHON_VERSION"

# Get script directory (works even if called from elsewhere)
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

# Step 1: Create venv and install Python deps
echo ""
echo "=== Step 1: Python environment ==="
if [ ! -d ".venv" ]; then
    echo "Creating virtual environment..."
    python3 -m venv .venv
fi

source .venv/bin/activate
echo "Installing Python dependencies..."
pip install -q pettingzoo gymnasium numpy flask flask-cors wandb sb3-contrib tensorboard ray
pip install -q -e polyterra-env-py/
echo "  Done."

# Step 2: Build C# backend
echo ""
echo "=== Step 2: Building C# backend ==="
cd "$SCRIPT_DIR/csharp-backend"
dotnet build -c Debug --nologo -v q
echo "  Done."

# Verify DLL exists
DLL_PATH="$SCRIPT_DIR/csharp-backend/bin/Debug/net8.0/PolyterraBackend.dll"
if [ ! -f "$DLL_PATH" ]; then
    echo "ERROR: Build succeeded but DLL not found at $DLL_PATH"
    exit 1
fi

# Verify gamedata.json exists next to DLL
if [ ! -f "$SCRIPT_DIR/csharp-backend/bin/Debug/net8.0/gamedata.json" ]; then
    echo "WARNING: gamedata.json not found in build output, copying..."
    cp "$SCRIPT_DIR/csharp-backend/gamedata.json" "$SCRIPT_DIR/csharp-backend/bin/Debug/net8.0/"
fi

echo ""
echo "=== Setup complete! ==="
echo ""
echo "To play the game:"
echo "  ./run_playable.sh"
echo ""
echo "To train with RLlib:"
echo "  source .venv/bin/activate"
echo "  cd training && python train_rllib.py"
