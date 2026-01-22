using System;

public class ReceiveDiplomacyMessageReaction : ReactionBase
{
	private readonly ReceiveDiplomacyMessageAction action;

	public ReceiveDiplomacyMessageReaction(ReceiveDiplomacyMessageAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		GameState gameState = GameManager.GameState;
		gameState.TryGetPlayer(action.PlayerId, out var player);
		gameState.TryGetPlayer(action.SenderId, out var otherPlayerState);
		gameState.GameLogicData.TryGetData(otherPlayerState.tribe, out var _);
		switch (action.MessageType)
		{
		case DiplomacyMessageType.PeaceRequest:
			onComplete();
			break;
		case DiplomacyMessageType.EstablishEmbassy:
			ReactionUtils.CameraFocusIfExplored(action.PlayerId, player.startTile, shouldNudgeToCenter: false, 0.8f, delegate
			{
				string linkedTribeNameWithSpace = otherPlayerState.GetLinkedTribeNameWithSpace(GameManager.GameState);
				Tile tileInstance = MapRenderer.Current.GetTileInstance(player.startTile);
				DiplomacyPopup diplomacyPopup = PopupManager.GetDiplomacyPopup();
				diplomacyPopup.Header = Localization.Get("diplomacy.establishedembassy.title");
				diplomacyPopup.Description = Localization.Get("diplomacy.establishedembassy.description2", linkedTribeNameWithSpace, gameState.GameLogicData.DiplomacyData.embassyIncome);
				diplomacyPopup.SetTribeInfoButtons(TextType.Description);
				diplomacyPopup.SetData(otherPlayerState, player, DiplomacyGraphics.Type.EmbassyEstablished);
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
				tileInstance.Render();
				AudioManager.PlaySFXAtTile(SFXTypes.Embassy, tileInstance.Coordinates);
				tileInstance.SpawnPuff();
				tileInstance.SpawnShine();
				tileInstance.SpawnSparkles(0.5f);
			});
			break;
		default:
			throw new Exception("Not implemented");
		}
	}
}
