using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using PolytopiaBackendBase.Common;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerButton : UIButtonBase
{
	protected enum Type
	{
		None,
		PlayerButton,
		TribeButton
	}

	[Header("Player Button")]
	public TMPLocalizer label;

	public Image icon;

	public TextMeshProUGUI text;

	public AvatarView avatarView;

	public float iconSizeMultiplier;

	public TribeButtonStarContainer starContainer;

	public bool enableShine = true;

	public bool shouldFitIconToParent;

	public bool shouldCenterWithPivot;

	public bool shouldOverrideBackgroundColor;

	public RectTransform localPlayerLabel;

	[Header("Bg")]
	public RectTransform bgContainer;

	public Image bg;

	public Image outline;

	public Image shine;

	public Image empesize;

	public Image padlock;

	[Header("Badge")]
	public RectTransform badgeHolder;

	public Image badgeIcon;

	public Image badgeBackground;

	[Header("Colors")]
	public RoundButtonColorStates bgColors;

	public RoundButtonColorStates outlineColors;

	protected Type type;

	private PlayerData friend;

	private TribeData tribe;

	private SkinType skinType;

	protected bool playerButtonEnable = true;

	protected bool isUnmigratedUser;

	public bool isEmphasized;

	private PollAnimation<float> hoverAnimation;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	private SpriteHandle badgeSpriteHandle = new SpriteHandle();

	private bool longClickProcessed;

	private const float LONG_CLICK_DURATION = 2f;

	public override bool Highlighted
	{
		get
		{
			return base.Highlighted;
		}
		set
		{
			base.Highlighted = value;
			UpdateBg();
		}
	}

	public bool PlayerButtonEnable
	{
		get
		{
			return playerButtonEnable;
		}
		set
		{
			playerButtonEnable = value;
			UpdateBg();
		}
	}

	public bool BadgeEnabled
	{
		get
		{
			return ((Component)badgeHolder).gameObject.activeSelf;
		}
		set
		{
			((Component)badgeHolder).gameObject.SetActive(value);
		}
	}

	public TribeData Tribe => tribe;

	public event Action<TribeData> OnLongClick;

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetFaceIcon(spriteHandle.sprite);
		});
		badgeSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetBadgeIcon(spriteHandle.sprite);
		});
		starContainer.Init();
		((Component)empesize).gameObject.SetActive(isEmphasized);
		longClickProcessed = false;
	}

	public void Update()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (hoverAnimation != null && (Object)(object)hoverObject != (Object)null)
		{
			float num = Mathf.LerpUnclamped(hoverAnimation.startValue, hoverAnimation.targetValue, hoverAnimation.GetEasedProgress());
			((Transform)hoverObject).localScale = new Vector3(num, num, 1f);
			if (hoverAnimation.IsCompleted())
			{
				hoverAnimation = null;
			}
		}
		if (buttonState == ButtonStates.Down)
		{
			List<PolytopiaTouch> currentTouches = InputManager.CurrentTouches;
			if (!longClickProcessed && currentTouches.Count == 1 && currentTouches[0].IsValidLongPress(2f))
			{
				longClickProcessed = true;
				this.OnLongClick?.Invoke(tribe);
			}
		}
		else if (buttonState == ButtonStates.Over)
		{
			longClickProcessed = false;
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.OnPointerEnter(eventData);
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			hoverAnimation = new PollAnimation<float>(0.2f, ((Transform)hoverObject).localScale.x, 1.1f);
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
		{
			hoverAnimation = new PollAnimation<float>(0.3f, ((Transform)hoverObject).localScale.x, 1f, Easing.OutBack, 5f);
		}
		if (type == Type.TribeButton && TribeSelectorScreen.IsButtonInMixer(this))
		{
			TribeSelectorScreen.MixedTribeButtonUp(this);
		}
	}

	public override void PointerDown(PointerEventData eventData)
	{
		base.PointerDown(eventData);
		if (type == Type.TribeButton && ButtonEnabled && tribe != null && playerButtonEnable && GameManager.GetPurchaseManager().IsTribeUnlocked(tribe.type) && starContainer.Stars >= 3)
		{
			if (!SystemManager.ShouldUseTouchInterface() && Input.GetKey((KeyCode)308))
			{
				TribeSelectorScreen.AddTribeToMixer(this);
			}
			else if (SystemManager.ShouldUseTouchInterface())
			{
				TribeSelectorScreen.AddTribeToMixer(this);
			}
			else
			{
				TribeSelectorScreen.ClearTribeMixer();
			}
		}
		else
		{
			TribeSelectorScreen.ClearTribeMixer();
		}
	}

	public override void PointerUp(PointerEventData eventData)
	{
		if (longClickProcessed)
		{
			buttonState = ButtonStates.Over;
			CancelTweens();
			if ((Object)(object)hoverObject != (Object)null && AnimationsEnabled)
			{
				pressTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)hoverObject, 1f, 0.3f), (Ease)27, 5f);
			}
		}
		else
		{
			base.PointerUp(eventData);
		}
		if (type == Type.TribeButton && TribeSelectorScreen.IsButtonInMixer(this))
		{
			TribeSelectorScreen.MixedTribeButtonUp(this);
		}
	}

	public override void PointerClick(PointerEventData eventData)
	{
		if (longClickProcessed)
		{
			buttonState = ButtonStates.Over;
		}
		else
		{
			base.PointerClick(eventData);
		}
	}

	public void SetFriendData(PlayerData friend)
	{
		type = Type.PlayerButton;
		this.friend = friend;
		string spriteStringForFriend = FriendUtils.GetSpriteStringForFriend(friend);
		if (this.friend.type == PlayerData.Type.Bot)
		{
			label.Text = Localization.Get("friendlist.bot", Localization.Get(GameModeUtils.GetDifficultyName(this.friend.botDifficulty)));
		}
		else
		{
			label.Text = spriteStringForFriend + this.friend.GetName();
		}
		switch (this.friend.type)
		{
		case PlayerData.Type.Bot:
			LoadFaceIcon("robot");
			break;
		case PlayerData.Type.None:
			LoadFaceIcon("neutral");
			break;
		case PlayerData.Type.Local:
		case PlayerData.Type.Friend:
		case PlayerData.Type.Player:
			if (friend.profile.avatarState != null)
			{
				LoadAvatarState(friend.profile.avatarState);
			}
			else if (this.friend.knownTribe)
			{
				LoadFaceIcon(tribe.style);
			}
			else
			{
				LoadFaceIcon("neutral");
			}
			break;
		}
		Refresh();
	}

	public void SetPlayerData(ParticipatorViewModel participator, AvatarState avatarState)
	{
		SetPlayerDataInternal(participator.UserId, participator.Name, avatarState, autoPlay: false, isDead: false, showHumanAsAvatar: false, PlayerDataUtils.GetPlatform(participator.GameVersion));
	}

	public void SetPlayerData(Guid playerId, string name, AvatarState avatarState)
	{
		SetPlayerDataInternal(playerId, name, avatarState);
	}

	public void SetButtonData(string label, SpriteAddress[] iconSpriteAddress)
	{
		type = Type.PlayerButton;
		this.label.Text = label;
		iconSpriteHandle.Request(iconSpriteAddress);
		starContainer.Score = -1;
		Refresh();
	}

	public void SetBotData(string name)
	{
		SetPlayerDataInternal(null, name, null, autoPlay: true);
	}

	public void SetPlayerSummary(GameStateSummary.GamePlayerSummary playerSummary, AvatarState avatarState, bool showHumanAsAvatar = false)
	{
		bool flag = playerSummary.AutoPlay;
		if (flag && showHumanAsAvatar)
		{
			flag = playerSummary.PolytopiaId == Guid.Empty;
		}
		Guid value = playerSummary.PolytopiaId ?? Guid.Empty;
		SetPlayerDataInternal(value, playerSummary.UserName, avatarState, flag, playerSummary.IsDead, showHumanAsAvatar);
	}

	public void SetBadge(string iconId, Color backgroundColor)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		badgeSpriteHandle.Request(SpriteData.GetIconSpriteAddress(iconId));
		((Graphic)badgeBackground).color = backgroundColor;
	}

	public void ShowLocalPlayerLabel(bool showLabel)
	{
		((Component)localPlayerLabel).gameObject.SetActive(showLabel);
	}

	private void SetPlayerDataInternal(Guid? playerId, string name, AvatarState avatarState, bool autoPlay = false, bool isDead = false, bool showHumanAsAvatar = false, Platform? platform = null)
	{
		type = Type.PlayerButton;
		string text = ((!platform.HasValue) ? FriendUtils.GetSpriteStringForFriendIdWithFormat(playerId, "<size=80%>{0}</size>") : FriendUtils.GetSpriteStringForFriendIdOnPlatformWithFormat(playerId, platform.Value, "<size=80%>{0}</size>"));
		label.Text = text + name;
		if (autoPlay)
		{
			LoadFaceIcon("robot");
		}
		else if (isDead && !showHumanAsAvatar)
		{
			LoadFaceIcon("dead");
		}
		else
		{
			SetAvatarStateInternal(avatarState);
		}
		starContainer.Score = -1;
		Refresh();
	}

	private void SetAvatarStateInternal(AvatarState avatarState)
	{
		if (avatarState != null)
		{
			LoadAvatarState(avatarState);
		}
		else
		{
			LoadFaceIcon("neutral");
		}
	}

	public void SetAvatarState(AvatarState avatarState, bool keepText = false)
	{
		SetAvatarStateInternal(avatarState);
		if (!keepText)
		{
			UnsetText();
		}
		Refresh();
	}

	public void SetIcon(string spriteId)
	{
		LoadIcon(spriteId);
	}

	public void UnsetText()
	{
		label.Key = "";
		label.Text = "";
	}

	public void SetTribeData(TribeData tribe)
	{
		type = Type.TribeButton;
		this.tribe = tribe;
		label.Key = this.tribe.displayName;
		LoadFaceIcon(this.tribe.style);
		id = tribe.style - 1;
		Refresh();
	}

	public void SetSkin(SkinType skinType)
	{
		this.skinType = skinType;
		LoadFaceIcon();
	}

	public void Refresh()
	{
		isUnmigratedUser = false;
		if (type == Type.TribeButton)
		{
			if (GameManager.PreliminaryGameSettings.GameType == GameType.SinglePlayer && GameManager.PreliminaryGameSettings.BaseGameMode != GameMode.Custom)
			{
				starContainer.gameMode = GameManager.PreliminaryGameSettings.RulesGameMode;
				bool flag = false;
				if (GameManager.PreliminaryGameSettings.RulesGameMode == GameMode.Perfection)
				{
					ScoreManager.TryGetTribeScore(tribe.type, out var score);
					starContainer.Score = score;
					flag = score >= 100000;
				}
				else
				{
					ScoreManager.TryGetTribeRating(tribe.type, out var rating);
					starContainer.Rating = rating;
					flag = rating >= 100;
				}
				((Component)empesize).gameObject.SetActive(flag || isEmphasized);
				playerButtonEnable = GameManager.PreliminaryGameSettings.IsTribeEnabled(tribe.type);
			}
			else
			{
				starContainer.Score = -1;
				playerButtonEnable = GameManager.PreliminaryGameSettings.IsTribeEnabled(tribe.type);
				if (GameManager.GameState != null && GameManager.GameState.Settings != null)
				{
					playerButtonEnable = GameManager.GameState.Settings.IsTribeEnabled(tribe.type);
				}
			}
			((Component)padlock).gameObject.SetActive(!GameManager.GetPurchaseManager().IsTribeUnlocked(tribe.type));
		}
		UpdateBg();
	}

	private void LoadAvatarState(AvatarState avatarState)
	{
		((Component)icon).gameObject.SetActive(false);
		((Component)avatarView).gameObject.SetActive(true);
		avatarView.SetState(avatarState);
	}

	private void LoadIcon(string iconId)
	{
		iconSpriteHandle.Request(SpriteData.GetIconSpriteAddress(iconId));
	}

	private void LoadFaceIcon(string faceId)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(faceId));
	}

	private void LoadFaceIcon(int tribe)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(tribe));
	}

	private void LoadFaceIcon(TribeData.Type tribe)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(tribe));
	}

	private void LoadFaceIcon()
	{
		SpriteAddress[] spriteAddresses = new SpriteAddress[2]
		{
			SpriteData.GetHeadSpriteAddress(skinType.GetName()),
			SpriteData.GetHeadSpriteAddress(tribe.type)
		};
		iconSpriteHandle.Request(spriteAddresses);
	}

	private void SetFaceIcon(Sprite faceIcon)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		icon.sprite = faceIcon;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		Vector2 sizeDelta = ((Graphic)icon).rectTransform.sizeDelta;
		((Graphic)icon).rectTransform.sizeDelta = sizeDelta * iconSizeMultiplier;
		RectTransform obj = ((Graphic)icon).rectTransform;
		float x = faceIcon.pivot.x;
		Rect rect = faceIcon.rect;
		float num = x / ((Rect)(ref rect)).width;
		float y = faceIcon.pivot.y;
		rect = faceIcon.rect;
		obj.pivot = new Vector2(num, y / ((Rect)(ref rect)).height);
		((Component)icon).gameObject.SetActive(true);
		if (shouldFitIconToParent)
		{
			UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
		}
		if (shouldCenterWithPivot)
		{
			RectTransform obj2 = ((Graphic)icon).rectTransform;
			Vector2 pivot = icon.sprite.pivot;
			rect = icon.sprite.rect;
			obj2.pivot = pivot / ((Rect)(ref rect)).size;
		}
	}

	private void SetBadgeIcon(Sprite icon)
	{
		badgeIcon.sprite = icon;
	}

	public void SetLabel(string text)
	{
		label.Text = text;
	}

	public void SetPadlockVisible(bool isVisible)
	{
		((Component)padlock).gameObject.SetActive(isVisible);
	}

	public void CenterTribeIcon()
	{
	}

	public void UpdateBg()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!shouldOverrideBackgroundColor)
		{
			((Graphic)bg).color = bgColors.GetColorForState(playerButtonEnable, highlighted: true);
		}
		((Graphic)outline).color = outlineColors.GetColorForState(playerButtonEnable, highlighted: true);
		((Component)shine).gameObject.SetActive(playerButtonEnable && enableShine);
	}
}
