using System.IO;

public class BreakIceCommand : CommandBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public BreakIceCommand()
	{
	}

	public BreakIceCommand(byte playerId, WorldCoordinates coordinates)
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
		UnitState unit = state.Map.GetTile(Coordinates).unit;
		if (unit == null)
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			return false;
		}
		if (!unit.CanBreakIce(state))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_BREAK_ICE;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		int version = state.Version;
		if ((uint)(version - 6) <= 4u)
		{
			ExecuteV10(state);
		}
		else
		{
			ExecuteV11(state);
		}
	}

	private void ExecuteV11(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit != null)
		{
			tile.unit.moved = true;
			tile.unit.attacked = true;
		}
		state.ActionStack.Add(new BreakIceAreaAction(base.PlayerId, Coordinates, 1));
	}

	private void ExecuteV10(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.unit != null)
		{
			tile.unit.moved = true;
			tile.unit.attacked = true;
		}
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, 1, allowDiagonal: true);
		if (areaSorted == null || areaSorted.Length == 0)
		{
			return;
		}
		for (int num = areaSorted.Length - 1; num >= 0; num--)
		{
			TileData tileData = areaSorted[num];
			if (tileData != null && tileData.CanBreakIce())
			{
				state.ActionStack.Add(new BreakIceAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.BreakIce;
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
		return base.ToString();
	}
}
