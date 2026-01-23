using UnityEngine;
using UnityEngine.UI;

public class CreditsCategory : UIBasicComponent
{
	[SerializeField]
	protected VerticalLayoutGroup list;

	[SerializeField]
	protected TMPLocalizer header;

	[SerializeField]
	protected CreditsItem creditsItemPrefab;

	public string HeaderKey
	{
		get
		{
			return header.Key;
		}
		set
		{
			header.Key = value;
		}
	}

	public float height
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return base.rectTransform.sizeDelta.y;
		}
		protected set
		{
			base.rectTransform.SetHeight(value);
		}
	}

	public void SetNames(CreditsList.CreditsNameData[] names)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		float num = header.rectTransform.sizeDelta.y + (float)((LayoutGroup)list).padding.top + (float)((LayoutGroup)list).padding.bottom;
		int num2 = names.Length;
		for (int i = 0; i < num2; i++)
		{
			CreditsItem creditsItem = Object.Instantiate<CreditsItem>(creditsItemPrefab, (Transform)(object)base.rectTransform);
			creditsItem.Text = names[i].name;
			creditsItem.Link = names[i].link;
			num += creditsItem.rectTransform.sizeDelta.y + ((HorizontalOrVerticalLayoutGroup)list).spacing;
		}
		height = num;
	}
}
