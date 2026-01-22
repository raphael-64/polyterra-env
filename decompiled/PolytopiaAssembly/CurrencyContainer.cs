using TMPro;
using UnityEngine;

public class CurrencyContainer : ResourceContainerBase
{
	[SerializeField]
	protected ResourceWidget resourceWidget;

	protected float income;

	protected float currency;

	public float Income
	{
		set
		{
			income = value;
			((TMP_Text)headerLabel).text = Localization.Get("topbar.stars", LocalizationUtils.FormatNumber(income));
			UpdateWidth();
		}
	}

	public float Currency
	{
		set
		{
			if (value != currency)
			{
				currency = value;
				resourceWidget.Amount = Mathf.Max(0f, value);
				UpdateWidth();
			}
		}
	}

	protected override void OnLanguageChanged(Localization.Languages language)
	{
		base.OnLanguageChanged(language);
		Income = income;
		Currency = currency;
	}

	protected void UpdateWidth()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.width = Mathf.Max(resourceWidget.rectTransform.sizeDelta.x, ((TMP_Text)headerLabel).preferredWidth);
	}
}
