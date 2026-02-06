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

# Step 1: Install uv if needed
echo ""
echo "=== Step 1: Python environment ==="

if ! command -v uv &> /dev/null; then
    echo "Installing uv..."
    curl -LsSf https://astral.sh/uv/install.sh | sh
fi

# Step 2: Build C# backend and copy to package
echo ""
echo "=== Step 2: Building C# backend ==="
python3 scripts/build_backend.py
echo "  Done."

# Step 3: Install Python dependencies
echo ""
echo "=== Step 3: Installing Python dependencies ==="
uv sync
echo "  Done."

echo ""
echo "=== Setup complete! ==="
echo ""
echo "To play the game:"
echo "  ./run_playable.sh"
echo ""
echo "To train with RLlib:"
echo "  source .venv/bin/activate"
echo "  cd training && python train_rllib.py"
