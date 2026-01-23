using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ResultScreen : UIBasicComponent
{
	protected static ResultScreen instance;

	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected ResultHeaderContainer headerContainer;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected ButtonRow buttonRow;

	[SerializeField]
	protected ButtonRow connectionInfoButtonRow;

	[SerializeField]
	protected ButtonRow externalConnectionButtonRow;

	[Header("Audio")]
	[SerializeField]
	protected AudioClip winMusic;

	[SerializeField]
	protected AudioClip looseMusic;

	[Header("Prefabs")]
	[SerializeField]
	protected StatsRow statsRowPrefab;

	[SerializeField]
	protected StarContainer starContainerPrefab;

	[SerializeField]
	protected LayoutElement spacerPrefab;

	protected List<GameObject> rows = new List<GameObject>();

	protected bool topScore;

	protected bool topTribe;

	protected bool highscoreEnabled;

	protected Selectable defaultSelectable;

	private string externalLink;

	public static Selectable DefaultSelectable
	{
		get
		{
			return instance.defaultSelectable;
		}
		set
		{
			instance.defaultSelectable = value;
		}
	}

	public override void Init()
	{
		base.Init();
		instance = this;
		((Component)this).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		InputEvents.OnButtonUp += OnButtonUp;
		NotificationManager.SetSilenceAlerts(silenced: true);
	}

	private void OnButtonUp(InputManager.Buttons button)
	{
		if (!PopupManager.PopupShowing && button == InputManager.Buttons.Accept)
		{
			OnShowHighscore(0, null);
		}
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		InputEvents.OnButtonDown -= OnButtonUp;
		NotificationManager.SetSilenceAlerts(silenced: false);
		((MonoBehaviour)this).StopAllCoroutines();
	}

	private void OnApplicationPause(bool isPaused)
	{
		if (!isPaused)
		{
			RetryUploadHighscore();
		}
	}

	public static void Hide()
	{
		if ((Object)(object)instance != (Object)null)
		{
			((Component)instance).gameObject.SetActive(false);
		}
	}

	protected string GetResultDescriptionForPlayer(GameState gameState, PlayerState player, bool isMultiplayerRatingPrimary)
	{
		string text = player.GetLocalizedTribeName(gameState);
		if (player.resignedTurn > -1)
		{
			text += string.Format(", {0}", Localization.Get("gamestats.resignedonturn", player.resignedTurn));
		}
		else if (player.IsAlive(gameState) || player.killedTurn == 0)
		{
			if (gameState.Settings.RulesGameMode == GameMode.Glory && isMultiplayerRatingPrimary)
			{
				text += string.Format(", {0}", Localization.Get("gamestatus.score", (int)player.score));
			}
			else if (gameState.Settings.RulesGameMode == GameMode.Might)
			{
				text += string.Format(", {0}: {1}", Localization.Get("gamestatus.capitals").ToLower(), player.CountCapitals(gameState));
			}
		}
		else
		{
			text += string.Format(", {0}", Localization.Get("gamestatus.killedonturn", player.killedTurn));
		}
		return text;
	}

	protected async void ShowInternal(bool localPlayerIsWinner, ScoreDetails scoreDetails, byte winnerId = 0)
	{
		NotificationManager.HideAlert();
		if (UIManager.Instance.CurrentScreen != UIConstants.Screens.None && UIManager.Instance.CurrentScreen != UIConstants.Screens.Hud)
		{
			UIManager.Instance.GetScreen(UIManager.Instance.CurrentScreen).Hide();
		}
		((Component)this).gameObject.SetActive(true);
		((Graphic)bg).color = Color.clear;
		bg.DOFade(0.8f, 0.2f);
		highscoreEnabled = IsHighscoreEnabled();
		bool flag = GameManager.GameState.Settings.GameType == GameType.Multiplayer || GameManager.GameState.Settings.GameType == GameType.Competitive;
		if (!flag)
		{
			SetHeader(localPlayerIsWinner, winnerId);
			StartMusic(localPlayerIsWinner);
		}
		InputManager.DisableInput(InputManager.GameInputs);
		((Component)buttonRow).gameObject.SetActive(false);
		if (GameManager.GameState.Settings.GameType == GameType.PassAndPlay)
		{
			AddScoreRankingRows();
		}
		else if (flag)
		{
			await AddMultiplayerResults();
		}
		else if (GameManager.GameState.Settings.GameType == GameType.SinglePlayer)
		{
			AddSingleplayerResults(localPlayerIsWinner, scoreDetails);
		}
		AddSpacer();
		AddButtonRow(buttonRow, highscoreEnabled ? "endscreen.showhiscore" : "endscreen.done", OnShowHighscore);
		if (!PolytopiaBackendAdapter.Instance.IsAuthenticated && !GameManager.CanBeAuthenticated())
		{
			AddButtonRow(connectionInfoButtonRow, "endscreen.connectioninfo", OnShowConnectionInfo);
		}
		((MonoBehaviour)this).StartCoroutine(DelayShowRow(0.2f));
		OnScreenUpdated();
	}

	private void SetHeader(bool localPlayerIsWinner, byte winnerId)
	{
		headerContainer.Won = localPlayerIsWinner;
		headerContainer.WinnerId = winnerId;
		headerContainer.Show();
	}

	private void StartMusic(bool localPlayerIsWinner)
	{
		AudioSource audioSource = AudioManager.GetAudioSource(AudioManager.AudioSourceTypes.ThemeMusic);
		audioSource.clip = (localPlayerIsWinner ? winMusic : looseMusic);
		audioSource.loop = false;
		audioSource.Play();
	}

	private void AddScoreRankingRows()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		foreach (PlayerState item in GameManager.GameState.GetPlayersSortedByRank())
		{
			if (item.Id != byte.MaxValue)
			{
				string resultDescriptionForPlayer = GetResultDescriptionForPlayer(GameManager.GameState, item, isMultiplayerRatingPrimary: false);
				AddStatsRow(item.UserName, (int)item.score, resultDescriptionForPlayer);
			}
		}
	}

	private async Task AddMultiplayerResults()
	{
		GameSummaryViewModel gameSummaryViewModel = null;
		if (GameManager.Client.IsReplay)
		{
			gameSummaryViewModel = GameManager.GetReplaysManager().GetCurrentReplaySummary();
		}
		else
		{
			GameManager.GetRemoteGameDataManager().TryGetGameSummaryViewModel(GameManager.Client.CurrentGameId.Value, out gameSummaryViewModel);
		}
		if (gameSummaryViewModel == null)
		{
			NetworkUtils.ShowLoader();
			ServerResponse<GameSummaryViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetGameSummaryViewModelByIdAsync(GameManager.Client.CurrentGameId.Value);
			if (serverResponse.Success)
			{
				gameSummaryViewModel = serverResponse.Data;
			}
			NetworkUtils.HideLoader();
		}
		List<PlayerState> playersSortedByRankForMultiplayerResults = GameManager.GameState.GetPlayersSortedByRankForMultiplayerResults();
		PlayerState playerState = playersSortedByRankForMultiplayerResults[0];
		if (gameSummaryViewModel == null || gameSummaryViewModel.Result == null)
		{
			foreach (PlayerState item in playersSortedByRankForMultiplayerResults)
			{
				if (item.Id != byte.MaxValue && item.AccountId.HasValue && !(item.AccountId == Guid.Empty))
				{
					string resultDescriptionForPlayer = GetResultDescriptionForPlayer(GameManager.GameState, item, isMultiplayerRatingPrimary: false);
					AddStatsRow(item.UserName, (int)item.score, resultDescriptionForPlayer);
				}
			}
		}
		else
		{
			List<PlayerRankingViewModel> list = new List<PlayerRankingViewModel>(gameSummaryViewModel.Result.PlayerRankings);
			list.Sort((PlayerRankingViewModel a, PlayerRankingViewModel b) => a.Rank.CompareTo(b.Rank));
			PlayerRankingViewModel playerRankingViewModel = list[0];
			GameManager.GameState.TryGetPlayer(playerRankingViewModel.PolytopiaUserId.Value, out playerState);
			foreach (PlayerRankingViewModel item2 in list)
			{
				if (!item2.PolytopiaUserId.HasValue || item2.PolytopiaUserId == Guid.Empty)
				{
					continue;
				}
				GameManager.GameState.TryGetPlayer(item2.PolytopiaUserId.Value, out var player);
				SerializationHelpers.FromByteArray<AvatarState>(GameInfoPopup.GetParticipatorFromUserId(gameSummaryViewModel.Participators, item2.PolytopiaUserId)?.AvatarStateData, out var result);
				Color playerColor = player.GetPlayerColor(GameManager.GameState);
				PlayerRankingViewModel? playerRankingViewModel2 = list.Find(delegate(PlayerRankingViewModel r)
				{
					Guid? polytopiaUserId = r.PolytopiaUserId;
					Guid? accountId = player.AccountId;
					if (polytopiaUserId.HasValue != accountId.HasValue)
					{
						return false;
					}
					return !polytopiaUserId.HasValue || polytopiaUserId.GetValueOrDefault() == accountId.GetValueOrDefault();
				});
				int resultingMultiplayerRating = playerRankingViewModel2.ResultingMultiplayerRating;
				int multiplayerRatingChange = playerRankingViewModel2.MultiplayerRatingChange;
				string resultDescriptionForPlayer2 = GetResultDescriptionForPlayer(GameManager.GameState, player, isMultiplayerRatingPrimary: true);
				AddStatsRow(player.UserName, resultingMultiplayerRating, resultDescriptionForPlayer2, showBg: false, null, result, playerColor, enableEloButton: true, multiplayerRatingChange);
			}
			if (gameSummaryViewModel.GameContext != null && gameSummaryViewModel.GameContext.ExternalMatchId.HasValue)
			{
				externalLink = $"https://www.challengermode.com/games/{gameSummaryViewModel.GameContext.ExternalMatchId.Value}";
				AddButtonRow(externalConnectionButtonRow, "onlineview.tournament.endscreen.linktomatch", OnChallengermodeLinkClicked);
			}
		}
		bool localPlayerIsWinner = playerState.AccountId == AccountManager.PlayerAccountId;
		byte id = playerState.Id;
		SetHeader(localPlayerIsWinner, id);
		StartMusic(localPlayerIsWinner);
	}

	private void AddSingleplayerResults(bool localPlayerIsWinner, ScoreDetails scoreDetails)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.GameState.Settings.RulesGameMode == GameMode.Perfection)
		{
			AddStatsRow(Localization.Get("endscreen.army&territory"), scoreDetails.territoryScore, Localization.Get("endscreen.units", scoreDetails.territoryInfoParams));
			AddStatsRow(Localization.Get("endscreen.science"), scoreDetails.techScore, Localization.Get("endscreen.techscore", scoreDetails.techInfoParams));
			AddStatsRow(Localization.Get("endscreen.cities"), scoreDetails.cityScore, Localization.Get("endscreen.citiescount", scoreDetails.cityInfoParams));
			AddStatsRow(Localization.Get("endscreen.monuments&temples"), scoreDetails.cultureScore, Localization.Get("endscreen.culture", scoreDetails.cultureInfoParams));
			AddStatsRow(Localization.Get("endscreen.bonus"), scoreDetails.difficultyBonus, $"{(1f + scoreDetails.difficultyBonusMultiplier) * 100f}% ");
			AddStatsRow(Localization.Get("endscreen.finalscore"), scoreDetails.TotalScoreIncludingBonus, "", showBg: true);
		}
		else if (GameManager.GameState.Settings.RulesGameMode == GameMode.Domination)
		{
			PlayerRating rating = GameManager.LocalPlayer.GetRating(GameManager.GameState, localPlayerIsWinner);
			int opponentCount = GameManager.GameState.Settings.OpponentCount;
			string valueFormat = "{0}%";
			if (localPlayerIsWinner)
			{
				AddStatsRow(Localization.Get("endscreen.speedskills"), rating.timeScore, Localization.Get("endscreen.domination.win", GameManager.GameState.CurrentTurn, rating.timeGoal), showBg: false, valueFormat);
			}
			else
			{
				AddStatsRow(Localization.Get("endscreen.speedskills"), rating.timeScore, Localization.Get("endscreen.domination.loss", GameManager.GameState.CurrentTurn), showBg: false, valueFormat);
			}
			AddStatsRow(Localization.Get("endscreen.battle"), rating.battleScore, Localization.Get("endscreen.battle.info", GameManager.LocalPlayer.casualities), showBg: false, valueFormat);
			AddStatsRow(Localization.Get("endscreen.destroyed"), rating.wipeScore, Localization.Get("endscreen.destroyed.info", GameManager.LocalPlayer.wipeOuts, opponentCount), showBg: false, valueFormat);
			AddStatsRow(Localization.Get("endscreen.rating"), rating.difficultyScore, Localization.Get(GameModeUtils.GetDifficultyName(GameManager.GameState.Settings.Difficulty)), showBg: false, valueFormat);
			AddStatsRow(Localization.Get("endscreen.finalrating"), rating.averageRating, "", showBg: true, valueFormat);
		}
		AddSpacer();
		AddStarContainer(scoreDetails.TotalScoreIncludingBonus, localPlayerIsWinner);
	}

	protected void AddStatsRow(string header, int value, string info, bool showBg = false, string valueFormat = null, AvatarState avatarState = null, Color avatarBgColor = default(Color), bool enableEloButton = false, int eloChange = 0)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)container);
		LayoutElement obj = ((Component)statsRow).gameObject.AddComponent<LayoutElement>();
		obj.minWidth = 366f;
		obj.minHeight = (string.IsNullOrEmpty(info) ? 30 : 50);
		statsRow.StatsName = header;
		statsRow.StatsValue = "";
		statsRow.Description = info;
		statsRow.RefreshLayout();
		statsRow.ValueFormat = valueFormat;
		statsRow.StatsValueInteger = value;
		if (avatarState != null)
		{
			statsRow.SetAvatarState(avatarState);
			statsRow.ShowIconBackground(show: true, avatarBgColor);
		}
		if (enableEloButton)
		{
			statsRow.EnableEloButton();
			statsRow.AnimateValue(value - eloChange, value, 2f);
			Color color = Color.white;
			string value2 = eloChange.ToString();
			if (eloChange > 0)
			{
				color = Color.green;
				value2 = "+" + eloChange;
			}
			else if (eloChange < 0)
			{
				color = Color.red;
			}
			statsRow.SetSmallStatsValue(value2, color);
		}
		statsRow.BgVisible = showBg;
		((Component)statsRow).gameObject.SetActive(false);
		rows.Add(((Component)statsRow).gameObject);
	}

	protected void AddButtonRow(ButtonRow buttonRow, string localizationKey, UIButtonBase.ButtonAction callback)
	{
		((Transform)buttonRow.rectTransform).SetParent((Transform)(object)container);
		buttonRow.buttonComp.OnClicked -= callback;
		buttonRow.buttonComp.OnClicked += callback;
		buttonRow.buttonComp.Key = localizationKey;
		((Component)buttonRow).gameObject.SetActive(false);
		rows.Add(((Component)buttonRow).gameObject);
	}

	public void OnChallengermodeLinkClicked(int id, BaseEventData eventData)
	{
		NativeHelpers.OpenURL(externalLink);
	}

	private bool IsHighscoreEnabled()
	{
		string message;
		if (GameModeUtils.HighscoreEnabled(GameManager.GameState.Settings.BaseGameMode))
		{
			return GameManager.GetVersioningInfoHolder().IsHighscoreEnabled(out message);
		}
		return false;
	}

	protected void AddStarContainer(int score, bool isLocalPlayerWinner)
	{
		StarContainer starContainer = Object.Instantiate<StarContainer>(starContainerPrefab, (Transform)(object)container);
		starContainer.Init();
		GameMode baseGameMode = GameManager.GameState.Settings.BaseGameMode;
		GameMode gameMode = (starContainer.gameMode = GameManager.GameState.Settings.RulesGameMode);
		topTribe = false;
		topScore = false;
		switch (gameMode)
		{
		case GameMode.Perfection:
		{
			ScoreManager.TryGetTribeScore(GameManager.LocalPlayer.tribe, out var score2);
			starContainer.aboveAllStarsMessageKey = "";
			starContainer.belowAllStarsMessageKey = "endscreen.nextstar";
			starContainer.PreviousScore = score2;
			starContainer.Score = score;
			topScore = score > ScoreManager.GetTopScore();
			if (baseGameMode != GameMode.Custom)
			{
				topTribe = score > score2;
				ScoreManager.SetTribeScore(GameManager.LocalPlayer.tribe, (uint)score);
			}
			break;
		}
		case GameMode.Domination:
		{
			int averageRating = GameManager.LocalPlayer.GetRating(GameManager.GameState, isLocalPlayerWinner).averageRating;
			ScoreManager.TryGetTribeRating(GameManager.LocalPlayer.tribe, out var rating);
			starContainer.aboveAllStarsMessageKey = "";
			starContainer.belowAllStarsMessageKey = "endscreen.nextstar.percent";
			starContainer.PreviouseRating = rating;
			starContainer.Rating = averageRating;
			topScore = averageRating > ScoreManager.GetTopRating();
			if (baseGameMode != GameMode.Custom)
			{
				topTribe = averageRating > rating;
				ScoreManager.SetTribeRating(GameManager.LocalPlayer.tribe, (uint)averageRating);
			}
			break;
		}
		default:
		{
			starContainer.aboveAllStarsMessageKey = "";
			starContainer.belowAllStarsMessageKey = "";
			int previousScore = (starContainer.Score = score);
			starContainer.PreviousScore = previousScore;
			break;
		}
		}
		((Component)starContainer).gameObject.SetActive(false);
		starContainer.IsBig = true;
		rows.Add(((Component)starContainer).gameObject);
	}

	protected void AddSpacer(float minHeight = 10f)
	{
		Object.Instantiate<LayoutElement>(spacerPrefab, (Transform)(object)container).minHeight = minHeight;
	}

	protected IEnumerator DelayShowRow(float delay = 0f)
	{
		int currRow = 0;
		yield return (object)new WaitForSeconds(delay);
		StatsRow statsRow = default(StatsRow);
		StarContainer starContainer = default(StarContainer);
		ButtonRow buttonRow = default(ButtonRow);
		while (currRow < rows?.Count)
		{
			float num = 0.6f;
			GameObject val = rows[currRow];
			if (!((Object)(object)val == (Object)null))
			{
				val.SetActive(true);
				if (val.TryGetComponent<StatsRow>(ref statsRow))
				{
					AudioManager.PlaySFX(SFXTypes.Hit1);
					int statsValueInteger = statsRow.StatsValueInteger;
					statsRow.AnimateValue(0, statsValueInteger, num);
				}
				else if (val.TryGetComponent<StarContainer>(ref starContainer))
				{
					num = 1f;
					starContainer.Animate(num);
					num += 0.2f;
				}
				else if (val.TryGetComponent<ButtonRow>(ref buttonRow))
				{
					AudioManager.PlaySFX(SFXTypes.Kill);
					buttonRow.buttonComp.OnSelect(new BaseEventData(EventSystem.current));
					defaultSelectable = buttonRow.GetMainSelectable();
				}
				currRow++;
				yield return (object)new WaitForSeconds(num);
			}
		}
		if (topTribe)
		{
			NotificationManager.Notify(Localization.Get("endscreen.topresult"), Localization.Get("endscreen.topresult.title"));
		}
		if (topScore)
		{
			NotificationManager.Notify(Localization.Get("endscreen.personal"), Localization.Get("endscreen.personal.title"));
		}
	}

	public void OnShowConnectionInfo(int id, BaseEventData eventData)
	{
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		string header = Localization.Get("onlineview.login.ios", Localization.Get((SystemManager.Platform == SystemManager.Platforms.iOS) ? "gameservice.ios" : "gameservice.android"));
		string description = Localization.Get("onlineview.login.ios.info", Localization.Get((SystemManager.Platform == SystemManager.Platforms.iOS) ? "gameservice.ios" : "gameservice.android"));
		basicPopup.Header = header;
		basicPopup.Description = description;
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("onlineview.fixit", PopupBase.PopupButtonData.States.Selected, async delegate
			{
				await FixLogin();
				RetryUploadHighscore();
			})
		};
		basicPopup.Show();
	}

	private void RetryUploadHighscore()
	{
		if (GameManager.CanBeAuthenticated() && GameManager.Client != null)
		{
			GameManager.Client.UploadHighscore();
		}
	}

	private async Task FixLogin()
	{
		await GameManager.GetLoginManager().LoginAsync(silent: false, forced: true);
	}

	public void OnShowHighscore(int id, BaseEventData eventData)
	{
		if ((Object)(object)headerContainer != (Object)null)
		{
			headerContainer.Clear();
		}
		if (GameManager.Client.IsReplay)
		{
			UIManager.QueuedDeepLink = UIConstants.Screens.ReplaysScreen;
		}
		else if (highscoreEnabled)
		{
			UIManager.QueuedDeepLink = UIConstants.Screens.Highscore;
			UIManager.QueuedDeepLinkData = new HighScoreScreen.HighscoreDeepLinkData(GameManager.LocalPlayer.tribe);
		}
		else if (GameManager.GameState.Settings.GameType == GameType.Multiplayer || GameManager.GameState.Settings.GameType == GameType.PassAndPlay)
		{
			UIManager.QueuedDeepLink = UIConstants.Screens.MultiplayerScreen;
		}
		GameManager.ReturnToMenu();
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		OnScreenUpdated();
	}

	public void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	public static void Show(bool localPlayerIsWinner, ScoreDetails scoreDetails, byte winnerId = 0)
	{
		instance.ShowInternal(localPlayerIsWinner, scoreDetails, winnerId);
	}

	public static bool IsShowing()
	{
		if ((Object)(object)instance != (Object)null)
		{
			return ((Component)instance).gameObject.activeInHierarchy;
		}
		return false;
	}
}
