using System.IO;

public class HealOthersAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public HealOthersAction()
	{
	}

	public HealOthersAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		foreach (TileData healOption in state.Map.GetTile(Coordinates).GetHealOptions(base.PlayerId, state))
		{
			state.ActionStack.Add(new HealAction(base.PlayerId, healOption.coordinates, 40));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.HealOthers;
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
