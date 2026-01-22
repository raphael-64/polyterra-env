using System;
using Polytopia.Data;
using UnityEngine;

public class CityRewardReaction : ReactionBase
{
	private readonly CityRewardAction action;

	public CityRewardReaction(CityRewardAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		Tile instance = tile.GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.Render();
		}
		if (GameManager.IsPlayerViewing(action.PlayerId) && GameManager.Client.IsSpectating && GameManager.GameState.GameLogicData.TryGetData(ImprovementData.Type.City, out var data) && GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState))
		{
			CityReward[] cityRewardsForLevel = data.GetCityRewardsForLevel(tile.improvement.level - 1);
			RewardPopup rewardPopup = PopupManager.GetRewardPopup();
			rewardPopup.SetData(playerState, tile, cityRewardsForLevel, RewardPopup.PopupType.CityLevelUp, isReplay: true);
			rewardPopup.Show();
			AudioManager.PlaySFX(SFXTypes.RewardStart);
			rewardPopup.ReplayClickButton(action.Reward, delegate
			{
				onComplete();
			});
		}
		else
		{
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
