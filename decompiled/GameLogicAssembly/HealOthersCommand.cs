using System.IO;

public class HealOthersCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public HealOthersCommand()
	{
	}

	public HealOthersCommand(byte playerId, WorldCoordinates coordinates)
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
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile == null || tile.unit == null)
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			return false;
		}
		if (!tile.unit.CanHealOthers(state))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_HEAL_OTHERS;
			return false;
		}
		return true;
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
		state.ActionStack.Add(new HealOthersAction(base.PlayerId, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.HealOthers;
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
