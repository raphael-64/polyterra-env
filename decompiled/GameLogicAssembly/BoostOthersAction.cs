using System.IO;

public class BoostOthersAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public BoostOthersAction()
	{
	}

	public BoostOthersAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		foreach (TileData boostOption in state.Map.GetTile(Coordinates).unit.GetBoostOptions(state))
		{
			state.ActionStack.Add(new BoostAction(base.PlayerId, boostOption.coordinates));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.BoostOthers;
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
}
