using UnityEngine;
using UnityEngine.UI;

public class ResourceBar : UIBasicComponent
{
	public ScoreContainer scoreContainer;

	public CurrencyContainer currencyContainer;

	public TurnsContainer turnsContainer;

	public TurnTimerContainer turnTimerContainer;

	protected static ResourceBar instance;

	public static ScoreContainer ScoreContainer => instance.scoreContainer;

	public static CurrencyContainer CurrencyContainer => instance.currencyContainer;

	public static TurnsContainer TurnsContainer => instance.turnsContainer;

	public override void Init()
	{
		base.Init();
		instance = this;
		GameEvents.OnMatchStart += OnMatchStart;
		GameEvents.OnMatchResumed += OnMatchStart;
	}

	public override void DeInit()
	{
		base.DeInit();
		GameEvents.OnMatchStart -= OnMatchStart;
		GameEvents.OnMatchResumed -= OnMatchStart;
	}

	private void OnDestroy()
	{
		GameEvents.OnMatchStart -= OnMatchStart;
		GameEvents.OnMatchResumed -= OnMatchStart;
	}

	private void OnEnable()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		BackendEvents.OnGameSummariesUpdated += OnGameSummariesUpdated;
		ResourceEvents.OnResourceChanged += OnResourceChanged;
		ResourceEvents.OnIncomeChanged += OnIncomeChanged;
		GameEvents.OnTurnEnded += OnTurnEnded;
		SystemEvents.OnSafeAreaChanged += OnSafeAreaChanged;
		RefreshSafeArea(ScreenManager.GetSafeArea());
		RefreshTimer();
		RefreshSkipStrikes();
	}

	private void OnDisable()
	{
		BackendEvents.OnGameSummariesUpdated -= OnGameSummariesUpdated;
		ResourceEvents.OnResourceChanged -= OnResourceChanged;
		ResourceEvents.OnIncomeChanged -= OnIncomeChanged;
		GameEvents.OnTurnEnded -= OnTurnEnded;
		SystemEvents.OnSafeAreaChanged -= OnSafeAreaChanged;
	}

	private void OnGameSummariesUpdated()
	{
		RefreshTimer();
		RefreshSkipStrikes();
	}

	private void OnSafeAreaChanged(Rect safeArea)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		RefreshSafeArea(safeArea);
	}

	private void OnMatchStart()
	{
		((Component)this).gameObject.SetActive(true);
		RefreshTimer();
	}

	private void Start()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		byte id = GameManager.LocalPlayer.Id;
		RefreshScore(id);
		RefreshCurrency(id);
		RefreshIncome(id);
		RefreshTurns();
		RefreshSafeArea(ScreenManager.GetSafeArea());
	}

	private void OnResourceChanged(byte playerId)
	{
		if (playerId == GameManager.LocalPlayer.Id)
		{
			RefreshScore(playerId);
			RefreshCurrency(playerId);
			RefreshIncome(playerId);
			RefreshTurns();
		}
	}

	private void OnIncomeChanged(byte playerId)
	{
		if (playerId == GameManager.LocalPlayer.Id)
		{
			RefreshIncome(playerId);
		}
	}

	private void OnTurnEnded()
	{
		RefreshTurns();
	}

	private void RefreshSafeArea(Rect safeArea)
	{
		base.rectTransform.SetAnchoredY(0f - ScreenManager.SafeTop * UICanvasScalerHelper.GetInvertedUIScale());
	}

	private void RefreshScore(byte playerId)
	{
		scoreContainer.Score = ResourceManager.GetResourceOfType(playerId, ResourceManager.Type.Score);
	}

	private void RefreshCurrency(byte playerId)
	{
		currencyContainer.Currency = ResourceManager.GetResourceOfType(playerId, ResourceManager.Type.Currency);
	}

	private void RefreshTurns()
	{
		int currentTurn = (int)GameManager.GameState.CurrentTurn;
		turnsContainer.Turns = currentTurn;
		Log.Learn("extra: turns [{0}]", new object[1] { currentTurn });
	}

	private void RefreshIncome(byte playerId)
	{
		int num = ResourceDataUtils.CalculateIncomeFor(GameManager.GameState, playerId);
		currencyContainer.Income = num;
		Log.Learn("extra: income [{0}]", new object[1] { num });
		ForceLayout();
	}

	private void RefreshTimer()
	{
		if (GameManager.GameState != null && GameManager.GameState.Settings.LiveGamePreset && !GameManager.Client.IsSpectating)
		{
			((Component)turnTimerContainer).gameObject.SetActive(true);
			turnTimerContainer.UpdateTimer();
			turnTimerContainer.UpdateAlert();
		}
		else
		{
			((Component)turnTimerContainer).gameObject.SetActive(false);
		}
	}

	private void RefreshSkipStrikes()
	{
		if (GameManager.GameState != null && GameManager.GameState.Settings.LiveGamePreset && GameManager.Client.CurrentGameId.HasValue && GameManager.GetRemoteGameDataManager().TryGetLocalParticipator(GameManager.Client.CurrentGameId.Value, out var participatorViewModel))
		{
			turnTimerContainer.Strikes = participatorViewModel.AutoSkipStrikeCount;
		}
		else
		{
			turnTimerContainer.Strikes = 0;
		}
	}

	private void ForceLayout()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}

	public void OnDebugGiveScore()
	{
		Log.Verbose("Local player currently have : {0} Score", new object[1] { ResourceManager.GetResourceOfType(GameManager.LocalPlayer.Id, ResourceManager.Type.Score) });
	}

	public void OnDebugGiveCurrency()
	{
		Log.Verbose("Local player currently have : {0} currency", new object[1] { ResourceManager.GetResourceOfType(GameManager.LocalPlayer.Id, ResourceManager.Type.Currency) });
	}
}
