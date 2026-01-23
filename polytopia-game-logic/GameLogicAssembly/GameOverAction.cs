using System.IO;

public class GameOverAction : ActionBase
{
	public byte WinningPlayerId { get; protected set; }

	public GameOverAction()
	{
	}

	public GameOverAction(byte playerId, byte winningPlayerId)
		: base(playerId)
	{
		WinningPlayerId = winningPlayerId;
	}

	public override bool IsValid(GameState state)
	{
		if (state.CurrentState != GameState.State.FinalTurn)
		{
			return state.CurrentState != GameState.State.Ended;
		}
		return false;
	}

	public override void Execute(GameState state)
	{
		if (state.TryGetPlayer(WinningPlayerId, out var playerState))
		{
			Log.Info("GAME OVER, {0}({1}) WINS!", new object[2] { playerState.UserName, playerState.tribe });
		}
		state.CurrentState = GameState.State.FinalTurn;
	}

	public override ActionType GetActionType()
	{
		return ActionType.GameOver;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(WinningPlayerId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		WinningPlayerId = reader.ReadByte();
	}
}
