using System.IO;
using Polytopia.Data;

public class ExplorerTask : TaskBase
{
	public int ExploredTiles { get; set; }

	public ExplorerTask()
	{
	}

	public ExplorerTask(bool completed)
		: base(completed)
	{
	}

	public ExplorerTask(int exploredTiles)
	{
		ExploredTiles = exploredTiles;
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		ExploredTiles = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			if (gameState.Map.Tiles[i].GetExplored(playerState.Id))
			{
				ExploredTiles++;
			}
		}
		base.IsCompleted = ExploredTiles >= gameState.Map.Tiles.Length;
		return base.IsCompleted;
	}

	public override bool Bump(GameState gameState, int amount = 1)
	{
		if (base.IsCompleted)
		{
			return true;
		}
		ExploredTiles += amount;
		return ExploredTiles >= gameState.Map.Tiles.Length;
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Explorer;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		return $"{ExploredTiles}/{gameState.Map.Tiles.Length}";
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		if (version > 17)
		{
			writer.Write(ExploredTiles);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		if (version > 17)
		{
			ExploredTiles = reader.ReadInt32();
		}
	}
}
