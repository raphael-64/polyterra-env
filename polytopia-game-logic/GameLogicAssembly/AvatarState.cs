using System.IO;

public class AvatarState : IBinarySerializable
{
	public AvatarPartState layer0;

	public AvatarPartState layer1;

	public AvatarPartState layer2;

	public AvatarPartState layer3;

	public AvatarPartState layer4;

	public void Serialize(BinaryWriter writer, int version)
	{
		if (version < 9)
		{
			Serialize8(writer, version);
		}
		else
		{
			Serialize9(writer, version);
		}
	}

	public void Serialize8(BinaryWriter writer, int version)
	{
		layer0.Serialize(writer, version);
		layer1.Serialize(writer, version);
		layer2.Serialize(writer, version);
		layer3.Serialize(writer, version);
		layer4.Serialize(writer, version);
	}

	public void Serialize9(BinaryWriter writer, int version)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryWriter writer2 = new BinaryWriter(memoryStream);
		layer0.Serialize(writer2, version);
		layer1.Serialize(writer2, version);
		layer2.Serialize(writer2, version);
		layer3.Serialize(writer2, version);
		layer4.Serialize(writer2, version);
		writer.Write((int)memoryStream.Length);
		memoryStream.WriteTo(writer.BaseStream);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (version < 9)
		{
			Deserialize8(reader, version);
		}
		else
		{
			Deserialize9(reader, version);
		}
	}

	public void Deserialize8(BinaryReader reader, int version)
	{
		layer0 = new AvatarPartState(reader, version);
		layer1 = new AvatarPartState(reader, version);
		layer2 = new AvatarPartState(reader, version);
		layer3 = new AvatarPartState(reader, version);
		layer4 = new AvatarPartState(reader, version);
	}

	public void Deserialize9(BinaryReader reader, int version)
	{
		int num = reader.ReadInt32();
		long position = reader.BaseStream.Position;
		layer0 = new AvatarPartState(reader, version);
		layer1 = new AvatarPartState(reader, version);
		layer2 = new AvatarPartState(reader, version);
		layer3 = new AvatarPartState(reader, version);
		layer4 = new AvatarPartState(reader, version);
		reader.BaseStream.Position = position + num;
	}

	public override string ToString()
	{
		return $"AvatarState: head {layer0}, hat {layer3}, eyes {layer2}, facialhair {layer4}, mouth {layer1}";
	}
}
