using System;
using Polytopia.Data;

public class ExamineRuinsReaction : ReactionBase
{
	private readonly ExamineRuinsAction action;

	public ExamineRuinsReaction(ExamineRuinsAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Coordinates);
		if (tileInstance.IsHidden)
		{
			tileInstance.StopRainbowFire();
			onComplete();
			return;
		}
		tileInstance.Render();
		tileInstance.SpawnShine();
		tileInstance.SpawnSparkles();
		AudioManager.PlaySFXAtTile(SFXTypes.Examine, tileInstance.Coordinates);
		if (!GameManager.IsPlayerViewing(action.PlayerId) || GameManager.Client.IsRecap)
		{
			onComplete();
			return;
		}
		Log.Learn("extra: examine_ruins", Array.Empty<object>());
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
			{
				onComplete();
			})
		};
		switch (action.Reward)
		{
		case RuinsReward.Seamonster:
		{
			GameManager.GameState.GameLogicData.TryGetData(UnitData.Type.Navalon, out var data3);
			iconPopup.Description = Localization.Get(arg0: iconPopup.Header = Localization.Get(data3.displayName), key: RuinsRewardData.GetRewardDescription(action.Reward));
			iconPopup.sprite = UIManager.IconData.GetSprite(RuinsReward.SuperUnit.ToString());
			break;
		}
		case RuinsReward.SuperUnit:
		{
			GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
			GameManager.GameState.GameLogicData.TryGetData(playerState.tribe, out var data);
			GameManager.GameState.GameLogicData.TryGetData(UnitData.Type.Giant, out var data2);
			iconPopup.Description = Localization.Get(arg0: iconPopup.Header = Localization.Get(GameManager.GameState.GameLogicData.GetOverride(data2, data).displayName), key: RuinsRewardData.GetRewardDescription(action.Reward));
			iconPopup.sprite = UIManager.IconData.GetSprite(action.Reward.ToString());
			break;
		}
		case RuinsReward.Swordsman:
			iconPopup.Header = Localization.Get(RuinsRewardData.GetRewardTitle(action.Reward));
			iconPopup.Description = Localization.Get(RuinsRewardData.GetRewardDescription(action.Reward));
			iconPopup.sprite = UIManager.IconData.GetSprite(RuinsReward.SuperUnit.ToString());
			break;
		case RuinsReward.FreeTech:
			iconPopup.Header = Localization.Get(RuinsRewardData.GetRewardTitle(action.Reward));
			iconPopup.Description = Localization.Get(RuinsRewardData.GetRewardDescription(action.Reward), Localization.Get(action.Tech.displayName));
			iconPopup.sprite = UIManager.IconData.GetSprite(action.Reward.ToString());
			break;
		default:
			iconPopup.Header = Localization.Get(RuinsRewardData.GetRewardTitle(action.Reward));
			iconPopup.Description = Localization.Get(RuinsRewardData.GetRewardDescription(action.Reward));
			iconPopup.sprite = UIManager.IconData.GetSprite(action.Reward.ToString());
			break;
		}
		iconPopup.Show();
	}
}
