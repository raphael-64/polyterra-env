using System.Collections.Generic;
using Polytopia.Data;

public class GeniusTask : TaskBase
{
	public GeniusTask()
	{
	}

	public GeniusTask(bool completed)
		: base(completed)
	{
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		bool isCompleted = true;
		if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			List<TechData> allTechForTribe = gameState.GameLogicData.GetAllTechForTribe(data);
			for (int i = 0; i < allTechForTribe.Count; i++)
			{
				if (!playerState.HasTech(allTechForTribe[i].type))
				{
					isCompleted = false;
					break;
				}
			}
		}
		base.IsCompleted = isCompleted;
		return base.IsCompleted;
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Genius;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			List<TechData> allTechForTribe = gameState.GameLogicData.GetAllTechForTribe(data);
			List<TechData.Type> availableTech = playerState.availableTech;
			return $"{availableTech.Count}/{allTechForTribe.Count}";
		}
		return base.GetCompletionStatus(gameState, playerState);
	}
}
