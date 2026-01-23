using System.IO;
using Polytopia.Data;

public class ResourceState
{
	public ResourceData.Type type;

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write((ushort)type);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		type = (ResourceData.Type)reader.ReadUInt16();
	}
}
