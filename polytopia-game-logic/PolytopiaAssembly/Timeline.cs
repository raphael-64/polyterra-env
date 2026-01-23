using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class Timeline : MonoBehaviour
{
	public class TimelineTurnData
	{
		public int turn;

		public List<PlayerState> players;

		public Dictionary<byte, Color> playerColors;

		public Dictionary<byte, List<TimelineCommandData>> playerCommands;

		public int firstCommandIndex;

		public int lastCommandIndex;
	}

	public class TimelineCommandData
	{
		public int index;

		public float position;

		public CommandBase command;
	}

	public const float TIMELINE_HEIGHT_NORMAL = 90f;

	public const float TIMELINE_HEIGHT_SLIM = 60f;

	public const float SEGMENT_EDGE_BUFFER = 20f;

	public const float SEGMENT_SPACING = 10f;

	public const float COASTING_THRESHOLD = 100f;

	public const float COASTING_STOP_THRESHOLD = 1.2f;

	public const float TIMELINE_SCROLL_SPEED = 0.3f;

	private const int START_COMMAND = 0;

	public RectTransform rectTransform;

	public PolytopiaScrollRect scrollRect;

	public RectTransform scrollRectTransform;

	public TimelinePlaybackControls playbackControls;

	public TextMeshProUGUI commandLog;

	public TimelineTurnContainer timelineTurnContainerPrefab;

	public Action onSeekComplete;

	private Vector2 currentScrollPosition = Vector2.zero;

	private Vector2 currentOffset = Vector2.zero;

	private Vector2 totalOffset = Vector2.zero;

	private int currentTurn;

	private int totalTurns;

	private int currentIndex;

	private List<TimelineTurnContainer> turns;

	private Dictionary<int, TimelineTurnData> timelineDataCache;

	private float[] commandPositionCache;

	private bool isInitialized;

	private bool isSimulating;

	private bool isCoasting;

	public bool isUserPaused;

	private PollAnimation<Vector2> timelineAnimation;

	private Action onAnimationComplete;

	private float dragVelocity;

	private float cachedRectTransformWidth;

	public void Start()
	{
		currentIndex = 0;
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		GameEvents.OnReplayStarted += OnReplayStarted;
		GameEvents.OnReplayEnded += OnReplayEnded;
		GameEvents.OnMatchResumed += OnReplayResumed;
		((UnityEvent<Vector2>)(object)((ScrollRect)scrollRect).onValueChanged).AddListener((UnityAction<Vector2>)OnScrollUpdate);
		PolytopiaScrollRect polytopiaScrollRect = scrollRect;
		polytopiaScrollRect.onClicked = (Action)Delegate.Combine(polytopiaScrollRect.onClicked, new Action(OnTimelineClicked));
		PolytopiaScrollRect polytopiaScrollRect2 = scrollRect;
		polytopiaScrollRect2.onDragStarted = (Action)Delegate.Combine(polytopiaScrollRect2.onDragStarted, new Action(OnScrubStart));
		PolytopiaScrollRect polytopiaScrollRect3 = scrollRect;
		polytopiaScrollRect3.onDragEnded = (Action)Delegate.Combine(polytopiaScrollRect3.onDragEnded, new Action(OnScrubEnd));
		playbackControls.OnClicked += OnPlaybackControlsClicked;
		playbackControls.SetIsPlaying(isPlaying: false);
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		GameEvents.OnReplayStarted -= OnReplayStarted;
		GameEvents.OnReplayEnded -= OnReplayEnded;
		GameEvents.OnMatchResumed -= OnReplayResumed;
		((UnityEvent<Vector2>)(object)((ScrollRect)scrollRect).onValueChanged).RemoveListener((UnityAction<Vector2>)OnScrollUpdate);
		PolytopiaScrollRect polytopiaScrollRect = scrollRect;
		polytopiaScrollRect.onClicked = (Action)Delegate.Remove(polytopiaScrollRect.onClicked, new Action(OnTimelineClicked));
		PolytopiaScrollRect polytopiaScrollRect2 = scrollRect;
		polytopiaScrollRect2.onDragStarted = (Action)Delegate.Remove(polytopiaScrollRect2.onDragStarted, new Action(OnScrubStart));
		PolytopiaScrollRect polytopiaScrollRect3 = scrollRect;
		polytopiaScrollRect3.onDragEnded = (Action)Delegate.Remove(polytopiaScrollRect3.onDragEnded, new Action(OnScrubEnd));
		playbackControls.OnClicked -= OnPlaybackControlsClicked;
	}

	private void OnDestroy()
	{
		onAnimationComplete = null;
	}

	private void OnReplayStarted()
	{
		playbackControls.SetIsPlaying(isPlaying: true);
	}

	private void OnReplayResumed()
	{
		playbackControls.SetIsPlaying(isPlaying: true);
	}

	private void OnReplayEnded()
	{
		playbackControls.SetIsPlaying(isPlaying: false);
	}

	private void OnPlaybackControlsClicked(int id, BaseEventData eventData = null)
	{
		if (GameManager.Client.ActionManager.IsPaused)
		{
			Play();
		}
		else
		{
			Pause();
		}
	}

	public void Play()
	{
		GameManager.Client.ActionManager.Resume();
		isUserPaused = false;
		playbackControls.SetIsPlaying(!GameManager.Client.ActionManager.IsPaused);
	}

	public void Pause()
	{
		isUserPaused = true;
		GameManager.Client.ActionManager.Pause();
		playbackControls.SetIsPlaying(!GameManager.Client.ActionManager.IsPaused);
	}

	public void JumpForward()
	{
		if (!isSimulating)
		{
			GameManager.Client.ActionManager.Pause();
			isSimulating = true;
			int targetCommand = GetNextPlayerCommandIndex(currentIndex);
			ScrollToCommand(targetCommand, 0.1f, delegate
			{
				SimulateToCommand(targetCommand);
			});
		}
	}

	public void JumpBack()
	{
		if (!isSimulating)
		{
			GameManager.Client.ActionManager.Pause();
			isSimulating = true;
			int targetCommand = GetPreviousPlayerCommandIndex(currentIndex);
			ScrollToCommand(targetCommand, 0.1f, delegate
			{
				SimulateToCommand(targetCommand);
			});
		}
	}

	private void OnScrubStart()
	{
		if (!isSimulating)
		{
			isCoasting = false;
			GameManager.Client.ActionManager.Pause();
			playbackControls.SetIsPlaying(isPlaying: false);
			timelineAnimation = null;
		}
	}

	private void OnTimelineClicked()
	{
		if (!isSimulating && isCoasting)
		{
			isCoasting = false;
			OnScrollComplete();
		}
	}

	private void OnScrubEnd()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(((ScrollRect)scrollRect).velocity.x) <= 100f)
		{
			OnScrollComplete();
		}
		else
		{
			isCoasting = true;
		}
	}

	private void Update()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.GameState != null && !isInitialized)
		{
			RefreshItems();
			isInitialized = true;
			UpdateUISize();
		}
		float num = cachedRectTransformWidth;
		Rect rect = rectTransform.rect;
		if (num != ((Rect)(ref rect)).width)
		{
			RefreshItems();
		}
		if (GameManager.Client != null && GameManager.Client.ActionManager != null && currentIndex < GameManager.Client.ActionManager.LastSeenCommand && !isSimulating && !isCoasting)
		{
			GameManager.Client.ActionManager.HoldExecution(value: true);
			ScrollToCommand(GameManager.Client.ActionManager.LastSeenCommand, 0.3f, delegate
			{
				int num2 = GameManager.Client.ActionManager.LastSeenCommand - 1;
				if (num2 < 0 || num2 == GameManager.Client.GameState.CommandStack.Count)
				{
					GameManager.Client.ActionManager.HoldExecution(value: false);
				}
				else if (GameManager.Client.GameState.CommandStack[num2] is EndTurnCommand)
				{
					GameManager.DelayCall(500, delegate
					{
						GameManager.Client.ActionManager.HoldExecution(value: false);
					});
				}
				else
				{
					GameManager.Client.ActionManager.HoldExecution(value: false);
				}
			});
		}
		if (timelineAnimation != null && !isCoasting)
		{
			((ScrollRect)scrollRect).content.anchoredPosition = Vector2.Lerp(timelineAnimation.startValue, timelineAnimation.targetValue, timelineAnimation.GetEasedProgress());
			if (timelineAnimation.IsCompleted())
			{
				timelineAnimation = null;
				onAnimationComplete?.Invoke();
			}
		}
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		UpdateUISize();
	}

	public void UpdateUISize()
	{
		if (isInitialized)
		{
			bool flag = Application.isMobilePlatform && ScreenManager.GetScreenWidth() < ScreenManager.GetScreenHeight();
			SetHeight(flag ? 90f : 60f);
			RefreshItems();
		}
	}

	public void SetHeight(float height)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.SetHeight(height);
		if (height < 90f)
		{
			((ScrollRect)scrollRect).content.offsetMin = new Vector2(((ScrollRect)scrollRect).content.offsetMin.x, 4f);
			((ScrollRect)scrollRect).content.offsetMax = new Vector2(((ScrollRect)scrollRect).content.offsetMax.x, -4f);
		}
		else
		{
			((ScrollRect)scrollRect).content.offsetMin = new Vector2(((ScrollRect)scrollRect).content.offsetMin.x, 10f);
			((ScrollRect)scrollRect).content.offsetMax = new Vector2(((ScrollRect)scrollRect).content.offsetMax.x, -10f);
		}
	}

	public void SetData(GameState gameState)
	{
		if (gameState != null)
		{
			isInitialized = false;
			CacheGameStateData(gameState);
		}
	}

	public void ScrollToCommand(int commandIndex, float duration = 1f, Action onComplete = null)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetCommandPosition(commandIndex, out var commandPosition))
		{
			currentIndex = commandIndex;
			currentTurn = GetTurnForCommand(currentIndex);
			ScrollToPosition(commandPosition, duration, onComplete);
		}
		else
		{
			onComplete?.Invoke();
		}
	}

	public void ScrollToPosition(Vector2 position, float duration = 1f, Action onComplete = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector2 targetValue = totalOffset - position;
		Vector2 anchoredPosition = ((ScrollRect)scrollRect).content.anchoredPosition;
		targetValue.y = anchoredPosition.y;
		onAnimationComplete = onComplete;
		timelineAnimation = new PollAnimation<Vector2>(duration, anchoredPosition, targetValue);
	}

	public bool TryGetCommandPosition(int commandIndex, out Vector2 commandPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		commandPosition = Vector2.zero;
		if (GameManager.GameState != null && GameManager.GameState.CommandStack.Count > 0)
		{
			for (int i = 0; i < GameManager.GameState.CommandStack.Count; i++)
			{
				if (i == commandIndex)
				{
					if (GameManager.GameState.CommandStack[i].PlayerId == byte.MaxValue)
					{
						commandPosition = new Vector2(commandPositionCache[Mathf.Max(i - 1, 0)], 0f);
					}
					else
					{
						commandPosition = new Vector2(commandPositionCache[i], 0f);
					}
					return true;
				}
			}
		}
		return false;
	}

	private void RefreshItems()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)this == (Object)null || GameManager.GameState == null || timelineDataCache == null)
		{
			return;
		}
		Rect rect = rectTransform.rect;
		int num = Mathf.CeilToInt((cachedRectTransformWidth = ((Rect)(ref rect)).width) / 64f);
		if (turns == null)
		{
			turns = new List<TimelineTurnContainer>();
		}
		float num2 = GetPlayheadPosition();
		for (int i = 0; i < turns.Count; i++)
		{
			TimelineTurnContainer timelineTurnContainer = turns[i];
			timelineTurnContainer.rectTransform.anchoredPosition = new Vector2(num2, 0f);
			num2 += timelineTurnContainer.GetWidth() + 10f;
		}
		while (turns.Count < num)
		{
			int key = ((turns.Count != 0) ? (GetLastIndex() + 1) : 0);
			if (!timelineDataCache.TryGetValue(key, out var value))
			{
				Log.Warning("Could not find turn: {0}", new object[1] { turns.Count });
				break;
			}
			TimelineTurnContainer timelineTurnContainer2 = Object.Instantiate<TimelineTurnContainer>(timelineTurnContainerPrefab, (Transform)(object)((ScrollRect)scrollRect).content);
			timelineTurnContainer2.SetData(value);
			timelineTurnContainer2.rectTransform.anchoredPosition = new Vector2(num2, 0f);
			turns.Add(timelineTurnContainer2);
			num2 += timelineTurnContainer2.GetWidth() + 10f;
		}
		if (turns.Count < num)
		{
			num2 += GetPlayheadPosition();
		}
		((ScrollRect)scrollRect).content.SetWidth(num2 - 10f);
		currentScrollPosition = ((ScrollRect)scrollRect).content.anchoredPosition;
		UpdateCurrentIndex();
		UpdatePositions();
	}

	private void OnScrollUpdate(Vector2 value)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.GameState == null)
		{
			return;
		}
		Vector2 val = ((ScrollRect)scrollRect).content.anchoredPosition - currentScrollPosition;
		if (val.x < 0f)
		{
			float x = ((ScrollRect)scrollRect).content.anchoredPosition.x;
			Rect rect = ((ScrollRect)scrollRect).content.rect;
			float num = x + ((Rect)(ref rect)).width;
			rect = scrollRectTransform.rect;
			if (num < ((Rect)(ref rect)).width + 20f)
			{
				MoveFirstToLast();
			}
		}
		else if (val.x > 0f && ((ScrollRect)scrollRect).content.anchoredPosition.x > -20f)
		{
			MoveLastToFirst();
		}
		currentScrollPosition = ((ScrollRect)scrollRect).content.anchoredPosition;
		if (isCoasting && Mathf.Abs(((ScrollRect)scrollRect).velocity.x) <= 1.2f)
		{
			isCoasting = false;
			OnScrollComplete();
		}
		CommandBase commandBase = GameManager.GameState.CommandStack[currentIndex];
		if (((Component)commandLog).gameObject.activeInHierarchy)
		{
			((TMP_Text)commandLog).text = $"{commandBase.GetType()}\n(T{currentTurn} P{commandBase.PlayerId} C{currentIndex})";
		}
	}

	private void OnScrollComplete()
	{
		float position = 0f - currentScrollPosition.x + totalOffset.x;
		int targetCommand = GetCommandAtPosition(position);
		if (targetCommand != -1)
		{
			isSimulating = true;
			((Behaviour)scrollRect).enabled = false;
			ScrollToCommand(targetCommand, 0.2f, delegate
			{
				SimulateToCommand(targetCommand);
			});
		}
	}

	private void SimulateToCommand(int targetCommand)
	{
		isSimulating = true;
		GameManager.Client.ClearTargetState();
		GameManager.Client.RewindToCommand((ushort)targetCommand, delegate
		{
			isSimulating = false;
			((Behaviour)scrollRect).enabled = true;
			currentIndex = targetCommand;
			MapRenderer.Current.RenderMap(GameManager.GameState.Map);
			TechEvents.RefreshAllTech();
			UIEvents.ForceRefreshHud();
			ResourceEvents.RefreshWallets(GameManager.GameState.CurrentPlayer);
			if (!isUserPaused)
			{
				GameManager.Client.ActionManager.Resume();
			}
			onSeekComplete?.Invoke();
		});
	}

	public void UpdateCurrentIndex()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (turns == null)
		{
			return;
		}
		float playheadPosition = GetPlayheadPosition();
		currentIndex = 0;
		for (int i = 0; i < turns.Count; i++)
		{
			TimelineTurnContainer timelineTurnContainer = turns[i];
			float num = ((ScrollRect)scrollRect).content.anchoredPosition.x + timelineTurnContainer.rectTransform.anchoredPosition.x;
			Rect rect = timelineTurnContainer.rectTransform.rect;
			if (num + ((Rect)(ref rect)).width + 10f > playheadPosition || i == turns.Count - 1)
			{
				currentIndex = i;
				break;
			}
		}
		currentTurn = GetTurnForCommand(currentIndex);
	}

	private void MoveFirstToLast()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if (timelineDataCache.TryGetValue(GetLastIndex() + 1, out var value))
		{
			TimelineTurnContainer timelineTurnContainer = turns[0];
			timelineTurnContainer.SetData(value);
			turns.RemoveAt(0);
			turns.Add(timelineTurnContainer);
			currentOffset += new Vector2(turns[0].rectTransform.anchoredPosition.x, 0f);
			totalOffset += currentOffset;
			UpdatePositions();
		}
	}

	private void MoveLastToFirst()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (timelineDataCache.TryGetValue(GetFirstIndex() - 1, out var value))
		{
			TimelineTurnContainer timelineTurnContainer = turns[turns.Count - 1];
			timelineTurnContainer.SetData(value);
			turns.RemoveAt(turns.Count - 1);
			turns.Insert(0, timelineTurnContainer);
			currentOffset -= new Vector2(timelineTurnContainer.GetWidth() + 10f, 0f);
			if (timelineTurnContainer.GetTurn() == 0)
			{
				currentOffset.x -= GetPlayheadPosition();
			}
			totalOffset += currentOffset;
			UpdatePositions();
		}
	}

	private void UpdatePositions()
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		for (int i = 0; i < turns.Count; i++)
		{
			TimelineTurnContainer timelineTurnContainer = turns[i];
			if (timelineTurnContainer.GetTurn() == 0)
			{
				num = GetPlayheadPosition();
			}
			timelineTurnContainer.rectTransform.anchoredPosition = new Vector2(num, 0f);
			num += timelineTurnContainer.GetWidth() + 10f;
			if (timelineTurnContainer.GetTurn() == totalTurns)
			{
				num += GetPlayheadPosition();
			}
		}
		((ScrollRect)scrollRect).content.SetWidth(num - 10f);
		scrollRect.OffsetContent(currentOffset);
		if (timelineAnimation != null)
		{
			PollAnimation<Vector2> pollAnimation = timelineAnimation;
			pollAnimation.startValue += currentOffset;
			PollAnimation<Vector2> pollAnimation2 = timelineAnimation;
			pollAnimation2.targetValue += currentOffset;
		}
		currentOffset = Vector2.zero;
		currentScrollPosition = ((ScrollRect)scrollRect).content.anchoredPosition;
	}

	private float GetPlayheadPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Rect rect = rectTransform.rect;
		return ((Rect)(ref rect)).width * 0.5f;
	}

	private void CacheGameStateData(GameState gameState)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		if (gameState == null || gameState.CommandStack == null || gameState.CommandStack.Count == 0)
		{
			Log.Error("Unable to parse GameState", Array.Empty<object>());
			return;
		}
		timelineDataCache = new Dictionary<int, TimelineTurnData>();
		commandPositionCache = new float[gameState.CommandStack.Count];
		totalOffset = Vector2.zero;
		int num = 0;
		byte b = 1;
		float num2 = 0f;
		float num3 = 0f;
		Log.Info("Building timeline data cache...", Array.Empty<object>());
		for (int i = 0; i < gameState.CommandStack.Count; i++)
		{
			CommandBase commandBase = gameState.CommandStack[i];
			if (commandBase.PlayerId == byte.MaxValue)
			{
				if (commandBase.GetCommandType() == CommandType.EndTurn)
				{
					if (timelineDataCache.TryGetValue(num, out var value))
					{
						value.lastCommandIndex = i - 1;
						timelineDataCache[num] = value;
					}
					num++;
					num2 += 10f;
				}
				continue;
			}
			if (i > 0)
			{
				num2 += 10f;
				num3 += 10f;
			}
			if (b != commandBase.PlayerId)
			{
				if (num3 < 64f)
				{
					num2 += 64f - num3;
				}
				num3 = 0f;
			}
			b = commandBase.PlayerId;
			if (!timelineDataCache.TryGetValue(num, out var value2))
			{
				value2 = new TimelineTurnData
				{
					turn = num,
					firstCommandIndex = i,
					playerCommands = new Dictionary<byte, List<TimelineCommandData>>()
				};
				timelineDataCache.Add(num, value2);
			}
			if (!value2.playerCommands.TryGetValue(b, out var value3))
			{
				value3 = new List<TimelineCommandData>();
				value2.playerCommands.Add(b, value3);
			}
			if (value2.players == null)
			{
				value2.players = new List<PlayerState>();
				value2.playerColors = new Dictionary<byte, Color>();
			}
			if (gameState.TryGetPlayer(commandBase.PlayerId, out var playerState) && !value2.players.Contains(playerState))
			{
				value2.players.Add(playerState);
				value2.playerColors.Add(playerState.Id, playerState.GetPlayerColor(gameState));
			}
			TimelineCommandData item = new TimelineCommandData
			{
				index = i,
				position = num2,
				command = commandBase
			};
			value3.Add(item);
			value2.playerCommands[b] = value3;
			timelineDataCache[num] = value2;
			commandPositionCache[i] = num2;
		}
		UpdateCommandPositions();
		totalTurns = num;
	}

	private void UpdateCommandPositions()
	{
		foreach (KeyValuePair<int, TimelineTurnData> item in timelineDataCache)
		{
			foreach (KeyValuePair<byte, List<TimelineCommandData>> playerCommand in item.Value.playerCommands)
			{
				int count = playerCommand.Value.Count;
				if ((float)count * 10f < 64f)
				{
					float position = playerCommand.Value[0].position;
					float num = 64f / (float)count;
					for (int i = 0; i < playerCommand.Value.Count; i++)
					{
						float num2 = position + num * (float)i;
						TimelineCommandData timelineCommandData = playerCommand.Value[i];
						commandPositionCache[timelineCommandData.index] = num2;
						timelineCommandData.position = num2;
						playerCommand.Value[i] = timelineCommandData;
					}
				}
			}
		}
	}

	private float GetSegmentWidth(int turn, byte player)
	{
		float num = 0f;
		if (timelineDataCache != null && timelineDataCache.TryGetValue(turn, out var value) && value.playerCommands != null && value.playerCommands.TryGetValue(player, out var value2))
		{
			num += Mathf.Max((float)value2.Count * 10f, 64f);
		}
		return num;
	}

	public float[] GetCommandPositions(GameState gameState)
	{
		if (GameManager.GameState == null)
		{
			return null;
		}
		float[] array = new float[GameManager.GameState.CommandStack.Count];
		if (gameState != null && gameState.CommandStack != null)
		{
			int num = 1;
			float num2 = 0f;
			for (int i = 0; i < gameState.CommandStack.Count; i++)
			{
				CommandBase commandBase = gameState.CommandStack[i];
				array[i] = num2;
				num2 += 10f;
				if (commandBase.PlayerId < num)
				{
					num2 += 10f;
				}
				else if (commandBase.PlayerId > num + 1)
				{
					num2 += 64f;
				}
				num = commandBase.PlayerId;
			}
		}
		return array;
	}

	public int GetCommandAtPosition(float position)
	{
		if (commandPositionCache == null)
		{
			return -1;
		}
		for (int i = 0; i < commandPositionCache.Length; i++)
		{
			if (commandPositionCache[i] >= position && i > 0)
			{
				return Mathf.Max(0, i - 1);
			}
			if (i == commandPositionCache.Length - 1 && position > commandPositionCache[i])
			{
				return i;
			}
		}
		return -1;
	}

	public int GetTurnForCommand(int commandIndex)
	{
		foreach (KeyValuePair<int, TimelineTurnData> item in timelineDataCache)
		{
			if (item.Value.firstCommandIndex <= commandIndex && item.Value.lastCommandIndex >= commandIndex)
			{
				return item.Key;
			}
		}
		return -1;
	}

	public int GetFirstIndex()
	{
		return turns[0].GetTurn();
	}

	public int GetCurrentIndex()
	{
		return currentIndex;
	}

	public int GetLastIndex()
	{
		if (turns.Count == 0)
		{
			return 0;
		}
		return turns[turns.Count - 1].GetTurn();
	}

	public int GetNextPlayerCommandIndex(int currentIndex)
	{
		byte playerId = GameManager.GameState.CommandStack[currentIndex].PlayerId;
		bool flag = false;
		for (int i = currentIndex + 1; i < GameManager.GameState.CommandStack.Count; i++)
		{
			if (i == GameManager.GameState.CommandStack.Count - 1)
			{
				return GameManager.GameState.CommandStack.Count - 1;
			}
			CommandBase commandBase = GameManager.GameState.CommandStack[i];
			if (commandBase.PlayerId != byte.MaxValue)
			{
				if (playerId != commandBase.PlayerId || flag)
				{
					return i;
				}
				if (commandBase.GetCommandType() == CommandType.EndTurn)
				{
					flag = true;
				}
			}
		}
		return currentIndex;
	}

	public int GetPreviousPlayerCommandIndex(int currentIndex)
	{
		byte playerId = GameManager.GameState.CommandStack[currentIndex].PlayerId;
		bool flag = false;
		for (int num = currentIndex - 1; num >= 0; num--)
		{
			if (num == 0)
			{
				return 0;
			}
			CommandBase commandBase = GameManager.GameState.CommandStack[num];
			if (commandBase.PlayerId != byte.MaxValue)
			{
				if (playerId != commandBase.PlayerId && flag)
				{
					return GetNextNonNatureCommand(num);
				}
				if (commandBase.GetCommandType() == CommandType.EndTurn)
				{
					playerId = commandBase.PlayerId;
					flag = true;
				}
			}
		}
		return currentIndex;
	}

	private int GetNextNonNatureCommand(int commandIndex)
	{
		commandIndex++;
		for (int i = commandIndex; i < GameManager.GameState.CommandStack.Count; i++)
		{
			if (GameManager.GameState.CommandStack[i].PlayerId != byte.MaxValue)
			{
				return i;
			}
		}
		return commandIndex;
	}
}
