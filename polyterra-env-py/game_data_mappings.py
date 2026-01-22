"""
Game data mappings for Polyterra environment.

This module provides bidirectional mappings between names and indices for game entities.
All mappings are extracted from gamedata_fixed.json.
"""

# Tribe mappings
TRIBE_NAME_TO_IDX = {
    "bardur": 4,
    "imperius": 7,
    "oumaji": 10,
    "xinxi": 13,
}

TRIBE_IDX_TO_NAME = {
    4: "bardur",
    7: "imperius",
    10: "oumaji",
    13: "xinxi",
}

# Unit mappings
UNIT_NAME_TO_IDX = {
    "scout": 1,
    "warrior": 2,
    "rider": 3,
    "archer": 4,
    "defender": 5,
    "swordsman": 6,
    "catapult": 7,
    "knight": 8,
    "mindbender": 9,
    "ship": 10,
    "battleship": 11,
    "boat": 13,
    "giant": 14,
}

UNIT_IDX_TO_NAME = {
    1: "scout",
    2: "warrior",
    3: "rider",
    4: "archer",
    5: "defender",
    6: "swordsman",
    7: "catapult",
    8: "knight",
    9: "mindbender",
    10: "ship",
    11: "battleship",
    13: "boat",
    14: "giant",
}

# Technology mappings
TECH_NAME_TO_IDX = {
    "basic": 0,
    "riding": 1,
    "freespirit": 2,
    "chivalry": 3,
    "roads": 4,
    "trade": 5,
    "organization": 6,
    "shields": 7,
    "farming": 8,
    "construction": 9,
    "fishing": 10,
    "whaling": 11,
    "aquatism": 12,
    "sailing": 13,
    "navigation": 14,
    "hunting": 15,
    "forestry": 16,
    "mathematics": 17,
    "archery": 18,
    "spiritualism": 19,
    "climbing": 20,
    "meditation": 21,
    "philosophy": 22,
    "mining": 23,
    "smithery": 24,
}

TECH_IDX_TO_NAME = {
    0: "basic",
    1: "riding",
    2: "freespirit",
    3: "chivalry",
    4: "roads",
    5: "trade",
    6: "organization",
    7: "shields",
    8: "farming",
    9: "construction",
    10: "fishing",
    11: "whaling",
    12: "aquatism",
    13: "sailing",
    14: "navigation",
    15: "hunting",
    16: "forestry",
    17: "mathematics",
    18: "archery",
    19: "spiritualism",
    20: "climbing",
    21: "meditation",
    22: "philosophy",
    23: "mining",
    24: "smithery",
}

# Improvement mappings
IMPROVEMENT_NAME_TO_IDX = {
    "none": 0,
    "city": 1,
    "ruin": 2,
    "road": 3,
    "customshouse": 4,
    "farm": 5,
    "windmill": 6,
    "fishing": 7,
    "port": 8,
    "hunting": 9,
    "clearforest": 10,
    "burnforest": 11,
    "lumberhut": 12,
    "sawmill": 13,
    "growforest": 14,
    "harvestfruit": 15,
    "whalehunting": 16,
    "temple": 17,
    "foresttemple": 18,
    "watertemple": 19,
    "mountaintemple": 20,
    "mine": 21,
    "forge": 22,
}

IMPROVEMENT_IDX_TO_NAME = {
    0: "none",
    1: "city",
    2: "ruin",
    3: "road",
    4: "customshouse",
    5: "farm",
    6: "windmill",
    7: "fishing",
    8: "port",
    9: "hunting",
    10: "clearforest",
    11: "burnforest",
    12: "lumberhut",
    13: "sawmill",
    14: "growforest",
    15: "harvestfruit",
    16: "whalehunting",
    17: "temple",
    18: "foresttemple",
    19: "watertemple",
    20: "mountaintemple",
    21: "mine",
    22: "forge",
}

# Terrain mappings
TERRAIN_NAME_TO_IDX = {
    "water": 1,
    "ocean": 2,
    "field": 3,
    "mountain": 4,
    "forest": 5,
}

TERRAIN_IDX_TO_NAME = {
    1: "water",
    2: "ocean",
    3: "field",
    4: "mountain",
    5: "forest",
}

# Resource mappings
RESOURCE_NAME_TO_IDX = {
    "fruit": 0,
    "game": 1,
    "fish": 2,
    "metal": 3,
}

RESOURCE_IDX_TO_NAME = {
    0: "fruit",
    1: "game",
    2: "fish",
    3: "metal",
}
