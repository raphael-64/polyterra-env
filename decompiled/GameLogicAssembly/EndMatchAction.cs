using System.IO;

public class EndMatchAction : ActionBase
{
	public EndMatchAction()
	{
	}

	public EndMatchAction(byte playerId)
		: base(playerId)
	{
	}

	public override void Execute(GameState state)
	{
	}

	public override ActionType GetActionType()
	{
		return ActionType.EndMatch;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId})";
	}
}
