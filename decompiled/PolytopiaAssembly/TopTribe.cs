using Polytopia.Data;

public class TopTribe
{
	public uint Score { get; }

	public TribeData.Type TribeType { get; }

	public TopTribe(uint score, TribeData.Type tribeType)
	{
		Score = score;
		TribeType = tribeType;
	}
}
