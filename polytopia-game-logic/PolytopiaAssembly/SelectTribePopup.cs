using System;
using System.Collections.Generic;
using DG.Tweening;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectTribePopup : BasicPopup
{
	public enum DisplayMode
	{
		None,
		Select,
		Purchase
	}

	public Action<SkinType> SkinButtonCallback;

	public Action<int> enabledChangedCallback;

	public DisplayMode Mode;

	public SkinType SkinType;

	[Header("Select Tribe Popup")]
	[SerializeField]
	protected UIWorldPreview UIWorldPreview;

	[SerializeField]
	protected UIToggleButton disableButton;

	[SerializeField]
	protected StarContainer starContainer;

	[Space(10f)]
	[SerializeField]
	protected UITextButton uiTextButton;

	[Space(10f)]
	[SerializeField]
	protected RectTransform skinButtonsParent;

	[SerializeField]
	protected UISkinButton skinButtonPrefab;

	protected TribeData tribeData;

	protected TribeData mixTribeData;

	protected string startTechSid = string.Empty;

	protected string tribeName;

	protected bool tribeEnabled = true;

	protected List<UISkinButton> skinButtons = new List<UISkinButton>();

	protected bool isSinglePlayer = true;

	protected Guid? gameOwnerId;

	public override PopupButtonData[] buttonData
	{
		set
		{
			buttonContainer.ResetContainer();
			base.buttonData = value;
			if (Buttons.Length > 1 && UIManager.Instance.CurrentScreen != UIConstants.Screens.GameSetup && !tribeEnabled)
			{
				Buttons[1].ButtonEnabled = tribeEnabled;
			}
		}
	}

	public Guid? GameOwnerId
	{
		get
		{
			return gameOwnerId;
		}
		set
		{
			gameOwnerId = value;
		}
	}

	public override void Init()
	{
		base.Init();
		starContainer.Init();
	}

	protected void OnEnable()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		OnScreenSizeChanged(Vector2.op_Implicit(Vector3.zero));
	}

	protected override void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		base.OnDisable();
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (fullscreenVariant && ((Component)this).gameObject.activeInHierarchy)
		{
			base.rectTransform.SetWidth(UIManager.GetUIWidth());
			base.rectTransform.SetHeight(UIManager.GetUIHeight());
			_ = base.rectTransform.sizeDelta.x;
			_ = base.rectTransform.sizeDelta.y;
		}
		else
		{
			RefreshHeight();
		}
	}

	public override void Show(Vector2 origin)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		PurchaseManager purchaseManager = GameManager.GetPurchaseManager();
		if (!purchaseManager.IsTribeUnlocked(tribeData.type))
		{
			decimal? price = purchaseManager.GetPrice(tribeData.type);
			if (price.HasValue)
			{
				string currency = purchaseManager.GetCurrency(tribeData.type);
				GameManager.GetAnalyticsManager().SendEvent("view_item", new Dictionary<string, object>
				{
					{ "value", price },
					{ "currency", currency },
					{
						"items",
						$"[{{ \"item_name\": \"{tribeData.type}\", \"item_category\": \"tribe\" }}]"
					},
					{ "tribe", tribeData.type }
				});
			}
		}
		base.Show(origin);
	}

	public override void Show()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Show(Vector2.op_Implicit(Vector3.zero));
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		gameOwnerId = null;
		disableButton.Highlighted = false;
	}

	public void SetData(TribeData tribeData, TribeData mixTribeData = null)
	{
		this.tribeData = tribeData;
		this.mixTribeData = mixTribeData;
		GameSettings gameSettings = GameManager.PreliminaryGameSettings;
		if (GameManager.GameState != null && GameManager.GameState.Settings != null)
		{
			gameSettings = GameManager.GameState.Settings;
		}
		tribeEnabled = gameSettings.IsTribeEnabled(tribeData.type);
		disableButton.Value = tribeEnabled;
		bool active = GameTypeUtils.ShouldAllowChangingDisabledTribes(gameSettings.GameType, gameOwnerId);
		((Component)disableButton).gameObject.SetActive(active);
		tribeName = ((mixTribeData != null) ? TribeExtensions.GetMixedTribeName(tribeData, mixTribeData) : Localization.Get(tribeData.displayName));
		Header = tribeName;
		((TMP_Text)header).fontStyle = (FontStyles)((tribeData.type != TribeData.Type.Elyrion) ? 16 : 0);
		foreach (TechData item in tribeData.startingTech)
		{
			if (item.type != TechData.Type.Basic)
			{
				startTechSid = item.displayName;
			}
		}
		starContainer.gameMode = gameSettings.RulesGameMode;
		isSinglePlayer = gameSettings.GameType == GameType.SinglePlayer;
		if (isSinglePlayer && mixTribeData == null)
		{
			if (gameSettings.RulesGameMode == GameMode.Perfection)
			{
				ScoreManager.TryGetTribeScore(tribeData.type, out var score);
				starContainer.aboveAllStarsMessageKey = "tribepicker.topscore";
				starContainer.belowAllStarsMessageKey = "tribepicker.topscore.next";
				starContainer.Score = score;
			}
			else
			{
				ScoreManager.TryGetTribeRating(tribeData.type, out var rating);
				starContainer.aboveAllStarsMessageKey = "tribepicker.toprating";
				starContainer.belowAllStarsMessageKey = "tribepicker.toprating.next";
				starContainer.Rating = rating;
			}
		}
		else
		{
			starContainer.Score = -1;
		}
		SetTribeSkins();
		SetDescription();
		SetBackground();
		SetMusic();
		SetOpinionButton();
	}

	public void StartWaitingForPurchase()
	{
		if (tribeData != null)
		{
			UITextButton buttonById = GetButtonById((int)tribeData.type);
			if (!((Object)(object)buttonById == (Object)null))
			{
				buttonById.Key = "tribepicker.waiting";
				buttonById.ButtonEnabled = false;
			}
		}
	}

	public void StopWaitingForPurchase()
	{
		for (int i = 0; i < skinButtons.Count; i++)
		{
			UISkinButton uISkinButton = skinButtons[i];
			bool flag = GameManager.GetPurchaseManager().IsSkinUnlocked(uISkinButton.SkinType) && GameManager.GetPurchaseManager().IsTribeUnlocked(tribeData.type);
			uISkinButton.SetPadlock(!flag);
		}
	}

	public void OnDisableTribe()
	{
		tribeEnabled = !tribeEnabled;
		GameSettings gameSettings = GameManager.PreliminaryGameSettings;
		if (GameManager.GameState != null && GameManager.GameState.Settings != null)
		{
			gameSettings = GameManager.GameState.Settings;
		}
		if (tribeEnabled)
		{
			gameSettings.EnableTribe(tribeData.type);
		}
		else
		{
			gameSettings.DisableTribe(tribeData.type);
		}
		gameSettings.SaveToDisk();
		if (Buttons.Length > 1)
		{
			Buttons[1].ButtonEnabled = tribeEnabled;
			UIUtils.SetExplicitNavigation(base.rectTransform);
			DefaultSelectable = (Selectable)(object)(tribeEnabled ? Buttons[1].button : disableButton.button);
		}
		enabledChangedCallback?.Invoke((int)tribeData.type);
		if (!tribeEnabled)
		{
			OnHide(-1, null);
		}
	}

	private void SetBackground()
	{
		UIWorldPreview.SetPreview(tribeData, SkinType, mixTribeData);
	}

	private void SetOpinionButton()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		uiTextButton.ButtonEnabled = false;
		if (SkinType == SkinType.Default)
		{
			((Component)uiTextButton).gameObject.SetActive(false);
			return;
		}
		((Component)uiTextButton).gameObject.SetActive(true);
		string text = string.Format(Localization.Get(SkinTypeExtensions.GetSkinNameKey()), Localization.Get(SkinType.GetLocalizationKey()));
		uiTextButton.text = text;
		UIButtonBase.ColorStates bgColorStates = uiTextButton.BgColorStates;
		bgColorStates.disabledColor = ColorUtil.ColorFromInt((uint)tribeData.color);
		uiTextButton.BgColorStates = bgColorStates;
		uiTextButton.UpdateSize();
	}

	private void SetTribeSkins()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (RectTransform item in (Transform)skinButtonsParent)
		{
			Object.Destroy((Object)(object)((Component)item).gameObject);
		}
		skinButtons.Clear();
		int version = GetVersion();
		SkinType selectedSkin = GameManager.PreliminaryGameSettings.GetSelectedSkin(tribeData.type);
		UIRoundButton uIRoundButton = SpawnButton(SkinType.Default);
		uIRoundButton.buttonActive = true;
		SkinType = (selectedSkin.CanUseInCurrentGameVersion(version) ? selectedSkin : SkinType.Default);
		if (SystemManager.IsSwitch)
		{
			return;
		}
		for (int i = 0; i < tribeData.skins.Count; i++)
		{
			SkinType skinType = tribeData.skins[i];
			if (!skinType.IsLocked())
			{
				UIRoundButton uIRoundButton2 = SpawnButton(skinType);
				if (selectedSkin == skinType)
				{
					uIRoundButton2.buttonActive = true;
					uIRoundButton.buttonActive = false;
				}
				if (!skinType.CanUseInCurrentGameVersion(version))
				{
					uIRoundButton2.buttonActive = false;
					((Behaviour)uIRoundButton2).enabled = false;
				}
			}
		}
		static int GetVersion()
		{
			int result = VersionManager.GameVersion;
			GameType gameType = GameManager.PreliminaryGameSettings.GameType;
			if ((gameType == GameType.Multiplayer || gameType == GameType.Competitive) && GameManager.Client != null && GameManager.Client is RemoteClient && GameManager.Client.GameState != null)
			{
				result = GameManager.Client.GameState.Version;
			}
			return result;
		}
		UIRoundButton SpawnButton(SkinType type)
		{
			UISkinButton uISkinButton = Object.Instantiate<UISkinButton>(skinButtonPrefab, (Transform)(object)skinButtonsParent);
			UIRoundButton button = uISkinButton.RoundButton;
			button.ButtonEnabled = true;
			button.text = Localization.Get(type.GetLocalizationKey());
			button.ShowLabel = false;
			((Object)button).name = "SkinRoundButton_" + type;
			button.OnClicked += delegate
			{
				Button_OnClicked(type, button);
			};
			string specialId = ((type != SkinType.Default) ? type.GetName() : ((mixTribeData != null) ? mixTribeData.style.ToString() : tribeData.style.ToString()));
			button.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(specialId));
			button.buttonActive = false;
			bool flag = GameManager.GetPurchaseManager().IsSkinUnlocked(type) && GameManager.GetPurchaseManager().IsTribeUnlocked(tribeData.type);
			uISkinButton.SetPadlock(!flag);
			uISkinButton.SkinType = type;
			skinButtons.Add(uISkinButton);
			return button;
		}
	}

	private void SetDescription()
	{
		if (SkinType == SkinType.Default)
		{
			base.Description = Localization.Get(tribeData.description, tribeName);
		}
		else
		{
			base.Description = Localization.Get(SkinType.GetLocalizationDescriptionKey(), tribeName);
		}
		base.Description = base.Description + "\n\n" + Localization.Get(tribeData.description2, tribeName, Localization.Get(startTechSid));
	}

	private void SetMusic()
	{
		AudioSource audioSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.TribeMusic);
		audioSource.volume = 0f;
		audioSource.clip = AudioManager.GetTribeMusic(tribeData.type.GetName(), SkinType.GetName());
		audioSource.Play();
		AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 1f, 0.6f, (Ease)1);
	}

	private void Button_OnClicked(SkinType type, UIRoundButton button)
	{
		if (SkinType != type)
		{
			SkinType = type;
			for (int i = 0; i < skinButtons.Count; i++)
			{
				UIRoundButton roundButton = skinButtons[i].RoundButton;
				roundButton.buttonActive = (Object)(object)roundButton == (Object)(object)button;
			}
			SetDescription();
			SetBackground();
			SetMusic();
			SetOpinionButton();
			if (GameManager.GetPurchaseManager().IsSkinUnlocked(SkinType))
			{
				GameManager.PreliminaryGameSettings.SetSelectedSkin(tribeData.type, SkinType);
				GameManager.PreliminaryGameSettings.SaveToDisk();
			}
			SkinButtonCallback?.Invoke(SkinType);
		}
	}
}
