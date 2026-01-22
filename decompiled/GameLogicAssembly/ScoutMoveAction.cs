using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class ScoutMoveAction : ActionBase
{
	public enum ScoutBehaviourType
	{
		Default,
		GoToNearestVillage
	}

	private const int EXPLORER_SEARCH_DISTANCE = 4;

	public uint UnitId { get; protected set; }

	public uint RemainingMoves { get; protected set; }

	public uint Seed { get; protected set; }

	public WorldCoordinates From { get; protected set; }

	public List<WorldCoordinates> Path { get; protected set; }

	public WorldCoordinates To { get; protected set; }

	public ScoutBehaviourType ScoutBehaviour { get; protected set; }

	public ScoutMoveAction()
	{
	}

	public ScoutMoveAction(byte playerId, uint unitId, uint remainingMoves, uint seed, WorldCoordinates from, List<WorldCoordinates> path)
		: base(playerId)
	{
		UnitId = unitId;
		RemainingMoves = remainingMoves;
		Seed = seed;
		From = from;
		Path = path;
		ScoutBehaviour = ScoutBehaviourType.Default;
	}

	public ScoutMoveAction(byte playerId, uint unitId, uint remainingMoves, uint seed, WorldCoordinates from, List<WorldCoordinates> path, ScoutBehaviourType scoutBehaviourType)
		: base(playerId)
	{
		UnitId = unitId;
		RemainingMoves = remainingMoves;
		Seed = seed;
		From = from;
		Path = path;
		ScoutBehaviour = scoutBehaviourType;
	}

	public override void Execute(GameState gameState)
	{
		TileData tileData = null;
		if (ScoutBehaviour == ScoutBehaviourType.GoToNearestVillage)
		{
			tileData = GetNextTileToVillage(gameState, out var remainingMoves);
			RemainingMoves = remainingMoves;
		}
		if (tileData == null)
		{
			tileData = ((gameState.Version >= 60) ? GetNextTargetDefault(gameState, base.PlayerId, From, Path) : GetNextTargetV59(gameState, base.PlayerId, From, Path));
		}
		if (tileData != null)
		{
			To = tileData.coordinates;
			Path.Add(tileData.coordinates);
			if (RemainingMoves > 1)
			{
				gameState.ActionStack.Add(new ScoutMoveAction(base.PlayerId, UnitId, RemainingMoves - 1, gameState.RandomHash.GetHash(Seed), To, Path, ScoutBehaviour));
			}
			int sightRange = 1;
			gameState.TryGetPlayer(base.PlayerId, out var playerState);
			ActionUtils.ExploreFromTile(gameState, playerState, tileData, sightRange, shouldUseActions: true);
		}
		else
		{
			To = From;
			RemainingMoves = 0u;
		}
	}

	protected TileData GetNextTileToVillage(GameState gameState, out uint remainingMoves)
	{
		WorldCoordinates worldCoordinates = gameState.Map.ClosestCity(From, 0);
		remainingMoves = RemainingMoves;
		if (!worldCoordinates.IsValid(gameState.Map.Width, gameState.Map.Height))
		{
			return null;
		}
		gameState.TryGetPlayer(byte.MaxValue, out var playerState);
		gameState.GameLogicData.TryGetData(TerrainData.Type.Forest, out var data);
		gameState.GameLogicData.TryGetData(TerrainData.Type.Field, out var data2);
		PathFinderSettings settings = PathFinderSettings.CreateDefault(playerState, new TerrainData[2] { data, data2 }, gameState.Version, gameState);
		List<WorldCoordinates> path = gameState.Map.GetPath(From, worldCoordinates, 10, settings);
		if (path == null || path.Count == 0)
		{
			return null;
		}
		if (path.Count > 1)
		{
			return gameState.Map.GetTile(path[path.Count - 2]);
		}
		remainingMoves = 0u;
		return gameState.Map.GetTile(worldCoordinates);
	}

	private TileData GetNextTargetDefault(GameState state, byte playerId, WorldCoordinates coordinates, List<WorldCoordinates> path)
	{
		state.TryGetPlayer(playerId, out var playerState);
		TileData tile = state.Map.GetTile(coordinates);
		List<TileData> list = new List<TileData>();
		List<TileData> list2 = new List<TileData>();
		List<TileData> list3 = new List<TileData>();
		List<TileData> tileNeighbors = state.Map.GetTileNeighbors(tile.coordinates);
		int num = 4;
		foreach (TileData item in tileNeighbors)
		{
			List<TerrainData> unlockedMovements = state.GameLogicData.GetUnlockedMovements(playerState);
			if ((state.Version < 43 && !unlockedMovements.Contains(item.terrain)) || (state.Version >= 43 && !item.CanBeAccessedByPlayer(state, playerState)))
			{
				continue;
			}
			list.Add(item);
			if (!path.Contains(item.coordinates))
			{
				list2.Add(item);
			}
			int? num2 = item.DistanceToUnexploredTile(playerState, state, unlockedMovements, num);
			if (num2.HasValue)
			{
				if (num2 < num)
				{
					list3.Clear();
					num = num2.Value;
					list3.Add(item);
				}
				else if (num2 == num)
				{
					list3.Add(item);
				}
			}
		}
		List<TileData> list4 = list;
		if (list3.Count > 0)
		{
			list4 = list3;
		}
		else if (list2.Count > 0)
		{
			list4 = list2;
		}
		else if (list.Count > 0)
		{
			list.Sort(delegate(TileData a, TileData b)
			{
				int num3 = path.LastIndexOf(a.coordinates);
				int value = path.LastIndexOf(b.coordinates);
				return num3.CompareTo(value);
			});
			return list[0];
		}
		if (list4.Count > 0)
		{
			return list4[state.RandomHash.Range(0, list4.Count, (int)Seed)];
		}
		return null;
	}

	private TileData GetNextTargetV59(GameState state, byte playerId, WorldCoordinates coordinates, List<WorldCoordinates> path)
	{
		state.TryGetPlayer(playerId, out var playerState);
		TileData tile = state.Map.GetTile(coordinates);
		List<TileData> list = new List<TileData>();
		List<TileData> list2 = new List<TileData>();
		List<TileData> list3 = new List<TileData>();
		foreach (TileData tileNeighbor in state.Map.GetTileNeighbors(tile.coordinates))
		{
			if ((state.Version < 43 && !state.GameLogicData.GetUnlockedMovements(playerState).Contains(tileNeighbor.terrain)) || (state.Version >= 43 && !tileNeighbor.CanBeAccessedByPlayer(state, playerState)))
			{
				continue;
			}
			list.Add(tileNeighbor);
			if (!path.Contains(tileNeighbor.coordinates))
			{
				list2.Add(tileNeighbor);
				if (tileNeighbor.HasUnexploredNeighbours(playerState.Id, state.Map))
				{
					list3.Add(tileNeighbor);
				}
			}
		}
		List<TileData> list4 = list;
		if (list3.Count > 0)
		{
			list4 = list3;
		}
		else if (list2.Count > 0)
		{
			list4 = list2;
		}
		if (list4.Count > 0)
		{
			return list4[state.RandomHash.Range(0, list4.Count, (int)Seed)];
		}
		return null;
	}

	public override ActionType GetActionType()
	{
		return ActionType.ScoutMove;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(UnitId);
		writer.Write(RemainingMoves);
		writer.Write(Seed);
		From.Serialize(writer, version);
		int value = ((Path != null) ? Path.Count : 0);
		writer.Write(value);
		foreach (WorldCoordinates item in Path)
		{
			item.Serialize(writer, version);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		UnitId = reader.ReadUInt32();
		RemainingMoves = reader.ReadUInt32();
		Seed = reader.ReadUInt32();
		From = new WorldCoordinates(reader, version);
		int num = reader.ReadInt32();
		Path = new List<WorldCoordinates>(num);
		for (int i = 0; i < num; i++)
		{
			WorldCoordinates worldCoordinates = default(WorldCoordinates);
			worldCoordinates = new WorldCoordinates(reader, version);
			Path.Add(worldCoordinates);
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, From: {From})";
	}
}
