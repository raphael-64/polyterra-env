using System;
using UnityEngine;
using UnityEngine.UI;

public class CreditsList : UIBasicComponent
{
	[Serializable]
	public class CreditsCategoryData
	{
		public string headerKey;

		public CreditsNameData[] names;
	}

	[Serializable]
	public class CreditsNameData
	{
		public string name;

		public string link;
	}

	[SerializeField]
	protected VerticalLayoutGroup list;

	[SerializeField]
	protected CreditsCategory categoryPrefab;

	[SerializeField]
	protected CreditsCategoryData[] categories;

	protected RectTransform listTr;

	protected float totalHeight;

	private void Start()
	{
		listTr = ((Component)list).GetComponent<RectTransform>();
		totalHeight = ((LayoutGroup)list).padding.top + ((LayoutGroup)list).padding.bottom;
		CreditsCategoryData[] array = categories;
		foreach (CreditsCategoryData data in array)
		{
			CreateCategory(data);
		}
		base.rectTransform.SetHeight(totalHeight);
	}

	protected void CreateCategory(CreditsCategoryData data)
	{
		CreditsCategory creditsCategory = Object.Instantiate<CreditsCategory>(categoryPrefab, (Transform)(object)listTr);
		creditsCategory.HeaderKey = data.headerKey;
		creditsCategory.SetNames(data.names);
		totalHeight += creditsCategory.height + ((HorizontalOrVerticalLayoutGroup)list).spacing;
	}
}
