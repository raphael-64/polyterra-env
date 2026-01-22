using System.IO;

public interface IBinarySerializable
{
	void Serialize(BinaryWriter writer, int version);

	void Deserialize(BinaryReader reader, int version);
}
