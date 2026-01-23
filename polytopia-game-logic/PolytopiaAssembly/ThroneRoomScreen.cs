using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ThroneRoomScreen : UIScreenBase
{
	[Header("Throne Room Screen")]
	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	[SerializeField]
	protected ScrollRect scrollRect;

	[SerializeField]
	protected RectTransform playerInfoList;

	[SerializeField]
	protected RectTransform scoreList;

	[SerializeField]
	protected RectTransform ratingsList;

	[SerializeField]
	protected RectTransform gdprNotice;

	[SerializeField]
	protected GameObject[] scoreRelatedObjects;

	[SerializeField]
	protected GameObject[] ratingRelatedObjects;

	[SerializeField]
	protected UIButtonBase resetButton;

	[SerializeField]
	protected AndroidSignInContainer signOutButton;

	[Header("Prefabs")]
	[SerializeField]
	protected StatsRow statsRowPrefab;

	protected List<StatsRow> playerInfoRows = new List<StatsRow>();

	protected List<StatsRow> scoreRows = new List<StatsRow>();

	protected List<StatsRow> ratingRows = new List<StatsRow>();

	protected bool haveScore;

	protected bool haveRating;

	protected StatsRow gamesPlayedRow;

	public void OnEnable()
	{
		SettingsEvents.OnSettingsUpdated += ConsentChanged;
		StartSceneBg.Bright = false;
	}

	public void OnDisable()
	{
		SettingsEvents.OnSettingsUpdated -= ConsentChanged;
	}

	private void ConsentChanged(SettingsUtils.SettingsType type)
	{
		if (type == SettingsUtils.SettingsType.PrivacyConsent)
		{
			ClearRows();
			PopulatePlayerInfoList();
			PopulateScoreList();
			PopulateRatingsList();
			((Component)resetButton).gameObject.SetActive(haveScore || haveRating);
		}
	}

	public override void Show(bool instant = false)
	{
		base.Show(instant);
		ClearRows();
		PopulatePlayerInfoList();
		PopulateScoreList();
		PopulateRatingsList();
		((Component)resetButton).gameObject.SetActive(haveScore || haveRating);
		((Component)signOutButton).gameObject.SetActive(false);
		OnScreenUpdated();
	}

	protected void PopulatePlayerInfoList()
	{
		GetStatsRow(playerInfoList).SetData("throne.alias", AccountManager.Alias, string.Empty);
		gamesPlayedRow = GetStatsRow(playerInfoList);
		int cachedNumSingleplayerGames = CacheManager.GetCachedNumSingleplayerGames();
		gamesPlayedRow.SetData("throne.played", LocalizationUtils.FormatNumber(cachedNumSingleplayerGames), string.Empty);
	}

	protected void PopulateScoreList()
	{
		haveScore = false;
		if (!SettingsUtils.PrivacyConsent)
		{
			return;
		}
		foreach (KeyValuePair<TribeData.Type, TribeData> allTribeDatum in PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).AllTribeData)
		{
			if (ScoreManager.TryGetTribeScore(allTribeDatum.Key, out var score))
			{
				StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)scoreList);
				statsRow.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(allTribeDatum.Value.style));
				statsRow.StatsNameKey = allTribeDatum.Value.displayName;
				statsRow.Description = string.Empty;
				statsRow.StatsValueInteger = score;
				statsRow.StatsValue = LocalizationUtils.FormatNumber(score);
				((Behaviour)statsRow.button).enabled = false;
				scoreRows.Add(statsRow);
				haveScore = true;
			}
		}
		if (scoreRows != null && scoreRows.Count > 0)
		{
			scoreRows.Sort((StatsRow x, StatsRow y) => -x.StatsValueInteger.CompareTo(y.StatsValueInteger));
			for (int num = 0; num < scoreRows.Count; num++)
			{
				((Component)scoreRows[num]).transform.SetSiblingIndex(num);
			}
		}
		GameObject[] array = scoreRelatedObjects;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			array[num2].SetActive(haveScore);
		}
	}

	protected void PopulateRatingsList()
	{
		haveRating = false;
		if (!SettingsUtils.PrivacyConsent)
		{
			return;
		}
		foreach (KeyValuePair<TribeData.Type, TribeData> allTribeDatum in PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).AllTribeData)
		{
			if (ScoreManager.TryGetTribeRating(allTribeDatum.Key, out var rating))
			{
				StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)ratingsList);
				statsRow.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(allTribeDatum.Value.style));
				statsRow.StatsNameKey = allTribeDatum.Value.displayName;
				statsRow.Description = string.Empty;
				statsRow.ValueFormat = "{0}%";
				statsRow.StatsValueInteger = rating;
				((Behaviour)statsRow.button).enabled = false;
				ratingRows.Add(statsRow);
				haveRating = true;
			}
		}
		if (ratingRows != null && ratingRows.Count > 0)
		{
			ratingRows.Sort((StatsRow x, StatsRow y) => -x.StatsValueInteger.CompareTo(y.StatsValueInteger));
			for (int num = 0; num < ratingRows.Count; num++)
			{
				((Component)ratingRows[num]).transform.SetSiblingIndex(num);
			}
		}
		GameObject[] array = ratingRelatedObjects;
		for (int num2 = 0; num2 < array.Length; num2++)
		{
			array[num2].SetActive(haveRating);
		}
	}

	protected StatsRow GetStatsRow(RectTransform container)
	{
		StatsRow statsRow = Object.Instantiate<StatsRow>(statsRowPrefab, (Transform)(object)container);
		playerInfoRows.Add(statsRow);
		((Behaviour)statsRow.button).enabled = false;
		return statsRow;
	}

	protected void ClearRows()
	{
		ClearPlayerInfoRows();
		ClearScoreRows();
		ClearRatingRows();
	}

	protected void ClearPlayerInfoRows()
	{
		foreach (StatsRow playerInfoRow in playerInfoRows)
		{
			Object.Destroy((Object)(object)((Component)playerInfoRow).gameObject);
		}
		playerInfoRows.Clear();
	}

	protected void ClearScoreRows()
	{
		foreach (StatsRow scoreRow in scoreRows)
		{
			Object.Destroy((Object)(object)((Component)scoreRow).gameObject);
		}
		scoreRows.Clear();
		GameObject[] array = scoreRelatedObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}

	protected void ClearRatingRows()
	{
		foreach (StatsRow ratingRow in ratingRows)
		{
			Object.Destroy((Object)(object)((Component)ratingRow).gameObject);
		}
		ratingRows.Clear();
		GameObject[] array = ratingRelatedObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}

	public void OnCopyId(int id)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		TextEditor val = new TextEditor
		{
			text = AccountManager.PlayerAccountId.ToString()
		};
		val.SelectAll();
		val.Copy();
		NotificationManager.Notify(Localization.Get("throne.clipboard"), Localization.Get("throne.clipboard.title"));
	}

	private void OnResetScores(int id, BaseEventData eventData = null)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("throne.resetwarning.title");
		basicPopup.Description = Localization.Get("throne.resetwarning");
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, OnResetScoresAccepted)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected async void OnResetScoresAccepted(int id, BaseEventData eventData)
	{
		if (await ScoreManager.ClearScoresAndRatings())
		{
			ClearScoreRows();
			ClearRatingRows();
			NotificationManager.Notify(Localization.Get("throne.reset.complete"));
		}
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	protected override void SubscribeButtonsEvents()
	{
		resetButton.OnClicked += OnResetScores;
	}

	protected override void UnsubscribeButtonsEvents()
	{
		resetButton.OnClicked -= OnResetScores;
	}
}
