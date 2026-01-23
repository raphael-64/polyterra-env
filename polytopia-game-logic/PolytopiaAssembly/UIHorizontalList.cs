using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHorizontalList : UIBasicComponent
{
	[SerializeField]
	protected TMPLocalizer header;

	[SerializeField]
	protected PolytopiaScrollRect scroller;

	[SerializeField]
	protected ScrollRectHighlightHelper scrollHelper;

	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected HorizontalLayoutGroup horizontalList;

	[SerializeField]
	protected float releaseInertia = 0.1f;

	[SerializeField]
	protected float relaseBaseAnimTime = 0.4f;

	[Header("Prefabs")]
	[SerializeField]
	protected UIHorizontalListItem itemPrefab;

	[HideInInspector]
	public Action<int> IndexChangedCallback;

	[HideInInspector]
	public Action<int> IndexSelectedCallback;

	[HideInInspector]
	public Action OnSelectDisabledItemCallback;

	[HideInInspector]
	public int EnabledItemCount = -1;

	protected int m_selectedIndex = -1;

	protected string[] data;

	protected int[] ids;

	[NonSerialized]
	public UIHorizontalListItem[] items;

	protected float[] scrollPositions;

	protected float totalWidth;

	protected RectTransform m_horizontalListRectTr;

	protected ScrollRectHighlightHelper m_scrollRectHighlightHelper;

	protected bool m_updateScroller;

	protected Tween scrollTween;

	protected bool itemPressed;

	protected int firstVisibleIdx;

	protected int lastVisibleIdx;

	protected bool isAnimating;

	private RectTransform HorizontalListRectTr
	{
		get
		{
			if ((Object)(object)m_horizontalListRectTr == (Object)null)
			{
				m_horizontalListRectTr = ((Component)horizontalList).GetComponent<RectTransform>();
			}
			return m_horizontalListRectTr;
		}
	}

	public string HeaderKey
	{
		get
		{
			return header.Key;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				base.rectTransform.SetHeight(64f);
				((Component)header).gameObject.SetActive(false);
			}
			else
			{
				((Component)header).gameObject.SetActive(true);
				header.Key = value;
				base.rectTransform.SetHeight(94f);
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return m_selectedIndex;
		}
		set
		{
			if (value != m_selectedIndex)
			{
				if (m_selectedIndex >= 0)
				{
					items[m_selectedIndex].Selected = false;
				}
				if (!isAnimating)
				{
					m_selectedIndex = Mathf.Max(0, Mathf.Min(value, items.Length - 1));
					items[m_selectedIndex].Selected = true;
					IndexChangedCallback?.Invoke(m_selectedIndex);
				}
			}
		}
	}

	public virtual bool UpdateScrollerOnHighlight
	{
		get
		{
			return m_updateScroller;
		}
		set
		{
			m_updateScroller = value;
		}
	}

	private void OnEnable()
	{
		scroller.routeToParent = true;
		ScrollRectCallbackHelper component = ((Component)scroller).GetComponent<ScrollRectCallbackHelper>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnDragEnd += ScrollerOnDragEnd;
		}
		if (m_selectedIndex >= 0)
		{
			RefreshScrollpositions();
		}
		SystemEvents.OnSafeAreaChanged += OnSafeAreaChanged;
		LocalizationEvents.OnLanguageChanged += OnLanguageChanged;
		InputEvents.OnOmnicursorSnapToUIElement += OnOmnicursorSnapToUIElement;
		UpdateOmnicursorSnappingTargets(enableRaycasting: false);
		UpdateBgSize();
	}

	private void OnDisable()
	{
		ScrollRectCallbackHelper component = ((Component)scroller).GetComponent<ScrollRectCallbackHelper>();
		if ((Object)(object)component != (Object)null)
		{
			component.OnDragEnd -= ScrollerOnDragEnd;
		}
		TweenUtils.KillTween(scrollTween);
		scrollTween = null;
		SystemEvents.OnSafeAreaChanged -= OnSafeAreaChanged;
		LocalizationEvents.OnLanguageChanged -= OnLanguageChanged;
		InputEvents.OnOmnicursorSnapToUIElement -= OnOmnicursorSnapToUIElement;
	}

	private void OnSafeAreaChanged(Rect safeArea)
	{
		UpdateBgSize();
	}

	private void UpdateBgSize()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Vector2 offsetMax = ((Graphic)bg).rectTransform.offsetMax;
		offsetMax.x = ScreenManager.SafeRight;
		((Graphic)bg).rectTransform.offsetMax = offsetMax;
		Vector2 offsetMin = ((Graphic)bg).rectTransform.offsetMin;
		offsetMin.x = 0f - ScreenManager.SafeLeft;
		((Graphic)bg).rectTransform.offsetMin = offsetMin;
		UpdateScrollHelperBounds();
	}

	private void UpdateScrollHelperBounds()
	{
		if (Object.op_Implicit((Object)(object)scrollHelper))
		{
			int num = (int)base.rectTransform.GetWidth();
			scrollHelper.scrollInset.left = -num / 2;
			scrollHelper.scrollInset.right = -num / 2;
		}
	}

	private void ScrollerOnDragEnd(PointerEventData eventData)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float num = ((ScrollRect)scroller).velocity.x * releaseInertia / totalWidth;
		int num2 = GetCenterItem(((ScrollRect)scroller).horizontalNormalizedPosition - num);
		if (!items[num2].ButtonEnabled)
		{
			Log.Info("Selected {0} but it is not enabled", new object[1] { num2 });
			num2 = GetClosestEnabledIndex(num2);
		}
		AnimateToIndex(num2);
	}

	private void AnimateToIndex(int index, bool switchSelection = true)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		TweenUtils.KillTween(scrollTween, complete: true);
		scrollTween = null;
		isAnimating = true;
		scrollTween = (Tween)(object)TweenSettingsExtensions.OnComplete<Tweener>(TweenSettingsExtensions.SetEase<Tweener>(((ScrollRect)(object)scroller).DOHorizontalNormalizedPos(GetScrollPosition(index), 0.4f), (Ease)27), (TweenCallback)delegate
		{
			isAnimating = false;
			if (switchSelection)
			{
				SwitchSelectionTo(index);
			}
		});
	}

	public void SetData(string[] data, int selectedIndex = 0)
	{
		this.data = data;
		CreateItems();
		RefreshScrollpositions();
		SelectItem(selectedIndex, instant: true);
		UpdateOmnicursorSnappingTargets(enableRaycasting: false);
	}

	public void SetIds(int[] ids)
	{
		this.ids = ids;
	}

	public void SetLocalizationKeys(string[] keys)
	{
		if (keys.Length != items.Length)
		{
			Log.Error("Number of localization keys didn't match UIHorizontalList length! Skipping!", Array.Empty<object>());
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			items[i].Key = keys[i];
		}
	}

	public void SetItemEnabled(int index, bool isEnabled)
	{
		if (index >= 0 && items.Length > index)
		{
			items[index].ButtonEnabled = isEnabled;
			items[index].Selected = SelectedIndex == index;
		}
	}

	protected void CreateItems()
	{
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		totalWidth = 0f;
		int num = data.Length;
		items = new UIHorizontalListItem[num];
		for (int i = 0; i < num; i++)
		{
			UIHorizontalListItem uIHorizontalListItem = Object.Instantiate<UIHorizontalListItem>(itemPrefab, ((Component)horizontalList).transform);
			uIHorizontalListItem.index = i;
			uIHorizontalListItem.Key = data[i];
			((Object)uIHorizontalListItem).name = $"item_{i}";
			uIHorizontalListItem.ButtonEnabled = EnabledItemCount < 0 || i < EnabledItemCount;
			uIHorizontalListItem.OnClicked += OnItemClicked;
			uIHorizontalListItem.OnClickedWhenDisabled += OnClickedDisabledButton;
			uIHorizontalListItem.OnDown += OnItemDown;
			uIHorizontalListItem.OnUp += OnItemUp;
			uIHorizontalListItem.OnSelected += OnItemSelected;
			uIHorizontalListItem.OnDeselected += OnItemDeselected;
			uIHorizontalListItem.id = i;
			uIHorizontalListItem.Init();
			totalWidth += uIHorizontalListItem.rectTransform.sizeDelta.x;
			items[i] = uIHorizontalListItem;
		}
		firstVisibleIdx = 0;
		lastVisibleIdx = num - 1;
		totalWidth += ((HorizontalOrVerticalLayoutGroup)horizontalList).spacing * (float)(num - 1);
		HorizontalListRectTr.SetWidth(totalWidth);
		LayoutRebuilder.ForceRebuildLayoutImmediate(HorizontalListRectTr);
		FixNavigationOnItem(firstVisibleIdx);
		FixNavigationOnItem(lastVisibleIdx);
	}

	private void UpdateSelectedIndex()
	{
		if (SelectedIndex < 0)
		{
			return;
		}
		if (SelectedIndex < items.Length && ((Component)items[SelectedIndex]).gameObject.activeSelf)
		{
			SelectItem(SelectedIndex, instant: true);
			return;
		}
		int num = 0;
		int num2 = SelectedIndex;
		for (int i = 0; i < items.Length; i++)
		{
			if (((Component)items[i]).gameObject.activeSelf)
			{
				num = i;
			}
			else if (num2 == i)
			{
				num2++;
			}
		}
		if (num2 == items.Length)
		{
			num2 = num;
		}
		SelectItem(num2, instant: true);
	}

	private void RefreshWidth()
	{
		int num = 0;
		for (int i = 0; i < items.Length; i++)
		{
			if (((Component)items[i]).gameObject.activeSelf)
			{
				num++;
			}
		}
		totalWidth += ((HorizontalOrVerticalLayoutGroup)horizontalList).spacing * (float)(num - 1);
		HorizontalListRectTr.SetWidth(totalWidth);
		LayoutRebuilder.ForceRebuildLayoutImmediate(HorizontalListRectTr);
		((LayoutGroup)horizontalList).CalculateLayoutInputHorizontal();
		((LayoutGroup)horizontalList).SetLayoutHorizontal();
		UpdateScrollHelperBounds();
	}

	public void RefreshScrollpositions()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		RefreshWidth();
		scrollPositions = new float[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			UIHorizontalListItem uIHorizontalListItem = items[i];
			scrollPositions[i] = (((Component)uIHorizontalListItem).gameObject.activeSelf ? (uIHorizontalListItem.rectTransform.anchoredPosition.x / totalWidth) : (-1f));
		}
		UpdateSelectedIndex();
	}

	public void RefreshNavigationIndexes()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		firstVisibleIdx = -1;
		lastVisibleIdx = -1;
		int num = items.Length;
		for (int i = 0; i < num; i++)
		{
			UIHorizontalListItem uIHorizontalListItem = items[i];
			Navigation navigation = ((Selectable)uIHorizontalListItem.button).navigation;
			((Navigation)(ref navigation)).mode = (Mode)3;
			((Selectable)uIHorizontalListItem.button).navigation = navigation;
			if (firstVisibleIdx == -1 && ((Component)uIHorizontalListItem).gameObject.activeSelf)
			{
				firstVisibleIdx = i;
			}
			if (lastVisibleIdx < i && ((Component)uIHorizontalListItem).gameObject.activeSelf)
			{
				lastVisibleIdx = i;
			}
		}
		FixNavigationOnItem(firstVisibleIdx);
		FixNavigationOnItem(lastVisibleIdx);
	}

	public void RefreshItems()
	{
		if (items != null && items.Length != 0)
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].Selected = i == SelectedIndex;
			}
		}
		RefreshScrollpositions();
	}

	private void OnItemDown(int id, BaseEventData eventData)
	{
		itemPressed = true;
	}

	private void OnItemUp(int index, BaseEventData eventData)
	{
		if (eventData is PointerEventData || PolytopiaInput.GetMouseButtonUp(0))
		{
			itemPressed = false;
		}
	}

	private void OnItemClicked(int index, BaseEventData eventData)
	{
		SelectItem(index);
		UpdateOmnicursorSnappingTargets(enableRaycasting: true);
	}

	private void OnItemSelected(int index, BaseEventData eventData)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (eventData is AxisEventData)
		{
			((Graphic)bg).color = ColorUtil.SetAlphaOnColor(ColorConstants.blue, 0.8f);
			if (!itemPressed)
			{
				SelectItem(index);
				FocusScroller();
			}
		}
	}

	public void FocusScroller()
	{
		if (UpdateScrollerOnHighlight && (Object)(object)GetScrollRectHighlightHelper() != (Object)null)
		{
			m_scrollRectHighlightHelper.ScrollToObject(base.rectTransform);
		}
	}

	private void OnItemDeselected(int id, BaseEventData eventData)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!(eventData is AxisEventData))
		{
			return;
		}
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Highlighted)
			{
				return;
			}
		}
		((Graphic)bg).color = ColorUtil.SetAlphaOnColor(Color.black, 0.3f);
	}

	public void SelectId(int id, bool instant = false, float animTime = -1f)
	{
		int index = Mathf.Max(0, Array.IndexOf(ids, id));
		SelectItem(index, instant, animTime);
	}

	public void SelectItem(int index, bool instant = false, float animTime = -1f)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		if (index >= items.Length)
		{
			index = items.Length - 1;
		}
		Tween tween = scrollTween;
		scrollTween = null;
		TweenUtils.KillTween(tween, complete: true);
		if (instant)
		{
			if (!items[index].ButtonEnabled)
			{
				OnSelectedDisabledIndex(index);
				return;
			}
			((ScrollRect)scroller).horizontalNormalizedPosition = GetScrollPosition(index);
			bool num = index != SelectedIndex;
			SelectedIndex = index;
			if (num)
			{
				IndexSelectedCallback?.Invoke(SelectedIndex);
			}
		}
		else
		{
			if (animTime < 0f)
			{
				animTime = 0.2f;
			}
			isAnimating = true;
			scrollTween = (Tween)(object)TweenSettingsExtensions.OnComplete<Tweener>(((ScrollRect)(object)scroller).DOHorizontalNormalizedPos(GetScrollPosition(index), 0.2f), (TweenCallback)delegate
			{
				isAnimating = false;
				SwitchSelectionTo(index);
			});
		}
	}

	public void SwitchSelectionTo(int index)
	{
		int num = Mathf.Clamp(0, index, items.Length - 1);
		if (!items[num].ButtonEnabled)
		{
			OnSelectedDisabledIndex(num);
			return;
		}
		SelectedIndex = num;
		FixNavigationOnItem(num);
		if (UINavigationManager.navigationType == UINavigationManager.NavigationType.Buttons)
		{
			AudioManager.PlaySFX(SFXTypes.Press);
		}
		itemPressed = false;
		IndexSelectedCallback?.Invoke(SelectedIndex);
	}

	private void OnClickedDisabledButton(int index, BaseEventData eventData)
	{
	}

	private void OnSelectedDisabledIndex(int index)
	{
		OnSelectDisabledItemCallback?.Invoke();
		int nextEnabledIndex = GetNextEnabledIndex(index);
		AnimateToIndex(nextEnabledIndex, switchSelection: false);
		PolytopiaInput.Omnicursor.AffixToUIElement(items[nextEnabledIndex].rectTransform);
	}

	public int GetNextEnabledIndex(int index)
	{
		if (index > SelectedIndex)
		{
			for (int i = index; i < items.Length; i++)
			{
				if (items[i].ButtonEnabled)
				{
					return i;
				}
			}
		}
		else
		{
			int num = index;
			while (items.Length != 0)
			{
				if (items[num].ButtonEnabled)
				{
					return num;
				}
				num--;
			}
		}
		return SelectedIndex;
	}

	public int GetClosestEnabledIndex(int index)
	{
		if (items[index].ButtonEnabled)
		{
			return index;
		}
		int num = 0;
		int num2 = -1;
		for (int i = 0; i < items.Length; i++)
		{
			Log.Info("### - {0} Enabled: {1}", new object[2]
			{
				i,
				items[i].ButtonEnabled
			});
			if (items[i].ButtonEnabled)
			{
				int num3 = Mathf.Abs(index - i);
				if (num2 == -1 || num3 < num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		return num2;
	}

	private void FixNavigationOnItem(int index)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Clamp(0, index, items.Length - 1);
		if (num == firstVisibleIdx || num == lastVisibleIdx)
		{
			_ = items[num];
			Selectable button = (Selectable)(object)items[num].button;
			Navigation navigation = button.navigation;
			((Navigation)(ref navigation)).mode = (Mode)4;
			((Navigation)(ref navigation)).selectOnLeft = button.FindSelectable(Vector3.left);
			((Navigation)(ref navigation)).selectOnUp = button.FindSelectable(Vector3.up);
			((Navigation)(ref navigation)).selectOnDown = button.FindSelectable(Vector3.down);
			((Navigation)(ref navigation)).selectOnRight = button.FindSelectable(Vector3.right);
			if (num == firstVisibleIdx)
			{
				((Navigation)(ref navigation)).selectOnLeft = null;
			}
			else if (num == lastVisibleIdx)
			{
				((Navigation)(ref navigation)).selectOnRight = null;
			}
			button.navigation = navigation;
		}
	}

	public void OnScrollRectChanged(Vector2 scrollPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		int centerItem = GetCenterItem(scrollPosition.x);
		if (centerItem != SelectedIndex)
		{
			SelectedIndex = centerItem;
		}
		UpdateScrollHelperBounds();
	}

	public Selectable GetSelectable(int index)
	{
		if (index >= 0 && index < items.Length)
		{
			return (Selectable)(object)items[index].button;
		}
		return null;
	}

	public Selectable GetCurrentSelectable()
	{
		return GetSelectable(SelectedIndex);
	}

	protected int GetCenterItem(float scrollPosition)
	{
		int result = 0;
		float num = float.MaxValue;
		int num2 = scrollPositions.Length;
		for (int i = 0; i < num2; i++)
		{
			float num3 = Mathf.Abs(scrollPosition - scrollPositions[i]);
			if (num3 < num && ((Component)items[i]).gameObject.activeSelf)
			{
				result = i;
				num = num3;
			}
		}
		return result;
	}

	protected float GetScrollPosition(int index)
	{
		return scrollPositions[index];
	}

	private void OnLanguageChanged(Localization.Languages language)
	{
		RefreshScrollpositions();
	}

	public int? GetIdForIndex(int index)
	{
		if (ids == null || ids.Length <= index)
		{
			return null;
		}
		return ids[index];
	}

	public int? GetIndexForId(int id)
	{
		if (ids == null)
		{
			return null;
		}
		int num = Array.IndexOf(ids, id);
		if (num < 0)
		{
			return null;
		}
		return num;
	}

	public ScrollRectHighlightHelper GetScrollRectHighlightHelper()
	{
		if ((Object)(object)m_scrollRectHighlightHelper == (Object)null)
		{
			m_scrollRectHighlightHelper = ((Component)this).GetComponentInParent<ScrollRectHighlightHelper>();
		}
		return m_scrollRectHighlightHelper;
	}

	private void OnOmnicursorSnapToUIElement(RectTransform transform)
	{
		UpdateOmnicursorSnappingTargets((Object)(object)((Transform)transform).parent == (Object)(object)base.rectTransform);
	}

	public void UpdateOmnicursorSnappingTargets(bool enableRaycasting)
	{
		if (items != null)
		{
			UIHorizontalListItem[] array = items;
			foreach (UIHorizontalListItem uIHorizontalListItem in array)
			{
				bool flag = enableRaycasting || uIHorizontalListItem.Selected;
				uIHorizontalListItem.omnicursorIgnore.ignore = !flag;
			}
		}
	}
}
