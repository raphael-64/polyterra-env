using System.IO;

public class RemoteGameData : IBinarySerializable
{
	public ushort lastSeenCommand;

	public RemoteGameData()
	{
		lastSeenCommand = 0;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(lastSeenCommand);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		lastSeenCommand = reader.ReadUInt16();
	}
}
