using Polytopia.Data;

public class NetworkTask : TaskBase
{
	public const int NETWORK_TARGET = 5;

	public NetworkTask()
	{
	}

	public NetworkTask(bool completed)
		: base(completed)
	{
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.owner == playerState.Id && tileData.HasImprovement(ImprovementData.Type.City) && !(tileData.coordinates == playerState.startTile) && tileData.improvement.connectedToCapitalOfPlayer == playerState.Id)
			{
				num++;
			}
		}
		base.IsCompleted = num >= 5;
		return base.IsCompleted;
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Network;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		int num = 0;
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.owner == playerState.Id && tileData.HasImprovement(ImprovementData.Type.City) && !(tileData.coordinates == playerState.startTile) && tileData.improvement.connectedToCapitalOfPlayer == playerState.Id)
			{
				num++;
			}
		}
		return $"{num}/{5}";
	}
}
