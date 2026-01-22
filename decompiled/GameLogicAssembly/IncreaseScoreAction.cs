using System.IO;

public class IncreaseScoreAction : ActionBase
{
	public int Amount { get; protected set; }

	public int Delay { get; protected set; }

	public WorldCoordinates Source { get; protected set; }

	public IncreaseScoreAction()
	{
	}

	public IncreaseScoreAction(byte playerId, int amount, WorldCoordinates source, int delay = 0)
		: base(playerId)
	{
		Amount = amount;
		Source = source;
		Delay = delay;
	}

	public override void Execute(GameState state)
	{
		if (state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			ActionUtils.AddScore(state, playerState, Amount);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.IncreaseScore;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Amount);
		writer.Write(Delay);
		Source.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Amount = reader.ReadInt32();
		Delay = reader.ReadInt32();
		Source = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Amount: {Amount}, Source: {Source}, Delay: {Delay})";
	}
}
