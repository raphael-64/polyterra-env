using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class BuildAction : ActionBase
{
	public ImprovementData.Type Type { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public bool DeductCost { get; private set; }

	public BuildAction()
	{
	}

	public BuildAction(byte playerId, ImprovementData.Type type, WorldCoordinates coordinates, bool deductCost = true)
		: base(playerId)
	{
		Type = type;
		Coordinates = coordinates;
		DeductCost = deductCost;
	}

	public override void Execute(GameState state)
	{
		if (state.Version < 21)
		{
			ExecuteV20(state);
		}
		else if (state.Version < 40)
		{
			ExecuteV21(state);
		}
		else if (state.Version < 42)
		{
			ExecuteV40(state);
		}
		else if (state.Version < 46)
		{
			ExecuteV45(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			if (data.type != ImprovementData.Type.Road)
			{
				ImprovementState improvementState = (tile.improvement = new ImprovementState
				{
					type = Type,
					borderSize = (ushort)data.borderSize,
					level = 0,
					xp = 0,
					production = 1,
					founded = (ushort)state.CurrentTurn,
					baseScore = (ushort)data.GetScoreReward(),
					founder = base.PlayerId
				});
				if (data.HasAbility(ImprovementAbility.Type.Patina))
				{
					improvementState.level = (ushort)ActionUtils.CalculateImprovementLevel(state, tile);
				}
				int num = state.CalculateImprovementScore(tile);
				if (num > 0)
				{
					AddSubAction(new IncreaseScoreAction(base.PlayerId, num, tile.coordinates));
				}
			}
			if (data.HasAbility(ImprovementAbility.Type.Consumed))
			{
				if (tile.resource != null)
				{
					AddSubAction(new DestroyResourceAction(base.PlayerId, Coordinates));
				}
				tile.improvement = null;
			}
			if (data.type == ImprovementData.Type.Road)
			{
				AddSubAction(new BuildRoadAction(base.PlayerId, Coordinates));
			}
			if (data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
			}
			uint currencyReward = data.GetCurrencyReward();
			for (uint num2 = 0u; num2 < currencyReward; num2++)
			{
				AddSubAction(new IncreaseCurrencyAction(base.PlayerId, tile.coordinates, 40));
			}
			Log.Verbose("{0} Deduct cost {1}", new object[2] { playerState.UserName, DeductCost });
			if (DeductCost)
			{
				playerState.Currency -= data.GetCurrencyCost();
			}
			TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
			if (tile2 != null)
			{
				uint populationReward = data.GetPopulationReward();
				for (int i = 0; i < populationReward; i++)
				{
					AddSubAction(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile2.coordinates, 60));
				}
			}
			if (data.creates != null && data.creates.Count > 0)
			{
				foreach (Creates create in data.creates)
				{
					if (create.terrain != null && create.terrain.type != TerrainData.Type.None)
					{
						tile.terrain = create.terrain.type;
					}
					if (create.resource != null && create.resource.type != ResourceData.Type.None)
					{
						AddSubAction(new CreateResourceAction(base.PlayerId, create.resource.type, Coordinates));
					}
					if (create.unit != null && create.unit.type != UnitData.Type.None && (tile.unit == null || tile.unit.owner == base.PlayerId))
					{
						AddSubAction(new TrainAction(base.PlayerId, create.unit.type, Coordinates, 0));
					}
				}
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
			if (Type == ImprovementData.Type.IceBank)
			{
				ActionUtils.CheckIceBankLevels(state);
			}
			if (data.HasAbility(ImprovementAbility.Type.Unique))
			{
				playerState.builtUniqueImprovements.Add(Type);
			}
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public void ExecuteV45(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			if (data.type != ImprovementData.Type.Road)
			{
				ImprovementState improvementState = (tile.improvement = new ImprovementState
				{
					type = Type,
					borderSize = (ushort)data.borderSize,
					level = 0,
					xp = 0,
					production = 1,
					founded = (ushort)state.CurrentTurn,
					baseScore = (ushort)data.GetScoreReward(),
					founder = base.PlayerId
				});
				if (data.HasAbility(ImprovementAbility.Type.Patina))
				{
					improvementState.level = (ushort)ActionUtils.CalculateImprovementLevel(state, tile);
				}
				int num = state.CalculateImprovementScore(tile);
				if (num > 0)
				{
					AddSubAction(new IncreaseScoreAction(base.PlayerId, num, tile.coordinates));
				}
			}
			if (data.HasAbility(ImprovementAbility.Type.Consumed))
			{
				if (tile.resource != null)
				{
					AddSubAction(new DestroyResourceAction(base.PlayerId, Coordinates));
				}
				tile.improvement = null;
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener())
			{
				AddSubAction(new BuildRoadAction(base.PlayerId, Coordinates));
			}
			if (data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
			}
			uint currencyReward = data.GetCurrencyReward();
			for (uint num2 = 0u; num2 < currencyReward; num2++)
			{
				AddSubAction(new IncreaseCurrencyAction(base.PlayerId, tile.coordinates, 40));
			}
			Log.Verbose("{0} Deduct cost {1}", new object[2] { playerState.UserName, DeductCost });
			if (DeductCost)
			{
				playerState.Currency -= data.GetCurrencyCost();
			}
			TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
			if (tile2 != null)
			{
				uint populationReward = data.GetPopulationReward();
				for (int i = 0; i < populationReward; i++)
				{
					AddSubAction(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile2.coordinates, 60));
				}
			}
			if (data.creates != null && data.creates.Count > 0)
			{
				foreach (Creates create in data.creates)
				{
					if (create.terrain != null && create.terrain.type != TerrainData.Type.None)
					{
						tile.terrain = create.terrain.type;
					}
					if (create.resource != null && create.resource.type != ResourceData.Type.None)
					{
						AddSubAction(new CreateResourceAction(base.PlayerId, create.resource.type, Coordinates));
					}
					if (create.unit != null && create.unit.type != UnitData.Type.None && (tile.unit == null || tile.unit.owner == base.PlayerId))
					{
						AddSubAction(new TrainAction(base.PlayerId, create.unit.type, Coordinates, 0));
					}
				}
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
			if (Type == ImprovementData.Type.IceBank)
			{
				ActionUtils.CheckIceBankLevels(state);
			}
			if (data.HasAbility(ImprovementAbility.Type.Unique))
			{
				playerState.builtUniqueImprovements.Add(Type);
			}
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public void ExecuteV40(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			if (data.type != ImprovementData.Type.Road)
			{
				ImprovementState improvementState = (tile.improvement = new ImprovementState
				{
					type = Type,
					borderSize = (ushort)data.borderSize,
					level = 0,
					xp = 0,
					production = 1,
					founded = (ushort)state.CurrentTurn,
					baseScore = (ushort)data.GetScoreReward(),
					founder = base.PlayerId
				});
				if (data.HasAbility(ImprovementAbility.Type.Patina))
				{
					improvementState.level = (ushort)ActionUtils.CalculateImprovementLevel(state, tile);
				}
				int num = state.CalculateImprovementScore(tile);
				if (num > 0)
				{
					AddSubAction(new IncreaseScoreAction(base.PlayerId, num, tile.coordinates));
				}
			}
			if (data.HasAbility(ImprovementAbility.Type.Consumed))
			{
				if (tile.resource != null)
				{
					AddSubAction(new DestroyResourceAction(base.PlayerId, Coordinates));
				}
				tile.improvement = null;
			}
			if (data.type == ImprovementData.Type.Road)
			{
				AddSubAction(new BuildRoadAction(base.PlayerId, Coordinates));
			}
			if (data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
			}
			uint currencyReward = data.GetCurrencyReward();
			for (uint num2 = 0u; num2 < currencyReward; num2++)
			{
				AddSubAction(new IncreaseCurrencyAction(base.PlayerId, tile.coordinates, 40));
			}
			Log.Verbose("{0} Deduct cost {1}", new object[2] { playerState.UserName, DeductCost });
			if (DeductCost)
			{
				playerState.Currency -= data.GetCurrencyCost();
			}
			TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
			if (tile2 != null)
			{
				uint populationReward = data.GetPopulationReward();
				for (int i = 0; i < populationReward; i++)
				{
					AddSubAction(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile2.coordinates, 60));
				}
			}
			if (data.creates != null && data.creates.Count > 0)
			{
				foreach (Creates create in data.creates)
				{
					if (create.terrain != null && create.terrain.type != TerrainData.Type.None)
					{
						tile.terrain = create.terrain.type;
					}
					if (create.resource != null && create.resource.type != ResourceData.Type.None)
					{
						AddSubAction(new CreateResourceAction(base.PlayerId, create.resource.type, Coordinates));
					}
					if (create.unit != null && create.unit.type != UnitData.Type.None && (tile.unit == null || tile.unit.owner == base.PlayerId))
					{
						AddSubAction(new TrainAction(base.PlayerId, create.unit.type, Coordinates, 0));
					}
				}
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
			if (Type == ImprovementData.Type.IceBank)
			{
				ActionUtils.CheckIceBankLevels(state);
			}
			if (data.HasAbility(ImprovementAbility.Type.Unique))
			{
				playerState.builtUniqueImprovements.Add(Type);
			}
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public void ExecuteV21(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			if (data.type != ImprovementData.Type.Road)
			{
				(tile.improvement = new ImprovementState
				{
					type = Type,
					borderSize = (ushort)data.borderSize,
					level = 0,
					xp = 0,
					production = 1,
					founded = (ushort)state.CurrentTurn,
					baseScore = (ushort)data.GetScoreReward(),
					founder = base.PlayerId
				}).level = (ushort)ActionUtils.CalculateImprovementLevel(state, tile);
				int num = state.CalculateImprovementScore(tile);
				if (num > 0)
				{
					AddSubAction(new IncreaseScoreAction(base.PlayerId, num, tile.coordinates));
				}
			}
			if (data.HasAbility(ImprovementAbility.Type.Consumed))
			{
				if (tile.resource != null)
				{
					AddSubAction(new DestroyResourceAction(base.PlayerId, Coordinates));
				}
				tile.improvement = null;
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener())
			{
				AddSubAction(new BuildRoadAction(base.PlayerId, Coordinates));
			}
			if (data.IsRouteOpener())
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener())
			{
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
			}
			uint currencyReward = data.GetCurrencyReward();
			for (uint num2 = 0u; num2 < currencyReward; num2++)
			{
				AddSubAction(new IncreaseCurrencyAction(base.PlayerId, tile.coordinates, 40));
			}
			playerState.Currency -= data.GetCurrencyCost();
			TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
			if (tile2 != null)
			{
				uint num3 = data.GetPopulationReward();
				if (tile.improvement != null && data.maxLevel > 0)
				{
					num3 *= tile.improvement.level;
				}
				for (int i = 0; i < num3; i++)
				{
					AddSubAction(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile2.coordinates, 60));
				}
			}
			if (data.creates != null && data.creates.Count > 0)
			{
				foreach (Creates create in data.creates)
				{
					if (create.terrain != null && create.terrain.type != TerrainData.Type.None)
					{
						tile.terrain = create.terrain.type;
					}
					if (create.resource != null && create.resource.type != ResourceData.Type.None)
					{
						AddSubAction(new CreateResourceAction(base.PlayerId, create.resource.type, Coordinates));
					}
					if (create.unit != null && create.unit.type != UnitData.Type.None && (tile.unit == null || tile.unit.owner == base.PlayerId))
					{
						AddSubAction(new TrainAction(base.PlayerId, create.unit.type, Coordinates, 0));
					}
				}
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
			if (Type == ImprovementData.Type.IceBank)
			{
				ActionUtils.CheckIceBankLevels(state);
			}
			if (data.HasAbility(ImprovementAbility.Type.Unique))
			{
				playerState.builtUniqueImprovements.Add(Type);
			}
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public void ExecuteV20(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null && state.GameLogicData.TryGetData(Type, out var data) && state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			if (data.type != ImprovementData.Type.Road)
			{
				(tile.improvement = new ImprovementState
				{
					type = Type,
					borderSize = (ushort)data.borderSize,
					level = 0,
					xp = 0,
					production = 1,
					founded = (ushort)state.CurrentTurn,
					baseScore = (ushort)data.GetScoreReward(),
					founder = base.PlayerId
				}).level = (ushort)ActionUtils.CalculateImprovementLevel(state, tile);
				int num = state.CalculateImprovementScore(tile);
				if (num > 0)
				{
					AddSubAction(new IncreaseScoreAction(base.PlayerId, num, tile.coordinates));
				}
			}
			if (data.HasAbility(ImprovementAbility.Type.Consumed))
			{
				if (tile.resource != null)
				{
					AddSubAction(new DestroyResourceAction(base.PlayerId, Coordinates));
				}
				tile.improvement = null;
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener())
			{
				AddSubAction(new BuildRoadAction(base.PlayerId, Coordinates));
			}
			if (data.IsRouteOpener())
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
			}
			if (data.type == ImprovementData.Type.Road || data.IsRouteOpener())
			{
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
			}
			uint currencyReward = data.GetCurrencyReward();
			for (uint num2 = 0u; num2 < currencyReward; num2++)
			{
				AddSubAction(new IncreaseCurrencyAction(base.PlayerId, tile.coordinates, 40));
			}
			playerState.Currency -= data.GetCurrencyCost();
			TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
			if (tile2 != null)
			{
				uint num3 = data.GetPopulationReward();
				if (tile.improvement != null && data.maxLevel > 0)
				{
					num3 *= tile.improvement.level;
				}
				for (int i = 0; i < num3; i++)
				{
					AddSubAction(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile2.coordinates, 60));
				}
			}
			if (data.creates != null && data.creates.Count > 0)
			{
				foreach (Creates create in data.creates)
				{
					if (create.terrain != null && create.terrain.type != TerrainData.Type.None)
					{
						tile.terrain = create.terrain.type;
					}
					if (create.resource != null && create.resource.type != ResourceData.Type.None)
					{
						AddSubAction(new CreateResourceAction(base.PlayerId, create.resource.type, Coordinates));
					}
					if (create.unit != null && create.unit.type != UnitData.Type.None && (tile.unit == null || tile.unit.owner == base.PlayerId))
					{
						AddSubAction(new TrainAction(base.PlayerId, create.unit.type, Coordinates, 0));
					}
				}
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
			if (Type == ImprovementData.Type.IceBank)
			{
				ActionUtils.CheckIceBankLevels(state);
			}
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public override ActionType GetActionType()
	{
		return ActionType.Build;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Type);
		Coordinates.Serialize(writer, version);
		if (version >= 40)
		{
			writer.Write(DeductCost);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (ImprovementData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
		if (version >= 40)
		{
			DeductCost = reader.ReadBoolean();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type}, Coordinates: {Coordinates})";
	}
}
