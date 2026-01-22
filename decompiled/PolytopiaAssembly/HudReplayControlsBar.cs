using DG.Tweening;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class HudReplayControlsBar : MonoBehaviour
{
	public CanvasGroup canvasGroup;

	public RectTransform rectTransform;

	[Header("Buttons")]
	public UITextButton skipReplayButton;

	private Tween buttonFade;

	private void OnEnable()
	{
		RefreshComponents();
		GameEvents.OnRecapStarted += ShowHUD;
		GameEvents.OnRecapEnded += HideHUD;
		GameEvents.OnCommandExecuted += OnCommandExecuted;
	}

	private void OnDisable()
	{
		GameEvents.OnRecapStarted -= ShowHUD;
		GameEvents.OnRecapEnded -= HideHUD;
		GameEvents.OnCommandExecuted -= OnCommandExecuted;
	}

	public void OnSkipReplay()
	{
		if (GameManager.Client == null || GameManager.Client.ActionManager == null || GameManager.Client.IsReplay || GameManager.Client.ActionManager.isAborting || GameManager.Client.ActionManager.IsSimulating)
		{
			return;
		}
		GameManager.Client.ActionManager.AbortExecution(delegate
		{
			if (GameManager.Client.HasTargetState())
			{
				GameManager.Client.ApplyTargetState();
			}
			else
			{
				GameManager.Client.ResetGameState(StateUpdateReason.StateReset);
			}
			ResourceEvents.RefreshWallets(GameManager.Client.GetCurrentLocalPlayer().Id);
		});
	}

	private void OnCommandExecuted()
	{
		if (GameManager.Client.IsRecap)
		{
			if (GameManager.Client.GetLastSeenCommand() > 1)
			{
				ShowHUD();
			}
			else
			{
				HideHUD();
			}
		}
	}

	private void ShowHUD()
	{
		if (GameManager.Client.GameState.Settings.GameType != GameType.PassAndPlay && !GameManager.Client.IsReplay && GameManager.Client.GetLastSeenCommand() > 1)
		{
			if (GameVersionUtils.HideEsport)
			{
				((Component)skipReplayButton).gameObject.SetActive(false);
			}
			else
			{
				((Component)skipReplayButton).gameObject.SetActive(true);
			}
			FadeButtons((Tween)(object)canvasGroup.DOFade(1f, 0.2f));
		}
	}

	private void HideHUD()
	{
		((Component)skipReplayButton).gameObject.SetActive(false);
		FadeButtons((Tween)(object)canvasGroup.DOFade(0f, 0.2f));
	}

	private void RefreshComponents()
	{
		if (GameManager.Client != null && GameManager.Client.ActionManager != null)
		{
			if (GameManager.Client.IsRecap)
			{
				ShowHUD();
			}
			else
			{
				HideHUD();
			}
			Tween obj = buttonFade;
			if (obj != null)
			{
				TweenExtensions.Complete(obj);
			}
		}
	}

	private void FadeButtons(Tween fade)
	{
		if (buttonFade != null && !TweenExtensions.IsComplete(buttonFade))
		{
			TweenExtensions.Complete(buttonFade);
		}
		buttonFade = fade;
	}

	private void OnDestroy()
	{
		Tween obj = buttonFade;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
	}
}
