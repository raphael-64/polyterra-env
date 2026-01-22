using System.Collections.Generic;
using System.IO;

public class FreezeAreaAction : ActionBase
{
	public int frozenTiles;

	public WorldCoordinates Coordinates { get; protected set; }

	public ushort Radius { get; protected set; }

	public bool FreezeUnits { get; protected set; }

	public bool OnlyOwnedTiles { get; protected set; }

	public FreezeAreaAction()
	{
	}

	public FreezeAreaAction(byte playerId, WorldCoordinates coordinates, ushort radius, bool freezeUnits = false, bool onlyOwnedTiles = false)
		: base(playerId)
	{
		Coordinates = coordinates;
		Radius = radius;
		FreezeUnits = freezeUnits;
		OnlyOwnedTiles = onlyOwnedTiles;
	}

	public override void Execute(GameState state)
	{
		switch (state.Version)
		{
		case 6:
		case 7:
		case 8:
		case 9:
			ExecuteV9(state);
			break;
		case 10:
		case 11:
		case 12:
		case 13:
		case 14:
		case 15:
		case 16:
		case 17:
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
			ExecuteV10(state);
			break;
		default:
			ExecuteV29(state);
			break;
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
		state.TryGetPlayer(base.PlayerId, out var playerState);
		for (int num = areaSorted.Length - 1; num >= 0; num--)
		{
			TileData tileData = areaSorted[num];
			if (tileData != null)
			{
				bool flag = false;
				if (tileData.IsFreezable(state, playerState, OnlyOwnedTiles))
				{
					flag = true;
					state.ActionStack.Add(new FreezeTileAction(playerState.Id, tileData.coordinates));
				}
				if (FreezeUnits && tileData.unit != null && tileData.unit.IsFreezable(state, playerState))
				{
					flag = true;
					state.ActionStack.Add(new FreezeUnitAction(playerState.Id, WorldCoordinates.NULL_COORDINATES, tileData.coordinates, 0));
				}
				if (flag)
				{
					frozenTiles++;
				}
			}
		}
	}

	private void ExecuteV10(GameState state)
	{
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, Radius, allowDiagonal: true);
		if (areaSorted == null || areaSorted.Length == 0)
		{
			return;
		}
		state.TryGetPlayer(base.PlayerId, out var playerState);
		for (int num = areaSorted.Length - 1; num >= 0; num--)
		{
			TileData tileData = areaSorted[num];
			if (tileData != null)
			{
				bool flag = false;
				if (tileData.IsFreezable(state, playerState, OnlyOwnedTiles))
				{
					flag = true;
					state.ActionStack.Add(new FreezeTileAction(playerState.Id, tileData.coordinates));
				}
				if (FreezeUnits && tileData.unit != null && tileData.unit.IsFreezable(state, playerState))
				{
					flag = true;
					state.ActionStack.Add(new FreezeUnitAction(playerState.Id, WorldCoordinates.NULL_COORDINATES, tileData.coordinates, 0));
				}
				if (flag)
				{
					frozenTiles++;
				}
			}
		}
	}

	private void ExecuteV9(GameState state)
	{
		TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, Radius, allowDiagonal: true);
		if (areaSorted == null || areaSorted.Length == 0)
		{
			return;
		}
		state.TryGetPlayer(base.PlayerId, out var playerState);
		for (int num = areaSorted.Length - 1; num >= 0; num--)
		{
			TileData tileData = areaSorted[num];
			if (tileData != null && tileData.IsFreezable(state, playerState))
			{
				frozenTiles++;
				state.ActionStack.Add(new FreezeTileAction(playerState.Id, tileData.coordinates));
				if (FreezeUnits && tileData.unit != null && tileData.unit.owner != playerState.Id)
				{
					state.ActionStack.Add(new FreezeUnitAction(playerState.Id, WorldCoordinates.NULL_COORDINATES, tileData.coordinates, 0));
				}
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.FreezeArea;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(Radius);
		writer.Write(FreezeUnits);
		if (version >= 11)
		{
			writer.Write(OnlyOwnedTiles);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Radius = reader.ReadUInt16();
		FreezeUnits = reader.ReadBoolean();
		if (version >= 11)
		{
			OnlyOwnedTiles = reader.ReadBoolean();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, Radius: {Radius}, FreezeUnits: {FreezeUnits}, OnlyOwnedTiles: {OnlyOwnedTiles})";
	}
}
