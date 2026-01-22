using System;
using Polytopia.Data;

public class MetropolisTask : TaskBase
{
	public const int METROPOLIS_TARGET = 5;

	public MetropolisTask()
	{
	}

	public MetropolisTask(bool completed)
		: base(completed)
	{
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.owner == playerState.Id && tileData.HasImprovement(ImprovementData.Type.City) && tileData.improvement.level >= 5)
			{
				base.IsCompleted = true;
				return base.IsCompleted;
			}
		}
		return false;
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Metropolis;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		int num = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.owner == playerState.Id && tileData.HasImprovement(ImprovementData.Type.City))
			{
				num = Math.Max(tileData.improvement.level, num);
			}
		}
		return $"{num}/{5}";
	}
}
