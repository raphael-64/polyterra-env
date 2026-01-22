using System;
using UI.Popups;
using UnityEngine;

public class InfiltrationRewardReaction : ReactionBase
{
	private readonly InfiltrationRewardAction action;

	public InfiltrationRewardReaction(InfiltrationRewardAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		TileData tileData = GameManager.GameState.Map.GetTile(action.Coordinates);
		Tile tile = MapRenderer.Current.GetTileInstance(action.Coordinates);
		AudioManager.PlaySFX(SFXTypes.Explode);
		tile.SpawnExplosion();
		tile.SpawnDarkPuff();
		tile.SpawnEmbers();
		tile.Improvement.UpdateObject();
		if ((Object)(object)tile.Unit != (Object)null)
		{
			tile.Unit.Sway();
			tile.Unit.UpdateObject();
		}
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, tile.Coordinates, shouldNudgeToCenter: false, 0.8f, delegate
			{
				if (GameManager.Client.IsSpectating)
				{
					onComplete();
				}
				else
				{
					ShowAttackerPopup();
				}
			});
			return;
		}
		if (!GameManager.IsPlayerViewing(tileData.owner))
		{
			GameManager.DelayCall(100, onComplete);
			return;
		}
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var playerState);
		ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, tile.Coordinates, shouldNudgeToCenter: false, 0.8f, delegate
		{
			if (GameManager.Client.IsSpectating)
			{
				onComplete();
			}
			else
			{
				ShowPopup();
			}
		});
		void OnRewardClicked()
		{
			onComplete();
		}
		void ShowAttackerPopup()
		{
			IconRewardPopup iconRewardPopup = PopupManager.GetIconRewardPopup();
			string popupTitle = CityRewardData.GetPopupTitle(action.Reward);
			string name = tileData.improvement.name;
			iconRewardPopup.Header = Localization.Get(popupTitle, name);
			iconRewardPopup.Description = Localization.Get("world.rebellion.attackerdescription", name);
			iconRewardPopup.SpriteHandle.Request(SpriteData.GetUISpriteAddress("riot_icon"));
			iconRewardPopup.RewardButton.sprite = null;
			iconRewardPopup.RewardButton.text = "";
			Sprite sprite = UIManager.IconData.GetSprite("Resource");
			_ = tile.Improvement;
			_ = tileData.improvement.level;
			string text = tileData.CalculateRawProduction(GameManager.GameState).ToString();
			iconRewardPopup.RewardButton.ShowIconAndTextContainer(sprite, text);
			iconRewardPopup.OnRewardClicked = OnRewardClicked;
			GameManager.DelayCall(300, iconRewardPopup.Show);
		}
		void ShowPopup()
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			string popupTitle = CityRewardData.GetPopupTitle(action.Reward);
			string name = tileData.improvement.name;
			basicPopup.Header = Localization.Get(popupTitle, name);
			string popupDescription = CityRewardData.GetPopupDescription(action.Reward);
			string arg = Localization.Get($"unit.names.{action.UnitType.GetName()}");
			basicPopup.Description = Localization.Get(popupDescription, arg, playerState.GetLinkedTribeNameWithSpace(GameManager.GameState), name);
			basicPopup.SetTribeInfoButtons(TextType.Description);
			basicPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					onComplete();
				})
			};
			basicPopup.Show();
		}
	}
}
