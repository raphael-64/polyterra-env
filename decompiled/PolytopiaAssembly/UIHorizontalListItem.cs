using System;
using UnityEngine;
using UnityEngine.UI;

public class UIHorizontalListItem : UITextButton, IOmnicursorAffixCallback
{
	public IgnoredByOmnicursorRaycast omnicursorIgnore;

	[HideInInspector]
	public Action<int> clickedCallback;

	[HideInInspector]
	public int index = -1;

	protected bool m_selected;

	public bool Selected
	{
		get
		{
			return m_selected;
		}
		set
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			m_selected = value;
			if (value)
			{
				((Graphic)BG).color = Color.white;
				((Graphic)labelLocalizer.TextComponent).color = Color.black;
				return;
			}
			((Graphic)BG).color = Color.clear;
			if (ButtonEnabled)
			{
				((Graphic)labelLocalizer.TextComponent).color = Color.white;
			}
			else
			{
				((Graphic)labelLocalizer.TextComponent).color = Color.Lerp(Color.grey, Color.white, 0.25f);
			}
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	public void Init()
	{
		Selected = false;
	}

	public void SetData(int index, string text)
	{
		this.index = index;
		base.text = text;
	}

	protected override void OnButtonClicked()
	{
		base.OnButtonClicked();
		clickedCallback?.Invoke(index);
	}

	public override void UpdateColors()
	{
		if (!Selected)
		{
			base.UpdateColors();
		}
	}

	public void OnOmnicursorAffixToGameObject()
	{
		OnButtonClicked();
	}
}
