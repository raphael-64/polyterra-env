using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerRequirementRow : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI statsNameLabel;

	[SerializeField]
	protected TextMeshProUGUI statusLabel;

	[SerializeField]
	protected TextMeshProUGUI subLabel;

	[SerializeField]
	protected UITextButton button;

	[SerializeField]
	protected LayoutElement layoutElement;

	protected Action clickCallback;

	protected bool isCompleted;

	protected bool shouldShowButton = true;

	public string StatsName
	{
		get
		{
			return ((TMP_Text)statsNameLabel).text;
		}
		set
		{
			((TMP_Text)statsNameLabel).text = string.Format("{0} {1}", value, ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ");
			RefreshLayout();
		}
	}

	public string StatusText
	{
		get
		{
			return ((TMP_Text)statusLabel).text;
		}
		set
		{
			((TMP_Text)statusLabel).text = value;
			RefreshLayout();
		}
	}

	public string Description
	{
		get
		{
			return ((TMP_Text)subLabel).text;
		}
		set
		{
			((TMP_Text)subLabel).text = value;
		}
	}

	public bool IsCompleted
	{
		get
		{
			return isCompleted;
		}
		set
		{
			isCompleted = value;
			RefreshLayout();
		}
	}

	public Action ClickCallback
	{
		protected get
		{
			return clickCallback;
		}
		set
		{
			clickCallback = value;
		}
	}

	protected float PreferedHeight
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			if (IsCompleted)
			{
				return 30f;
			}
			return Mathf.Max(((TMP_Text)subLabel).GetPreferredValues().y + 30f, button.rectTransform.GetHeight() + 30f) + 10f;
		}
	}

	private void OnEnable()
	{
		button.OnSizeChanged += OnSizeChanged;
	}

	private void OnDisable()
	{
		button.OnSizeChanged -= OnSizeChanged;
	}

	private void OnSizeChanged(float newSize)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)subLabel).rectTransform.offsetMax = new Vector2(0f - (newSize + 10f), ((TMP_Text)subLabel).rectTransform.offsetMax.y);
		RefreshLayout();
	}

	public void SetData(string statsName, string description, bool completed, bool shouldShowButton)
	{
		isCompleted = completed;
		this.shouldShowButton = shouldShowButton;
		((TMP_Text)statsNameLabel).text = string.Format("{0} {1}", statsName, ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ");
		((TMP_Text)statusLabel).text = Localization.Get(IsCompleted ? "onlineview.completed" : "onlineview.required");
		((TMP_Text)subLabel).text = description;
		RefreshLayout();
	}

	protected void RefreshLayout()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)statusLabel).ForceMeshUpdate(false, false);
		((TMP_Text)subLabel).ForceMeshUpdate(false, false);
		float width = base.rectTransform.GetWidth() - ((TMP_Text)statsNameLabel).rectTransform.anchoredPosition.x - (((TMP_Text)statusLabel).GetRenderedValues(true).x + 5f);
		((TMP_Text)statsNameLabel).rectTransform.SetWidth(width);
		((Component)button).gameObject.SetActive(!IsCompleted && shouldShowButton);
		((Component)subLabel).gameObject.SetActive(!IsCompleted);
		float y = ((TMP_Text)subLabel).rectTransform.anchoredPosition.y;
		float width2 = base.rectTransform.GetWidth() - (button.rectTransform.GetWidth() + 10f);
		if (!IsCompleted && button.rectTransform.GetWidth() > base.rectTransform.GetWidth() * 0.5f)
		{
			y = button.rectTransform.anchoredPosition.y - button.rectTransform.GetHeight();
			width2 = base.rectTransform.GetWidth();
		}
		((TMP_Text)subLabel).rectTransform.SetWidth(width2);
		((TMP_Text)subLabel).rectTransform.SetAnchoredY(y);
		layoutElement.preferredHeight = Mathf.Abs(((TMP_Text)subLabel).rectTransform.anchoredPosition.y) + ((TMP_Text)subLabel).GetRenderedValues(true).y + 10f;
	}

	public void OnClicked()
	{
		ClickCallback?.Invoke();
	}
}
