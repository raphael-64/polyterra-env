using System;
using System.Collections.Generic;
using System.IO;

public class HotseatProfilesState : IBinarySerializable
{
	public const int PLAYER_COUNT = 12;

	public List<PlayerProfileState> players = new List<PlayerProfileState>();

	public static HotseatProfilesState CreateRandom(int version, int seed)
	{
		HotseatProfilesState hotseatProfilesState = new HotseatProfilesState();
		for (int i = 0; i < 12; i++)
		{
			PlayerProfileState playerProfileState = new PlayerProfileState();
			playerProfileState.avatarState = AvatarExtensions.CreateRandomState(version, i + seed);
			playerProfileState.id = new Guid(i, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			hotseatProfilesState.players.Add(playerProfileState);
		}
		return hotseatProfilesState;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(players.Count);
		foreach (PlayerProfileState player in players)
		{
			player.Serialize(writer, version);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		int num = reader.ReadInt32();
		players.Clear();
		for (int i = 0; i < num; i++)
		{
			PlayerProfileState playerProfileState = new PlayerProfileState();
			playerProfileState.Deserialize(reader, version);
			playerProfileState.id = new Guid(i, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0);
			players.Add(playerProfileState);
		}
	}
}
