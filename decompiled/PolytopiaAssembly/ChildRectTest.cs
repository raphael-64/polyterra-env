using UnityEngine;

public class ChildRectTest : MonoBehaviour
{
	public bool recalculate;

	public RectTransform testObject;

	protected RectTransform m_rectTransform;

	protected Rect currRect = Rect.zero;

	public Rect ActualRect
	{
		get
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01de: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
			Log.Verbose("Child count : {0}", new object[1] { ((Transform)rectTransform).childCount });
			Rect rect = rectTransform.rect;
			Rect val2 = default(Rect);
			foreach (RectTransform item in (Transform)rectTransform)
			{
				RectTransform val = item;
				Rect rect2 = val.rect;
				Vector2 position = ((Rect)(ref rect2)).position;
				rect2 = val.rect;
				((Rect)(ref val2))._002Ector(position, ((Rect)(ref rect2)).size);
				((Rect)(ref rect)).xMin = Mathf.Min(((Rect)(ref rect)).xMin, ((Transform)val).localPosition.x + ((Rect)(ref val2)).xMin);
				((Rect)(ref rect)).yMin = Mathf.Min(((Rect)(ref rect)).yMin, ((Transform)val).localPosition.y + ((Rect)(ref val2)).yMin);
				((Rect)(ref rect)).xMax = Mathf.Max(((Rect)(ref rect)).xMax, ((Transform)val).localPosition.x + ((Rect)(ref val2)).xMax);
				((Rect)(ref rect)).yMax = Mathf.Max(((Rect)(ref rect)).yMax, ((Transform)val).localPosition.y + ((Rect)(ref val2)).yMax);
				Log.Verbose("Child : {0} rect: {1}, min: {2}, max{3}, child Rect og pos {4}, child anchored position: {5}", new object[6]
				{
					((Object)val).name,
					val2,
					((Rect)(ref val2)).min,
					((Rect)(ref val2)).max,
					((Rect)(ref val2)).position,
					val.anchoredPosition
				});
			}
			Log.Verbose("Actual rect with children (before new pos) :: pos: {0}, center: {1}, size: {2}", new object[3]
			{
				((Rect)(ref rect)).position,
				((Rect)(ref rect)).center,
				((Rect)(ref rect)).size
			});
			Vector2 position2 = rectTransform.anchoredPosition + new Vector2(((Rect)(ref rect)).center.x, ((Rect)(ref rect)).center.y);
			((Rect)(ref rect)).position = position2;
			return rect;
		}
	}

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
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		currRect = ActualRect;
		UpdateTestObject();
	}

	private void Update()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (recalculate)
		{
			currRect = ActualRect;
			Log.Verbose("Actual rect with children :: pos: {0}, center: {1}, size: {2}", new object[3]
			{
				((Rect)(ref currRect)).position,
				((Rect)(ref currRect)).center,
				((Rect)(ref currRect)).size
			});
			UpdateTestObject();
			recalculate = false;
		}
	}

	private void UpdateTestObject()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		testObject.anchoredPosition = ((Rect)(ref currRect)).position;
		testObject.sizeDelta = ((Rect)(ref currRect)).size;
	}
}
