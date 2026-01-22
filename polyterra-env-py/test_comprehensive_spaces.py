"""
Test comprehensive observation and action spaces in PolyterraEnvV2

This demonstrates the full game state available to RL agents and the
parameterized action space supporting all game actions.
"""

from polyterra_env_v2 import PolyterraEnvV2
import numpy as np

def print_observation_summary(obs, agent_name):
    """Print a summary of the observation structure"""
    print(f"\n{'='*70}")
    print(f"OBSERVATION SUMMARY FOR {agent_name.upper()}")
    print('='*70)

    # Global state
    print(f"\n[GLOBAL STATE]")
    print(f"  Turn: {obs.get('turn')}")
    print(f"  Current Player Index: {obs.get('current_player_idx')}")

    # Player state
    print(f"\n[PLAYER STATE]")
    print(f"  Player ID: {obs.get('player_id')}")
    print(f"  Tribe: {obs.get('tribe')}")
    print(f"  Currency: {obs.get('currency')} stars")
    print(f"  Score: {obs.get('score')} points")
    print(f"  Cities: {obs.get('num_cities')}")
    print(f"  Kills: {obs.get('num_kills')}")
    print(f"  Casualties: {obs.get('num_casualties')}")

    # Available techs
    available_techs = obs.get('available_techs', np.array([]))
    num_techs = np.sum(available_techs)
    print(f"  Available Technologies: {num_techs} researched")

    # Map state
    tiles = obs.get('tiles', [])
    visible_tiles = [t for t in tiles if t.get('visible')]
    print(f"\n[MAP STATE]")
    print(f"  Total Tiles: {len(tiles)}")
    print(f"  Visible Tiles: {len(visible_tiles)}")

    # Terrain breakdown
    terrain_counts = {}
    for tile in visible_tiles:
        terrain = tile.get('terrain', 0)
        terrain_counts[terrain] = terrain_counts.get(terrain, 0) + 1
    print(f"  Terrain Distribution: {terrain_counts}")

    # Improvements
    improvements = [t for t in visible_tiles if t.get('improvement_type', 0) > 0]
    print(f"  Improvements Visible: {len(improvements)}")

    # Units
    units = obs.get('units', [])
    print(f"\n[UNITS]")
    print(f"  Own Units: {len(units)}")

    if units:
        print(f"  Unit Details:")
        for i, unit in enumerate(units[:5]):  # Show first 5 units
            unit_type = unit.get('type')
            health = unit.get('health')
            max_health = unit.get('max_health')
            x, y = unit.get('x'), unit.get('y')
            moved = unit.get('moved')
            attacked = unit.get('attacked')
            print(f"    Unit {i}: Type={unit_type}, HP={health}/{max_health}, "
                  f"Pos=({x},{y}), Moved={moved}, Attacked={attacked}")
        if len(units) > 5:
            print(f"    ... and {len(units) - 5} more units")

    # Cities
    cities = obs.get('cities', [])
    print(f"\n[CITIES]")
    print(f"  Own Cities: {len(cities)}")

    if cities:
        print(f"  City Details:")
        for city in cities:
            x, y = city.get('x'), city.get('y')
            level = city.get('level')
            pop = city.get('population')
            prod = city.get('production')
            is_cap = city.get('is_capital')
            cap_str = " (CAPITAL)" if is_cap else ""
            print(f"    City at ({x},{y}): Level {level}, Pop {pop}, "
                  f"Production {prod}{cap_str}")

    # Opponents
    opponents = obs.get('opponents', [])
    print(f"\n[OPPONENTS]")
    print(f"  Known Opponents: {len(opponents)}")
    for opp in opponents:
        opp_id = opp.get('id')
        tribe = opp.get('tribe')
        score = opp.get('score')
        cities_count = opp.get('num_cities')
        alive = opp.get('is_alive')
        status = "Alive" if alive else "Eliminated"
        print(f"    Player {opp_id}: Tribe={tribe}, Score={score}, "
              f"Cities={cities_count}, Status={status}")

    # Action mask
    action_mask = obs.get('action_mask', np.array([]))
    if len(action_mask) > 1:
        valid_actions = np.sum(action_mask)
        print(f"\n[ACTION MASK]")
        print(f"  Valid Actions: {valid_actions}/{len(action_mask)}")

    print('='*70)


def demonstrate_action_space():
    """Demonstrate how to construct actions"""
    print(f"\n{'='*70}")
    print("ACTION SPACE STRUCTURE")
    print('='*70)
    print("""
The action space is a MultiDiscrete with 6 components:
[action_type, target_x, target_y, unit_id_idx, param1, param2]

Action types:
  0  = END_TURN (no params)
  1  = MOVE (unit_id_idx, target_x, target_y)
  2  = ATTACK (unit_id_idx, target_x, target_y)
  3  = BUILD (target_x, target_y, improvement_type=param1)
  4  = TRAIN (target_x, target_y, unit_type=param1)
  5  = RESEARCH (tech_type=param1)
  6  = UPGRADE (target_x, target_y, unit_type=param1)
  7  = RECOVER (unit_id_idx)
  8  = HEAL_OTHERS (unit_id_idx, target_x, target_y)
  9  = PROMOTE (unit_id_idx)
  10 = EXAMINE_RUINS (target_x, target_y)
  11 = DISBAND (unit_id_idx)
  12 = DESTROY (target_x, target_y)
  13 = CAPTURE (target_x, target_y)
  ... and more

Example actions:
""")

    # Example actions
    examples = [
        ("END_TURN", np.array([0, 0, 0, 0, 0, 0])),
        ("MOVE unit 0 to (5, 7)", np.array([1, 5, 7, 0, 0, 0])),
        ("ATTACK with unit 1 at (8, 3)", np.array([2, 8, 3, 1, 0, 0])),
        ("BUILD farm at (10, 12)", np.array([3, 10, 12, 0, 5, 0])),  # 5 = farm idx
        ("TRAIN warrior at (4, 6)", np.array([4, 4, 6, 0, 2, 0])),  # 2 = warrior idx
        ("RESEARCH riding", np.array([5, 0, 0, 0, 1, 0])),  # 1 = riding idx
    ]

    for desc, action in examples:
        print(f"  {desc:30s} -> {list(action)}")

    print('='*70)


def main():
    """Main test function"""
    print("\n" + "="*70)
    print("POLYTERRA ENV V2 - COMPREHENSIVE OBSERVATION & ACTION SPACES")
    print("="*70)

    # Create environment
    print("\nInitializing PolyterraEnvV2...")
    env = PolyterraEnvV2(
        num_players=4,
        game_mode="perfection",
        max_turns=30,
        render_mode=None,  # Disable rendering for now
        use_action_masking=True
    )

    # Reset environment
    print("Resetting environment...")
    env.reset(seed=42)

    # Display observation space structure
    print("\n" + "="*70)
    print("OBSERVATION SPACE STRUCTURE")
    print("="*70)
    print("""
The observation space is a Dict containing:

Global State:
  - turn: Current game turn (0 to max_turns)
  - current_player_idx: Active player index (0-3)

Player State (current agent):
  - player_id, currency, score, tribe
  - num_cities, num_kills, num_casualties
  - available_techs: One-hot array of researched technologies

Map State:
  - tiles: List of all map tiles with:
    * Position (x, y)
    * Terrain type, owner, visibility, resources
    * Improvement details (type, level, city info)
    * Unit details (type, health, owner, status)

Units: List of own units with:
  - id, type, owner, position
  - health, promotion level
  - action status (moved, attacked)

Cities: List of own cities with:
  - position, owner, level
  - population, production
  - capital status

Opponents: Partial info about other players:
  - id, tribe, score, cities, alive status

Action Mask: Binary mask of valid actions (10000 actions)
""")

    # Get and display observations for each player
    for agent_name in env.agents:
        obs = env.observe(agent_name)
        print_observation_summary(obs, agent_name)

    # Demonstrate action space
    demonstrate_action_space()

    # Run a few turns with END_TURN actions
    print("\n" + "="*70)
    print("RUNNING 3 TURNS WITH END_TURN ACTIONS")
    print("="*70)

    for turn in range(3):
        for player_idx in range(4):
            agent = env.agent_selection
            obs = env.observe(agent)

            # Take END_TURN action
            action = np.array([0, 0, 0, 0, 0, 0])  # END_TURN

            print(f"\nTurn {obs.get('turn')}, {agent}: Taking END_TURN action")
            env.step(action)

            # Check if game ended
            if env.terminations[agent] or env.truncations[agent]:
                print(f"  Game ended for {agent}")
                break

    # Final observation
    print("\n" + "="*70)
    print("FINAL OBSERVATION AFTER 3 TURNS")
    print("="*70)

    agent = env.agent_selection
    obs = env.observe(agent)
    print_observation_summary(obs, agent)

    # Cleanup
    env.close()

    print("\n" + "="*70)
    print("TEST COMPLETE!")
    print("="*70)
    print("""
Summary:
- Created PolyterraEnvV2 with comprehensive observation and action spaces
- Observation includes full game state: tiles, units, cities, opponents, techs
- Action space supports 37 command types with parameterized actions
- Successfully ran multiple turns with action execution
- Environment provides rich context for RL agent training

Next steps:
1. Implement action masking for valid actions only
2. Add reward shaping based on game state changes
3. Create RL agent training loop
4. Test with various game scenarios
""")


if __name__ == "__main__":
    main()
