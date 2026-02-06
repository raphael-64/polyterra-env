"""
Locate the C# backend binary for PolyterraEnv.

Resolution order:
1. POLYTERRA_BACKEND_DIR env var (explicit override)
2. Bundled backend/ directory inside this package
3. Dev fallback: csharp-backend/bin/Debug/net8.0/ relative to repo root

Returns (path, is_self_contained):
- Self-contained: path to native executable, run directly
- Not self-contained: path to .dll, run with `dotnet <path>`
"""

import os
import sys
from pathlib import Path


def find_backend() -> tuple[str, bool]:
    """Find the C# backend binary.

    Returns:
        Tuple of (path, is_self_contained)
        - is_self_contained=True: path is a native executable, run it directly
        - is_self_contained=False: path is a .dll, run with `dotnet <path>`

    Raises:
        FileNotFoundError: if the backend cannot be found in any known location
    """
    # Native exe name varies by platform
    exe_name = "PolyterraBackend.exe" if sys.platform == "win32" else "PolyterraBackend"

    # 1. Env var override
    env_dir = os.environ.get("POLYTERRA_BACKEND_DIR")
    if env_dir:
        exe = os.path.join(env_dir, exe_name)
        if os.path.isfile(exe):
            return exe, True
        dll = os.path.join(env_dir, "PolyterraBackend.dll")
        if os.path.isfile(dll):
            return dll, False
        raise FileNotFoundError(
            f"POLYTERRA_BACKEND_DIR={env_dir} set but no backend binary found there"
        )

    # 2. Bundled backend/ directory (inside the installed package)
    package_dir = Path(__file__).resolve().parent
    bundled_exe = package_dir / "backend" / exe_name
    if bundled_exe.is_file():
        return str(bundled_exe), True
    bundled_dll = package_dir / "backend" / "PolyterraBackend.dll"
    if bundled_dll.is_file():
        return str(bundled_dll), False

    # 3. Dev fallback: repo root / csharp-backend / bin / Debug / net8.0
    candidate = package_dir
    for _ in range(6):
        candidate = candidate.parent
        dev_dll = candidate / "csharp-backend" / "bin" / "Debug" / "net8.0" / "PolyterraBackend.dll"
        if dev_dll.is_file():
            return str(dev_dll), False

    raise FileNotFoundError(
        "Could not find PolyterraBackend. Options:\n"
        "  1. Run ./setup.sh to build the C# backend\n"
        "  2. Set POLYTERRA_BACKEND_DIR to the directory containing the binary\n"
        "  3. Install the package with the bundled backend (pip install polyterra-env)"
    )


# Backward compat
def find_backend_dll() -> str:
    """Find the backend DLL path (legacy wrapper)."""
    path, _ = find_backend()
    return path
