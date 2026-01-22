using System.IO;
using Polytopia.Data;

public class CreateResourceAction : ActionBase
{
	public enum CreateReason
	{
		None,
		Attract
	}

	public ResourceData.Type Type { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public CreateReason Reason { get; private set; }

	public CreateResourceAction()
	{
	}

	public CreateResourceAction(byte playerId, ResourceData.Type type, WorldCoordinates coordinates, CreateReason reason = CreateReason.None)
		: base(playerId)
	{
		Type = type;
		Coordinates = coordinates;
		Reason = reason;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile != null)
		{
			tile.resource = new ResourceState
			{
				type = Type
			};
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tile);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.CreateResource;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Type);
		Coordinates.Serialize(writer, version);
		writer.Write((ushort)Reason);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (ResourceData.Type)reader.ReadUInt16();
		Coordinates = new WorldCoordinates(reader, version);
		Reason = (CreateReason)reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Type: {Type}, Coordinates: {Coordinates})";
	}
}
