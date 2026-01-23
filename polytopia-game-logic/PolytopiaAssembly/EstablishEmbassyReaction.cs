using System;

public class EstablishEmbassyReaction : ReactionBase
{
	private readonly EstablishEmbassyAction action;

	public EstablishEmbassyReaction(EstablishEmbassyAction action)
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
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			if (UIManager.Instance.CurrentScreen == UIConstants.Screens.StatsScreen)
			{
				UIManager.Instance.OnBack();
			}
			ReactionUtils.CameraFocusIfExplored(action.PlayerId, opponent.startTile, shouldNudgeToCenter: false, 0.8f, delegate
			{
				ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, gameState.GameLogicData.DiplomacyData.embassyCost, null, "Build embassy");
				string linkedTribeNameWithSpace = opponent.GetLinkedTribeNameWithSpace(GameManager.GameState);
				DiplomacyPopup diplomacyPopup = PopupManager.GetDiplomacyPopup();
				diplomacyPopup.Header = Localization.Get("diplomacy.establishedembassy.title");
				diplomacyPopup.Description = Localization.Get("diplomacy.establishedembassy.description", linkedTribeNameWithSpace, gameState.GameLogicData.DiplomacyData.embassyIncome);
				diplomacyPopup.SetTribeInfoButtons(TextType.Description);
				diplomacyPopup.SetData(opponent, player, DiplomacyGraphics.Type.EmbassyEstablished);
				diplomacyPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
					{
						if (!GameManager.Client.IsSpectating)
						{
							onComplete();
						}
					}, -1, !GameManager.Client.IsSpectating)
				};
				if (GameManager.Client.IsSpectating)
				{
					diplomacyPopup.ReplayClickButton(0, delegate
					{
						onComplete();
					}, 2f);
				}
				GameManager.DelayCall(1000, diplomacyPopup.Show);
				capitalTile.Render();
				AudioManager.PlaySFXAtTile(SFXTypes.Embassy, capitalTile.Coordinates);
				capitalTile.SpawnPuff();
				capitalTile.SpawnShine();
				capitalTile.SpawnSparkles(0.5f);
			});
		}
		else
		{
			capitalTile.Render();
			onComplete();
		}
	}
}
