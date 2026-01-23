using System.IO;

public class RevealCapitalAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public RevealCapitalAction()
	{
	}

	public RevealCapitalAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override ActionType GetActionType()
	{
		return ActionType.RevealCapital;
	}

	public override void Execute(GameState state)
	{
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
