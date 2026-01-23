using System;
using Polytopia.Data;

public class PeaceRequestResponseReaction : ReactionBase
{
	private readonly PeaceRequestResponseAction action;

	public PeaceRequestResponseReaction(PeaceRequestResponseAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		GameManager.GameState.TryGetPlayer(action.SenderId, out var sender);
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var player);
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			if (action.Accepted)
			{
				foreach (Unit renderedUnit in MapRenderer.Current.RenderedUnits)
				{
					if (renderedUnit.State.owner == action.SenderId)
					{
						renderedUnit.UpdateObject();
					}
				}
				MapRenderer.Current.GetTileInstance(sender.startTile).RenderImprovement();
				MapRenderer.Current.GetTileInstance(player.startTile).RenderImprovement();
				ResourceEvents.IncomeChanged(player.Id);
				AudioManager.PlaySFX(SFXTypes.TreatyAccept);
				NotificationManager.Notify(Localization.Get("diplomacy.acceptpeace.notification.description", sender.UserName), Localization.Get("action.info.peacetreaty"), player, sender, DiplomacyGraphics.Type.PeaceTreatyAccepted);
			}
			else
			{
				AudioManager.PlaySFX(SFXTypes.TreatyReject);
				NotificationManager.Notify(Localization.Get("diplomacy.denypeace.notification.description", sender.UserName), Localization.Get("diplomacy.denypeace.notification.title"), player, sender, DiplomacyGraphics.Type.PeaceTreatyRejected);
			}
			onComplete();
		}
		else if (GameManager.IsPlayerViewing(action.SenderId))
		{
			if (action.Accepted)
			{
				MapRenderer.Current.GetTileInstance(sender.startTile).RenderImprovement();
				MapRenderer.Current.GetTileInstance(player.startTile).RenderImprovement();
				ResourceEvents.IncomeChanged(sender.Id);
				WorldCoordinates currentCapitalCoordinates = player.GetCurrentCapitalCoordinates(GameManager.GameState);
				ReactionUtils.CameraFocusIfExplored(action.PlayerId, currentCapitalCoordinates, shouldNudgeToCenter: false, 0.8f, delegate
				{
					string linkedTribeNameWithSpace = player.GetLinkedTribeNameWithSpace(GameManager.GameState);
					string localizedTribeName = player.GetLocalizedTribeName(GameManager.GameState);
					DiplomacyPopup diplomacyPopup = PopupManager.GetDiplomacyPopup();
					diplomacyPopup.Header = Localization.Get("diplomacy.acceptpeace.title", localizedTribeName);
					diplomacyPopup.Description = Localization.Get("diplomacy.acceptpeace.description", linkedTribeNameWithSpace);
					diplomacyPopup.SetTribeInfoButtons(TextType.Description);
					diplomacyPopup.SetData(sender, player, DiplomacyGraphics.Type.PeaceTreatyAccepted);
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
					diplomacyPopup.Show();
					AudioManager.PlaySFX(SFXTypes.TreatyAccept);
				});
				return;
			}
			ReactionUtils.CameraFocusIfExplored(action.PlayerId, player.startTile, shouldNudgeToCenter: false, 0.8f, delegate
			{
				string linkedTribeNameWithSpace = player.GetLinkedTribeNameWithSpace(GameManager.GameState);
				DiplomacyPopup diplomacyPopup = PopupManager.GetDiplomacyPopup();
				diplomacyPopup.Header = Localization.Get("diplomacy.denypeace.title", linkedTribeNameWithSpace);
				diplomacyPopup.Description = Localization.Get("diplomacy.denypeace.description", linkedTribeNameWithSpace);
				diplomacyPopup.SetTribeInfoButtons(TextType.Header);
				diplomacyPopup.SetTribeInfoButtons(TextType.Description);
				diplomacyPopup.SetData(sender, player, DiplomacyGraphics.Type.PeaceTreatyRejected);
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
				diplomacyPopup.Show();
				AudioManager.PlaySFX(SFXTypes.TreatyReject);
			});
		}
		else if (action.Accepted && localPlayer.KnowsPlayer(action.PlayerId) && localPlayer.KnowsPlayer(action.SenderId) && localPlayer.HasTech(TechData.Type.Diplomacy))
		{
			NotificationManager.Notify(Localization.Get("diplomacy.acceptpeace.notification.description2", sender.GetLocalizedTribeName(GameManager.GameState), player.GetLocalizedTribeName(GameManager.GameState)), Localization.Get("player.abilities.peacetreaty"), player, sender, DiplomacyGraphics.Type.PeaceTreatyAccepted);
			onComplete();
		}
		else
		{
			onComplete();
		}
	}
}
