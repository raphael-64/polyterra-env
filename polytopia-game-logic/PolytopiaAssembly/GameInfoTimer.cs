using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInfoTimer : UIBasicComponent
{
	private enum Mode
	{
		None,
		CountDown,
		CountUp
	}

	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected TextMeshProUGUI timeLabel;

	[SerializeField]
	protected TextMeshProUGUI subLabel;

	[SerializeField]
	protected TextMeshProUGUI superLabel;

	[SerializeField]
	protected Image icon;

	public Button button;

	private Mode mode;

	private Guid gameId;

	private TimeSpan timeLimit;

	private DateTime? startTime;

	private TimeSpan diff = new TimeSpan(-2147483648L);

	private float timeSinceTimerUpdate;

	public void SetGameId(Guid gameId)
	{
		this.gameId = gameId;
		UpdateTimer();
	}

	public void SetTimeLimit(TimeSpan timeLimit)
	{
		this.timeLimit = timeLimit;
	}

	private void UpdateTimer()
	{
		_ = gameId;
		if (mode == Mode.CountDown)
		{
			TimeSpan? gameTimeLeftForCurrentPlayer = GameManager.GetRemoteGameDataManager().GetGameTimeLeftForCurrentPlayer(gameId);
			UpdateCountDown(gameTimeLeftForCurrentPlayer);
			return;
		}
		TimeSpan value = ((!startTime.HasValue) ? default(TimeSpan) : CountUpDiff(startTime));
		if (HasHighestValueChanged(value, diff))
		{
			UpdateCountDown(value);
		}
		diff = value;
	}

	private void Update()
	{
		timeSinceTimerUpdate += Time.deltaTime;
		if (timeSinceTimerUpdate > 0.33f)
		{
			UpdateTimer();
			timeSinceTimerUpdate = 0f;
		}
	}

	public void RefreshCountDown(DateTime? startTime, TimeSpan timeLimit)
	{
		mode = Mode.CountDown;
		this.startTime = startTime;
		this.timeLimit = timeLimit;
		((Component)superLabel).gameObject.SetActive(false);
		((Component)bg).gameObject.SetActive(true);
		if ((Object)(object)icon != (Object)null)
		{
			((Component)icon).gameObject.SetActive(false);
		}
		if (!startTime.HasValue)
		{
			SetTimedOut();
			return;
		}
		TimeSpan? gameTimeLeftForCurrentPlayer = GameManager.GetRemoteGameDataManager().GetGameTimeLeftForCurrentPlayer(gameId);
		UpdateCountDown(gameTimeLeftForCurrentPlayer);
	}

	public void RefreshCountUp(DateTime? startTime)
	{
		mode = Mode.CountUp;
		this.startTime = startTime;
		((Component)superLabel).gameObject.SetActive(true);
		((Component)bg).gameObject.SetActive(false);
		if ((Object)(object)icon != (Object)null)
		{
			((Component)icon).gameObject.SetActive(false);
		}
		TimeSpan timeSpan = CountUpDiff(startTime);
		UpdateTimeText(timeSpan);
	}

	public void SetTimedOut()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)timeLabel).text = "!";
		((TMP_Text)subLabel).text = Localization.Get("gameitem.timeup");
		((Graphic)bg).color = ColorUtil.SetAlphaOnColor(ColorConstants.red, 0.8f);
	}

	public void SetIconVisible(bool isVisible)
	{
		if (!((Object)(object)icon == (Object)null))
		{
			((Component)icon).gameObject.SetActive(isVisible);
			if (isVisible)
			{
				((Component)superLabel).gameObject.SetActive(false);
				((Component)bg).gameObject.SetActive(false);
				((Component)subLabel).gameObject.SetActive(false);
				((Component)timeLabel).gameObject.SetActive(false);
			}
		}
	}

	private bool HasHighestValueChanged(TimeSpan diff, TimeSpan previousDiff)
	{
		if (diff.Days != 0 || previousDiff.Days != 0)
		{
			return diff.Days != previousDiff.Days;
		}
		if (diff.Hours != 0 || previousDiff.Hours != 0)
		{
			return diff.Hours != previousDiff.Hours;
		}
		if (diff.Minutes != 0 || previousDiff.Minutes != 0)
		{
			return diff.Minutes != previousDiff.Minutes;
		}
		if (diff.Seconds != 0 || previousDiff.Seconds != 0)
		{
			return diff.Seconds != previousDiff.Seconds;
		}
		return false;
	}

	private TimeSpan CountDownDiff(DateTime? startTime, TimeSpan timeLimit)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!startTime.HasValue)
		{
			return default(TimeSpan);
		}
		return timeLimit.Subtract(utcNow.Subtract(startTime.Value));
	}

	private TimeSpan CountUpDiff(DateTime? startTime)
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!startTime.HasValue)
		{
			return default(TimeSpan);
		}
		return utcNow.Subtract(startTime.Value);
	}

	private void UpdateCountDown(TimeSpan? timeLeft)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Color sourceColor = ColorConstants.green;
		if (!timeLeft.HasValue)
		{
			((TMP_Text)timeLabel).text = "";
			((TMP_Text)subLabel).text = "";
			return;
		}
		if (timeLeft.Value.TotalSeconds < timeLimit.TotalSeconds * 0.5)
		{
			sourceColor = ColorConstants.yellow;
		}
		((Graphic)bg).color = ColorUtil.SetAlphaOnColor(sourceColor, 0.8f);
		if (timeLeft.Value.TotalSeconds > 0.0)
		{
			UpdateTimeText(timeLeft.Value);
		}
		else
		{
			SetTimedOut();
		}
	}

	private void UpdateTimeText(TimeSpan diff)
	{
		string[] array = LocalizationUtils.GetTimeString(diff).Split(' ');
		((TMP_Text)timeLabel).text = array[0];
		((TMP_Text)subLabel).text = array[1];
	}
}
