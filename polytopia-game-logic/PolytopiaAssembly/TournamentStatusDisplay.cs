using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TournamentStatusDisplay : MonoBehaviour
{
	private static float TIMER_UPDATE_INTERVAL = 1f;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected TextMeshProUGUI label;

	private bool showTimer;

	private string text;

	private DateTime currentTimerTarget;

	private float timerValue;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	private Action timerRanOutCallback;

	private UnityAction<string, string> labelLinkCallback;

	public virtual UnityAction<string, string> LabelLinkCallback
	{
		get
		{
			return labelLinkCallback;
		}
		set
		{
			labelLinkCallback = value;
			TMPLinkHelper tMPLinkHelper2 = default(TMPLinkHelper);
			if (labelLinkCallback != null)
			{
				TMPLinkHelper tMPLinkHelper = ((Component)label).GetComponent<TMPLinkHelper>();
				if ((Object)(object)tMPLinkHelper == (Object)null)
				{
					tMPLinkHelper = ((Component)label).gameObject.AddComponent<TMPLinkHelper>();
					if (tMPLinkHelper.OnClick == null)
					{
						tMPLinkHelper.OnClick = new TMPLinkHelper.TMPLinkEvent();
					}
				}
				((UnityEvent<string, string>)tMPLinkHelper.OnClick).AddListener(labelLinkCallback);
			}
			else if ((Object)(object)label != (Object)null && ((Component)label).TryGetComponent<TMPLinkHelper>(ref tMPLinkHelper2))
			{
				Object.Destroy((Object)(object)tMPLinkHelper2);
			}
		}
	}

	private void Awake()
	{
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetIcon(spriteHandle.sprite);
		});
	}

	public void LoadIcon(string iconId)
	{
		if (string.IsNullOrEmpty(iconId))
		{
			((Component)icon).gameObject.SetActive(false);
		}
		else
		{
			iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(iconId));
		}
	}

	private void SetIcon(Sprite sprite)
	{
		((Component)icon).gameObject.SetActive(true);
		icon.sprite = sprite;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
	}

	public void SetLabel(string text, DateTime? timerTarget = null, Action timerRanOutCallback = null)
	{
		if (timerTarget.HasValue)
		{
			showTimer = true;
			this.text = text;
			currentTimerTarget = timerTarget.Value;
			this.timerRanOutCallback = timerRanOutCallback;
			TimeSpan timeLeft = currentTimerTarget - DateTime.UtcNow;
			((TMP_Text)label).text = string.Format(text, GetTimeFormat(timeLeft));
		}
		else
		{
			showTimer = false;
			this.text = text;
			((TMP_Text)label).text = text;
			this.timerRanOutCallback = null;
		}
	}

	private void Update()
	{
		if (!showTimer)
		{
			return;
		}
		timerValue += Time.deltaTime;
		if (timerValue >= TIMER_UPDATE_INTERVAL)
		{
			TimeSpan timeLeft = currentTimerTarget - DateTime.UtcNow;
			if (timeLeft.TotalSeconds <= 0.0)
			{
				timeLeft = TimeSpan.Zero;
				timerRanOutCallback?.Invoke();
				timerRanOutCallback = null;
			}
			((TMP_Text)label).text = string.Format(text, GetTimeFormat(timeLeft));
			timerValue = 0f;
		}
	}

	private string GetTimeFormat(TimeSpan timeLeft)
	{
		if (timeLeft.TotalDays > 1.0)
		{
			return LocalizationUtils.GetTimeString(timeLeft);
		}
		return timeLeft.ToString("hh\\:mm\\:ss");
	}
}
