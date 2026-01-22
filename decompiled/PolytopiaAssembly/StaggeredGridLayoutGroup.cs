using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
public class StaggeredGridLayoutGroup : MonoBehaviour
{
	[SerializeField]
	[Min(0f)]
	private int maxRowWidth;

	[SerializeField]
	[Min(0f)]
	private int rowHeight;

	[SerializeField]
	[Min(0f)]
	private int spacingX;

	[SerializeField]
	[Min(0f)]
	private int spacingY;

	private VerticalLayoutGroup verticalLayoutGroup;

	private List<HorizontalLayoutGroup> rows;

	private List<RectTransform> content;

	private const float UPDATE_LATENCY = 0.1f;

	private float lastTimeUpdated;

	private void OnValidate()
	{
		TryFixDependencies();
		((HorizontalOrVerticalLayoutGroup)verticalLayoutGroup).spacing = spacingY;
		RebuildGrid();
	}

	private void Awake()
	{
		TryFixDependencies();
	}

	private void TryFixDependencies()
	{
		if ((Object)(object)verticalLayoutGroup == (Object)null)
		{
			verticalLayoutGroup = ((Component)this).GetComponent<VerticalLayoutGroup>();
		}
		if (content == null)
		{
			content = new List<RectTransform>();
		}
		if (rows == null)
		{
			rows = new List<HorizontalLayoutGroup>();
		}
	}

	private void LateUpdate()
	{
		if (Time.time - lastTimeUpdated < 0.1f)
		{
			return;
		}
		lastTimeUpdated = Time.time;
		List<RectTransform> list = new List<RectTransform>(content);
		content.Clear();
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			Transform child = ((Component)this).transform.GetChild(i);
			if ((Object)(object)((Component)child).GetComponent<HorizontalLayoutGroup>() != (Object)null)
			{
				for (int j = 0; j < child.childCount; j++)
				{
					RectTransform component = ((Component)child.GetChild(j)).GetComponent<RectTransform>();
					if ((Object)(object)component != (Object)null)
					{
						content.Add(component);
					}
				}
			}
			else
			{
				RectTransform component2 = ((Component)child).GetComponent<RectTransform>();
				if ((Object)(object)component2 != (Object)null)
				{
					content.Add(component2);
				}
			}
		}
		if (content.Count < list.Count)
		{
			RebuildGrid();
			return;
		}
		for (int num = content.Count - 1; num >= 0; num--)
		{
			if (!list.Contains(content[num]))
			{
				RebuildGrid();
				break;
			}
		}
	}

	private void RebuildGrid()
	{
		if (content == null || content.Count == 0)
		{
			return;
		}
		foreach (RectTransform item in content)
		{
			((Component)item).transform.SetParent(((Component)this).transform, false);
		}
		for (int i = 0; i < rows.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)rows[i]).gameObject);
		}
		rows.Clear();
		HorizontalLayoutGroup val = AddNewRow();
		float num = 0f;
		for (int j = 0; j < content.Count; j++)
		{
			float width = content[j].GetWidth();
			if (num + width > (float)maxRowWidth)
			{
				val = AddNewRow();
				num = (float)(((LayoutGroup)verticalLayoutGroup).padding.left + ((LayoutGroup)verticalLayoutGroup).padding.right + ((LayoutGroup)val).padding.left + ((LayoutGroup)val).padding.right) + width;
			}
			else
			{
				num += width + ((HorizontalOrVerticalLayoutGroup)val).spacing;
			}
			((Component)content[j]).transform.SetParent(((Component)val).transform, false);
		}
	}

	private HorizontalLayoutGroup AddNewRow()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject();
		val.transform.parent = ((Component)this).transform;
		HorizontalLayoutGroup val2 = val.AddComponent<HorizontalLayoutGroup>();
		((HorizontalOrVerticalLayoutGroup)val2).spacing = spacingX;
		((HorizontalOrVerticalLayoutGroup)val2).childForceExpandHeight = false;
		((HorizontalOrVerticalLayoutGroup)val2).childForceExpandWidth = false;
		((HorizontalOrVerticalLayoutGroup)val2).childControlHeight = false;
		((HorizontalOrVerticalLayoutGroup)val2).childControlWidth = false;
		RectTransform component = ((Component)val2).GetComponent<RectTransform>();
		((Transform)component).localScale = Vector3.one;
		component.SetWidth(maxRowWidth);
		component.SetHeight(rowHeight);
		if (rows == null)
		{
			rows = new List<HorizontalLayoutGroup>();
		}
		((Object)((Component)val2).transform).name = "Row #" + rows.Count;
		rows.Add(val2);
		return val2;
	}
}
