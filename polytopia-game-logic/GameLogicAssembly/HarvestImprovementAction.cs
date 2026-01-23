using System.IO;

public class HarvestImprovementAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public HarvestImprovementAction()
	{
	}

	public HarvestImprovementAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		_ = state.Version;
		ExecuteV1(state);
	}

	private void ExecuteV1(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		TileData tile2 = state.Map.GetTile(tile.rulingCityCoordinates);
		if (tile.improvement != null)
		{
			int num = tile.improvement.level - 1;
			for (int i = 0; i < num; i++)
			{
				AddSubAction(new IncreasePopulationAction(base.PlayerId, tile.coordinates, tile2.coordinates, 60));
			}
			tile.improvement = null;
		}
		CommitSubActionsToStack(state.ActionStack);
	}

	public override ActionType GetActionType()
	{
		return ActionType.HarvestImprovement;
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
