using System.IO;
using Polytopia.Data;

public class KillerTask : TaskBase
{
	public const int KILLER_TARGET = 10;

	public int Kills { get; protected set; }

	public KillerTask()
	{
	}

	public KillerTask(bool completed)
		: base(completed)
	{
	}

	public KillerTask(int kills)
	{
		Kills = kills;
	}

	public override void EnableTask()
	{
		base.EnableTask();
		Kills++;
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		base.IsCompleted = Kills >= 10;
		return base.IsCompleted;
	}

	public override bool Bump(GameState gameState, int amount = 1)
	{
		if (base.IsCompleted)
		{
			return true;
		}
		int kills = Kills + 1;
		Kills = kills;
		return Kills >= 10;
	}

	public override void Reset()
	{
		if (!base.IsCompleted)
		{
			Kills = 0;
		}
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Killer;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		return $"{Kills}/{10}";
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Kills);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Kills = reader.ReadInt32();
	}
}
