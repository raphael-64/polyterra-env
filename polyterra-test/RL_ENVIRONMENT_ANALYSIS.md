# Polytopia RL Environment Analysis

## 🎯 Map Generation - FULLY DETERMINISTIC ✅

**Yes, map generation is pseudorandom with a seed!** This is perfect for RL environments.

### How it works:
```csharp
// MapGenerator.cs line 100
private MapData GenerateInternal(int seed, GameState state, MapGeneratorSettings settings)
{
    Log.Verbose("Generating map with random seed {0}", new object[1] { seed });
    random = new Random(seed);  // ✅ Deterministic RNG initialized with seed
    // ... rest of generation
}
```

### For RL `reset(seed)`:
- **Same seed = Identical map every time**
- Map generation uses a single `Random` instance seeded at the start
- All terrain, resources, player positions are deterministically placed
- Perfect for reproducible training episodes

**Example:**
```csharp
gameRunner.InitializeGame(numPlayers: 4, gameMode: GameMode.Perfection, seed: 42);
// This will ALWAYS generate the exact same 16x16 map
```

---

## 📊 Observation Space - FULLY ACCESSIBLE

### Core State Structure

```csharp
GameState {
    // Global state
    int Seed                      // Random seed used
    uint CurrentTurn              // Current turn number
    byte CurrentPlayerIndex       // Index of current player (0-4)
    State CurrentState            // Lobby/Started/FinalTurn/Ended

    // Map (16x16 grid = 256 tiles)
    MapData Map {
        ushort Width, Height      // Map dimensions
        TileData[] Tiles          // Array of all tiles (256 for 16x16)
        WorldContinent[] Continents
    }

    // Players (5 total: Nature + 4 players)
    List<PlayerState> PlayerStates

    // Command/Action history
    List<CommandBase> CommandStack
    List<ActionBase> ActionStack
}
```

### Per-Tile Observation (TileData)

Each of the 256 tiles contains:
```csharp
TileData {
    // Position
    WorldCoordinates coordinates   // (x, y)

    // Terrain
    TerrainData.Type terrain       // Field/Forest/Mountain/Water/Ocean
    int altitude                   // -2 to +N
    int climate                    // Climate zone

    // Ownership
    byte owner                     // Player ID who owns this tile
    byte capitalOf                 // Is this a capital? (player ID)
    WorldCoordinates rulingCityCoordinates  // Which city rules this

    // Improvements
    ImprovementState improvement   // City/Farm/Mine/Temple/etc
    ResourceState resource         // Game/Crop/Fish/Metal/Fruit/Whale

    // Units
    UnitState unit                 // Unit present on tile (if any)

    // Fog of war
    List<byte> explorers           // Which players can see this tile

    // Infrastructure
    bool hasRoad                   // Road built
    bool hasRoute                  // Trade route
}
```

### Per-Player Observation (PlayerState)

```csharp
PlayerState {
    // Identity
    byte Id                        // Player ID (1-4, 255=Nature)
    string UserName
    TribeData.Type tribe           // Imperius/Bardur/Oumaji/Xinxi/etc

    // Resources
    int Currency                   // Stars (money)
    uint score                     // Victory points

    // Technology
    List<TechData.Type> availableTech  // Unlocked techs

    // Territory
    WorldCoordinates startTile     // Starting position
    int cities                     // Number of cities

    // Military
    uint kills                     // Units killed
    uint casualties                // Units lost
    uint wipeOuts                  // Players eliminated

    // Game state
    int wipedAtCommand             // -1 if alive, >=0 if eliminated
    int resignedAtCommandIndex     // -1 if playing, >=0 if resigned
    bool hasChosenTribe            // Ready status

    // AI (if AI player)
    AIState aiState
    bool AutoPlay                  // Is this AI controlled?

    // Diplomacy
    Dictionary<byte, DiplomacyRelation> relations
    List<byte> knownPlayers
    Dictionary<byte, int> aggressions
}
```

### Key Observation Helpers

```csharp
// Fog of war / visibility
PlayerMapData mapData = new PlayerMapData(gameState, player);
// Returns only tiles/units visible to that player

// Check what player can see
bool canSee = tile.explorers.Contains(playerId);

// Get all units for a player
var myUnits = gameState.Map.Tiles
    .Where(t => t.unit != null && t.unit.owner == playerId)
    .Select(t => t.unit);

// Get all cities for a player
var myCities = gameState.Map.Tiles
    .Where(t => t.improvement?.type == ImprovementData.Type.City
                && t.owner == playerId);
```

---

## 🎮 Action Space - 37 COMMAND TYPES

### CommandType Enum (All Available Actions)

```csharp
public enum CommandType : ushort {
    None = 0,

    // Unit Actions
    Move = 6,              // Move unit to new position
    Attack = 2,            // Attack enemy unit
    Capture = 7,           // Capture enemy city
    Recover = 3,           // Heal unit
    HealOthers = 4,        // Heal adjacent units
    Promote = 13,          // Upgrade veteran unit
    Disband = 10,          // Remove unit
    Stay = 21,             // Unit does nothing (stays)

    // City/Building Actions
    Build = 1,             // Build improvement (farm/mine/temple/etc)
    Train = 5,             // Train new unit in city
    CityReward = 11,       // Level up city (choose bonus)
    Destroy = 9,           // Destroy improvement

    // Technology
    Research = 8,          // Research new tech

    // Combat/Special
    ExamineRuins = 14,     // Explore ruins for rewards
    Upgrade = 16,          // Upgrade unit (warrior→swordsman)

    // Special Abilities (tribe-specific)
    FreezeArea = 17,       // Polaris: Freeze tiles
    BreakIce = 18,         // Break frozen tiles
    Harvest = 23,          // Cymanti: Harvest resources
    Explode = 24,          // Cymanti: Explode unit
    Boost = 25,            // Cymanti: Boost unit
    Decompose = 26,        // Cymanti: Decompose
    Clone = 31,            // Cymanti: Clone unit
    Hide = 33,             // Cymanti: Hide unit
    InfiltrateReward = 34, // Cymanti: Infiltrate

    // Diplomacy
    PeaceTreaty = 27,      // Offer peace
    PeaceRequestResponse = 28,  // Accept/reject peace
    BreakPeace = 29,       // Declare war
    EstablishEmbassy = 30, // Build embassy
    UpgradeEmbassy = 32,   // Upgrade embassy

    // Game Control
    EndTurn = 15,          // End player's turn
    StartMatch = 20,       // Start game (from lobby)
    EndMatch = 22,         // End game
    SelectTribe = 19,      // Choose tribe
    Resign = 35            // Forfeit game
}
```

### Command Structure

All commands extend `CommandBase`:

```csharp
public class CommandBase {
    byte PlayerId;         // Who is executing this command

    // Validation
    bool IsValid(GameState state, out string validationError);

    // Execution
    void Execute(GameState state);

    // Type identification
    CommandType GetCommandType();
}
```

### Example Commands with Parameters

```csharp
// Move unit
MoveCommand {
    byte PlayerId,
    WorldCoordinates from,
    WorldCoordinates to,
    List<WorldCoordinates> path  // Full path to follow
}

// Attack
AttackCommand {
    byte PlayerId,
    WorldCoordinates from,      // Attacker position
    WorldCoordinates to         // Target position
}

// Build improvement
BuildCommand {
    byte PlayerId,
    WorldCoordinates coordinates,
    ImprovementData.Type improvementType
}

// Train unit
TrainCommand {
    byte PlayerId,
    WorldCoordinates cityCoordinates,
    UnitData.Type unitType
}

// Research tech
ResearchCommand {
    byte PlayerId,
    TechData.Type techType
}

// Simple commands
EndTurnCommand { byte PlayerId }
ResignCommand { byte PlayerId }
```

### Action Validation

Every command has built-in validation:

```csharp
if (!command.IsValid(gameState, out string error)) {
    // Invalid action - can't be executed
    // error contains human-readable reason
    // e.g., "Not enough resources", "Can't attack", etc.
}
```

**Common validation errors:**
- `VALIDATION_ERROR_CANT_AFFORD` - Not enough stars
- `VALIDATION_ERROR_TILE_OCCUPIED` - Tile has unit
- `VALIDATION_ERROR_CANT_ATTACK` - Invalid attack
- `VALIDATION_ERROR_MISSING_UNIT` - No unit on tile
- etc. (See CommandBase.cs for full list)

---

## 🤖 AI Integration Points

### Current AI System

```csharp
// AI.cs - Built-in AI
public static CommandBase GetMove(GameState gameState,
                                   PlayerState player,
                                   CommandType specificCommand = CommandType.None)
{
    // Returns a valid command for the player to execute
    // Uses PlayerMapData for fog-of-war aware decisions
    // Implements missions/strategies
}
```

### For RL Agent Integration

You can replace `AI.GetMove()` with your RL policy:

```csharp
// Your RL agent
public class RLAgent {
    public CommandBase SelectAction(GameState state, PlayerState player) {
        // 1. Extract observation from state
        var obs = ExtractObservation(state, player);

        // 2. Run through neural network
        var action = policy.Predict(obs);

        // 3. Convert to CommandBase
        return ActionToCommand(action, state, player);
    }
}

// In game loop
if (player.AutoPlay) {
    // Use RL agent instead of built-in AI
    command = rlAgent.SelectAction(gameState, player);
} else {
    command = AI.GetMove(gameState, player);
}
```

---

## 🏋️ Recommended RL Environment Structure

```python
class PolyterraEnv(gym.Env):
    def __init__(self, num_players=4, game_mode="perfection"):
        self.observation_space = spaces.Dict({
            # Global
            "current_turn": spaces.Discrete(100),
            "current_player": spaces.Discrete(5),

            # Map (16x16 = 256 tiles)
            "terrain": spaces.MultiDiscrete([6] * 256),  # 6 terrain types
            "owner": spaces.MultiDiscrete([6] * 256),    # 5 players + neutral
            "units": spaces.MultiDiscrete([60] * 256),   # Unit types or None
            "improvements": spaces.MultiDiscrete([60] * 256),
            "resources": spaces.MultiDiscrete([8] * 256),

            # Player state (per player)
            "currency": spaces.Box(0, 10000, (5,)),
            "score": spaces.Box(0, 100000, (5,)),
            "tech_unlocked": spaces.MultiBinary((5, 25)),  # 25 techs
            # ... more features
        })

        self.action_space = spaces.Dict({
            "command_type": spaces.Discrete(37),  # CommandType enum
            "target_tile": spaces.Discrete(256),  # For move/build/attack
            "unit_type": spaces.Discrete(60),     # For train
            "tech_type": spaces.Discrete(25),     # For research
            # ... more parameters as needed
        })

    def reset(self, seed=None):
        # Initialize game with seed
        self.gameRunner.InitializeGame(
            numPlayers=self.num_players,
            gameMode=self.game_mode,
            seed=seed  # ✅ Deterministic!
        )
        self.gameRunner.StartMatch()
        return self._get_obs()

    def step(self, action):
        # Convert action to CommandBase
        command = self._action_to_command(action)

        # Validate and execute
        if not self.actionManager.ExecuteCommand(command, out error):
            # Invalid action - negative reward
            return self._get_obs(), -1.0, False, {"error": error}

        # Check if episode done
        done = self.gameState.CurrentState == GameState.State.Ended

        # Calculate reward
        reward = self._calculate_reward()

        return self._get_obs(), reward, done, {}
```

---

## 🎯 Key Takeaways for RL

### ✅ Strengths
1. **Deterministic seeding** - Perfect for reproducibility
2. **Complete state access** - No hidden information (except fog of war)
3. **Validated actions** - Built-in legality checking
4. **Structured commands** - Clear action space
5. **Turn-based** - No real-time complexity
6. **Existing AI** - Baseline to compare against

### ⚠️ Considerations
1. **Large observation space** - 256 tiles × many features
2. **Complex action space** - 37 command types with varying parameters
3. **Fog of war** - Partial observability per player
4. **Long episodes** - Games can be 30-100+ turns
5. **Multi-agent** - 4 players interacting

### 🔧 Recommended Approach
1. **Start simple**: Single-player vs AI on small maps
2. **Hierarchical actions**: High-level (research/expand/attack) → Low-level (specific commands)
3. **Curriculum learning**: Easy scenarios → Complex 4-player games
4. **Reward shaping**: Not just win/loss - territory, score, kills, etc.
5. **Imitation learning**: Learn from built-in AI first

---

## 📝 Next Steps

1. **Create C# RL Wrapper** - Expose GameState as observation, accept actions
2. **Python Bindings** - Use pythonnet or gRPC to connect Python RL frameworks
3. **Gym Environment** - Implement standard OpenAI Gym interface
4. **Baseline Agent** - Train simple PPO/DQN on single-player scenarios
5. **Multi-agent** - Scale to competitive 4-player training

Let me know which direction you want to go first! 🚀
