using TMPro;
using UnityEngine;

public class ScoreContainer : ResourceContainerBase
{
	protected float score;

	public float Score
	{
		set
		{
			if (value != score)
			{
				score = value;
				((TMP_Text)amountLabel).text = LocalizationUtils.FormatNumber(score);
				base.width = Mathf.Max(((TMP_Text)amountLabel).preferredWidth, ((TMP_Text)headerLabel).preferredWidth);
			}
		}
	}

	protected override void OnLanguageChanged(Localization.Languages language)
	{
		base.OnLanguageChanged(language);
		Score = score;
	}
}
