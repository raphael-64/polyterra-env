using System;
using System.IO;
using Polytopia.Data;

public class TaskBase
{
	public bool IsStarted { get; protected set; }

	public bool IsCompleted { get; protected set; }

	public TaskBase()
	{
	}

	public TaskBase(bool completed)
	{
		IsStarted = true;
		IsCompleted = true;
	}

	public virtual void EnableTask()
	{
		IsStarted = true;
	}

	public virtual bool Bump(GameState gameState, int amount = 1)
	{
		return false;
	}

	public virtual void Reset()
	{
	}

	public virtual bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		return false;
	}

	protected virtual void OnTaskCompleted(GameState gameState, PlayerState playerState)
	{
		IsCompleted = true;
		gameState.ActionStack.Add(new TaskCompletedAction(playerState.Id, GetTaskType()));
	}

	public virtual TaskData.Type GetTaskType()
	{
		return TaskData.Type.None;
	}

	public virtual string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		return string.Empty;
	}

	public virtual void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(IsStarted);
		writer.Write(IsCompleted);
	}

	public virtual void Deserialize(BinaryReader reader, int version)
	{
		IsStarted = reader.ReadBoolean();
		IsCompleted = reader.ReadBoolean();
	}

	public static TaskBase GetTask(TaskData.Type type)
	{
		return type switch
		{
			TaskData.Type.Pacifist => new PacifistTask(), 
			TaskData.Type.Genius => new GeniusTask(), 
			TaskData.Type.Network => new NetworkTask(), 
			TaskData.Type.Wealth => new WealthTask(), 
			TaskData.Type.Killer => new KillerTask(), 
			TaskData.Type.Metropolis => new MetropolisTask(), 
			TaskData.Type.Explorer => new ExplorerTask(), 
			_ => throw new Exception("Task type " + type.ToString() + " is not implemented"), 
		};
	}

	public static void SerializeTask(TaskBase task, BinaryWriter writer, int version)
	{
		writer.Write((ushort)task.GetTaskType());
		task.Serialize(writer, version);
	}

	public static TaskBase DeserializeTask(BinaryReader reader, int version)
	{
		TaskBase task = GetTask((TaskData.Type)reader.ReadUInt16());
		task.Deserialize(reader, version);
		return task;
	}
}
