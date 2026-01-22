using System.IO;

public class BoostCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public BoostCommand()
	{
	}

	public BoostCommand(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		if (state.Map.GetTile(Coordinates).unit.GetBoostOptions(state).Count == 0)
		{
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit != null)
		{
			tile.unit.attacked = true;
			tile.unit.moved = true;
		}
		state.ActionStack.Add(new BoostOthersAction(base.PlayerId, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Boost;
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
