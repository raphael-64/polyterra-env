using System;
using Polytopia.Data;

public class CityRewardPopupReaction : ReactionBase
{
	private readonly CityRewardPopupAction action;

	public CityRewardPopupReaction(CityRewardPopupAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.GameState.GameLogicData.TryGetData(ImprovementData.Type.City, out var data))
		{
			onComplete();
			return;
		}
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		CityReward[] cityRewardsForLevel = data.GetCityRewardsForLevel(tile.improvement.level - 1);
		RewardPopup rewardPopup = PopupManager.GetRewardPopup();
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
		rewardPopup.SetData(playerState, tile, cityRewardsForLevel, RewardPopup.PopupType.CityLevelUp);
		rewardPopup.RewardChoosenCallback = delegate(TileData tileData, CityReward cityReward)
		{
			CityRewardCommand command = new CityRewardCommand(GameManager.GameState.CurrentPlayer, cityReward, tileData.coordinates);
			if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
			{
				GameManager.Client.SendCommand(command);
			}
			AudioManager.PlaySFX(SFXTypes.RewardEnd);
			onComplete();
		};
		rewardPopup.Show();
		AudioManager.PlaySFX(SFXTypes.RewardStart);
	}
}
