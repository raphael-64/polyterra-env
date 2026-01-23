using System.IO;

public struct CommandTrigger
{
	public byte playerId;

	public byte opponentId;

	public CommandTriggerType type;

	public WorldCoordinates coordinates;

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(playerId);
		writer.Write((ushort)type);
		coordinates.Serialize(writer, version);
		if (version >= 60)
		{
			writer.Write(opponentId);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		playerId = reader.ReadByte();
		type = (CommandTriggerType)reader.ReadUInt16();
		coordinates = new WorldCoordinates(reader, version);
		if (version >= 60)
		{
			opponentId = reader.ReadByte();
		}
	}
}
