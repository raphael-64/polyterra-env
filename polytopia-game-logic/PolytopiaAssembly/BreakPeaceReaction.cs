using System;
using Polytopia.Data;

public class BreakPeaceReaction : ReactionBase
{
	private readonly BreakPeaceAction action;

	public BreakPeaceReaction(BreakPeaceAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		GameManager.GameState.TryGetPlayer(action.OpponentId, out var opponent);
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var player);
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			foreach (Unit renderedUnit in MapRenderer.Current.RenderedUnits)
			{
				if (renderedUnit.State.owner == action.OpponentId || renderedUnit.State.owner == action.PlayerId)
				{
					renderedUnit.UpdateObject();
				}
			}
			MapRenderer.Current.GetTileInstance(opponent.startTile).RenderImprovement();
			MapRenderer.Current.GetTileInstance(player.startTile).RenderImprovement();
			ResourceEvents.IncomeChanged(player.Id);
			NotificationManager.Notify(Localization.Get("diplomacy.breakpeace.notification.description", opponent.UserName), Localization.Get("action.info.breakpeace"), player, opponent, DiplomacyGraphics.Type.PeaceTreatyRejected);
			AudioManager.PlaySFX(SFXTypes.TreatyReject);
			onComplete();
		}
		else if (GameManager.IsPlayerViewing(action.OpponentId))
		{
			MapRenderer.Current.GetTileInstance(opponent.startTile).RenderImprovement();
			MapRenderer.Current.GetTileInstance(player.startTile).RenderImprovement();
			ResourceEvents.IncomeChanged(opponent.Id);
			WorldCoordinates currentCapitalCoordinates = player.GetCurrentCapitalCoordinates(GameManager.GameState);
			ReactionUtils.CameraFocusIfExplored(action.PlayerId, currentCapitalCoordinates, shouldNudgeToCenter: false, 0.8f, delegate
			{
				string linkedTribeNameWithSpace = player.GetLinkedTribeNameWithSpace(GameManager.GameState);
				DiplomacyPopup diplomacyPopup = PopupManager.GetDiplomacyPopup();
				diplomacyPopup.Header = Localization.Get("diplomacy.breakpeace.title", linkedTribeNameWithSpace);
				diplomacyPopup.Description = Localization.Get("diplomacy.breakpeace.description", linkedTribeNameWithSpace);
				diplomacyPopup.SetTribeInfoButtons(TextType.Header);
				diplomacyPopup.SetTribeInfoButtons(TextType.Description);
				diplomacyPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
					{
						onComplete();
					})
				};
				diplomacyPopup.SetData(player, opponent, DiplomacyGraphics.Type.PeaceTreatyRejected);
				diplomacyPopup.Show();
				AudioManager.PlaySFX(SFXTypes.TreatyReject);
			});
		}
		else if (localPlayer.KnowsPlayer(action.PlayerId) && localPlayer.KnowsPlayer(action.OpponentId) && localPlayer.HasTech(TechData.Type.Diplomacy))
		{
			NotificationManager.Notify(Localization.Get("diplomacy.breakpeace.notification.description2", opponent.GetLocalizedTribeName(GameManager.GameState), player.GetLocalizedTribeName(GameManager.GameState)), Localization.Get("action.info.breakpeace"), player, opponent, DiplomacyGraphics.Type.PeaceTreatyRejected);
			onComplete();
		}
		else
		{
			onComplete();
		}
	}
}
