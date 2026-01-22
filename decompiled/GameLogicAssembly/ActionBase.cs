using System.Collections.Generic;
using System.IO;

public abstract class ActionBase
{
	private List<ActionBase> subActions;

	public byte PlayerId { get; protected set; }

	public bool HoldForCommand { get; protected set; }

	public ActionBase()
	{
	}

	public ActionBase(byte playerId)
	{
		PlayerId = playerId;
	}

	public virtual void Execute(GameState state)
	{
	}

	public virtual bool IsValid(GameState state)
	{
		return true;
	}

	protected void AddSubAction(ActionBase subAction)
	{
		if (subActions == null)
		{
			subActions = new List<ActionBase>();
		}
		subActions.Add(subAction);
	}

	protected void CommitSubActionsToStack(List<ActionBase> actionStack)
	{
		if (subActions != null)
		{
			for (int num = subActions.Count - 1; num >= 0; num--)
			{
				actionStack.Add(subActions[num]);
			}
		}
	}

	public abstract ActionType GetActionType();

	public virtual void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(PlayerId);
	}

	public virtual void Deserialize(BinaryReader reader, int version)
	{
		PlayerId = reader.ReadByte();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {PlayerId})";
	}
}
