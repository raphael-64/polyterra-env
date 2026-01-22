using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LadderDetailsTimeline : MonoBehaviour
{
	private const float UPDATE_INTERVAL = 1f;

	[SerializeField]
	protected TextMeshProUGUI label;

	[SerializeField]
	protected Slider slider;

	private DateTime startDate;

	private DateTime endDate;

	private float updateDelta;

	public void SetData(DateTime startDate, DateTime endDate)
	{
		this.startDate = startDate;
		this.endDate = endDate;
		Updatetimer();
	}

	private void Update()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			if (updateDelta >= 1f)
			{
				Updatetimer();
			}
			else
			{
				updateDelta += Time.deltaTime;
			}
		}
	}

	public void Updatetimer()
	{
		TimeSpan time = endDate - DateTime.UtcNow;
		if (time.TotalSeconds == 0.0)
		{
			((TMP_Text)label).text = Localization.Get("onlineview.ladder.ended");
		}
		else
		{
			((TMP_Text)label).text = Localization.Get("onlineview.ladder.ends", LocalizationUtils.GetTimeString(time));
		}
		float value = Mathf.Clamp01((float)(DateTime.UtcNow.Ticks - startDate.Ticks) / (float)(endDate.Ticks - startDate.Ticks));
		slider.value = value;
		updateDelta = 0f;
	}
}
