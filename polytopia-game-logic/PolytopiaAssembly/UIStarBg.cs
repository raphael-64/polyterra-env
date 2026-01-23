using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIStarBg : MonoBehaviour
{
	public int starCount = 100;

	public float margin;

	public float moveMultiplier = 0.1f;

	public ScrollRect techScroller;

	private Vector2 parentSize;

	private Vector2 parentHalfSize;

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

	private void Start()
	{
		((UnityEvent<Vector2>)(object)techScroller.onValueChanged).AddListener((UnityAction<Vector2>)OnScrollRectUpdated);
	}

	public void GenerateStars()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = ((RectTransform)((Transform)rectTransform).parent).rect;
		parentSize = ((Rect)(ref rect)).size;
		rectTransform.sizeDelta = new Vector2(parentSize.x + margin * 2f, parentSize.y + margin * 2f);
		parentHalfSize = rectTransform.sizeDelta * 0.5f;
		for (int i = 0; i < starCount; i++)
		{
			GameObject val = new GameObject();
			val.transform.SetParent((Transform)(object)rectTransform);
			((Object)val).name = "star_" + i;
			val.AddComponent<Image>();
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchoredPosition = new Vector2(Random.Range(0f - parentHalfSize.x, parentHalfSize.x), Random.Range(0f - parentHalfSize.y, parentHalfSize.y));
			component.sizeDelta = new Vector2(4f, 4f) * Random.Range(0.5f, 0.9f);
		}
		OnScrollRectUpdated(techScroller.normalizedPosition);
	}

	public void OnScrollRectUpdated(Vector2 offset)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.anchoredPosition = techScroller.content.anchoredPosition * moveMultiplier;
	}
}
