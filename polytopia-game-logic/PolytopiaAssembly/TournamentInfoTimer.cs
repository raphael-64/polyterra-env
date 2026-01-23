using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentInfoTimer : UIBasicComponent
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

	public Action OnCountdownEnded;

	private Guid gameId;

	private DateTime startTime;

	public void SetStartTime(DateTime startTime)
	{
		this.startTime = startTime;
		((Component)bg).gameObject.SetActive(true);
	}

	private void UpdateTimer()
	{
		_ = gameId;
		UpdateCountDown(startTime - DateTime.UtcNow);
	}

	private void Update()
	{
		UpdateTimer();
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

	private void UpdateCountDown(TimeSpan timeLeft)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Color sourceColor = ColorConstants.green;
		if (timeLeft.TotalHours < 1.0)
		{
			sourceColor = ColorConstants.yellow;
		}
		if (timeLeft.TotalSeconds > 0.0)
		{
			UpdateTimeText(timeLeft);
		}
		else
		{
			OnCountdownEnded?.Invoke();
			sourceColor = ColorConstants.blue;
		}
		((Graphic)bg).color = ColorUtil.SetAlphaOnColor(sourceColor, 0.8f);
	}

	private void UpdateTimeText(TimeSpan diff)
	{
		string[] array = LocalizationUtils.GetTimeString(diff).Split(' ');
		((TMP_Text)timeLabel).text = array[0];
		((TMP_Text)subLabel).text = array[1];
	}
}
