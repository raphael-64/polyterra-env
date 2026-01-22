using TMPro;
using UnityEngine;

public class GameSetupInfoRow : UIBasicComponent
{
	[SerializeField]
	protected TMPLocalizer label;

	private float totalHeight;

	public string Text
	{
		get
		{
			return label.Text;
		}
		set
		{
			label.Text = value;
			Height = ((TMP_Text)label.TextComponent).preferredHeight + 20f;
		}
	}

	public float Height
	{
		get
		{
			return totalHeight;
		}
		set
		{
			totalHeight = value;
			base.rectTransform.SetHeight(totalHeight);
		}
	}
}
