using System.IO;

public class StayCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public StayCommand()
	{
	}

	public StayCommand(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		return PassesBasicValidation(state, out validationError);
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit != null)
		{
			tile.unit.attacked = true;
			tile.unit.moved = true;
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Stay;
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
