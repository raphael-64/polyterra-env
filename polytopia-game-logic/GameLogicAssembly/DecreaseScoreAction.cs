using System.IO;

public class DecreaseScoreAction : ActionBase
{
	public int Amount { get; protected set; }

	public int Delay { get; protected set; }

	public WorldCoordinates Source { get; protected set; }

	public DecreaseScoreAction()
	{
	}

	public DecreaseScoreAction(byte playerId, int amount)
		: base(playerId)
	{
		Amount = amount;
	}

	public override void Execute(GameState state)
	{
		if (state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			ActionUtils.RemoveScore(playerState, Amount);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.DecreaseScore;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Amount);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Amount = reader.ReadInt32();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Amount: {Amount}";
	}
}
