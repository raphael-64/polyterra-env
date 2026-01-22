using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICollapsableContainer : MonoBehaviour
{
	public UILabelButton Button;

	public VerticalLayoutGroup ContentLayoutGroup;

	public string ShowLocalizationKey;

	public string HideLocalizationKey;

	private RectTransform cachedContentRectTransform;

	private bool isCollapsed;

	[NonSerialized]
	public Action<bool> CollapsedStateChangedCallback;

	private void OnEnable()
	{
		Button.OnClicked += ToggleCollapse;
		UpdateCollapsedState();
	}

	private void OnDisable()
	{
		Button.OnClicked -= ToggleCollapse;
	}

	private void ToggleCollapse(int id, BaseEventData eventData = null)
	{
		SetCollapsed(!isCollapsed);
	}

	private void UpdateCollapsedState()
	{
		if (isCollapsed)
		{
			Button.Key = "settings.hideadvanced";
			((Component)ContentLayoutGroup).gameObject.SetActive(true);
		}
		else
		{
			Button.Key = "settings.showadvanced";
			((Component)ContentLayoutGroup).gameObject.SetActive(false);
		}
		Transform parent = ((Component)this).transform.parent;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((parent is RectTransform) ? parent : null));
	}

	public RectTransform GetContentRectTransform()
	{
		if ((Object)(object)cachedContentRectTransform == (Object)null)
		{
			ref RectTransform reference = ref cachedContentRectTransform;
			Transform transform = ((Component)ContentLayoutGroup).transform;
			reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
		return cachedContentRectTransform;
	}

	public void SetCollapsed(bool isCollapsed)
	{
		this.isCollapsed = isCollapsed;
		UpdateCollapsedState();
		CollapsedStateChangedCallback?.Invoke(isCollapsed);
	}
}
