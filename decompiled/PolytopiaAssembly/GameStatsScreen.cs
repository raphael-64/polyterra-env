using System;
using System.Collections.Generic;
using System.Linq;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStatsScreen : UIScreenBase
{
	[Header("Game Stats Screen")]
	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected TMPLocalizer scoreHeader;

	[SerializeField]
	protected RectTransform statsList;

	[SerializeField]
	protected TextMeshProUGUI headerText;

	[Header("ScoreList")]
	[SerializeField]
	protected Color normalTextColor;

	[SerializeField]
	protected Color dimmedTextColor;

	[SerializeField]
	protected RectTransform scoresList;

	[SerializeField]
	protected RectTransform deadScoresList;

	[SerializeField]
	protected GameObject[] deadScoresListComponents;

	[SerializeField]
	private MapSizeButtonWrapper mapSizeButton;

	[SerializeField]
	private GameModeButtonWrapper gameModeButton;

	[SerializeField]
	private MoreInfoButtonWrapper moreInfoButton;

	[Header("Tasks")]
	[SerializeField]
	protected GameObject[] taskComponents;

	[SerializeField]
	protected TextMeshProUGUI tasksHeader;

	[SerializeField]
	protected RectTransform tasksList;

	[Header("Prefabs")]
	[SerializeField]
	protected StatsRow statsRowPrefab;

	protected List<StatsRow> rows = new List<StatsRow>();

	private GameSettings GameSettings => GameManager.GameState.Settings;

	private void OnEnable()
	{
		GameEvents.OnCommandExecuted += OnRefreshGameStats;
	}

	private void OnDisable()
	{
		GameEvents.OnCommandExecuted -= OnRefreshGameStats;
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		if (GameSettings.RulesGameMode == GameMode.Might)
		{
			scoreHeader.Key = "gamestatus.capitals";
		}
		else
		{
			scoreHeader.Key = "gamestatus.scores";
		}
		if (GameSettings.GameType == GameType.SinglePlayer)
		{
			((TMP_Text)headerText).text = string.Format(Localization.Get("tribepicker.gamemode", Localization.Get(GameModeUtils.GetTitle(GameSettings.BaseGameMode)) + " (" + Localization.Get(GameModeUtils.GetDifficultyName(GameSettings.Difficulty)) + ")"));
		}
		else
		{
			((TMP_Text)headerText).text = GameSettings.GameName;
		}
		mapSizeButton.SetData(GameSettings.MapSize, GameSettings.mapPreset, GameManager.GameState.Seed);
		gameModeButton.SetData(GameSettings.RulesGameMode, GameSettings.GameType, GameSettings.rules.ScoreLimit);
		PopulateScreen();
	}

	public override void Hide(bool instant = false)
	{
		base.Hide(instant);
		ClearStatsRows();
	}

	private void PopulateScreen()
	{
		ClearStatsRows();
		moreInfoButton.SetData(PrepareGameInfo());
		if (GameSettings.RulesGameMode == GameMode.Domination)
		{
			PopulateStatsList();
		}
		PopulatePlayers();
		PopulateTasks();
	}

	private void PopulateStatsList()
	{
		GetStatsRow(statsList).SetData("gamestats.speed", $"{ScoreManager.GetTimeScore(GameManager.GameState)}%", Localization.Get("gamestats.speed.info", (int)GameManager.GameState.CurrentTurn, ScoreManager.GetTimeGoal(GameManager.GameState)));
		GetStatsRow(statsList).SetData("gamestats.battle", $"{ScoreManager.GetBattleScore(GameManager.LocalPlayer)}%", Localization.Get("gamestats.battle.info", GameManager.LocalPlayer.kills, GameManager.LocalPlayer.casualities));
		GetStatsRow(statsList).SetData("gamestatus.tribes", $"{ScoreManager.GetWipeOutScore(GameManager.GameState, GameManager.LocalPlayer)}%", $"{GameManager.LocalPlayer.wipeOuts}/{GameManager.GameState.Settings.OpponentCount}");
		GetStatsRow(statsList).SetData("gamestatus.difficulty", $"{ScoreManager.GetDifficultyScore(GameManager.GameState)}%", Localization.Get(GameModeUtils.GetDifficultyName(GameManager.GameState.Settings.Difficulty)));
	}

	private string GetDescription(PlayerState player, StatsRow playerRow, int cityCount, string userName)
	{
		bool isSpectating = GameManager.Client.IsSpectating;
		bool num = player == GameManager.LocalPlayer && !isSpectating;
		_ = player.AutoPlay;
		bool flag = player.IsAlive(GameManager.GameState);
		bool num2 = num || GameManager.LocalPlayer.KnowsPlayer(player.Id) || !flag || (isSpectating && GameManager.LocalPlayer.Id == player.Id);
		string empty = string.Empty;
		empty = ((!num2) ? Localization.Get("gamestatus.unknown.ruler") : Localization.Get("gamestatus.ruled", userName));
		if (!flag)
		{
			return $"{empty}";
		}
		if (GameManager.GameState.Settings.RulesGameMode == GameMode.Might)
		{
			return string.Format("{0}, {1}", empty, Localization.Get("gamestatus.score", LocalizationUtils.FormatNumber(player.score)));
		}
		return string.Format("{0}, {1}", empty, Localization.Get((cityCount > 1) ? "gamestatus.cities" : "gamestatus.city", cityCount));
	}

	private void PopulatePlayers()
	{
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerState> playersSortedByRank = GameManager.GameState.GetPlayersSortedByRank();
		PlayerState localPlayer = GameManager.LocalPlayer;
		bool active = playersSortedByRank.Any((PlayerState x) => !x.IsAlive(GameManager.GameState));
		GameObject[] array = deadScoresListComponents;
		for (int num = 0; num < array.Length; num++)
		{
			array[num].SetActive(active);
		}
		foreach (PlayerState player in playersSortedByRank)
		{
			player.opinions.UpdateOpinions(GameManager.GameState, player);
			if (player.Id == byte.MaxValue)
			{
				continue;
			}
			bool flag = player == localPlayer;
			bool autoPlay = player.AutoPlay;
			bool flag2 = player.IsAlive(GameManager.GameState);
			bool flag3 = flag || GameManager.LocalPlayer.KnowsPlayer(player.Id) || !flag2;
			StatsRow statsRow = GetStatsRow(flag2 ? scoresList : deadScoresList);
			string empty = string.Empty;
			string spriteStringForFriendIdWithFormat = FriendUtils.GetSpriteStringForFriendIdWithFormat(player.AccountId ?? Guid.Empty, " <size=80%>{0}</size>");
			string userName = (autoPlay ? string.Format("{0} ({1})", spriteStringForFriendIdWithFormat + player.UserName, Localization.Get("gamestatus.ruled.bot")) : (spriteStringForFriendIdWithFormat + player.UserName));
			if (GameManager.GameState.Version >= 60)
			{
				((Behaviour)statsRow.button).enabled = true;
				statsRow.OnClicked += delegate
				{
					//IL_0011: Unknown result type (might be due to invalid IL or missing references)
					PlayerInfoPopup playerInfoPopup = PopupManager.GetPlayerInfoPopup();
					playerInfoPopup.SetData(player);
					playerInfoPopup.Show(InputManager.GetInputPosition());
				};
			}
			else
			{
				((Behaviour)statsRow.button).enabled = false;
			}
			if (flag3)
			{
				statsRow.SetPlayerInfo(player, GameManager.LocalPlayer);
				empty = player.GetLocalizedTribeName(GameManager.GameState);
			}
			else
			{
				empty = Localization.Get("gamestatus.unknown.tribe");
			}
			string statsValue = LocalizationUtils.FormatNumber(player.score);
			int num2 = 0;
			if (GameSettings.RulesGameMode == GameMode.Might)
			{
				foreach (PlayerState item in playersSortedByRank)
				{
					if (item.Id != byte.MaxValue)
					{
						num2++;
					}
				}
			}
			int num3 = 0;
			if (flag2)
			{
				if (GameSettings.RulesGameMode == GameMode.Might)
				{
					num3 = player.CountCapitals(GameManager.GameState);
					statsValue = $"{num3}/{num2}";
				}
				else
				{
					num3 = player.CountCities(GameManager.GameState);
				}
			}
			else
			{
				string text = Localization.Get("gamestatus.destroyed");
				statsValue = text[0].ToString().ToUpper() + text.Substring(1);
			}
			int totalIncomeFromEmbassiesAndDividend = localPlayer.GetTotalIncomeFromEmbassiesAndDividend(player, GameManager.GameState);
			statsRow.EmbassyIncomeText = totalIncomeFromEmbassiesAndDividend.ToString();
			statsRow.EmbassyIncomeContainerVisible = totalIncomeFromEmbassiesAndDividend > 0;
			string description = FitDescription(player, statsRow, num3, userName);
			statsRow.SetData(null, statsValue, description);
			statsRow.SetTextColor(flag2 ? normalTextColor : dimmedTextColor);
			statsRow.StatsName = empty;
			statsRow.BgVisible = flag;
			statsRow.ButtonEnabled = flag3;
		}
	}

	private string FitDescription(PlayerState player, StatsRow row, int cityCount, string userName)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		string text = "...";
		Rect val = row.DescriptionRect();
		float width = ((Rect)(ref val)).width;
		int num = userName.Length;
		string description = GetDescription(player, row, cityCount, userName);
		while (width < row.DescriptionGetPreferredValues(description).x && num != 0)
		{
			description = GetDescription(player, row, cityCount, userName.Substring(0, --num) + text);
		}
		return description;
	}

	private void PopulateTasks()
	{
		bool flag = false;
		if (GameManager.GameState.TryGetPlayer(GameManager.LocalPlayer.Id, out var playerState))
		{
			TaskBase[] array = playerState.tasks.ToArray();
			int num = array.Length;
			flag = num > 0;
			if (flag)
			{
				((TMP_Text)tasksHeader).text = Localization.Get("gamestatus.tasks", num, GameManager.GameState.GameLogicData.AllTaskData.Count - 1);
				for (int i = 0; i < num; i++)
				{
					TaskBase taskBase = array[i];
					if (GameManager.GameState.GameLogicData.TryGetData(taskBase.GetTaskType(), out var data))
					{
						StatsRow statsRow = GetStatsRow(tasksList);
						statsRow.shouldFitIconToParent = true;
						statsRow.StatsNameKey = data.displayName;
						statsRow.Description = Localization.Get(data.description);
						statsRow.StatsValue = (taskBase.IsCompleted ? Localization.Get("gamestatus.tasks.complete") : taskBase.GetCompletionStatus(GameManager.GameState, playerState));
						statsRow.IconSprite = (taskBase.IsCompleted ? UIManager.IconData.GetSprite("AchievementIcon") : null);
						((Behaviour)statsRow.button).enabled = false;
						statsRow.RefreshLayout();
					}
				}
			}
		}
		GameObject[] array2 = taskComponents;
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j].SetActive(flag);
		}
	}

	private string PrepareGameInfo()
	{
		List<string> list = new List<string>();
		list.Add(string.Format(Localization.Get("gameinfo.turn"), GameManager.GameState.CurrentTurn));
		list.Add(string.Format(Localization.Get("gameinfo.gameversion"), GameManager.GameState.Version));
		if (GameSettings.GameType == GameType.SinglePlayer)
		{
			list.Add(Localization.Get("gamesettings.difficulty") + ": " + Localization.Get(GameModeUtils.GetDifficultyName(GameSettings.Difficulty)));
		}
		if (GameSettings.RulesGameMode == GameMode.Perfection)
		{
			int num = Mathf.RoundToInt((1f + ScoreSheet.GetDifficultyBonusMultiplier(GameSettings.Difficulty, GameManager.GameState.PlayerCount - 1)) * 100f);
			list.Add(string.Format(Localization.Get("gamesettings.info.difficulty.bonus"), num));
		}
		string text = string.Empty;
		foreach (string item in list)
		{
			text = text + item + "\n";
		}
		return text;
	}

	protected StatsRow GetStatsRow(RectTransform container)
	{
		StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)container);
		rows.Add(statsRow);
		return statsRow;
	}

	protected void ClearStatsRows()
	{
		if (rows == null || rows.Count == 0)
		{
			return;
		}
		foreach (StatsRow row in rows)
		{
			Object.Destroy((Object)(object)((Component)row).gameObject);
		}
		rows.Clear();
	}

	private void OnRefreshGameStats()
	{
		PopulateScreen();
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}
}
