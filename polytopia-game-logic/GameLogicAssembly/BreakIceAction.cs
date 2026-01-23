using System.IO;
using Polytopia.Data;

public class BreakIceAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public BreakIceAction()
	{
	}

	public BreakIceAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.altitude == -1)
		{
			tile.terrain = TerrainData.Type.Water;
		}
		else if (tile.altitude == -2)
		{
			tile.terrain = TerrainData.Type.Ocean;
		}
		ActionUtils.CheckIceBankLevels(state);
	}

	public override ActionType GetActionType()
	{
		return ActionType.BreakIce;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
	}

	public override string ToString()
	{
		return base.ToString();
	}
}
