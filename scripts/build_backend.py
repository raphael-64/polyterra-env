#!/usr/bin/env python3
"""
Build the C# backend and copy it into the Python package.

Usage:
    python scripts/build_backend.py          # Debug build (faster, needs dotnet runtime)
    python scripts/build_backend.py --release # Release build
    python scripts/build_backend.py --self-contained  # Self-contained (no .NET SDK needed)
"""

import argparse
import platform
import shutil
import subprocess
import sys
from pathlib import Path


def detect_rid() -> str:
    """Detect the .NET Runtime Identifier for the current platform."""
    system = platform.system().lower()
    machine = platform.machine().lower()

    if system == "darwin":
        return "osx-arm64" if machine == "arm64" else "osx-x64"
    elif system == "linux":
        return "linux-arm64" if machine == "aarch64" else "linux-x64"
    elif system == "windows":
        return "win-x64"
    else:
        raise RuntimeError(f"Unsupported platform: {system}/{machine}")


def main():
    parser = argparse.ArgumentParser(description="Build C# backend for polyterra-env")
    parser.add_argument("--release", action="store_true", help="Build in Release configuration")
    parser.add_argument("--self-contained", action="store_true",
                        help="Self-contained build (no .NET SDK needed on target)")
    args = parser.parse_args()

    # Paths
    repo_root = Path(__file__).resolve().parent.parent
    csharp_dir = repo_root / "csharp-backend"
    backend_dest = repo_root / "src" / "polyterra_env" / "backend"

    if not csharp_dir.exists():
        print(f"ERROR: C# backend not found at {csharp_dir}", file=sys.stderr)
        sys.exit(1)

    config = "Release" if args.release else "Debug"

    # Build command
    cmd = ["dotnet", "build", "-c", config, "--nologo", "-v", "q"]
    print(f"Building C# backend ({config})...")
    subprocess.run(cmd, cwd=csharp_dir, check=True)

    # Source: build output
    build_output = csharp_dir / "bin" / config / "net8.0"

    if args.self_contained:
        rid = detect_rid()
        print(f"Publishing self-contained for {rid}...")
        pub_cmd = [
            "dotnet", "publish",
            "-c", config,
            "--self-contained",
            "-r", rid,
            "--nologo", "-v", "q",
        ]
        subprocess.run(pub_cmd, cwd=csharp_dir, check=True)
        build_output = csharp_dir / "bin" / config / "net8.0" / rid / "publish"

    # Verify DLL exists
    dll_path = build_output / "PolyterraBackend.dll"
    if not dll_path.exists():
        print(f"ERROR: Build succeeded but DLL not found at {dll_path}", file=sys.stderr)
        sys.exit(1)

    # Copy to backend/ directory
    print(f"Copying build output to {backend_dest}...")
    # Clean existing (except .gitkeep and .gitignore)
    for item in backend_dest.iterdir():
        if item.name in (".gitkeep", ".gitignore"):
            continue
        if item.is_dir():
            shutil.rmtree(item)
        else:
            item.unlink()

    # Copy all files from build output
    for item in build_output.iterdir():
        dest = backend_dest / item.name
        if item.is_dir():
            shutil.copytree(item, dest, dirs_exist_ok=True)
        else:
            shutil.copy2(item, dest)

    # Verify gamedata.json was copied
    gamedata = backend_dest / "gamedata.json"
    if not gamedata.exists():
        src_gamedata = csharp_dir / "gamedata.json"
        if src_gamedata.exists():
            shutil.copy2(src_gamedata, gamedata)
            print("Copied gamedata.json to backend/")
        else:
            print("WARNING: gamedata.json not found")

    print(f"Done! Backend files in {backend_dest}")


if __name__ == "__main__":
    main()
