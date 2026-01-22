from setuptools import setup, find_packages

setup(
    name="polyterra-env",
    version="0.1.0",
    description="PettingZoo environment for Polytopia game",
    author="Polyterra Team",
    py_modules=["polyterra_env"],
    install_requires=[
        "pettingzoo>=1.24.0",
        "gymnasium>=0.29.0",
        "numpy>=1.21.0",
    ],
    python_requires=">=3.8",
)
