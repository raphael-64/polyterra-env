using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultHeaderContainer : UIBasicComponent
{
	[SerializeField]
	protected Image gradient;

	[SerializeField]
	protected Image glow;

	[SerializeField]
	protected Image victoryIcon;

	[SerializeField]
	protected Image defeatIcon;

	[SerializeField]
	protected TextMeshProUGUI header;

	[SerializeField]
	protected TextMeshProUGUI description;

	protected bool won;

	protected Tween gradientScaleTween;

	protected Tween gradientFadeTween;

	protected Tween headerScaleTween;

	protected Tween glowScaleTween;

	protected Tween glowRotationTween;

	protected Tween glowFadeTween;

	protected Tween iconTween;

	public bool Won
	{
		set
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			won = value;
			((Graphic)gradient).color = (won ? ColorConstants.glowColor : ColorConstants.red);
			((Component)victoryIcon).gameObject.SetActive(won);
			((Component)defeatIcon).gameObject.SetActive(!won);
			((TMP_Text)header).text = Localization.Get(won ? "endscreen.victory" : "endscreen.gameover");
			if (GameManager.GameState.Settings.RulesGameMode == GameMode.Glory)
			{
				if (won && GameManager.LocalPlayer != null)
				{
					((TMP_Text)description).text = Localization.Get(GameModeUtils.GetWinMessage(GameManager.GameState.Settings.RulesGameMode), GameManager.GameState.Settings.rules.ScoreLimit, GameManager.LocalPlayer.GetLocalizedTribeName(GameManager.GameState));
				}
				else
				{
					((TMP_Text)description).text = Localization.Get(GameModeUtils.GetLooseMessage(GameManager.GameState.Settings.RulesGameMode));
				}
			}
			else
			{
				((TMP_Text)description).text = Localization.Get(won ? GameModeUtils.GetWinMessage(GameManager.GameState.Settings.RulesGameMode) : GameModeUtils.GetLooseMessage(GameManager.GameState.Settings.RulesGameMode));
			}
		}
	}

	public byte WinnerId
	{
		set
		{
			if (GameManager.GameState.Settings.GameType != GameType.SinglePlayer && value != 0 && GameManager.GameState.TryGetPlayer(value, out var playerState))
			{
				((TMP_Text)header).text = Localization.Get("endscreen.winner", playerState.UserName);
			}
		}
	}

	public void Show()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Expected O, but got Unknown
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		ClearAllTweens();
		float num = 0.6f;
		float num2 = 0.6f;
		((Transform)((Graphic)gradient).rectTransform).localScale = new Vector3(1f, 0f, 1f);
		gradientScaleTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScaleY((Transform)(object)((Graphic)gradient).rectTransform, 1f, num), (Ease)27, 5f), num2);
		gradientFadeTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Color, Color, ColorOptions>>(gradient.DOFade(0f, 2f), num2 + num), TweenUtils.GetRoughEase());
		((Transform)((TMP_Text)header).rectTransform).localScale = Vector3.zero;
		headerScaleTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((TMP_Text)header).rectTransform, 1f, num), (Ease)27, 5f), num2);
		((Transform)((Graphic)glow).rectTransform).localScale = Vector3.zero;
		glowScaleTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((Graphic)glow).rectTransform, Vector3.one, num), (Ease)27, 5f), num2);
		glowRotationTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Quaternion, Vector3, QuaternionOptions>>(ShortcutExtensions.DORotate((Transform)(object)((Graphic)glow).rectTransform, new Vector3(0f, 0f, 360f), 20f, (RotateMode)2), (Ease)1), -1);
		glowFadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Color, Color, ColorOptions>>(glow.DOFade(0f, 1.6f), num2 + num + 1f), new TweenCallback(OnGlowFaded));
		if (won)
		{
			((Transform)((Graphic)victoryIcon).rectTransform).localScale = Vector3.zero;
			iconTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((Graphic)victoryIcon).rectTransform, Vector3.one, num), (Ease)27, 5f), num2);
		}
		else
		{
			((Transform)((Graphic)defeatIcon).rectTransform).localScale = Vector3.zero;
			iconTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((Graphic)defeatIcon).rectTransform, Vector3.one, num), (Ease)27, 5f), num2);
		}
	}

	private void OnGlowFaded()
	{
		TweenUtils.KillTween(glowRotationTween, complete: true);
	}

	public void Clear()
	{
		ClearAllTweens();
	}

	protected void ClearAllTweens()
	{
		TweenUtils.KillTween(gradientScaleTween);
		TweenUtils.KillTween(gradientFadeTween);
		TweenUtils.KillTween(headerScaleTween);
		TweenUtils.KillTween(glowScaleTween);
		TweenUtils.KillTween(glowRotationTween, complete: true);
		TweenUtils.KillTween(glowFadeTween);
		TweenUtils.KillTween(iconTween);
	}
}
