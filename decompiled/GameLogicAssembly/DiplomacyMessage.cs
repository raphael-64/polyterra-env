using System.IO;

public class DiplomacyMessage
{
	public DiplomacyMessageType Type;

	public byte Sender;

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write((byte)Type);
		writer.Write(Sender);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		Type = (DiplomacyMessageType)reader.ReadByte();
		Sender = reader.ReadByte();
	}
}
