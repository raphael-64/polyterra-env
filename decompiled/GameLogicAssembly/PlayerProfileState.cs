using System;
using System.Collections.Generic;
using System.IO;
using PolytopiaBackendBase.Common;

public class PlayerProfileState
{
	public Guid id;

	public string name;

	public AvatarState avatarState;

	public int numGames;

	public int numMultiplayerGames;

	public int numFriends;

	public int gameVersion;

	public int multiplayerRating;

	public DateTime? lastLoginDate;

	public Platform platform;

	public Dictionary<Guid, int> victories;

	public Dictionary<Guid, int> defeats;

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(id.ToString());
		writer.Write((name == null) ? "" : name);
		avatarState.Serialize(writer, version);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (version < 10)
		{
			Deserialize9(reader, version);
		}
		else
		{
			Deserialize10(reader, version);
		}
	}

	public void Deserialize9(BinaryReader reader, int version)
	{
		id = Guid.NewGuid();
		name = reader.ReadString();
		avatarState = new AvatarState();
		avatarState.Deserialize(reader, version);
	}

	public void Deserialize10(BinaryReader reader, int version)
	{
		id = Guid.Parse(reader.ReadString());
		name = reader.ReadString();
		avatarState = new AvatarState();
		avatarState.Deserialize(reader, version);
	}
}
