using System;

public class PeaceTreatyReaction : ReactionBase
{
	private readonly PeaceTreatyAction action;

	public PeaceTreatyReaction(PeaceTreatyAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		GameManager.GameState.TryGetPlayer(action.PlayerId, out var _);
		GameManager.GameState.TryGetPlayer(action.OpponentId, out var playerState2);
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			AudioManager.PlaySFX(SFXTypes.TreatySend);
			NotificationManager.Notify(Localization.Get("diplomacy.peacetreaty.notification.description", playerState2.tribe), Localization.Get("action.info.peacetreaty"));
		}
		onComplete();
	}
}
