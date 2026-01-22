using System;
using UnityEngine;
using UnityEngine.UI;

public class IconPopup : BasicPopup
{
	[Header("Icon Popup")]
	[SerializeField]
	protected RectTransform m_iconContainer;

	[SerializeField]
	protected ResourceWidget m_resourceWidget;

	[SerializeField]
	protected PlayerInfoIcon m_playerInfoIcon;

	protected Image m_iconImage;

	[NonSerialized]
	public SpriteHandle spriteHandle = new SpriteHandle();

	public PlayerInfoIcon PlayerInfoIcon => m_playerInfoIcon;

	public Sprite sprite
	{
		set
		{
			if ((Object)(object)value != (Object)null)
			{
				iconImage = UIUtils.GetImage(value);
				((Transform)((Graphic)iconImage).rectTransform).SetParent((Transform)(object)iconContainer, false);
				UIUtils.FitImageContentInParent(((Graphic)iconImage).rectTransform);
			}
		}
	}

	public RectTransform iconContainer => m_iconContainer;

	public Image iconImage
	{
		get
		{
			return m_iconImage;
		}
		set
		{
			m_iconImage = value;
		}
	}

	public float cost
	{
		get
		{
			return m_resourceWidget.Amount;
		}
		set
		{
			((Component)m_resourceWidget).gameObject.SetActive(value > 0f);
			m_resourceWidget.Amount = value;
		}
	}

	public override void Init()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		spriteHandle.SetCompletion(delegate(SpriteHandle handle)
		{
			sprite = handle.sprite;
		});
		LayoutElement component = ((Component)m_iconContainer).GetComponent<LayoutElement>();
		if (Object.op_Implicit((Object)(object)component))
		{
			m_iconContainer.sizeDelta = new Vector2(component.minWidth, component.minHeight);
		}
	}

	public override void ResetPopup()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.ResetPopup();
		if (Object.op_Implicit((Object)(object)iconImage))
		{
			Object.Destroy((Object)(object)((Component)iconImage).gameObject);
		}
		else if (((Transform)iconContainer).childCount > 0)
		{
			foreach (Transform item in (Transform)iconContainer)
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
		((Component)m_resourceWidget).gameObject.SetActive(false);
		if (Object.op_Implicit((Object)(object)m_playerInfoIcon))
		{
			((Component)m_playerInfoIcon).gameObject.SetActive(false);
		}
	}
}
