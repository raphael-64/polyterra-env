using System.Collections;
using UnityEngine;

public class PopupHeaderContainer : UIBasicComponent
{
	[SerializeField]
	protected RectTransform masterComponent;

	[SerializeField]
	protected RectTransform slaveComponent;

	[SerializeField]
	protected float spacing;

	private void OnTransformChildrenChanged()
	{
		UpdateLayout();
	}

	private void OnEnable()
	{
		UpdateLayout();
	}

	public void UpdateLayout()
	{
		if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(WaitForFrameEnd());
		}
	}

	private IEnumerator WaitForFrameEnd()
	{
		yield return (object)new WaitForEndOfFrame();
		DoLayout();
	}

	private void DoLayout()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = base.rectTransform.rect;
		float num = ((Rect)(ref rect)).width - (((Component)masterComponent).gameObject.activeSelf ? (masterComponent.sizeDelta.x + spacing) : 0f);
		slaveComponent.SetWidth(num - slaveComponent.anchoredPosition.x);
	}
}
