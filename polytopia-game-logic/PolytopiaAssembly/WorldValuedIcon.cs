using TMPro;
using UnityEngine;

public class WorldValuedIcon : WorldIcon
{
	public TextMeshProUGUI label;

	public override float Amount
	{
		get
		{
			return base.Amount;
		}
		set
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			base.Amount = value;
			((TMP_Text)label).text = $"+{LocalizationUtils.FormatNumber(base.Amount)}p";
			Vector2 preferredValues = ((TMP_Text)label).GetPreferredValues(((TMP_Text)label).text, ((TMP_Text)label).rectTransform.sizeDelta.x, ((TMP_Text)label).rectTransform.sizeDelta.y);
			((TMP_Text)label).rectTransform.sizeDelta = preferredValues;
		}
	}

	public override void Show()
	{
		base.Show();
	}
}
