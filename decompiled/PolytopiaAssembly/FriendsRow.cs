using System;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FriendsRow : UIBasicButton, IListCellNavigation
{
	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected AvatarView avatarView;

	[SerializeField]
	protected TextMeshProUGUI label;

	[SerializeField]
	protected UIButtonBase removeButton;

	[SerializeField]
	protected UITextButton textButton;

	[SerializeField]
	protected ColorStates labelColorStates = new ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 1f),
		hoverColor = new Color(0f, 0f, 0f, 1f),
		highlightedColor = new Color(0f, 0f, 0f, 1f),
		highlightedHoverColor = new Color(0f, 0f, 0f, 1f),
		disabledColor = new Color(1f, 1f, 1f, 1f)
	};

	[HideInInspector]
	public Action<FriendActions, Guid> FriendActionCallback;

	protected PlayerData data;

	protected FriendActions buttonAction;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	public override bool UpdateScrollerOnHighlight
	{
		set
		{
			base.UpdateScrollerOnHighlight = value;
			removeButton.UpdateScrollerOnHighlight = value;
			textButton.UpdateScrollerOnHighlight = value;
		}
	}

	public Guid Id => data.profile.id;

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetFaceIcon(spriteHandle.sprite);
		});
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		textButton.OnSizeChanged -= TextButtonOnSizeChanged;
		base.OnClicked -= OnButtonClicked;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += OnButtonClicked;
	}

	public override void UpdateColors()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		switch (buttonState)
		{
		case ButtonStates.None:
			if (!ButtonEnabled)
			{
				((Graphic)label).color = labelColorStates.disabledColor;
			}
			else if (Highlighted && UINavigationManager.navigationType == UINavigationManager.NavigationType.Buttons && (Object)(object)selectionObject == (Object)null)
			{
				((Graphic)label).color = labelColorStates.highlightedColor;
			}
			else
			{
				((Graphic)label).color = labelColorStates.defaultColor;
			}
			break;
		case ButtonStates.Over:
		case ButtonStates.Down:
			if (!ButtonEnabled)
			{
				((Graphic)label).color = labelColorStates.disabledColor;
			}
			else if (Highlighted && UINavigationManager.navigationType == UINavigationManager.NavigationType.Buttons && (Object)(object)selectionObject == (Object)null)
			{
				((Graphic)label).color = labelColorStates.highlightedHoverColor;
			}
			else if (UINavigationManager.navigationType == UINavigationManager.NavigationType.Mouse)
			{
				((Graphic)label).color = labelColorStates.hoverColor;
			}
			else
			{
				((Graphic)label).color = labelColorStates.defaultColor;
			}
			break;
		}
	}

	public void SetData(PlayerData data)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		this.data = data;
		bool flag = this.data.type == PlayerData.Type.Player;
		string name = data.GetName();
		if (this.data.type == PlayerData.Type.Player)
		{
			((TMP_Text)label).text = Localization.Get("playerpickerview.you", name);
		}
		else if (data.type == PlayerData.Type.Bot)
		{
			((TMP_Text)label).text = Localization.Get("playerpickerview.bot", name, Localization.Get(GameModeUtils.GetDifficultyName(this.data.botDifficulty)));
		}
		else
		{
			((TMP_Text)label).text = name;
		}
		((Graphic)BG).color = (Color)(flag ? ColorUtil.SetAlphaOnColor(ColorConstants.blue, 0.8f) : new Color(0f, 0f, 0f, 0.8f));
		bool flag2 = false;
		bool flag3 = false;
		string key = string.Empty;
		switch (this.data.state)
		{
		case PlayerData.State.None:
		case PlayerData.State.Rejected:
			flag3 = true;
			key = "friendlist.new.button";
			break;
		case PlayerData.State.IsYou:
			flag2 = false;
			flag3 = false;
			break;
		case PlayerData.State.Accepted:
			flag2 = true;
			break;
		case PlayerData.State.SentRequest:
			flag2 = true;
			break;
		case PlayerData.State.ReceivedRequest:
			flag3 = true;
			key = "friendlist.acceptinvite.button";
			break;
		}
		buttonAction = FriendUtils.GetActionFromState(this.data.state);
		ButtonEnabled = false;
		switch (this.data.type)
		{
		case PlayerData.Type.Bot:
			flag3 = false;
			flag2 = true;
			buttonAction = FriendActions.Remove;
			LoadFaceIcon("robot");
			break;
		case PlayerData.Type.None:
			LoadFaceIcon("neutral");
			break;
		case PlayerData.Type.Local:
			ButtonEnabled = true;
			goto case PlayerData.Type.Friend;
		case PlayerData.Type.Friend:
		case PlayerData.Type.Player:
			if (data.profile.avatarState != null)
			{
				LoadAvatarState(data.profile.avatarState);
			}
			else if (this.data.knownTribe)
			{
				LoadFaceIcon(this.data.tribe);
			}
			else
			{
				LoadFaceIcon("neutral");
			}
			break;
		}
		((Component)removeButton).gameObject.SetActive(flag2);
		((Component)textButton).gameObject.SetActive(flag3);
		if (flag3)
		{
			textButton.OnSizeChanged += TextButtonOnSizeChanged;
			textButton.Key = key;
		}
		if (flag2)
		{
			UpdateLabelSize(0f - (removeButton.rectTransform.sizeDelta.x + Mathf.Abs(removeButton.rectTransform.anchoredPosition.x) + 10f));
		}
		else
		{
			UpdateLabelSize(-12f);
		}
		UpdateColors();
	}

	private void TextButtonOnSizeChanged(float newSize)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		UpdateLabelSize(0f - (newSize + Mathf.Abs(textButton.rectTransform.anchoredPosition.x) + 10f));
	}

	private void UpdateLabelSize(float rightOffset)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Vector2 offsetMax = default(Vector2);
		((Vector2)(ref offsetMax))._002Ector(rightOffset, ((TMP_Text)label).rectTransform.offsetMax.y);
		((TMP_Text)label).rectTransform.offsetMax = offsetMax;
		TextMeshProUGUI obj = label;
		Rect rect = ((TMP_Text)label).rectTransform.rect;
		float width = ((Rect)(ref rect)).width;
		rect = ((TMP_Text)label).rectTransform.rect;
		float y = ((TMP_Text)obj).GetPreferredValues(width, ((Rect)(ref rect)).height).y;
		base.rectTransform.SetHeight(Mathf.Max(74f, y + 20f));
	}

	private void LoadAvatarState(AvatarState avatarState)
	{
		((Component)icon).gameObject.SetActive(false);
		((Component)avatarView).gameObject.SetActive(true);
		avatarView.SetState(avatarState);
	}

	private void LoadFaceIcon(string faceId)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(faceId));
	}

	private void LoadFaceIcon(TribeData.Type type)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(type));
	}

	private void SetFaceIcon(Sprite faceIcon)
	{
		((Component)icon).gameObject.SetActive(true);
		((Component)avatarView).gameObject.SetActive(false);
		icon.sprite = faceIcon;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
	}

	public void OnRemove()
	{
		FriendActionCallback?.Invoke(buttonAction, data.profile.id);
	}

	public void OnTextButtonClicked()
	{
		FriendActionCallback?.Invoke(buttonAction, data.profile.id);
	}

	public void Select()
	{
		UINavigationManager.Select(GetMainSelectable());
	}

	public Selectable GetMainSelectable()
	{
		if (((Behaviour)base.button).enabled)
		{
			return (Selectable)(object)base.button;
		}
		if (((Component)removeButton).gameObject.activeSelf)
		{
			return (Selectable)(object)removeButton.button;
		}
		if (((Component)textButton).gameObject.activeSelf)
		{
			return (Selectable)(object)textButton.button;
		}
		return null;
	}

	public Selectable GetAccessorySelectable()
	{
		if (((Component)removeButton).gameObject.activeSelf)
		{
			return (Selectable)(object)removeButton.button;
		}
		if (((Component)textButton).gameObject.activeSelf)
		{
			return (Selectable)(object)textButton.button;
		}
		if (((Behaviour)base.button).enabled)
		{
			return (Selectable)(object)base.button;
		}
		return null;
	}

	public void SetButtonEnabled(bool enabled)
	{
		if (((Component)removeButton).gameObject.activeSelf)
		{
			removeButton.ButtonEnabled = enabled;
		}
		else if (((Component)textButton).gameObject.activeSelf)
		{
			textButton.ButtonEnabled = enabled;
		}
	}

	private void OnButtonClicked(int id, BaseEventData eventData)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		UpdateHotseatPlayerPopup popup = PopupManager.GetUpdateHotseatPlayerPopup();
		popup.Header = Localization.Get("updateavatar.title");
		popup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("updateavatar.save", PopupBase.PopupButtonData.States.Selected, delegate(int innerId, BaseEventData innerEventData)
			{
				popup.OnMainButtonClicked(innerId);
				LoadAvatarState(data.profile.avatarState);
				((TMP_Text)label).text = data.GetName();
			})
		};
		popup.SetData(data.profile);
		popup.Show(InputManager.GetInputPosition());
	}
}
