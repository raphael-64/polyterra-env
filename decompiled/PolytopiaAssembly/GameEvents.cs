public static class GameEvents
{
	public delegate void OnStartupDoneEvent();

	public delegate void OnRefreshBadgesEvent();

	public delegate void OnMapGeneratedEvent(MapData map);

	public delegate void OnStateUpdatingEvent();

	public delegate void OnStateUpdatedEvent();

	public delegate void OnPassPlayerEvent();

	public delegate void OnMapLoadEvent(MapData data);

	public delegate void OnMapLoadedEvent();

	public delegate void OnMatchStartEvent();

	public delegate void OnMatchResumedEvent();

	public delegate void OnMatchEndEvent(bool localPlayerIsWinner, ScoreDetails scoreDetails);

	public delegate void OnTurnStartedEvent();

	public delegate void OnTurnEndedEvent();

	public delegate void OnStartedProcessingEvent();

	public delegate void OnFinishedProcessingEvent();

	public delegate void OnWaitingForServerEvent(bool isWaiting);

	public delegate void OnSessionEndedEvent();

	public delegate void OnRecapStartedEvent();

	public delegate void OnRecapEndedEvent();

	public delegate void OnReplayStartedEvent();

	public delegate void OnReplayEndedEvent();

	public delegate void OnCommandExecutedEvent();

	public static event OnStartupDoneEvent OnStartupDone;

	public static event OnRefreshBadgesEvent OnRefreshBadges;

	public static event OnMapGeneratedEvent OnMapGenerated;

	public static event OnStateUpdatingEvent OnStateUpdating;

	public static event OnStateUpdatedEvent OnStateUpdated;

	public static event OnPassPlayerEvent OnPassPlayer;

	public static event OnMapLoadEvent OnMapLoad;

	public static event OnMapLoadedEvent OnMapLoaded;

	public static event OnMatchStartEvent OnMatchStart;

	public static event OnMatchResumedEvent OnMatchResumed;

	public static event OnMatchEndEvent OnMatchEnd;

	public static event OnTurnStartedEvent OnTurnStarted;

	public static event OnTurnEndedEvent OnTurnEnded;

	public static event OnStartedProcessingEvent OnStartedProcessing;

	public static event OnFinishedProcessingEvent OnFinishedProcessing;

	public static event OnWaitingForServerEvent OnWaitingForServer;

	public static event OnSessionEndedEvent OnSessionEnded;

	public static event OnRecapStartedEvent OnRecapStarted;

	public static event OnRecapEndedEvent OnRecapEnded;

	public static event OnReplayStartedEvent OnReplayStarted;

	public static event OnReplayEndedEvent OnReplayEnded;

	public static event OnCommandExecutedEvent OnCommandExecuted;

	public static void StartupDone()
	{
		GameEvents.OnStartupDone?.Invoke();
	}

	public static void RefreshBadges()
	{
		GameEvents.OnRefreshBadges?.Invoke();
	}

	public static void MapGenerated(MapData map)
	{
		GameEvents.OnMapGenerated?.Invoke(map);
	}

	public static void StateUpdating()
	{
		GameEvents.OnStateUpdating?.Invoke();
	}

	public static void StateUpdated()
	{
		GameEvents.OnStateUpdated?.Invoke();
	}

	public static void PassPlayer()
	{
		GameEvents.OnPassPlayer?.Invoke();
	}

	public static void MapLoading(MapData data)
	{
		GameEvents.OnMapLoad?.Invoke(data);
	}

	public static void MapLoaded()
	{
		GameEvents.OnMapLoaded?.Invoke();
	}

	public static void MatchStart()
	{
		GameEvents.OnMatchStart?.Invoke();
	}

	public static void MatchResumed()
	{
		GameEvents.OnMatchResumed?.Invoke();
	}

	public static void MatchEnd(bool localPlayerIsWinner, ScoreDetails scoreDetails)
	{
		GameEvents.OnMatchEnd?.Invoke(localPlayerIsWinner, scoreDetails);
	}

	public static void TurnStarted()
	{
		GameEvents.OnTurnStarted?.Invoke();
	}

	public static void TurnEnded()
	{
		GameEvents.OnTurnEnded?.Invoke();
	}

	public static void StartedProcessing()
	{
		GameEvents.OnStartedProcessing?.Invoke();
	}

	public static void FinishedProcessing()
	{
		GameEvents.OnFinishedProcessing?.Invoke();
	}

	public static void WaitingForServer(bool isWaiting)
	{
		GameEvents.OnWaitingForServer?.Invoke(isWaiting);
	}

	public static void SessionEnded()
	{
		GameEvents.OnSessionEnded?.Invoke();
	}

	public static void RecapStarted()
	{
		GameEvents.OnRecapStarted?.Invoke();
	}

	public static void RecapEnded()
	{
		GameEvents.OnRecapEnded?.Invoke();
	}

	public static void ReplayStarted()
	{
		GameEvents.OnReplayStarted?.Invoke();
	}

	public static void ReplayEnded()
	{
		GameEvents.OnReplayEnded?.Invoke();
	}

	public static void CommandExecuted()
	{
		GameEvents.OnCommandExecuted?.Invoke();
	}
}
