using System;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TournamentInfoRow : UIBasicButton, IListCellNavigation
{
	private const float TIME_UPDATE_INTERVAL = 1f;

	[Header("Tournament Info row")]
	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected Image labelIcon;

	[SerializeField]
	protected RectTransform basicIcon;

	[SerializeField]
	protected RectTransform officialIcon;

	[SerializeField]
	protected TextMeshProUGUI nameLabel;

	[SerializeField]
	protected TextMeshProUGUI infoLabel;

	[SerializeField]
	protected UIValueBoxWidget registered;

	[SerializeField]
	protected UIValueBoxWidget confirmed;

	[SerializeField]
	protected ColorStates labelColorStates = new ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 1f),
		hoverColor = new Color(0f, 0f, 0f, 1f),
		highlightedColor = new Color(0f, 0f, 0f, 1f),
		highlightedHoverColor = new Color(0f, 0f, 0f, 1f),
		disabledColor = new Color(1f, 1f, 1f, 0.502f)
	};

	protected TournamentViewModel tournamentViewModel;

	protected bool isButtonHeld;

	protected Vector2 buttonStartPosition;

	protected float buttonPressTime;

	protected float updateTimer;

	protected bool isWaitingForConfirmationState = true;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	public override void Awake()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetLabelIcon(spriteHandle.sprite);
		});
		registered.SetLabel(Localization.Get("onlineview.tournament.info.registered"));
		registered.SetBackgroundColor(ColorConstants.gray);
		confirmed.SetLabel(Localization.Get("onlineview.tournament.info.confirmed"));
		confirmed.SetBackgroundColor(ColorConstants.green);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += OnButtonClick;
		base.OnDown += OnButtonPress;
		base.OnUp += OnButtonRelease;
		base.OnExit += OnButtonExit;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.OnClicked -= OnButtonClick;
		base.OnDown -= OnButtonPress;
		base.OnUp -= OnButtonRelease;
		base.OnExit -= OnButtonExit;
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
		if (updateTimer >= 1f)
		{
			updateTimer = 0f;
			UpdateInfoLabel();
			if (isWaitingForConfirmationState && tournamentViewModel.ReadyTime.HasValue && DateTime.UtcNow > tournamentViewModel.ReadyTime.Value)
			{
				RefreshRow();
			}
		}
		else
		{
			updateTimer += Time.deltaTime;
		}
	}

	public void SetData(TournamentViewModel tournamentViewModel)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		this.tournamentViewModel = tournamentViewModel;
		Color sourceColor = Color.black;
		((Component)labelIcon).gameObject.SetActive(false);
		((Component)registered).gameObject.SetActive(false);
		((Component)confirmed).gameObject.SetActive(false);
		((Component)basicIcon).gameObject.SetActive(true);
		((Component)officialIcon).gameObject.SetActive(false);
		if (tournamentViewModel == null)
		{
			((TMP_Text)nameLabel).text = Localization.Get("gameinfo.nodata.title");
			((TMP_Text)infoLabel).text = Localization.Get("gameinfo.nodata");
			bgColorStates.defaultColor = ColorUtil.SetAlphaOnColor(ColorConstants.red, 0.8f);
			UpdateColors();
			return;
		}
		if (tournamentViewModel.State == TournamentState.Published)
		{
			((Component)registered).gameObject.SetActive(true);
			registered.SetValue(tournamentViewModel.NumberRegistered.ToString());
			if (tournamentViewModel.ReadyTime.HasValue && tournamentViewModel.ReadyTime.Value > DateTime.UtcNow)
			{
				isWaitingForConfirmationState = true;
			}
			else
			{
				isWaitingForConfirmationState = false;
				int num = 0;
				if (tournamentViewModel.Members != null)
				{
					for (int i = 0; i < tournamentViewModel.Members.Count; i++)
					{
						if (tournamentViewModel.Members[i].IsConfirmed)
						{
							num++;
						}
					}
				}
				((Component)confirmed).gameObject.SetActive(true);
				confirmed.SetValue($"{num}/{tournamentViewModel.TotalSlots}");
			}
		}
		else
		{
			isWaitingForConfirmationState = false;
		}
		if (tournamentViewModel.PersonalViewModel.PariticipationComplete)
		{
			sourceColor = ColorConstants.green;
		}
		else if (!tournamentViewModel.PersonalViewModel.HasSignedUp)
		{
			sourceColor = ColorConstants.blue;
		}
		else if (tournamentViewModel.PersonalViewModel.HasSignedUp && !tournamentViewModel.PersonalViewModel.HasConfirmed)
		{
			sourceColor = ((!tournamentViewModel.ReadyTime.HasValue || !(DateTime.UtcNow < tournamentViewModel.ReadyTime.Value)) ? ColorConstants.blue : Color.black);
		}
		((Component)basicIcon).gameObject.SetActive(!tournamentViewModel.OfficialTournament);
		((Component)officialIcon).gameObject.SetActive(tournamentViewModel.OfficialTournament);
		((TMP_Text)nameLabel).text = tournamentViewModel.Name;
		bgColorStates.defaultColor = ColorUtil.SetAlphaOnColor(sourceColor, 0.8f);
		UpdateInfoLabel();
		UpdateColors();
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}

	private void UpdateInfoLabel()
	{
		if (tournamentViewModel != null)
		{
			switch (tournamentViewModel.State)
			{
			case TournamentState.Starting:
			case TournamentState.Running:
				((TMP_Text)infoLabel).text = Localization.Get("onlineview.tournament.state.ongoing");
				return;
			case TournamentState.Concluded:
			case TournamentState.Completed:
				((TMP_Text)infoLabel).text = Localization.Get("onlineview.tournament.state.ended");
				return;
			case TournamentState.Unknown:
			case TournamentState.Unpublished:
			case TournamentState.Cancelling:
			case TournamentState.Cancelled:
			case TournamentState.Closing:
			case TournamentState.Closed:
				((TMP_Text)infoLabel).text = Localization.Get("onlineview.tournament.state.cancelled");
				return;
			}
			if (tournamentViewModel.ScheduledStartTime.HasValue)
			{
				if (tournamentViewModel.ScheduledStartTime.Value > DateTime.UtcNow)
				{
					((TMP_Text)infoLabel).text = $"{LocalizationUtils.GetDateString(tournamentViewModel.ScheduledStartTime.Value)}\n{LocalizationUtils.GetCountdownString(tournamentViewModel.ScheduledStartTime.Value)}";
				}
				else
				{
					((TMP_Text)infoLabel).text = $"{LocalizationUtils.GetDateString(tournamentViewModel.ScheduledStartTime.Value)}\n";
				}
			}
		}
		else
		{
			((TMP_Text)infoLabel).text = Localization.Get("gameinfo.nodata");
		}
	}

	public override void UpdateColors()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		Color colorForState = GetColorForState(labelColorStates);
		TextMeshProUGUI obj = nameLabel;
		Color color = (((Graphic)infoLabel).color = colorForState);
		((Graphic)obj).color = color;
		registered.SetLabelColor(colorForState);
		confirmed.SetLabelColor(colorForState);
	}

	public void RefreshRow()
	{
		if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeInHierarchy)
		{
			SetData(tournamentViewModel);
		}
	}

	private void LoadLabelIcon(string iconId)
	{
		iconSpriteHandle.Request(SpriteData.GetUISpriteAddress(iconId));
	}

	private void SetLabelIcon(Sprite sprite)
	{
		if ((Object)(object)sprite != (Object)null)
		{
			labelIcon.sprite = sprite;
			((Component)labelIcon).gameObject.SetActive(true);
		}
		else
		{
			((Component)labelIcon).gameObject.SetActive(false);
		}
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
			ShowTournamentInfo();
		}
	}

	private void OnButtonClick(int id, BaseEventData eventData)
	{
		if (!(eventData is PointerEventData))
		{
			ShowTournamentInfo();
		}
	}

	private void OnButtonExit(int id, BaseEventData eventData)
	{
		isButtonHeld = false;
	}

	private void ShowTournamentInfo()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		TournamentInfoPopup tournamentInfoPopup = PopupManager.GetTournamentInfoPopup();
		if (tournamentViewModel != null)
		{
			tournamentInfoPopup.SetData(tournamentViewModel);
		}
		tournamentInfoPopup.Show(InputManager.GetInputPosition());
	}

	public void GetChallengermodeApp()
	{
		NativeHelpers.OpenURL(tournamentViewModel.OverviewUrl);
	}

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)base.button;
	}

	public Selectable GetAccessorySelectable()
	{
		return (Selectable)(object)base.button;
	}
}
