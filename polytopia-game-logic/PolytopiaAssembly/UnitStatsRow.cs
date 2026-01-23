using TMPro;
using UnityEngine;

public class UnitStatsRow : MonoBehaviour
{
	public TextMeshProUGUI statsNameLabel;

	public TextMeshProUGUI statsValueLabel;

	public RectTransform dottedLine;

	private RectTransform m_rectTransform;

	[SerializeField]
	private float nameMinWidth = 75f;

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public void SetData(string statsName, float value)
	{
		SetData(statsName, value.ToString());
	}

	public void SetData(string statsName, string value)
	{
		((TMP_Text)statsNameLabel).text = string.Format("{0} {1}", Localization.Get(statsName), ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ");
		((TMP_Text)statsValueLabel).text = value;
	}

	public void UpdateSize()
	{
		UpdateSize(rectTransform.GetWidth());
	}

	public void UpdateSize(float width)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = width - ((TMP_Text)statsNameLabel).rectTransform.anchoredPosition.x - (((TMP_Text)statsValueLabel).GetPreferredValues(((TMP_Text)statsValueLabel).text).x + 4f);
		((TMP_Text)statsNameLabel).rectTransform.SetWidth(Mathf.Max(nameMinWidth, num));
	}
}
