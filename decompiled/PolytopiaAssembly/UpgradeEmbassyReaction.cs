using System;

public class UpgradeEmbassyReaction : ReactionBase
{
	private readonly UpgradeEmbassyAction action;

	public UpgradeEmbassyReaction(UpgradeEmbassyAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		ResourceEvents.IncomeChanged(action.PlayerId);
		ResourceEvents.IncomeChanged(action.OpponentId);
		GameState gameState = GameManager.GameState;
		gameState.TryGetPlayer(action.PlayerId, out var player);
		gameState.TryGetPlayer(action.OpponentId, out var opponent);
		Tile capitalTile = MapRenderer.Current.GetTileInstance(opponent.startTile);
		capitalTile.Render();
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			ReactionUtils.CameraFocusIfExplored(action.PlayerId, opponent.startTile, shouldNudgeToCenter: false, 0.8f, delegate
			{
				ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, gameState.GameLogicData.DiplomacyData.embassyUpgradeCost, null, "Upgrade embassy");
				int embassyLevel = player.GetEmbassyLevel(opponent);
				IconPopup iconPopup = PopupManager.GetIconPopup();
				iconPopup.Header = Localization.Get("diplomacy.upgradeembassy.title");
				iconPopup.Description = Localization.Get("diplomacy.upgradeembassy.description", opponent.tribe, gameState.GameLogicData.DiplomacyData.embassyIncome, embassyLevel);
				iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddress(opponent.GetTribeStyle(gameState)));
				iconPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
					{
						onComplete();
					})
				};
				iconPopup.Show();
				AudioManager.PlaySFXAtTile(SFXTypes.Embassy, capitalTile.Coordinates);
			});
		}
		else if (GameManager.IsPlayerViewing(action.OpponentId))
		{
			ReactionUtils.CameraFocusIfExplored(action.OpponentId, player.startTile, shouldNudgeToCenter: false, 0.8f, delegate
			{
				int embassyLevel = player.GetEmbassyLevel(opponent);
				IconPopup iconPopup = PopupManager.GetIconPopup();
				iconPopup.Header = Localization.Get("diplomacy.upgradeembassy.title");
				iconPopup.Description = Localization.Get("diplomacy.upgradeembassy.description2", player.tribe, gameState.GameLogicData.DiplomacyData.embassyIncome, embassyLevel);
				iconPopup.spriteHandle.Request(SpriteData.GetHeadSpriteAddress(player.GetTribeStyle(GameManager.GameState)));
				iconPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
					{
						onComplete();
					})
				};
				iconPopup.Show();
			});
		}
		else
		{
			onComplete();
		}
	}
}
