using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WorldScore : UIWorldScoreBase
{
	public TextMeshProUGUI label;

	public float bigFontSize = 28f;

	private string format = "+{0}p";

	private float defaultFontSize = 18f;

	public override float Amount
	{
		get
		{
			return base.Amount;
		}
		set
		{
			base.Amount = value;
			((TMP_Text)label).text = string.Format(Format, LocalizationUtils.FormatNumber(base.Amount));
			if (base.Amount >= 100f)
			{
				((TMP_Text)label).fontSize = bigFontSize;
			}
		}
	}

	public string Format
	{
		get
		{
			return format;
		}
		set
		{
			format = value;
			Amount = Amount;
		}
	}

	public Color Color
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)label).color = value;
		}
	}

	private void Awake()
	{
		defaultFontSize = ((TMP_Text)label).fontSize;
	}

	public override void ResetItem()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		base.ResetItem();
		Format = "+{0}p";
		Color = Color.white;
		((TMP_Text)label).fontSize = defaultFontSize;
	}
}
