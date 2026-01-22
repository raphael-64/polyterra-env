using System.IO;
using Polytopia.Data;

public class PacifistTask : TaskBase
{
	public const int PACIFIST_TARGET = 5;

	public int Turns { get; protected set; }

	public PacifistTask()
	{
	}

	public PacifistTask(bool completed)
		: base(completed)
	{
	}

	public PacifistTask(int turns)
	{
		Turns = turns;
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		base.IsCompleted = Turns >= 5;
		return base.IsCompleted;
	}

	public override bool Bump(GameState gameState, int amount = 1)
	{
		if (base.IsCompleted)
		{
			return true;
		}
		int turns = Turns + 1;
		Turns = turns;
		return Turns >= 5;
	}

	public override void Reset()
	{
		if (!base.IsCompleted)
		{
			Turns = 0;
		}
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Pacifist;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		return $"{Turns}/{5}";
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Turns);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Turns = reader.ReadInt32();
	}
}
