using System;
using System.IO;

public class DestroyResourceAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public DestroyResourceAction()
	{
	}

	public DestroyResourceAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.resource == null)
		{
			Log.Verbose("Uh-oh, trying to remove resource that was already removed", Array.Empty<object>());
			return;
		}
		ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		tile.resource = null;
	}

	public override ActionType GetActionType()
	{
		return ActionType.DestroyResource;
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
