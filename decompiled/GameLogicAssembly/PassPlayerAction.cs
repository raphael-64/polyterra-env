using System.IO;

public class PassPlayerAction : ActionBase
{
	public PassPlayerAction()
	{
	}

	public PassPlayerAction(byte playerId)
		: base(playerId)
	{
	}

	public override void Execute(GameState state)
	{
		if (state.Version == 6)
		{
			ExecuteV6(state);
		}
		else
		{
			ExecuteV7(state);
		}
	}

	private void ExecuteV7(GameState state)
	{
		state.ActionStack.Add(new StartTurnAction(base.PlayerId));
	}

	private void ExecuteV6(GameState state)
	{
		state.ActionStack.Add(new StartTurnAction(state.CurrentPlayer));
	}

	public override ActionType GetActionType()
	{
		return ActionType.PassPlayer;
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
		return base.ToString();
	}
}
