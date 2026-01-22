using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class DestroyImprovementAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public DestroyImprovementAction()
	{
	}

	public DestroyImprovementAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 9)
		{
			ExecuteV9(state);
		}
		else if (state.Version < 21)
		{
			ExecuteV10(state);
		}
		else if (state.Version < 26)
		{
			ExecuteV21(state);
		}
		else if (state.Version < 40)
		{
			ExecuteV26(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		int amount = state.CalculateImprovementScore(tile);
		state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, amount));
		if (state.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			int num = data.CalculateImprovementPopulationAtLevel(tile.improvement.level);
			for (int i = 0; i < num; i++)
			{
				AddSubAction(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
			}
			if (data.terrainRequirements != null && data.terrainRequirements.Count > 0)
			{
				foreach (TerrainRequirements terrainRequirement in data.terrainRequirements)
				{
					if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None)
					{
						tile.resource = new ResourceState
						{
							type = terrainRequirement.resource.type
						};
					}
				}
			}
			if (data.IsRouteOpener() || data.HasAbility(ImprovementAbility.Type.Bridge))
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
				tile.HasRoad = false;
			}
		}
		tile.improvement = null;
		ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		CommitSubActionsToStack(state.ActionStack);
	}

	private void ExecuteV26(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		int amount = state.CalculateImprovementScore(tile);
		state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, amount));
		if (state.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			uint num = data.GetPopulationReward();
			if (data.maxLevel > 0 && !data.HasAbility(ImprovementAbility.Type.Patina))
			{
				num *= tile.improvement.level;
			}
			for (int i = 0; i < num; i++)
			{
				AddSubAction(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
			}
			if (data.terrainRequirements != null && data.terrainRequirements.Count > 0)
			{
				foreach (TerrainRequirements terrainRequirement in data.terrainRequirements)
				{
					if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None)
					{
						tile.resource = new ResourceState
						{
							type = terrainRequirement.resource.type
						};
					}
				}
			}
			if (data.IsRouteOpener())
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
				AddSubAction(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
				tile.HasRoad = false;
			}
		}
		tile.improvement = null;
		ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		CommitSubActionsToStack(state.ActionStack);
	}

	private void ExecuteV21(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		int amount = state.CalculateImprovementScore(tile);
		state.ActionStack.Add(new DecreaseScoreAction(base.PlayerId, amount));
		if (state.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			uint num = data.GetPopulationReward();
			if (data.maxLevel > 0 && !data.HasAbility(ImprovementAbility.Type.Patina))
			{
				num *= tile.improvement.level;
			}
			for (int i = 0; i < num; i++)
			{
				AddSubAction(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
			}
			if (data.terrainRequirements != null && data.terrainRequirements.Count > 0)
			{
				foreach (TerrainRequirements terrainRequirement in data.terrainRequirements)
				{
					if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None)
					{
						tile.resource = new ResourceState
						{
							type = terrainRequirement.resource.type
						};
					}
				}
			}
			if (data.IsRouteOpener())
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
				tile.HasRoad = false;
			}
		}
		tile.improvement = null;
		ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		CommitSubActionsToStack(state.ActionStack);
	}

	private void ExecuteV10(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (state.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			uint num = data.GetPopulationReward();
			if (data.maxLevel > 0 && !data.HasAbility(ImprovementAbility.Type.Patina))
			{
				num *= tile.improvement.level;
			}
			for (int i = 0; i < num; i++)
			{
				AddSubAction(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
			}
			if (data.terrainRequirements != null && data.terrainRequirements.Count > 0)
			{
				foreach (TerrainRequirements terrainRequirement in data.terrainRequirements)
				{
					if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None)
					{
						tile.resource = new ResourceState
						{
							type = terrainRequirement.resource.type
						};
					}
				}
			}
			if (data.IsRouteOpener())
			{
				AddSubAction(new UpdateRoutesAction(base.PlayerId));
				tile.HasRoad = false;
			}
		}
		tile.improvement = null;
		ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		CommitSubActionsToStack(state.ActionStack);
	}

	private void ExecuteV9(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (state.GameLogicData.TryGetData(tile.improvement.type, out var data))
		{
			uint num = data.GetPopulationReward();
			if (data.maxLevel > 0 && !data.HasAbility(ImprovementAbility.Type.Patina))
			{
				num *= tile.improvement.level;
			}
			for (int i = 0; i < num; i++)
			{
				AddSubAction(new DecreasePopulationAction(base.PlayerId, tile.rulingCityCoordinates, 200));
			}
			if (data.terrainRequirements != null && data.terrainRequirements.Count > 0)
			{
				foreach (TerrainRequirements terrainRequirement in data.terrainRequirements)
				{
					if (terrainRequirement.resource != null && terrainRequirement.resource.type != ResourceData.Type.None)
					{
						tile.resource = new ResourceState
						{
							type = terrainRequirement.resource.type
						};
					}
				}
			}
		}
		tile.improvement = null;
		ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		CommitSubActionsToStack(state.ActionStack);
	}

	public override ActionType GetActionType()
	{
		return ActionType.DestroyImprovement;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
