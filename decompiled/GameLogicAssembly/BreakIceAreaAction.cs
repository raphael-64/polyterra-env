using System.Collections.Generic;
using System.IO;

public class BreakIceAreaAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public ushort Radius { get; protected set; }

	public BreakIceAreaAction()
	{
	}

	public BreakIceAreaAction(byte playerId, WorldCoordinates coordinates, ushort radius)
		: base(playerId)
	{
		Coordinates = coordinates;
		Radius = radius;
	}

	public override void Execute(GameState state)
	{
		int version = state.Version;
		if ((uint)(version - 6) <= 22u)
		{
			ExecuteV28(state);
		}
		else
		{
			ExecuteV29(state);
		}
	}

	private void ExecuteV29(GameState state)
	{
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, Radius, allowDiagonal: true);
		if (areaSorted == null || areaSorted.Length == 0)
		{
			return;
		}
		state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte> { base.PlayerId }));
		state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
		for (int num = areaSorted.Length - 1; num >= 0; num--)
		{
			TileData tileData = areaSorted[num];
			if (tileData != null && tileData.CanBreakIce())
			{
				state.ActionStack.Add(new BreakIceAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	private void ExecuteV28(GameState state)
	{
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, Radius, allowDiagonal: true);
		if (areaSorted == null || areaSorted.Length == 0)
		{
			return;
		}
		state.TryGetPlayer(base.PlayerId, out var _);
		for (int num = areaSorted.Length - 1; num >= 0; num--)
		{
			TileData tileData = areaSorted[num];
			if (tileData != null && tileData.CanBreakIce())
			{
				state.ActionStack.Add(new BreakIceAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.BreakIceArea;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(Radius);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Radius = reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, Radius: {Radius})";
	}
}
