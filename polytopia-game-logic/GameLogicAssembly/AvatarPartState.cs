using System.IO;
using Polytopia.Data;

public struct AvatarPartState
{
	public AvatarPart.Type id;

	public int color;

	public AvatarPartState(BinaryReader reader, int version)
	{
		id = (AvatarPart.Type)reader.ReadInt32();
		color = reader.ReadInt32();
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write((int)id);
		writer.Write(color);
	}

	public override string ToString()
	{
		return $"AvatarPartState: id {id}, color {color}";
	}
}
