using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class TileData
{
	public WorldCoordinates coordinates;

	public TerrainData.Type terrain = TerrainData.Type.Water;

	public int climate;

	public SkinType skinType;

	public int altitude = -1;

	public byte owner;

	public byte capitalOf;

	public List<byte> explorers;

	public WorldCoordinates rulingCityCoordinates = WorldCoordinates.NULL_COORDINATES;

	public ImprovementState improvement;

	public ResourceState resource;

	public UnitState unit;

	private bool hasRoad;

	public bool hasRoute;

	public WorldContinent continent;

	public bool hadRoute;

	public bool IsWater
	{
		get
		{
			if (terrain != TerrainData.Type.Water)
			{
				return terrain == TerrainData.Type.Ocean;
			}
			return true;
		}
	}

	public bool HasRoad
	{
		get
		{
			if (!hasRoad)
			{
				return HasImprovement(ImprovementData.Type.City);
			}
			return true;
		}
		set
		{
			hasRoad = value;
		}
	}

	public bool IsConnected
	{
		get
		{
			if (HasImprovement(ImprovementData.Type.City))
			{
				bool num = improvement.connectedToCapitalOfPlayer == owner && owner != 0;
				bool flag = capitalOf == owner && owner != 0;
				return num || flag;
			}
			Log.Warning("Is connected is only intended to be used on city tiles", Array.Empty<object>());
			return false;
		}
	}

	public bool IsFreezable(GameState gameState, PlayerState player, bool onlyOwnedTiles = false)
	{
		if (onlyOwnedTiles && owner != player.Id)
		{
			return false;
		}
		if (!GetExplored(player.Id))
		{
			return false;
		}
		UnitState unitState = GetUnit(gameState, player.Id);
		if (gameState.Version < 80)
		{
			unitState = unit;
			if (gameState.Version >= 10 && IsWater && unitState != null)
			{
				return false;
			}
			if (IsWater && unitState == null)
			{
				return true;
			}
		}
		else
		{
			unitState = GetUnit(gameState, player.Id);
			if (IsWater)
			{
				return true;
			}
		}
		if ((owner == 0 || owner == player.Id) && climate != player.GetTribeClimate(gameState))
		{
			return true;
		}
		if (unitState != null && unitState.IsFreezable(gameState, player))
		{
			return true;
		}
		return false;
	}

	public bool IsFrozen(GameState state)
	{
		if (terrain == TerrainData.Type.Ice)
		{
			return true;
		}
		if (state.Version > 14 && climate == 15 && !IsWater)
		{
			return true;
		}
		return false;
	}

	public bool IsAlienClimate(GameState gameState)
	{
		return gameState.GameLogicData.IsAlienClimate(climate);
	}

	public bool CanBreakIce()
	{
		if (terrain == TerrainData.Type.Ice && unit == null)
		{
			return true;
		}
		return false;
	}

	public bool CanDestroy(GameState gameState, PlayerState playerState)
	{
		if (gameState.Version <= 17)
		{
			return gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Destroy, playerState);
		}
		if (improvement == null)
		{
			return false;
		}
		if (owner != playerState.Id)
		{
			return false;
		}
		if (gameState.Version < 84)
		{
			if (unit != null && unit.owner != playerState.Id)
			{
				return false;
			}
		}
		else if (GetUnit(gameState, playerState.Id) != null && unit.owner != playerState.Id)
		{
			return false;
		}
		if (unit != null && gameState.GameLogicData.TryGetData(improvement.type, out var data) && data.HasAbility(ImprovementAbility.Type.Bridge))
		{
			return false;
		}
		if (!gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Destroy, playerState))
		{
			return gameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Decompose, playerState);
		}
		return true;
	}

	public void SetExplored(byte playerId, bool explored)
	{
		if (explorers == null)
		{
			explorers = new List<byte>();
		}
		if (!explorers.Contains(playerId))
		{
			if (explored)
			{
				explorers.Add(playerId);
			}
			else
			{
				explorers.Remove(playerId);
			}
		}
	}

	public ResourceState GetResource(GameState gameState, byte playerId)
	{
		if (!GetExplored(playerId))
		{
			return null;
		}
		if (resource == null)
		{
			return null;
		}
		if (improvement != null)
		{
			return null;
		}
		if (gameState.TryGetPlayer(playerId, out var playerState) && playerState.KnowsResource(resource.type, gameState))
		{
			return resource;
		}
		return null;
	}

	public UnitState GetUnit(GameState gameState, byte playerId, bool includeHidden = false)
	{
		if (!includeHidden && !GetExplored(playerId))
		{
			return null;
		}
		if (unit == null)
		{
			return null;
		}
		gameState.TryGetPlayer(playerId, out var playerState);
		if (unit.HasEffect(UnitEffect.Invisible) && unit.owner != playerId && !playerState.HasPeaceWith(unit.owner))
		{
			return null;
		}
		return unit;
	}

	public bool GetExplored(byte playerId)
	{
		if (playerId == byte.MaxValue)
		{
			return true;
		}
		if (explorers == null || explorers.Count == 0)
		{
			return false;
		}
		return explorers.Contains(playerId);
	}

	public bool HasImprovement(ImprovementData.Type type)
	{
		if (improvement != null && improvement.type != ImprovementData.Type.None)
		{
			return improvement.type == type;
		}
		return false;
	}

	public bool HasResource(ResourceData.Type type)
	{
		if (resource != null && resource.type != ResourceData.Type.None)
		{
			return resource.type == type;
		}
		return false;
	}

	public bool IsBeingCaptured(GameState state)
	{
		if (HasImprovement(ImprovementData.Type.City) && unit != null && unit.CanOccupy(state, this))
		{
			if (unit.owner != owner)
			{
				return owner != 0;
			}
			return false;
		}
		return false;
	}

	public bool IsRouteOpener(GameState state)
	{
		if (improvement != null && state.GameLogicData.TryGetData(improvement.type, out var data))
		{
			return data.IsRouteOpener();
		}
		return false;
	}

	public void SetImprovement(ImprovementState improvement)
	{
		this.improvement = improvement;
	}

	public void SetUnit(UnitState unit)
	{
		this.unit = unit;
	}

	public void SetResource(ResourceState resource)
	{
		this.resource = resource;
	}

	public bool CanBeAccessedByPlayer(GameState gameState, PlayerState player)
	{
		List<TerrainData> unlockedMovements = gameState.GameLogicData.GetUnlockedMovements(player);
		return CanBeAccessedWithMovements(gameState, unlockedMovements);
	}

	public bool CanBeAccessedWithUnlockedTech(GameState gameState, List<TechData> tech)
	{
		List<TerrainData> movementsWithUnlockedTeck = gameState.GameLogicData.GetMovementsWithUnlockedTeck(tech);
		return CanBeAccessedWithMovements(gameState, movementsWithUnlockedTeck);
	}

	public bool CanBeAccessedWithMovements(GameState gameState, List<TerrainData> terrainData)
	{
		if (terrainData != null && terrainData.Count > 0)
		{
			foreach (TerrainData terrainDatum in terrainData)
			{
				if (terrainDatum.type == terrain)
				{
					return true;
				}
			}
		}
		if (improvement != null && gameState.GameLogicData.TryGetData(improvement.type, out var data) && data != null && data.HasAbility(ImprovementAbility.Type.Bridge))
		{
			return true;
		}
		return false;
	}

	public bool CanBeReachedByPlayer(GameState state, PlayerState player, MapData map)
	{
		if (GetExplored(player.Id))
		{
			return true;
		}
		List<TileData> tileNeighbors = map.GetTileNeighbors(coordinates);
		for (int i = 0; i < tileNeighbors.Count; i++)
		{
			TileData tileData = tileNeighbors[i];
			if (tileData.GetExplored(player.Id) && (tileData.owner == player.Id || tileData.owner == 0) && tileData.CanBeAccessedByPlayer(state, player))
			{
				return true;
			}
			if (tileData.unit != null && tileData.unit.owner == player.Id)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasUnexploredNeighbours(byte playerId, MapData map)
	{
		if (!GetExplored(playerId))
		{
			return true;
		}
		List<TileData> tileNeighbors = map.GetTileNeighbors(coordinates);
		for (int i = 0; i < tileNeighbors.Count; i++)
		{
			if (!tileNeighbors[i].GetExplored(playerId))
			{
				return true;
			}
		}
		return false;
	}

	public int? DistanceToUnexploredTile(PlayerState playerState, GameState gameState, List<TerrainData> allowedTerrain, int maxDistance)
	{
		if (HasUnexploredNeighbours(playerState.Id, gameState.Map))
		{
			return 1;
		}
		TileData[] areaSorted = gameState.Map.GetAreaSorted(coordinates, maxDistance - 1, allowDiagonal: true, includeCenter: false);
		bool flag = false;
		int num = maxDistance;
		foreach (TileData tileData in areaSorted)
		{
			if (!tileData.HasUnexploredNeighbours(playerState.Id, gameState.Map))
			{
				continue;
			}
			PathFinderSettings settings = PathFinderSettings.CreateForScout(playerState, allowedTerrain, gameState);
			List<WorldCoordinates> path = gameState.Map.GetPath(coordinates, tileData.coordinates, num - 1, settings);
			if (path != null)
			{
				flag = true;
				num = Math.Min(num, path.Count);
				int num2 = MapDataExtensions.ChebyshevDistance(coordinates, tileData.coordinates) + 1;
				if (num == num2)
				{
					return num2;
				}
			}
		}
		if (flag)
		{
			return num;
		}
		return null;
	}

	public bool HasMatchingTransportPath(TileData otherTile, GameState gameState)
	{
		if (gameState.Version < 46)
		{
			if (!HasRoad || !otherTile.HasRoad)
			{
				if (hasRoute)
				{
					return otherTile.hasRoute;
				}
				return false;
			}
			return true;
		}
		if (IsConnectable(gameState) && otherTile.IsConnectable(gameState))
		{
			return true;
		}
		if (hasRoute && otherTile.hasRoute)
		{
			return true;
		}
		return false;
	}

	private bool IsOwnedByDifferentPlayers(TileData otherTile)
	{
		if (owner != otherTile.owner && owner != 0)
		{
			return otherTile.owner != 0;
		}
		return false;
	}

	public bool HasOpponentCity(byte playerId)
	{
		if (HasImprovement(ImprovementData.Type.City) && owner != 0)
		{
			return owner != playerId;
		}
		return false;
	}

	public bool HasOpponentUnit(byte playerId)
	{
		if (unit != null && unit.owner != byte.MaxValue)
		{
			return unit.owner != playerId;
		}
		return false;
	}

	public int GetMovementCost(MapData map, TileData fromTile, PathFinderSettings settings)
	{
		UnitState unitState = settings.unit;
		_ = settings.playerState.Id;
		ImprovementData data = null;
		if (improvement != null)
		{
			settings.gameState.GameLogicData.TryGetData(improvement.type, out data);
		}
		if (settings.version < 84 && fromTile.IsWater != IsWater && settings.isRequiredToUsePortToGoIntoWater && HasImprovement(ImprovementData.Type.Port))
		{
			return 1000;
		}
		if (settings.version >= 84 && settings.isRequiredToUsePortToGoIntoWater && HasImprovement(ImprovementData.Type.Port))
		{
			return 1000;
		}
		if (settings.gameState.Version < 84)
		{
			if (fromTile.IsWater && !IsWater && unitState != null && unitState.HasAbility(UnitAbility.Type.Carry, settings.gameState))
			{
				return 1000;
			}
		}
		else if (!IsWater && unitState != null && unitState.HasAbility(UnitAbility.Type.Carry, settings.gameState))
		{
			return 1000;
		}
		if (settings.gameState.Version < 50 && improvement != null && data != null && data.HasAbility(ImprovementAbility.Type.Poison) && !settings.playerState.HasTribeAbility(TribeAbility.Type.PoisonResist, settings.gameState))
		{
			return 1000;
		}
		if (settings.version < 20)
		{
			foreach (TileData tileNeighbor in map.GetTileNeighbors(coordinates))
			{
				if (tileNeighbor.unit != null && tileNeighbor.unit.owner != settings.playerState.Id)
				{
					return 1000;
				}
			}
		}
		else if (!settings.shouldAllowOccupiedTiles && settings.unit != null && !settings.unitData.HasAbility(UnitAbility.Type.Sneak) && !settings.unitData.HasAbility(UnitAbility.Type.Hide))
		{
			foreach (TileData tileNeighbor2 in map.GetTileNeighbors(coordinates))
			{
				if (tileNeighbor2.unit != null && tileNeighbor2.unit.owner != settings.playerState.Id && !settings.playerState.HasPeaceWith(tileNeighbor2.unit.owner) && !tileNeighbor2.unit.HasEffect(UnitEffect.Invisible))
				{
					return 1000;
				}
			}
		}
		if (unitState == null)
		{
			if (settings.shouldFollowTransportPaths && (HasRoad || hasRoute))
			{
				return 5;
			}
			return 10;
		}
		if (settings.unit != null && settings.unitData.HasAbility(UnitAbility.Type.Fly))
		{
			return 10;
		}
		if (settings.unit != null && settings.unitData.HasAbility(UnitAbility.Type.Creep))
		{
			return 10;
		}
		if (fromTile.IsWater && !IsWater && !HasImprovement(ImprovementData.Type.City))
		{
			return 1000;
		}
		bool flag = false;
		flag = ((settings.version < 10) ? settings.unitData.HasAbility(UnitAbility.Type.Skate) : (settings.unitData.HasAbility(UnitAbility.Type.Skate) || settings.playerState.availableTech.Contains(TechData.Type.Polarism)));
		if (HasRoadTo(fromTile, settings.gameState, settings.unit.owner) && !IsWater && !fromTile.IsWater && terrain != TerrainData.Type.Ice && !settings.unitData.HasAbility(UnitAbility.Type.Skate) && !settings.unitData.HasAbility(UnitAbility.Type.Creep))
		{
			return 5;
		}
		if (improvement != null && data != null && data.HasAbility(ImprovementAbility.Type.Bridge))
		{
			return 30;
		}
		if (terrain == TerrainData.Type.Forest || terrain == TerrainData.Type.Mountain)
		{
			return 30;
		}
		if (flag)
		{
			if (terrain == TerrainData.Type.Ice && (fromTile.terrain == TerrainData.Type.Ice || fromTile.HasRoad))
			{
				return 5;
			}
			if (settings.unitData.HasAbility(UnitAbility.Type.Skate))
			{
				return 20;
			}
		}
		if (!IsWater && settings.unitData.HasAbility(UnitAbility.Type.Swim))
		{
			return 20;
		}
		return 10;
	}

	public bool HasRoadTo(TileData fromTile, GameState state, byte unitOwnerId)
	{
		if (!fromTile.HasRoad || !HasRoad)
		{
			return false;
		}
		PlayerState playerState;
		bool num = fromTile.owner == 0 || fromTile.owner == unitOwnerId || (state.TryGetPlayer(fromTile.owner, out playerState) && playerState.HasPeaceWith(unitOwnerId));
		PlayerState playerState2;
		bool flag = owner == 0 || owner == unitOwnerId || (state.TryGetPlayer(owner, out playerState2) && playerState2.HasPeaceWith(unitOwnerId));
		return num && flag;
	}

	public bool IsConnectable(GameState gameState)
	{
		if (hasRoad)
		{
			return true;
		}
		if (improvement != null && gameState.GameLogicData.TryGetData(improvement.type, out var data) && (data.type == ImprovementData.Type.City || data.IsRouteOpener()))
		{
			return true;
		}
		return false;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		coordinates.Serialize(writer, version);
		writer.Write((ushort)terrain);
		writer.Write((short)climate);
		writer.Write((short)altitude);
		writer.Write(owner);
		writer.Write(capitalOf);
		rulingCityCoordinates.Serialize(writer, version);
		bool flag = resource != null;
		writer.Write(flag);
		if (flag)
		{
			resource.Serialize(writer, version);
		}
		bool flag2 = improvement != null;
		writer.Write(flag2);
		if (flag2)
		{
			improvement.Serialize(writer, version);
		}
		bool flag3 = unit != null;
		writer.Write(flag3);
		if (flag3)
		{
			unit.Serialize(writer, version);
		}
		bool flag4 = explorers != null && explorers.Count > 0;
		writer.Write((byte)(flag4 ? ((uint)explorers.Count) : 0u));
		if (flag4)
		{
			for (int i = 0; i < explorers.Count; i++)
			{
				writer.Write(explorers[i]);
			}
		}
		writer.Write(hasRoad);
		writer.Write(hasRoute);
		if (version >= 86)
		{
			writer.Write((int)skinType);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		coordinates = new WorldCoordinates(reader, version);
		terrain = (TerrainData.Type)reader.ReadUInt16();
		climate = reader.ReadInt16();
		altitude = reader.ReadInt16();
		owner = reader.ReadByte();
		capitalOf = reader.ReadByte();
		rulingCityCoordinates = new WorldCoordinates(reader, version);
		if (reader.ReadBoolean())
		{
			if (resource == null)
			{
				resource = new ResourceState();
			}
			resource.Deserialize(reader, version);
		}
		if (reader.ReadBoolean())
		{
			if (improvement == null)
			{
				improvement = new ImprovementState();
			}
			improvement.Deserialize(reader, version);
		}
		if (reader.ReadBoolean())
		{
			if (unit == null)
			{
				unit = new UnitState();
			}
			unit.Deserialize(reader, version);
		}
		byte b = reader.ReadByte();
		if (explorers == null || b > explorers.Count)
		{
			explorers = new List<byte>();
		}
		for (int i = 0; i < b; i++)
		{
			if (i < explorers.Count)
			{
				explorers[i] = reader.ReadByte();
			}
			else
			{
				explorers.Add(reader.ReadByte());
			}
		}
		hasRoad = reader.ReadBoolean();
		hasRoute = reader.ReadBoolean();
		if (version >= 86)
		{
			skinType = (SkinType)reader.ReadInt32();
		}
	}
}
