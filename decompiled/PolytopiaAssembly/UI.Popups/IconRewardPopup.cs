using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Popups;

public class IconRewardPopup : BasicPopup
{
	[Header("Icon")]
	[SerializeField]
	protected RectTransform m_iconContainer;

	[Header("Reward button")]
	[SerializeField]
	private UIRoundButton rewardButton;

	protected Image iconImage;

	private SpriteHandle spriteHandle = new SpriteHandle();

	public Action OnRewardClicked;

	public UIRoundButton RewardButton
	{
		get
		{
			return rewardButton;
		}
		set
		{
			rewardButton = value;
		}
	}

	public Image IconImage
	{
		get
		{
			return iconImage;
		}
		set
		{
			iconImage = value;
		}
	}

	public SpriteHandle SpriteHandle
	{
		get
		{
			return spriteHandle;
		}
		set
		{
			spriteHandle = value;
		}
	}

	public RectTransform iconContainer => m_iconContainer;

	public Sprite sprite
	{
		set
		{
			if ((Object)(object)value != (Object)null)
			{
				IconImage = UIUtils.GetImage(value);
				((Transform)((Graphic)IconImage).rectTransform).SetParent((Transform)(object)iconContainer, false);
				UIUtils.FitImageContentInParent(((Graphic)IconImage).rectTransform);
			}
		}
	}

	public override void Init()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		SpriteHandle.SetCompletion(delegate(SpriteHandle handle)
		{
			sprite = handle.sprite;
		});
		LayoutElement component = ((Component)m_iconContainer).GetComponent<LayoutElement>();
		if (Object.op_Implicit((Object)(object)component))
		{
			m_iconContainer.sizeDelta = new Vector2(component.minWidth, component.minHeight);
		}
		base.IsUnskippable = true;
		rewardButton.buttonActive = true;
		rewardButton.OnClicked += RewardButtonClicked;
	}

	protected override void OnShowComplete()
	{
		base.OnShowComplete();
		UIUtils.SetExplicitNavigation(base.rectTransform);
		UINavigationManager.Select((Selectable)(object)rewardButton.button);
	}

	private void RewardButtonClicked(int id, BaseEventData eventData)
	{
		OnHide(id, eventData);
		OnRewardClicked?.Invoke();
	}

	public override void ResetPopup()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.ResetPopup();
		if (Object.op_Implicit((Object)(object)IconImage))
		{
			Object.Destroy((Object)(object)((Component)IconImage).gameObject);
		}
		else if (((Transform)iconContainer).childCount > 0)
		{
			foreach (Transform item in (Transform)iconContainer)
			{
				Object.Destroy((Object)(object)((Component)item).gameObject);
			}
		}
		rewardButton.Highlighted = false;
	}
}
