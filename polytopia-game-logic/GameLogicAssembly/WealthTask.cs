using Polytopia.Data;

public class WealthTask : TaskBase
{
	public const int WEALTH_TARGET = 100;

	public WealthTask()
	{
	}

	public WealthTask(bool completed)
		: base(completed)
	{
	}

	public override bool IsTaskCompleted(GameState gameState, PlayerState playerState)
	{
		if (base.IsCompleted)
		{
			return false;
		}
		base.IsCompleted = playerState.Currency >= 100;
		return base.IsCompleted;
	}

	public override TaskData.Type GetTaskType()
	{
		return TaskData.Type.Wealth;
	}

	public override string GetCompletionStatus(GameState gameState, PlayerState playerState)
	{
		return $"{playerState.Currency}/{100}";
	}
}
