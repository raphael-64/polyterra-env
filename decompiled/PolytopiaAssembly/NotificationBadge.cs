using TMPro;
using UnityEngine;

public class NotificationBadge : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI label;

	[SerializeField]
	protected bool hideIfZero = true;

	protected int value;

	public int Value
	{
		get
		{
			return value;
		}
		set
		{
			this.value = value;
			if (hideIfZero)
			{
				((Component)this).gameObject.SetActive(this.value > 0);
			}
			((TMP_Text)label).text = LocalizationUtils.FormatNumber(this.value);
		}
	}

	private void Start()
	{
		Value = value;
	}
}
