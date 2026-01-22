using System;
using PolytopiaBackendBase.Game;

public class StartTurnReaction : ReactionBase
{
	private readonly StartTurnAction action;

	public StartTurnReaction(StartTurnAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		NotificationManager.UpdateIngameAlert();
		PlayerState currentLocalPlayer = GameManager.Client.GetCurrentLocalPlayer();
		if (GameManager.Client.IsRecap)
		{
			if (GameManager.Client.IsReplay)
			{
				ReplayClient replayClient = GameManager.Client as ReplayClient;
				MapRenderer.Current.Refresh();
				ResourceEvents.ResourceChanged(currentLocalPlayer.Id);
				if (GameManager.GameState.CurrentPlayer != byte.MaxValue && replayClient.GetAutoSwitchPlayers())
				{
					CameraController.Instance.CenterOnPosition(currentLocalPlayer.startTile.ToPosition(), 1f, delegate
					{
						onComplete();
					});
				}
				else
				{
					onComplete();
				}
				GameEvents.TurnStarted();
			}
			else
			{
				onComplete();
				GameEvents.TurnStarted();
			}
			return;
		}
		MapRenderer.Current.Refresh();
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			GameEvents.TurnStarted();
			return;
		}
		if (GameManager.GameState.Settings.GameType != GameType.PassAndPlay && !GameManager.Client.IsSpectating)
		{
			DoStartTurnNotification();
		}
		GameManager.DelayCall(500, onComplete);
		GameEvents.TurnStarted();
	}

	private void DoStartTurnNotification()
	{
		string empty = string.Empty;
		string message = string.Empty;
		if (GameManager.GameState.Settings.rules.TurnLimit > 0)
		{
			if (GameManager.GameState.CurrentTurn >= GameManager.GameState.Settings.rules.TurnLimit)
			{
				empty = Localization.Get("world.turn.last");
			}
			else
			{
				empty = Localization.Get("world.turn.your");
				message = Localization.Get("world.turn.remaining", GameManager.GameState.Settings.rules.TurnLimit - GameManager.GameState.CurrentTurn);
			}
		}
		else
		{
			empty = string.Format("{0}!", Localization.Get("world.turn.your"));
		}
		if (GameManager.GameState.Settings.GameType == GameType.Multiplayer || GameManager.GameState.Settings.GameType == GameType.Competitive || GameManager.GameState.Settings.GameType == GameType.Matchmaking)
		{
			AudioManager.PlaySFX(SFXTypes.Discover);
		}
		NotificationManager.ShowCenterNotification(empty, message, 1f);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
