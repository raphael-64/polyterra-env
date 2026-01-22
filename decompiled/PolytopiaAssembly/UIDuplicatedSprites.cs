using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIDuplicatedSprites : MonoBehaviour
{
	private RectTransform m_rectTransform;

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public void Clear()
	{
		if (!((Object)(object)this == (Object)null) && !((Object)(object)rectTransform == (Object)null))
		{
			for (int num = ((Transform)rectTransform).childCount - 1; num >= 0; num--)
			{
				Transform child = ((Transform)rectTransform).GetChild(num);
				child.SetParent((Transform)null);
				Object.Destroy((Object)(object)((Component)child).gameObject);
			}
		}
	}
}
