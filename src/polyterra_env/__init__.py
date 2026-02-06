"""Polyterra: A PettingZoo environment for Polytopia-style games."""

from polyterra_env.env import PolyterraEnv, env
from polyterra_env.game_data_mappings import (
    TERRAIN_NAME_TO_IDX, TERRAIN_IDX_TO_NAME,
    RESOURCE_NAME_TO_IDX, RESOURCE_IDX_TO_NAME,
    UNIT_NAME_TO_IDX, UNIT_IDX_TO_NAME,
    IMPROVEMENT_NAME_TO_IDX, IMPROVEMENT_IDX_TO_NAME,
    TECH_NAME_TO_IDX, TECH_IDX_TO_NAME,
    TRIBE_NAME_TO_IDX, TRIBE_IDX_TO_NAME,
)

__all__ = [
    "PolyterraEnv",
    "env",
    "TERRAIN_NAME_TO_IDX", "TERRAIN_IDX_TO_NAME",
    "RESOURCE_NAME_TO_IDX", "RESOURCE_IDX_TO_NAME",
    "UNIT_NAME_TO_IDX", "UNIT_IDX_TO_NAME",
    "IMPROVEMENT_NAME_TO_IDX", "IMPROVEMENT_IDX_TO_NAME",
    "TECH_NAME_TO_IDX", "TECH_IDX_TO_NAME",
    "TRIBE_NAME_TO_IDX", "TRIBE_IDX_TO_NAME",
]
