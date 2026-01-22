using System.IO;

public class ClimateChangeAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public int Climate { get; protected set; }

	public ClimateChangeAction()
	{
	}

	public ClimateChangeAction(byte playerId, WorldCoordinates coordinates, int climate)
		: base(playerId)
	{
		Coordinates = coordinates;
		Climate = climate;
	}

	public override bool IsValid(GameState state)
	{
		return state.Map.GetTile(Coordinates).climate != Climate;
	}

	public override void Execute(GameState state)
	{
		state.Map.GetTile(Coordinates).climate = Climate;
	}

	public override ActionType GetActionType()
	{
		return ActionType.ClimateChange;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(Climate);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Climate = reader.ReadInt32();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
