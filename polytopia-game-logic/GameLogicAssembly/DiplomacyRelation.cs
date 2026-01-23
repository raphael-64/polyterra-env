using System.IO;

public class DiplomacyRelation
{
	private const int DEFAULT_TURN = -100;

	public DiplomacyRelationState State;

	public int LastAttackTurn = -100;

	public int EmbassyLevel;

	public int LastPeaceBrokenTurn = -100;

	public int FirstMeet = -100;

	public int EmbassyBuildTurn = -100;

	public int PreviousAttackTurn = -100;

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write((byte)State);
		writer.Write(LastAttackTurn);
		writer.Write((byte)EmbassyLevel);
		writer.Write(LastPeaceBrokenTurn);
		writer.Write(FirstMeet);
		writer.Write(EmbassyBuildTurn);
		if (version >= 81)
		{
			writer.Write(PreviousAttackTurn);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		State = (DiplomacyRelationState)reader.ReadByte();
		LastAttackTurn = reader.ReadInt32();
		EmbassyLevel = reader.ReadByte();
		LastPeaceBrokenTurn = reader.ReadInt32();
		FirstMeet = reader.ReadInt32();
		EmbassyBuildTurn = reader.ReadInt32();
		if (version >= 81)
		{
			PreviousAttackTurn = reader.ReadInt32();
		}
	}
}
