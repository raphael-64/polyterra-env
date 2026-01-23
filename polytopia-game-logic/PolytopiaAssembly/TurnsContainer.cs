using TMPro;
using UnityEngine;

public class TurnsContainer : ResourceContainerBase
{
	protected int turns;

	public int Turns
	{
		set
		{
			turns = value;
			if (GameManager.Client.GameState.Settings.rules.TurnLimit <= 0)
			{
				((TMP_Text)amountLabel).text = turns.ToString();
			}
			else
			{
				((TMP_Text)amountLabel).text = $"{turns}/{GameManager.Client.GameState.Settings.rules.TurnLimit}";
			}
			base.width = Mathf.Max(((TMP_Text)amountLabel).preferredWidth, ((TMP_Text)headerLabel).preferredWidth);
		}
	}

	protected override void OnLanguageChanged(Localization.Languages language)
	{
		base.OnLanguageChanged(language);
		Turns = turns;
	}
}
