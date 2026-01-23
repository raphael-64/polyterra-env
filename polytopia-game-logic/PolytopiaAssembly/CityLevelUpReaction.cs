using System;
using System.Collections.Generic;

public class CityLevelUpReaction : ReactionBase
{
	private readonly CityLevelUpAction action;

	public CityLevelUpReaction(CityLevelUpAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return true;
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (instance.IsHidden)
		{
			onComplete();
			return;
		}
		if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
		{
			Log.Learn("extra: city_level [{0}]", new object[1] { instance.Data.improvement.level });
			GameManager.GetAnalyticsManager().SendEvent("level_up", new Dictionary<string, object>
			{
				{
					"game_id",
					GameManager.Client.CurrentGameId
				},
				{
					"level",
					instance.Data.improvement.level
				}
			});
		}
		instance.LevelUpCity(onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
