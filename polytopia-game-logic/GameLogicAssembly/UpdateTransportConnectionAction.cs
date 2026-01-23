using System.Collections.Generic;
using System.IO;

public class UpdateTransportConnectionAction : ActionBase
{
	private List<byte> playersToUpdate;

	public UpdateTransportConnectionAction()
	{
	}

	public UpdateTransportConnectionAction(byte playerId, List<byte> playersToUpdate)
		: base(playerId)
	{
		this.playersToUpdate = playersToUpdate;
	}

	public override void Execute(GameState state)
	{
		List<TileData> cityTiles = new List<TileData>();
		List<TileData> connectedCities = new List<TileData>();
		foreach (byte item in playersToUpdate)
		{
			if (item != 0)
			{
				state.TryGetPlayer(item, out var playerState);
				ActionUtils.UpdateCityConnections(state, playerState, cityTiles, connectedCities);
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.UpdateTransportConnections;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		int num = ((playersToUpdate != null) ? playersToUpdate.Count : 0);
		writer.Write(num);
		for (int i = 0; i < num; i++)
		{
			if (playersToUpdate == null)
			{
				break;
			}
			writer.Write(playersToUpdate[i]);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		int num = reader.ReadInt32();
		playersToUpdate = new List<byte>(num);
		for (int i = 0; i < num; i++)
		{
			playersToUpdate.Add(reader.ReadByte());
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, OtherPlayers: {playersToUpdate.ToString()})";
	}
}
