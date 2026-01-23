using System.IO;

public class CityRewardPopupAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public CityRewardPopupAction()
	{
	}

	public CityRewardPopupAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		base.HoldForCommand = true;
	}

	public override ActionType GetActionType()
	{
		return ActionType.CityRewardPopup;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(base.HoldForCommand);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		base.HoldForCommand = reader.ReadBoolean();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, Hold: {base.HoldForCommand})";
	}
}
