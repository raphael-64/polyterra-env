using System.IO;
using Polytopia.Data;

public class IncreaseCurrencyAction : ActionBase
{
	public const int DEFAULT_DELAY_MILLISECONDS = 40;

	public const int ADDITIONAL_DELAY_MILLISECONDS = 150;

	public int Delay { get; protected set; }

	public int Amount { get; protected set; }

	public WorldCoordinates Source { get; protected set; }

	public IncreaseCurrencyAction()
	{
	}

	public IncreaseCurrencyAction(byte playerId, WorldCoordinates source, int delay = 0)
		: base(playerId)
	{
		Delay = delay;
		Source = source;
		Amount = 1;
	}

	public IncreaseCurrencyAction(byte playerId, WorldCoordinates source, int amount, int delay = 0)
		: base(playerId)
	{
		Delay = delay;
		Source = source;
		Amount = amount;
	}

	public override void Execute(GameState state)
	{
		if (state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			playerState.Currency += Amount;
			if (state.TryGetTask(playerState, TaskData.Type.Wealth, out var task))
			{
				state.CheckTask(playerState, task);
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.IncreaseCurrency;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Delay);
		Source.Serialize(writer, version);
		if (version >= 9)
		{
			writer.Write(Amount);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Delay = reader.ReadInt32();
		Source = new WorldCoordinates(reader, version);
		if (version >= 9)
		{
			Amount = reader.ReadInt32();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Source: {Source}, Amount: {Amount}, Delay: {Delay})";
	}
}
