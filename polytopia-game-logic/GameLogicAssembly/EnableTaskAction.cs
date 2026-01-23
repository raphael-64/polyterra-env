using System.IO;
using Polytopia.Data;

public class EnableTaskAction : ActionBase
{
	public TaskData.Type Type { get; protected set; }

	public EnableTaskAction()
	{
	}

	public EnableTaskAction(byte playerId, TaskData.Type type)
		: base(playerId)
	{
		Type = type;
	}

	public override ActionType GetActionType()
	{
		return ActionType.EnableTask;
	}

	public override void Execute(GameState state)
	{
		if (!state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		for (int i = 0; i < playerState.tasks.Count; i++)
		{
			if (playerState.tasks[i].GetTaskType() == Type)
			{
				return;
			}
		}
		TaskBase task = TaskBase.GetTask(Type);
		task.EnableTask();
		playerState.tasks.Add(task);
		state.CheckTask(playerState, task);
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((int)Type);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (TaskData.Type)reader.ReadInt32();
	}
}
