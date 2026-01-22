using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnTimerContainer : ResourceContainerBase
{
	private static float WARNING_THRESHOLD = 6.2f;

	private static Color DISABLED = new Color(1f, 1f, 1f, 0.25f);

	public List<UIGradient> topFades = new List<UIGradient>();

	public List<Image> images = new List<Image>();

	public UIRoundButton button;

	private int strikes = -1;

	private BasicPopup timerExplanationPopup;

	private bool isLowOnTime;

	private bool showAlert;

	private List<Color> topFadeOriginalColors = new List<Color>();

	private List<Color> colorOriginalColors = new List<Color>();

	private PollAnimation<float> pulseAnimation;

	public int Strikes
	{
		get
		{
			return strikes;
		}
		set
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			if (strikes != value)
			{
				strikes = value;
				if (strikes == 0)
				{
					button.bgColors.activeColor = ColorConstants.blue;
				}
				else if (strikes == 1)
				{
					button.bgColors.activeColor = ColorConstants.orange;
				}
				else if (strikes == 2)
				{
					button.bgColors.activeColor = ColorConstants.red;
				}
				else
				{
					button.bgColors.activeColor = Color.black;
				}
				button.buttonActive = true;
				button.shineActive = false;
			}
		}
	}

	private void Awake()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < topFades.Count; i++)
		{
			topFadeOriginalColors.Add(topFades[i].m_color1);
		}
		for (int j = 0; j < images.Count; j++)
		{
			colorOriginalColors.Add(((Graphic)images[j]).color);
		}
	}

	private void OnEnable()
	{
		GameEvents.OnFinishedProcessing += OnFinishedProcessing;
		ResetColors();
	}

	private void OnDisable()
	{
		GameEvents.OnFinishedProcessing -= OnFinishedProcessing;
		ResetColors();
	}

	private void Update()
	{
		if (GameManager.Client != null && GameManager.GameState != null && GameManager.Client.CurrentGameId.HasValue)
		{
			UpdateTimer();
			UpdateAlert();
		}
	}

	public void UpdateTimer()
	{
		if (GameManager.Client != null && GameManager.Client.CurrentGameId.HasValue)
		{
			TimeSpan? gameTimeLeftForLocalPlayer = GameManager.GetRemoteGameDataManager().GetGameTimeLeftForLocalPlayer(GameManager.Client.CurrentGameId.Value);
			((TMP_Text)amountLabel).text = FormatTimeSpanToString(gameTimeLeftForLocalPlayer ?? TimeSpan.Zero);
			if (gameTimeLeftForLocalPlayer.HasValue && gameTimeLeftForLocalPlayer.Value.TotalSeconds < (double)WARNING_THRESHOLD)
			{
				isLowOnTime = true;
			}
			else
			{
				isLowOnTime = false;
			}
		}
	}

	public void UpdateAlert()
	{
		if (isLowOnTime && GameManager.GameState != null && GameManager.GameState.CurrentState != GameState.State.Ended)
		{
			if (pulseAnimation == null)
			{
				pulseAnimation = new PollAnimation<float>(0.2f, Easing.OutQuad);
				showAlert = true;
				return;
			}
			if (pulseAnimation.IsCompleted())
			{
				if (showAlert)
				{
					AudioManager.PlaySFX(SFXTypes.Score);
					pulseAnimation = new PollAnimation<float>(0.8f, Easing.OutQuad);
					showAlert = false;
				}
				else
				{
					pulseAnimation = new PollAnimation<float>(0.2f, Easing.OutQuad);
					showAlert = true;
				}
			}
			if (pulseAnimation != null)
			{
				UpdateHeaderColors(pulseAnimation.GetEasedProgress());
			}
		}
		else if (pulseAnimation != null)
		{
			pulseAnimation = null;
			showAlert = false;
			UpdateHeaderColors(1f);
		}
	}

	private void UpdateHeaderColors(float alpha)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (topFades.Count == 0)
		{
			return;
		}
		if (showAlert)
		{
			for (int i = 0; i < topFades.Count; i++)
			{
				topFades[i].SetColor(Color.Lerp(topFadeOriginalColors[i], ColorConstants.red, alpha));
			}
			for (int j = 0; j < images.Count; j++)
			{
				((Graphic)images[j]).color = Color.Lerp(colorOriginalColors[j], ColorConstants.red, alpha);
			}
		}
		else
		{
			for (int k = 0; k < topFades.Count; k++)
			{
				topFades[k].SetColor(Color.Lerp(ColorConstants.red, topFadeOriginalColors[k], alpha));
			}
			for (int l = 0; l < images.Count; l++)
			{
				((Graphic)images[l]).color = Color.Lerp(ColorConstants.red, colorOriginalColors[l], alpha);
			}
		}
	}

	public static string FormatTimeSpanToString(TimeSpan timeSpan)
	{
		if (timeSpan.TotalSeconds < 0.0)
		{
			timeSpan = TimeSpan.Zero;
		}
		if (timeSpan.TotalDays > 1.0)
		{
			return string.Format("{0} {1}", Math.Floor(timeSpan.TotalDays), Localization.Get("date.days"));
		}
		if (timeSpan.TotalHours > 1.0)
		{
			return timeSpan.ToString("h':'mm");
		}
		return timeSpan.ToString("m':'ss");
	}

	private void ResetColors()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < topFades.Count; i++)
		{
			topFades[i].SetColor(topFadeOriginalColors[i]);
		}
		for (int j = 0; j < images.Count; j++)
		{
			((Graphic)images[j]).color = colorOriginalColors[j];
		}
	}

	private void OnFinishedProcessing()
	{
		if (!isLowOnTime)
		{
			ResetColors();
		}
	}

	public void TriggerTimerExplanationPopup()
	{
		if ((Object)(object)timerExplanationPopup == (Object)null)
		{
			timerExplanationPopup = PopupManager.GetBasicPopup();
		}
		if (!timerExplanationPopup.IsShowing())
		{
			timerExplanationPopup.Header = Localization.Get("gameitem.timelimit.timeleft");
			timerExplanationPopup.Description = string.Format(Localization.Get("gameitem.timelimit.timeleft.info"), Strikes, 3);
			timerExplanationPopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.back")
			};
			timerExplanationPopup.Show();
		}
	}
}
