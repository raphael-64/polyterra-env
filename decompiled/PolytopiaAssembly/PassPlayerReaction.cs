using System;

public class PassPlayerReaction : ReactionBase
{
	private readonly PassPlayerAction action;

	public PassPlayerReaction(PassPlayerAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.Client.IsRecap)
		{
			onComplete();
			return;
		}
		GameManager.GameState.TryGetPlayer(GameManager.GameState.CurrentPlayer, out var playerState);
		ShowOverlay(playerState, instant: false, onComplete);
	}

	public static void ShowOverlay(PlayerState nextPlayer, bool instant = false, Action onComplete = null)
	{
		NotificationManager.HideAlert();
		if (GameManager.GameState == null)
		{
			return;
		}
		HotSeatOverlay.Player = nextPlayer;
		Action continueCallback = null;
		continueCallback = delegate
		{
			if (GameManager.Client.ActionManager.isAborting)
			{
				Log.Verbose("Aborting hotseat pass", Array.Empty<object>());
				onComplete();
			}
			else
			{
				Log.Verbose("Player {0}'s turn, last seen command is: {1}/{2}", new object[3]
				{
					nextPlayer.Id,
					GameManager.Client.GetLastSeenCommand(),
					GameManager.GameState.LastProcessedCommand
				});
				InputManager.ResetInputBlocker();
				InputManager.BlockInputIfShowingLoadingScreen();
				if ((GameManager.Client?.CurrentGameId).HasValue)
				{
					GameManager.Client.SaveSession(GameManager.Client.CurrentGameId.Value, showSaveErrorPopup: true);
				}
				GameManager.Client.RewindToCommand(GameManager.Client.GetLastSeenCommand(), delegate(bool success)
				{
					if (!success)
					{
						Log.Verbose("Failed to rewind", Array.Empty<object>());
						HotSeatOverlay.ContinueCallback = continueCallback;
					}
					else
					{
						MapRenderer.Current.RenderMap(GameManager.GameState.Map);
						ResourceEvents.RefreshWallets(nextPlayer.Id);
						if (GameManager.GameState.CurrentTurn == 0)
						{
							GameManager.Client.ActionManager.Resume();
						}
						GameEvents.PassPlayer();
						if (UIManager.Instance.CurrentScreen == UIConstants.Screens.HotSeatOverlay)
						{
							UIManager.Instance.OnBack();
						}
					}
				});
				onComplete?.Invoke();
			}
		};
		HotSeatOverlay.ContinueCallback = continueCallback;
		HotSeatOverlay.ShowCompleteCallback = delegate
		{
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			if (nextPlayer.AutoPlay)
			{
				if (onComplete != null)
				{
					onComplete();
				}
				else
				{
					GameManager.Client.ActionManager.Resume();
				}
			}
			else
			{
				CameraController.Instance.CenterOnPosition(nextPlayer.startTile.ToPosition(), 0f);
			}
		};
		HotSeatOverlay.HideCompleteCallback = delegate
		{
			if (GameManager.GameState.CurrentTurn != 0)
			{
				if (!GameManager.Client.ActionManager.IsProcessing)
				{
					GameManager.Client.ActionManager.Resume();
				}
				GameEvents.PassPlayer();
			}
		};
		HotSeatOverlay.ExitCallback = null;
		if (UIManager.Instance.CurrentScreen == UIConstants.Screens.HotSeatOverlay)
		{
			UIManager.Instance.OnBack();
			instant = true;
		}
		UIManager.Instance.ShowScreen(UIConstants.Screens.HotSeatOverlay, instant);
	}
}
