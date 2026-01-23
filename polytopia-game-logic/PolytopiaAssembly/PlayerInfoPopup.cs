using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInfoPopup : BasicPopup
{
	[Header("Player Info Popup")]
	[SerializeField]
	private VerticalLayoutGroup contentLayoutGroup;

	[Space(5f)]
	[SerializeField]
	private PlayerInfoIcon playerInfoIcon;

	[SerializeField]
	private TextMeshProUGUI summaryText;

	[SerializeField]
	private LayoutElement summaryContainer;

	[Space(5f)]
	[SerializeField]
	private TextMeshProUGUI relationText;

	[SerializeField]
	private GameObject sliderGameObject;

	[SerializeField]
	private Slider relationSlider;

	[SerializeField]
	private TextMeshProUGUI opinionText;

	[Space(5f)]
	[SerializeField]
	private LayoutElement tribeRelationContainer;

	[SerializeField]
	private TribeRelationItem tribeRelationItemPrefab;

	[Space(5f)]
	[SerializeField]
	private RectTransform statsContainer;

	[SerializeField]
	private StatsRow statsRowPrefab;

	[Space(5f)]
	[SerializeField]
	private RectTransform lockedInfoContainer;

	[SerializeField]
	private TextMeshProUGUI lockedInfoText;

	[Space(5f)]
	[SerializeField]
	private RectTransform actionButtonSpacer;

	[SerializeField]
	private RectTransform actionButtonContainer;

	[SerializeField]
	private UIRoundButton roundButtonPrefab;

	[Space(5f)]
	[SerializeField]
	private UITextButton opinionButtonPrefab;

	[Space(5f)]
	[SerializeField]
	private float defaultWidth = 400f;

	[Header("Embassy")]
	[SerializeField]
	private RectTransform embassyIncomeContainer;

	[SerializeField]
	private TextMeshProUGUI embassyIncomeText;

	private PlayerState player;

	protected List<StatsRow> statsRows = new List<StatsRow>(4);

	private List<TribeRelationItem> tribeRelations = new List<TribeRelationItem>(10);

	private List<UIRoundButton> actionButtons = new List<UIRoundButton>(3);

	private List<UITextButton> existingOpinionButtons = new List<UITextButton>();

	private Coroutine headerTextSwapCoroutine;

	private Coroutine opinionTextSwapCoroutine;

	private void OnEnable()
	{
		GameEvents.OnStartedProcessing += Refresh;
		GameEvents.OnFinishedProcessing += Refresh;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameEvents.OnStartedProcessing -= Refresh;
		GameEvents.OnFinishedProcessing -= Refresh;
	}

	public override void Show()
	{
		base.Show();
		hasNullFallbackSelectable = false;
		CurrentSelectable = GetCurrentSelectableOrFallback();
		if (!GameManager.LocalPlayer.KnowsPlayer(player.Id) && GameManager.LocalPlayer.Id != player.Id)
		{
			return;
		}
		AudioClip clip = AudioManager.GetTribeMusic(player.tribe.GetName(), player.skinType.GetName());
		AudioSource tribeMusicSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.TribeMusic);
		if (tribeMusicSource.isPlaying && (Object)(object)tribeMusicSource.clip == (Object)(object)clip)
		{
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 1f, 0.6f, (Ease)1);
		}
		else if (tribeMusicSource.isPlaying)
		{
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 0.6f, (Ease)1, delegate
			{
				if (!((Object)(object)this == (Object)null) && ((Component)this).gameObject.activeSelf)
				{
					PlayTribeMusicOnAudioSource(tribeMusicSource, clip);
				}
			});
		}
		else
		{
			PlayTribeMusicOnAudioSource(tribeMusicSource, clip);
		}
		static void PlayTribeMusicOnAudioSource(AudioSource val, AudioClip audioClip)
		{
			val.volume = 0f;
			val.clip = audioClip;
			if (!val.isPlaying)
			{
				val.Play();
			}
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 1f, 0.6f, (Ease)1);
		}
	}

	public override void Hide()
	{
		base.Hide();
		if (((player != null) & (GameManager.LocalPlayer != null)) && (GameManager.LocalPlayer.KnowsPlayer(player.Id) || GameManager.LocalPlayer.Id == player.Id))
		{
			AudioManager.FadeAudioSource(AudioManager.AudioSourceTypes.TribeMusic, 0f, 0.6f, (Ease)1);
		}
	}

	public void SetData(PlayerState player)
	{
		this.player = player;
		Refresh();
	}

	private void Refresh()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		GameState gameState = GameManager.GameState;
		Header = player.GetLocalizedTribeName(gameState);
		PlayerState localPlayer = GameManager.LocalPlayer;
		((Component)playerInfoIcon).gameObject.SetActive(true);
		bool flag = player.IsAlive(gameState);
		bool flag2 = localPlayer.KnowsPlayer(player.Id);
		playerInfoIcon.SetData(player, localPlayer, PlayerInfoIcon.Mood.DataBased, shouldShowDiplomaticState: true, forceKnownPlayer: true);
		((TMP_Text)opinionText).text = "";
		sliderGameObject.SetActive(false);
		((Component)relationText).gameObject.SetActive(false);
		ClearExistingButtons();
		if (!flag)
		{
			SetDeadText(player);
		}
		else if (localPlayer.Id == player.Id && !GameManager.Client.IsSpectating)
		{
			((TMP_Text)summaryText).text = Localization.Get("playerinfopopup.you.description");
		}
		else if (!flag2)
		{
			((TMP_Text)summaryText).text = Localization.Get("playerinfo.triberelations.unknown");
		}
		else
		{
			SetAliveText(player);
		}
		float y = ((TMP_Text)summaryText).GetPreferredValues(((TMP_Text)summaryText).text, ((TMP_Text)summaryText).rectTransform.sizeDelta.x, float.MaxValue).y;
		_003F val = summaryContainer;
		Transform transform = ((Component)playerInfoIcon).transform;
		((LayoutElement)val).minHeight = Mathf.Max(((RectTransform)((transform is RectTransform) ? transform : null)).sizeDelta.y, y) + 10f;
		bool flag3 = (localPlayer.HasTech(TechData.Type.Diplomacy) || localPlayer.Id == player.Id) && flag && flag2;
		((Component)tribeRelationContainer).gameObject.SetActive(flag3);
		bool flag4 = false;
		int totalIncomeFromEmbassiesAndDividend = localPlayer.GetTotalIncomeFromEmbassiesAndDividend(player, gameState);
		((TMP_Text)embassyIncomeText).text = totalIncomeFromEmbassiesAndDividend.ToString();
		((Component)embassyIncomeContainer).gameObject.SetActive(totalIncomeFromEmbassiesAndDividend > 0);
		((Component)statsContainer).gameObject.SetActive(flag4);
		((Component)lockedInfoContainer).gameObject.SetActive(!flag3 && flag && flag2 && gameState.Version >= 60);
		((TMP_Text)lockedInfoText).text = Localization.Get("playerinfo.locked.researchdiplomacy");
		if (flag3)
		{
			UpdateTribeRelations(player);
		}
		if (flag4)
		{
			UpdateStats(player);
		}
		UpdateDiplomacyActionButtons(player);
	}

	private void UpdateDiplomacyActionButtons(PlayerState player)
	{
		foreach (UIRoundButton actionButton in actionButtons)
		{
			Object.Destroy((Object)(object)((Component)actionButton).gameObject);
		}
		actionButtons.Clear();
		PlayerState localPlayer = GameManager.LocalPlayer;
		GameState gameState = GameManager.GameState;
		List<CommandBase> diplomacyCommandsForOpponents = CommandUtils.GetDiplomacyCommandsForOpponents(gameState, localPlayer, new List<byte>(1) { player.Id }, includeUnavailable: true);
		bool flag = diplomacyCommandsForOpponents.Count > 0 && localPlayer.Id != player.Id && player.IsAlive(gameState) && gameState.Version >= 60;
		((Component)actionButtonContainer).gameObject.SetActive(flag);
		((Component)actionButtonSpacer).gameObject.SetActive(flag);
		if (!flag)
		{
			return;
		}
		foreach (CommandBase item in diplomacyCommandsForOpponents)
		{
			UIRoundButton uIRoundButton = CreateDiplomacyActionButton(Localization.Get("action.info." + item.Id), actionButtonContainer);
			uIRoundButton.sprite = UIManager.IconData.GetSprite(item.Id);
			HandleButtonSpecialCases(uIRoundButton, item, localPlayer, player);
			actionButtons.Add(uIRoundButton);
		}
		UIRoundButton CreateDiplomacyActionButton(string text, RectTransform container)
		{
			UIRoundButton uIRoundButton2 = Object.Instantiate<UIRoundButton>(roundButtonPrefab, (Transform)(object)container);
			uIRoundButton2.text = LocalizationUtils.CapitalizeString(text);
			uIRoundButton2.buttonActive = true;
			uIRoundButton2.Cost = -1f;
			return uIRoundButton2;
		}
	}

	public void HandleButtonSpecialCases(UIRoundButton button, CommandBase command, PlayerState player, PlayerState otherPlayer)
	{
		GameState gameState = GameManager.GameState;
		string descriptionKey = "actionbox.cantperformaction.description";
		bool flag = ClientActionManager.CanExecuteCommand(command, gameState);
		if (flag)
		{
			button.OnClicked += delegate
			{
				DiplomacyButton_OnClicked(command);
			};
		}
		if (command.GetCommandType() == CommandType.EstablishEmbassy)
		{
			int embassyCost = gameState.GameLogicData.DiplomacyData.embassyCost;
			button.Cost = embassyCost;
			if (!player.HasTech(TechData.Type.Diplomacy) && gameState.GameLogicData.TryGetData(TechData.Type.Diplomacy, out var techData))
			{
				button.OnClicked += delegate
				{
					OnUnavailableDiplomacyCommandClicked(command, techData);
				};
				button.buttonActive = false;
			}
			else if (!otherPlayer.OwnsTheirCapital(gameState))
			{
				descriptionKey = "actionbox.establishembassynocapital.description";
				button.OnClicked += delegate
				{
					ShowOneButtonPopup(command.GetShortInfo(), Localization.Get(descriptionKey));
				};
				button.buttonActive = false;
			}
			else if (player.GetEmbassyLevel(otherPlayer) != 0)
			{
				descriptionKey = "actionbox.embassyestablished.description";
				button.OnClicked += delegate
				{
					ShowOneButtonPopup(command.GetShortInfo(), Localization.Get(descriptionKey));
				};
				button.buttonActive = false;
				button.SetButtonBought();
			}
			else if (!flag)
			{
				bool num = player.CanAfford(embassyCost);
				button.buttonActive = false;
				if (!num)
				{
					descriptionKey = "actionbox.insufficientfunds";
					button.buttonExpensive = true;
				}
				button.OnClicked += delegate
				{
					ShowOneButtonPopup(command.GetShortInfo(), Localization.Get(descriptionKey));
				};
			}
			else
			{
				button.buttonActive = true;
			}
		}
		else if (command is PeaceTreatyCommand peaceTreatyCommand)
		{
			gameState.TryGetPlayer(peaceTreatyCommand.OpponentId, out var opponent);
			bool flag2 = opponent.HasMessage(DiplomacyMessageType.PeaceRequest, peaceTreatyCommand.PlayerId);
			button.buttonActive = !flag2;
			if (!player.HasTech(TechData.Type.Shields) && gameState.GameLogicData.TryGetData(TechData.Type.Shields, out var techData2))
			{
				button.buttonActive = false;
				button.buttonExpensive = false;
				button.OnClicked += delegate
				{
					OnUnavailableDiplomacyCommandClicked(command, techData2);
				};
			}
			else if (flag2)
			{
				button.OnClicked += delegate
				{
					ShowOneButtonPopup(Localization.Get("actionbox.pendingpeacetreaty.title"), Localization.Get("actionbox.pendingpeacetreaty.description", opponent.GetLocalizedTribeName(gameState)));
				};
			}
			else if (!flag)
			{
				button.buttonActive = false;
				button.buttonExpensive = false;
				button.OnClicked += delegate
				{
					ShowOneButtonPopup(command.GetShortInfo(), Localization.Get(descriptionKey));
				};
			}
		}
		else if (command is BreakPeaceCommand && !flag)
		{
			button.buttonActive = false;
			button.buttonExpensive = false;
			button.OnClicked += delegate
			{
				ShowOneButtonPopup(command.GetShortInfo(), Localization.Get(descriptionKey));
			};
		}
	}

	private void DiplomacyButton_OnClicked(CommandBase command)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (SettingsUtils.InfoOnBuild || command.ShouldAlwaysAskForConfirmation())
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("actionbox.confirm", command.GetShortInfo());
			basicPopup.Description = command.GetConfirmInfo();
			PopupButtonData.States state = PopupButtonData.States.Selected;
			basicPopup.buttonData = new PopupButtonData[2]
			{
				new PopupButtonData("buttons.back"),
				new PopupButtonData("actionbox.building.doit", state, delegate
				{
					ExecuteCommand(command);
				}, 0)
			};
			basicPopup.Show(InputManager.GetInputPosition());
		}
		else
		{
			ExecuteCommand(command);
		}
		static void ExecuteCommand(CommandBase command2)
		{
			if (ClientActionManager.CanExecuteCommand(command2, GameManager.GameState))
			{
				InputEvents.SelectionCleared();
				GameManager.Client.SendCommand(command2);
			}
		}
	}

	private void ShowOneButtonPopup(string header, string description)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = header;
		basicPopup.Description = description;
		basicPopup.buttonData = new PopupButtonData[1]
		{
			new PopupButtonData("buttons.ok", PopupButtonData.States.Selected)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	private void OnUnavailableDiplomacyCommandClicked(CommandBase command, TechData techData)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.Header = Localization.Get("action.info." + command.Id);
		iconPopup.Description = string.Format(Localization.Get("technology.requirements"), Localization.Get(techData.displayName));
		iconPopup.sprite = UIManager.IconData.GetSprite(command.Id);
		iconPopup.buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back"),
			new PopupButtonData("actionbox.building.techtree", PopupButtonData.States.Selected, OnUnlockablePopupAccepted, 0)
		};
		iconPopup.Show(InputManager.GetInputPosition());
		void OnUnlockablePopupAccepted(int id, BaseEventData eventData)
		{
			InputEvents.SelectionCleared();
			UIManager.Instance.ShowScreen(UIConstants.Screens.TechTree);
			Hide();
		}
	}

	private void UpdateStats(PlayerState player)
	{
		if (statsRows.Count == 0)
		{
			for (int i = 0; i < 4; i++)
			{
				StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)statsContainer);
				statsRow.Description = "";
				statsRows.Add(statsRow);
			}
		}
		StatsRow statsRow2 = statsRows[0];
		statsRow2.StatsNameKey = "wcontroller.examine.stars.title";
		statsRow2.StatsValue = player.Currency.ToString();
		StatsRow statsRow3 = statsRows[1];
		statsRow3.StatsNameKey = "playerinfo.stats.income";
		statsRow3.StatsValue = ResourceDataUtils.CalculateIncomeFor(GameManager.GameState, player.Id).ToString();
		StatsRow statsRow4 = statsRows[2];
		statsRow4.StatsNameKey = "topbar.score";
		statsRow4.StatsValue = player.score.ToString();
		StatsRow statsRow5 = statsRows[3];
		statsRow5.StatsNameKey = "endscreen.cities";
		statsRow5.StatsValue = player.cities.ToString();
	}

	private void UpdateTribeRelations(PlayerState player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		PlayerState localPlayer = GameManager.LocalPlayer;
		GameState gameState = GameManager.GameState;
		int num = 0;
		int num2 = 0;
		Transform transform = ((Component)tribeRelationItemPrefab).transform;
		Vector2 sizeDelta = ((RectTransform)((transform is RectTransform) ? transform : null)).sizeDelta;
		Transform transform2 = ((Component)tribeRelationContainer).transform;
		Vector2 sizeDelta2 = ((RectTransform)((transform2 is RectTransform) ? transform2 : null)).sizeDelta;
		Mathf.FloorToInt((Mathf.Max(defaultWidth, sizeDelta2.x) - sizeDelta.x) / (sizeDelta.x + (float)num2));
		for (int i = 0; i < player.knownPlayers.Count; i++)
		{
			gameState.TryGetPlayer(player.knownPlayers[i], out var playerState);
			if (playerState.Id == player.Id || playerState.Id == byte.MaxValue)
			{
				continue;
			}
			bool hasPeace = player.HasPeaceWith(playerState.Id);
			bool hasWar = player.HasWarWith(playerState, gameState);
			if (!playerState.IsAlive(gameState))
			{
				continue;
			}
			if (num == tribeRelations.Count)
			{
				TribeRelationItem item = Object.Instantiate<TribeRelationItem>(tribeRelationItemPrefab, ((Component)tribeRelationContainer).transform);
				tribeRelations.Add(item);
			}
			TribeRelationItem tribeRelationItem = tribeRelations[num];
			tribeRelationItem.Button.ResetEvents();
			((Component)tribeRelationItem).gameObject.SetActive(true);
			tribeRelationItem.PlayerInfoIcon.SetData(playerState, player, PlayerInfoIcon.Mood.None);
			bool hasPeaceWithLocalPlayer = localPlayer.HasPeaceWith(playerState.Id);
			bool hasWarWithLocalPlayer = localPlayer.HasWarWith(playerState, gameState);
			localPlayer.HasBrokenPeaceWith(playerState.Id);
			string otherPlayerText = GetPlayerName(playerState);
			string playerText = GetPlayerName(player);
			tribeRelationItem.Button.OnClicked += delegate
			{
				//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
				string text = Localization.Get("playerinfo.triberelations.popup.title");
				string text2 = (hasPeace ? "peace" : (hasWar ? "war" : "neutral"));
				string arg3 = (hasPeaceWithLocalPlayer ? "peace" : (hasWarWithLocalPlayer ? "war" : "neutral"));
				if (player == localPlayer)
				{
					arg3 = "neutral";
				}
				if (text2 == "neutral")
				{
					arg3 = "";
				}
				string text3 = Localization.Get($"playerinfo.triberelations.popup.{text2}{arg3}.description", playerText, otherPlayerText);
				PopupManager.GetBasicPopup(new PopupManager.BasicPopupData(text, text3, new PopupButtonData[1]
				{
					new PopupButtonData("buttons.ok", PopupButtonData.States.Selected)
				})).Show(InputManager.GetInputPosition());
			};
			string arg = (hasPeace ? Localization.Get("diplomacy.state.peace") : (hasWar ? Localization.Get("diplomacy.state.war") : Localization.Get("diplomacy.state.neutral")));
			Color col = ((localPlayer.Id != player.Id && localPlayer.Id != playerState.Id) ? ((!(hasPeaceWithLocalPlayer && hasPeace) && !(hasWarWithLocalPlayer && hasWar)) ? ((!(hasPeaceWithLocalPlayer && hasWar) && !(hasWarWithLocalPlayer && hasPeace)) ? Color.white : ColorConstants.red) : ColorConstants.green) : (hasPeace ? ColorConstants.green : ((!hasWar) ? Color.white : ColorConstants.red)));
			string arg2 = ColorUtil.ColorToHex(col);
			if (hasPeace || hasWar)
			{
				((TMP_Text)tribeRelationItem.Label).text = string.Format("{0}\n(<color=#{2}>{1}</color>)", otherPlayerText, arg, arg2);
			}
			else
			{
				((TMP_Text)tribeRelationItem.Label).text = $"{otherPlayerText}";
			}
			num++;
		}
		for (int num3 = num; num3 < tribeRelations.Count; num3++)
		{
			((Component)tribeRelations[num3]).gameObject.SetActive(false);
		}
		bool active = num > 0;
		((Component)tribeRelationContainer).gameObject.SetActive(active);
		string GetPlayerName(PlayerState playerState2)
		{
			string text = "";
			if (localPlayer.Id == playerState2.Id)
			{
				return Localization.Get("playerpickerview.you", playerState2.GetLocalizedTribeName(gameState));
			}
			if (localPlayer.KnowsPlayer(playerState2.Id))
			{
				return playerState2.GetLocalizedTribeName(gameState);
			}
			return Localization.Get("gamestatus.unknown.tribe");
		}
	}

	private void SetDeadText(PlayerState player)
	{
		GameState gameState = GameManager.GameState;
		gameState.TryGetPlayer(player.killerId, out var playerState);
		((TMP_Text)summaryText).text = Localization.Get("playerinfo.summary.dead", player.GetLocalizedTribeName(gameState), playerState.GetLocalizedTribeName(gameState), player.killedTurn);
	}

	private void SetAliveText(PlayerState player)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		GameState gameState = GameManager.GameState;
		PlayerState localPlayer = GameManager.LocalPlayer;
		string text = null;
		Color relationColor;
		string text2;
		if (localPlayer.HasPeaceWith(player.Id))
		{
			text = ((player.score > localPlayer.score) ? "epithet.divine" : "epithet.faithful");
			relationColor = ColorConstants.green;
			text2 = "diplomacy.state.peace";
		}
		else if (localPlayer.HasBrokenPeaceWith(player.Id))
		{
			text = ((player.score > localPlayer.score) ? "epithet.terrible" : "epithet.pathetic");
			relationColor = ColorConstants.gray;
			text2 = "diplomacy.state.hostile";
		}
		else if (localPlayer.HasWarWith(player, gameState))
		{
			text = ((player.score > localPlayer.score) ? "epithet.terrible" : "epithet.pathetic");
			relationColor = ColorConstants.red;
			text2 = "diplomacy.state.war";
		}
		else
		{
			relationColor = ColorConstants.yellow;
			text2 = "diplomacy.state.neutral";
		}
		string text3 = Header + " " + Localization.Get(text2).GetLinkedText();
		string localizedTribeName = player.GetLocalizedTribeName(gameState);
		string text4 = (player.AutoPlay ? string.Format("{0} ({1})", player.UserName, Localization.Get("gamestatus.ruled.bot")) : player.UserName);
		string arg = ((text == null) ? text4 : Localization.Get(text, text4));
		string text5 = Localization.Get("playerinfo.summary.ruledby", localizedTribeName, arg);
		player.opinions.UpdateOpinions(gameState, player);
		float opinion = player.opinions.GetOpinion(localPlayer.Id);
		bool flag = player.KnowsPlayer(localPlayer.Id);
		bool flag2 = player.AutoPlay && gameState.Version >= 60;
		string opinionTextFromValue = GetOpinionTextFromValue(opinion, flag);
		Dictionary<string, Color> opinionColors;
		string localizedTopReasons = GetLocalizedTopReasons(player.opinions.GetReasons(localPlayer.Id), out opinionColors);
		string text6 = (string.IsNullOrEmpty(localizedTopReasons) ? "" : Localization.Get("playerinfo.summary.reasons", localizedTopReasons));
		if (headerTextSwapCoroutine != null)
		{
			((MonoBehaviour)UIManager.Instance).StopCoroutine(headerTextSwapCoroutine);
			((TMP_Text)header).text = "";
		}
		if (opinionTextSwapCoroutine != null)
		{
			((MonoBehaviour)UIManager.Instance).StopCoroutine(opinionTextSwapCoroutine);
			((TMP_Text)opinionText).text = "";
		}
		((TMP_Text)summaryText).text = text5;
		((TMP_Text)opinionText).text = (flag ? text6 : Localization.Get("opinion.noopinion"));
		((TMP_Text)relationText).text = opinionTextFromValue;
		((TMP_Text)header).text = text3;
		relationSlider.value = opinion;
		sliderGameObject.SetActive(flag && flag2);
		((Component)relationText).gameObject.SetActive(flag2);
		((Component)opinionText).gameObject.SetActive(flag2);
		SwapButtons(relationColor, text2, opinionColors);
	}

	private string GetLocalizedTopReasons(List<KeyValuePair<OpinionManager.Type, float>> reasons, out Dictionary<string, Color> opinionColors)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		int num = Mathf.Min(3, reasons.Count);
		opinionColors = new Dictionary<string, Color>();
		for (int i = 0; i < num; i++)
		{
			string name = reasons[i].Key.GetName();
			string text2 = Localization.Get("opinion.reason." + name);
			text2 = text2.GetLinkedText("Opinion_" + name);
			opinionColors.Add(name, (reasons[i].Value > 0f) ? ColorConstants.green : ColorConstants.red);
			text += text2;
			if (i < num - 2)
			{
				text += ", ";
			}
			else if (i == num - 2)
			{
				text += string.Format(" {0} ", Localization.Get("stringtools.typelist.and"));
			}
		}
		return text;
	}

	private string GetOpinionTextFromValue(float opinionValue, bool knowPlayer)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		string key = "diplomacy.relation.indifferent";
		if (!knowPlayer)
		{
			return "";
		}
		if (opinionValue < -0.5f)
		{
			key = "diplomacy.relation.poor";
		}
		if (opinionValue < -2f)
		{
			key = "diplomacy.relation.horrible";
		}
		if (opinionValue > 0.5f)
		{
			key = "diplomacy.relation.decent";
		}
		if (opinionValue > 2f)
		{
			key = "diplomacy.relation.great";
		}
		Color col = Color.Lerp(ColorConstants.yellow, (opinionValue > 0f) ? ColorConstants.green : ColorConstants.red, Mathf.Abs(opinionValue));
		return Localization.Get("diplomacy.relation.title") + ": <color=#" + ColorUtil.ColorToHex(col) + ">" + Localization.Get(key);
	}

	private void SwapButtons(Color relationColor, string relationStateKey, Dictionary<string, Color> opinionColorsDictionary)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		ClearExistingButtons();
		headerTextSwapCoroutine = ((MonoBehaviour)UIManager.Instance).StartCoroutine(SwapDelayed(header, isRelation: true));
		opinionTextSwapCoroutine = ((MonoBehaviour)UIManager.Instance).StartCoroutine(SwapDelayed(opinionText, isRelation: false));
		static void SpawnedButton_OnClicked(PopupManager.BasicPopupData basicPopupData)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			PopupManager.GetBasicPopup(basicPopupData).Show(InputManager.GetInputPosition());
		}
		IEnumerator SwapDelayed(TextMeshProUGUI textMesh, bool isRelation)
		{
			if (((TMP_Text)textMesh).textInfo != null)
			{
				((TMP_Text)textMesh).textInfo.Clear();
			}
			while (((TMP_Text)textMesh).textInfo == null || (((TMP_Text)textMesh).textInfo.characterCount == 0 && ((TMP_Text)textMesh).text.Length > 0))
			{
				yield return null;
			}
			TMP_LinkInfo[] linkInfo = ((TMP_Text)textMesh).textInfo.linkInfo;
			for (int i = 0; i < ((TMP_Text)textMesh).textInfo.linkCount; i++)
			{
				TMP_LinkInfo linkInfo2 = linkInfo[i];
				Bounds linkBounds = textMesh.GetLinkBounds(linkInfo2);
				string linkText = ((TMP_LinkInfo)(ref linkInfo2)).GetLinkText();
				string text = ((TMP_LinkInfo)(ref linkInfo2)).GetLinkID().Replace("Opinion_", "");
				string text2 = (string.IsNullOrEmpty(text) ? "" : Localization.Get("opinion.reason." + text + ".description"));
				UITextButton uITextButton = Object.Instantiate<UITextButton>(opinionButtonPrefab, ((Bounds)(ref linkBounds)).center, Quaternion.identity, ((TMP_Text)textMesh).transform);
				UIButtonBase.ColorStates bgColorStates = uITextButton.BgColorStates;
				if (isRelation || !opinionColorsDictionary.TryGetValue(text, out bgColorStates.defaultColor))
				{
					bgColorStates.defaultColor = relationColor;
				}
				uITextButton.BgColorStates = bgColorStates;
				PopupManager.BasicPopupData popupData = new PopupManager.BasicPopupData
				{
					buttonData = new PopupButtonData[1]
					{
						new PopupButtonData("buttons.ok", PopupButtonData.States.Selected)
					},
					description = (isRelation ? Localization.Get(relationStateKey + ".description") : text2),
					header = (isRelation ? Localization.Get(relationStateKey) : linkText)
				};
				uITextButton.OnClicked += delegate
				{
					SpawnedButton_OnClicked(popupData);
				};
				existingOpinionButtons.Add(uITextButton);
				float scaleFactor = UIManager.CanvasScaler.scaleFactor;
				float num = ((Transform)((TMP_Text)textMesh).rectTransform).lossyScale.x / scaleFactor;
				Vector3 center = ((Bounds)(ref linkBounds)).center;
				center.x -= 7f * num * scaleFactor;
				if (isRelation)
				{
					center.y += ((TMP_Text)textMesh).fontSize * 0.1f * num * scaleFactor;
				}
				Vector3 size = ((Bounds)(ref linkBounds)).size;
				size /= num;
				size /= scaleFactor;
				size.x += 14f;
				size.y = uITextButton.rectTransform.sizeDelta.y;
				((Transform)uITextButton.rectTransform).position = center;
				uITextButton.rectTransform.sizeDelta = Vector2.op_Implicit(size);
				uITextButton.text = linkText;
			}
		}
	}

	private void ClearExistingButtons()
	{
		for (int i = 0; i < existingOpinionButtons.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)existingOpinionButtons[i]).gameObject);
		}
		existingOpinionButtons.Clear();
	}
}
