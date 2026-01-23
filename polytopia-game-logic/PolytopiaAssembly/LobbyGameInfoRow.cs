using System;
using System.Linq;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyGameInfoRow : UIBasicButton, IListCellNavigation
{
	[Header("Lobby Info row")]
	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected Image labelIcon;

	[SerializeField]
	protected TextMeshProUGUI nameLabel;

	[SerializeField]
	protected TextMeshProUGUI infoLabel;

	[SerializeField]
	protected GameInfoTimer timer;

	private UIBasicButton timerButton;

	[SerializeField]
	protected ColorStates labelColorStates = new ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 1f),
		hoverColor = new Color(0f, 0f, 0f, 1f),
		highlightedColor = new Color(0f, 0f, 0f, 1f),
		highlightedHoverColor = new Color(0f, 0f, 0f, 1f),
		disabledColor = new Color(1f, 1f, 1f, 0.502f)
	};

	protected LobbyGameViewModel lobbyGameViewModel;

	protected ParticipatorViewModel localParticipator;

	protected ParticipatorViewModel ownerParticipator;

	protected bool isButtonHeld;

	protected Vector2 buttonStartPosition;

	protected float buttonPressTime;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetFaceIcon(spriteHandle.sprite);
		});
		GameInfoTimer gameInfoTimer = timer;
		timerButton = ((gameInfoTimer != null) ? ((Component)gameInfoTimer).GetComponent<UIBasicButton>() : null);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += OnButtonClick;
		base.OnDown += OnButtonPress;
		base.OnUp += OnButtonRelease;
		base.OnExit += OnButtonExit;
		if ((Object)(object)timerButton != (Object)null)
		{
			timerButton.OnClicked += OnTimerClicked;
			timerButton.OnClicked += OnTimerButtonExit;
			timerButton.OnUp += OnTimerButtonExit;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.OnClicked -= OnButtonClick;
		base.OnDown -= OnButtonPress;
		base.OnUp -= OnButtonRelease;
		base.OnExit -= OnButtonExit;
		if ((Object)(object)timerButton != (Object)null)
		{
			timerButton.OnClicked -= OnTimerClicked;
			timerButton.OnClicked -= OnTimerButtonExit;
			timerButton.OnUp -= OnTimerButtonExit;
		}
	}

	private void Update()
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (isButtonHeld)
		{
			Vector2 val = (buttonStartPosition - PolytopiaInput.mousePosition) / ScalingUtils.GetDPI();
			if (((Vector2)(ref val)).sqrMagnitude > 0.0035f)
			{
				isButtonHeld = false;
			}
		}
	}

	public void OnTimerClicked(int timerID, BaseEventData timerEventData)
	{
	}

	private void OnTimerButtonExit(int id, BaseEventData eventData = null)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		if (eventData is PointerEventData)
		{
			OnPointerExit((PointerEventData)eventData);
		}
	}

	public void SetData(LobbyGameViewModel lobbyGameViewModel)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		this.lobbyGameViewModel = lobbyGameViewModel;
		LobbyManager.HasOwner(lobbyGameViewModel);
		bool flag = lobbyGameViewModel.OwnerId == AccountManager.PlayerAccountId;
		int acceptedPlayersCount = LobbyManager.GetAcceptedPlayersCount(lobbyGameViewModel);
		localParticipator = LobbyManager.GetLocalParticipator(lobbyGameViewModel);
		ownerParticipator = LobbyManager.GetOwnerParticipator(lobbyGameViewModel);
		((TMP_Text)nameLabel).text = this.lobbyGameViewModel.Name;
		if (localParticipator != null)
		{
			((Component)labelIcon).gameObject.SetActive(lobbyGameViewModel.ChallengermodeGameId.HasValue);
			Color sourceColor = bgColorStates.defaultColor;
			string lobbyDescription = LobbyManager.GetLobbyDescription(lobbyGameViewModel, AccountManager.PlayerAccountId);
			bool num = localParticipator == null || localParticipator.InvitationState != PlayerInvitationState.Accepted;
			bool flag2 = flag && acceptedPlayersCount < 2;
			bool flag3 = LobbyManager.CanBeStartedByPlayer(lobbyGameViewModel, AccountManager.PlayerAccountId);
			if (num || flag2 || flag3)
			{
				sourceColor = bgColorStates.highlightedColor;
			}
			if (localParticipator.InvitationState == PlayerInvitationState.Accepted)
			{
				LoadFaceIcon((TribeData.Type)localParticipator.SelectedTribe);
			}
			else
			{
				LoadFaceIcon("neutral");
			}
			if (lobbyGameViewModel.TimeLimit == -1)
			{
				((Component)timer).gameObject.SetActive(true);
				timer.SetIconVisible(isVisible: true);
			}
			else
			{
				((Component)timer).gameObject.SetActive(false);
				timer.SetIconVisible(isVisible: false);
			}
			((TMP_Text)infoLabel).text = lobbyDescription;
			bgColorStates.defaultColor = ColorUtil.SetAlphaOnColor(sourceColor, 0.8f);
			UpdateColors();
		}
	}

	public override void UpdateColors()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		TextMeshProUGUI obj = nameLabel;
		Color color = (((Graphic)infoLabel).color = GetColorForState(labelColorStates));
		((Graphic)obj).color = color;
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
		icon.sprite = faceIcon;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
	}

	private void OnButtonPress(int id, BaseEventData eventData)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		isButtonHeld = true;
		buttonPressTime = Time.time;
		buttonStartPosition = PolytopiaInput.mousePosition;
	}

	private void OnButtonRelease(int id, BaseEventData eventData)
	{
		if (isButtonHeld)
		{
			isButtonHeld = false;
			ShowLobbyInfo();
		}
	}

	private void OnButtonClick(int id, BaseEventData eventData)
	{
		if (!(eventData is PointerEventData))
		{
			ShowLobbyInfo();
		}
	}

	private void OnButtonExit(int id, BaseEventData eventData)
	{
		isButtonHeld = false;
	}

	private void ShowLobbyInfo()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (!PopupManager.IsPopupShowing<LobbyPopup>())
		{
			LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
			if (lobbyGameViewModel != null)
			{
				lobbyPopup.SetData(lobbyGameViewModel);
			}
			lobbyPopup.Show(InputManager.GetInputPosition());
		}
	}

	private ParticipatorViewModel GetParticipatorViewModel(Guid? userId)
	{
		return lobbyGameViewModel.Participators.FirstOrDefault(delegate(ParticipatorViewModel participator)
		{
			Guid userId2 = participator.UserId;
			Guid? guid = userId;
			return userId2 == guid;
		});
	}

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)base.button;
	}

	public Selectable GetAccessorySelectable()
	{
		if (UINavigationManager.IsValidSelectable((Selectable)(object)timer.button))
		{
			return (Selectable)(object)timer.button;
		}
		return (Selectable)(object)base.button;
	}
}
