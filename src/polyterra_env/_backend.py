"""
Locate the C# backend binary for PolyterraEnv.

Resolution order:
1. POLYTERRA_BACKEND_DIR env var (explicit override)
2. Bundled backend/ directory inside this package
3. Dev fallback: csharp-backend/bin/Debug/net8.0/ relative to repo root
"""

import os
from pathlib import Path


def find_backend_dll() -> str:
    """Find the PolyterraBackend.dll path.

    Returns:
        Absolute path to PolyterraBackend.dll

    Raises:
        FileNotFoundError: if the DLL cannot be found in any known location
    """
    # 1. Env var override
    env_dir = os.environ.get("POLYTERRA_BACKEND_DIR")
    if env_dir:
        dll = os.path.join(env_dir, "PolyterraBackend.dll")
        if os.path.isfile(dll):
            return dll
        raise FileNotFoundError(
            f"POLYTERRA_BACKEND_DIR={env_dir} set but PolyterraBackend.dll not found there"
        )

    # 2. Bundled backend/ directory (inside the installed package)
    package_dir = Path(__file__).resolve().parent
    bundled = package_dir / "backend" / "PolyterraBackend.dll"
    if bundled.is_file():
        return str(bundled)

    # 3. Dev fallback: repo root / csharp-backend / bin / Debug / net8.0
    #    Walk up from package dir to find repo root (has csharp-backend/)
    candidate = package_dir
    for _ in range(6):  # up to 6 levels
        candidate = candidate.parent
        dev_dll = candidate / "csharp-backend" / "bin" / "Debug" / "net8.0" / "PolyterraBackend.dll"
        if dev_dll.is_file():
            return str(dev_dll)

    raise FileNotFoundError(
        "Could not find PolyterraBackend.dll. Options:\n"
        "  1. Run ./setup.sh to build the C# backend\n"
        "  2. Set POLYTERRA_BACKEND_DIR to the directory containing the DLL\n"
        "  3. Install the package with the bundled backend (pip install polyterra-env)"
    )
