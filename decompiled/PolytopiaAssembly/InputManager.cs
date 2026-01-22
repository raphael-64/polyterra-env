using System;
using System.Collections.Generic;
using IchiGamepad;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
	public enum Buttons
	{
		None,
		Accept,
		Cancel,
		Start,
		Select,
		Tech,
		EndTurn,
		Menu,
		Hint,
		ButtonBarMoveLeft,
		ButtonBarMoveRight,
		GameStats
	}

	[Flags]
	public enum InputType
	{
		Camera = 1,
		Map = 2,
		Input = 4,
		UI = 8,
		Loading = 0x10
	}

	[SerializeField]
	protected bool emulateMultitouch;

	[Info]
	[SerializeField]
	protected string info = "No Info";

	private static Vector3 viewportPosition;

	private static Dictionary<InputType, int> blockers;

	private static List<string> eatenButtons = new List<string>();

	private static Vector2 multiTouchStart;

	public static InputType enabledInputs = InputType.Camera | InputType.Map;

	private static List<PolytopiaTouch> currentTouches = new List<PolytopiaTouch>();

	private static List<PolytopiaTouch> previousTouches = new List<PolytopiaTouch>();

	private static List<PointerEventData> releasedPointerEventDatas = new List<PointerEventData>();

	public static InputManager gamepadInputManager;

	public static InputType GameInputs => InputType.Camera | InputType.Map;

	public static Vector3 ViewportPosition => viewportPosition;

	public static bool OutsideWindow
	{
		get
		{
			if (!(viewportPosition.x < 0f) && !(viewportPosition.x > 1f) && !(viewportPosition.y < 0f))
			{
				return viewportPosition.y > 1f;
			}
			return true;
		}
	}

	public static List<PolytopiaTouch> CurrentTouches => currentTouches;

	private static Dictionary<InputType, int> Blockers
	{
		get
		{
			if (blockers == null)
			{
				blockers = new Dictionary<InputType, int>
				{
					{
						InputType.Camera,
						0
					},
					{
						InputType.Input,
						0
					},
					{
						InputType.Map,
						0
					},
					{
						InputType.UI,
						0
					},
					{
						InputType.Loading,
						0
					}
				};
			}
			return blockers;
		}
	}

	public void Awake()
	{
	}

	public void OnDestroy()
	{
		if (gamepadInputManager != null)
		{
			gamepadInputManager.Destroy();
		}
		gamepadInputManager = null;
	}

	public void Update()
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (gamepadInputManager != null)
		{
			gamepadInputManager.Update();
		}
		UpdateTouches();
		UpdateReleasedPointerDatas();
		for (int i = 0; i < eatenButtons.Count; i++)
		{
			string text = eatenButtons[i];
			if (!Input.GetButton(text) && !Input.GetButtonUp(text))
			{
				eatenButtons.RemoveAt(i--);
			}
		}
		if (Object.op_Implicit((Object)(object)Camera.main))
		{
			viewportPosition = Camera.main.ScreenToViewportPoint(Vector2.op_Implicit(PolytopiaInput.mousePosition));
		}
		if (IsKeyInputEnabled())
		{
			if (Input.GetButtonDown("Cancel"))
			{
				InputEvents.ButtonDown(Buttons.Cancel);
			}
			if (Input.GetButtonUp("Cancel"))
			{
				InputEvents.ButtonUp(Buttons.Cancel);
			}
			if (Input.GetButtonDown("Accept"))
			{
				InputEvents.ButtonDown(Buttons.Accept);
			}
			else if (Input.GetButtonUp("Accept"))
			{
				InputEvents.ButtonUp(Buttons.Accept);
			}
		}
		if (gamepadInputManager != null)
		{
			CheckPressRelease((LogicalButton)2, Buttons.Cancel);
			CheckPressRelease((LogicalButton)131072, Buttons.EndTurn);
			CheckPressRelease((LogicalButton)8, Buttons.Tech);
			CheckPressRelease((LogicalButton)16, Buttons.GameStats);
			CheckPressRelease((LogicalButton)8192, Buttons.ButtonBarMoveLeft);
			CheckPressRelease((LogicalButton)16384, Buttons.ButtonBarMoveRight);
			CheckPressRelease((LogicalButton)262144, Buttons.Hint);
		}
	}

	private void CheckPressRelease(LogicalButton button, Buttons e)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (gamepadInputManager.IsPressedThisFrame(button))
		{
			InputEvents.ButtonDown(e);
		}
		if (gamepadInputManager.IsReleasedThisFrame(button))
		{
			InputEvents.ButtonUp(e);
		}
	}

	public static void BlockInputIfShowingLoadingScreen()
	{
		if (UIBlackFader.IsShowing())
		{
			DisableInputDuringLoadingScreen();
		}
	}

	public static void DisableInputDuringLoadingScreen()
	{
		DisableInput(InputType.Camera | InputType.Map | InputType.Input | InputType.UI);
	}

	public static void EnableInputDuringLoadingScreen()
	{
		EnableInput(InputType.Camera | InputType.Map | InputType.Input | InputType.UI);
	}

	public static void DisableAllInput()
	{
		EventSystem.current.sendNavigationEvents = false;
		DisableInput(InputType.Camera | InputType.Map | InputType.Input | InputType.UI | InputType.Loading);
	}

	public static void EnableAllInput()
	{
		EnableInput(InputType.Camera | InputType.Map | InputType.Input | InputType.UI | InputType.Loading);
		EventSystem.current.sendNavigationEvents = true;
	}

	public static void EnableInput(InputType input)
	{
		enabledInputs |= input;
		InputEvents.EnabledInputChanged(enabledInputs);
		if ((input & InputType.Camera) != 0)
		{
			Blockers[InputType.Camera] = Mathf.Max(Blockers[InputType.Camera] - 1, 0);
		}
		if ((input & InputType.Input) != 0)
		{
			Blockers[InputType.Input] = Mathf.Max(Blockers[InputType.Input] - 1, 0);
		}
		if ((input & InputType.Map) != 0)
		{
			Blockers[InputType.Map] = Mathf.Max(Blockers[InputType.Map] - 1, 0);
		}
		if ((input & InputType.UI) != 0)
		{
			Blockers[InputType.UI] = Mathf.Max(Blockers[InputType.UI] - 1, 0);
		}
		if ((input & InputType.Loading) != 0)
		{
			Blockers[InputType.Loading] = Mathf.Max(Blockers[InputType.Loading] - 1, 0);
		}
		Log.Verbose("[InputManager] Enable input {0}. Blocking status (Input: {1}, Camera: {2}, Map: {3}, UI {4}, Loading {5})", new object[6]
		{
			input,
			Blockers[InputType.Input],
			Blockers[InputType.Camera],
			Blockers[InputType.Map],
			Blockers[InputType.UI],
			Blockers[InputType.Loading]
		});
		UIManager.UpdateCanvasInteraction();
		UIBlackFader.UpdateCanvasInteraction();
	}

	public static void DisableInput(InputType input)
	{
		enabledInputs &= ~input;
		InputEvents.EnabledInputChanged(enabledInputs);
		if ((input & InputType.Camera) != 0)
		{
			Blockers[InputType.Camera]++;
		}
		if ((input & InputType.Input) != 0)
		{
			Blockers[InputType.Input]++;
		}
		if ((input & InputType.Map) != 0)
		{
			Blockers[InputType.Map]++;
		}
		if ((input & InputType.UI) != 0)
		{
			Blockers[InputType.UI]++;
		}
		if ((input & InputType.Loading) != 0)
		{
			Blockers[InputType.Loading]++;
		}
		Log.Verbose("[InputManager] Disable input {0}. Blocking status (Input: {1}, Camera: {2}, Map: {3}), UI {4}, Loading {5})", new object[6]
		{
			input,
			Blockers[InputType.Input],
			Blockers[InputType.Camera],
			Blockers[InputType.Map],
			Blockers[InputType.UI],
			Blockers[InputType.Loading]
		});
		UIManager.UpdateCanvasInteraction();
		UIBlackFader.UpdateCanvasInteraction();
	}

	public static void ResetInputBlocker()
	{
		Log.Verbose("[InputManager] Resetting input blockers", Array.Empty<object>());
		Blockers[InputType.Camera] = 0;
		Blockers[InputType.Input] = 0;
		Blockers[InputType.Map] = 0;
		Blockers[InputType.UI] = 0;
		Blockers[InputType.Loading] = 0;
		UIManager.UpdateCanvasInteraction();
		UIBlackFader.UpdateCanvasInteraction();
	}

	public static bool IsEnabled(InputType input)
	{
		bool flag = true;
		if ((input & InputType.Camera) != 0)
		{
			flag = flag && Blockers[InputType.Camera] <= 0;
		}
		if ((input & InputType.Input) != 0)
		{
			flag = flag && Blockers[InputType.Input] <= 0;
		}
		if ((input & InputType.Map) != 0)
		{
			flag = flag && Blockers[InputType.Map] <= 0;
		}
		if ((input & InputType.UI) != 0)
		{
			flag = flag && Blockers[InputType.UI] <= 0;
		}
		if ((input & InputType.Loading) != 0)
		{
			flag = flag && Blockers[InputType.Loading] <= 0;
		}
		return flag;
	}

	private static bool IsKeyInputEnabled()
	{
		if (!DebugConsole.IsOpen)
		{
			return IsEnabled(InputType.Input);
		}
		return false;
	}

	public static bool GetKeyDown(KeyCode key)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsKeyInputEnabled())
		{
			return false;
		}
		return Input.GetKeyDown(key);
	}

	public static bool GetKeyUp(KeyCode key)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsKeyInputEnabled())
		{
			return false;
		}
		return Input.GetKeyUp(key);
	}

	public static bool GetKey(KeyCode key)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsKeyInputEnabled())
		{
			return false;
		}
		return Input.GetKey(key);
	}

	public static bool GetButton(string buttonName)
	{
		if (!IsKeyInputEnabled() || eatenButtons.Contains(buttonName))
		{
			return false;
		}
		if (gamepadInputManager != null && buttonName == "Jump" && gamepadInputManager.IsHeldDown((LogicalButton)4))
		{
			return true;
		}
		return Input.GetButton(buttonName);
	}

	public static bool GetButtonDown(string buttonName)
	{
		if (!IsKeyInputEnabled() || eatenButtons.Contains(buttonName))
		{
			return false;
		}
		if (gamepadInputManager != null && buttonName == "Jump" && gamepadInputManager.IsPressedThisFrame((LogicalButton)4))
		{
			return true;
		}
		return Input.GetButtonDown(buttonName);
	}

	public static bool GetButtonUp(string buttonName, bool isUINavigation = false)
	{
		if (((!isUINavigation || !((Object)(object)EventSystem.current != (Object)null) || !EventSystem.current.sendNavigationEvents) && !IsKeyInputEnabled()) || eatenButtons.Contains(buttonName))
		{
			return false;
		}
		if (gamepadInputManager != null && buttonName == "Jump" && gamepadInputManager.IsReleasedThisFrame((LogicalButton)4))
		{
			return true;
		}
		return Input.GetButtonUp(buttonName);
	}

	public static float GetAxis(string axis)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (gamepadInputManager != null)
		{
			if (axis == "SecondaryHorizontal")
			{
				Vector2 secondaryStick = gamepadInputManager.GetSecondaryStick();
				if (secondaryStick.x != 0f)
				{
					return secondaryStick.x;
				}
			}
			else if (axis == "SecondaryVertical")
			{
				Vector2 secondaryStick2 = gamepadInputManager.GetSecondaryStick();
				if (secondaryStick2.y != 0f)
				{
					return secondaryStick2.y;
				}
			}
		}
		if (!IsKeyInputEnabled())
		{
			return 0f;
		}
		return Input.GetAxis(axis);
	}

	public static float GetAxisRaw(string axis)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (gamepadInputManager != null)
		{
			if (axis == "SecondaryHorizontal")
			{
				Vector2 secondaryStick = gamepadInputManager.GetSecondaryStick();
				if (secondaryStick.x != 0f)
				{
					return secondaryStick.x;
				}
			}
			else if (axis == "SecondaryVertical")
			{
				Vector2 secondaryStick2 = gamepadInputManager.GetSecondaryStick();
				if (secondaryStick2.y != 0f)
				{
					return secondaryStick2.y;
				}
			}
		}
		if (!IsKeyInputEnabled())
		{
			return 0f;
		}
		return Input.GetAxisRaw(axis);
	}

	public static void EatButton(string button)
	{
		eatenButtons.Add(button);
	}

	private void AddFakeTouch(int index, bool isTouchEnd, bool isTouchBegin, bool isTouchContinued, bool shouldForceStationary)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		PolytopiaTouch polytopiaTouch;
		if (isTouchEnd)
		{
			Vector2 mousePosition = PolytopiaInput.mousePosition;
			polytopiaTouch = FindOrCreateTouchWithId(index, mousePosition);
			polytopiaTouch.UpdateWithData((TouchPhase)3, mousePosition, mousePosition - polytopiaTouch.position);
		}
		else if (isTouchBegin)
		{
			Vector2 mousePosition2 = PolytopiaInput.mousePosition;
			polytopiaTouch = FindOrCreateTouchWithId(index, mousePosition2);
			polytopiaTouch.UpdateWithData((TouchPhase)0, mousePosition2, new Vector2(0f, 0f));
		}
		else if (isTouchContinued)
		{
			Vector2 val = PolytopiaInput.mousePosition;
			polytopiaTouch = FindOrCreateTouchWithId(index, val);
			if (shouldForceStationary)
			{
				val = polytopiaTouch.position;
			}
			bool flag = polytopiaTouch.position == val;
			polytopiaTouch.UpdateWithData((TouchPhase)((!flag) ? 1 : 2), val, val - polytopiaTouch.position);
		}
		else
		{
			polytopiaTouch = null;
		}
		if (polytopiaTouch != null)
		{
			currentTouches.Add(polytopiaTouch);
		}
	}

	private void AddAltFakeTouch(int index)
	{
		AddFakeTouch(index, GetKeyUp((KeyCode)308), GetKeyDown((KeyCode)308), GetKey((KeyCode)308), shouldForceStationary: true);
	}

	private void AddFakeTouchForMouseButton(int index)
	{
		AddFakeTouch(index, PolytopiaInput.GetMouseButtonUp(index), PolytopiaInput.GetMouseButtonDown(index), PolytopiaInput.GetMouseButton(index), shouldForceStationary: false);
	}

	private PolytopiaTouch FindPreviousTouchWithId(int fingerId)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		for (int i = 0; i < previousTouches.Count; i++)
		{
			PolytopiaTouch polytopiaTouch = previousTouches[i];
			if (polytopiaTouch.fingerId == fingerId && (int)polytopiaTouch.phase != 3 && (int)polytopiaTouch.phase != 4)
			{
				return polytopiaTouch;
			}
		}
		return null;
	}

	private PolytopiaTouch FindOrCreateTouchWithId(int fingerId, Vector2 startPosition)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PolytopiaTouch polytopiaTouch = FindPreviousTouchWithId(fingerId);
		if (polytopiaTouch == null)
		{
			polytopiaTouch = PolytopiaTouch.Create(fingerId, startPosition);
		}
		return polytopiaTouch;
	}

	private void UpdateTouches()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		previousTouches.Clear();
		previousTouches.AddRange(currentTouches);
		currentTouches.Clear();
		Touch[] touches = PolytopiaInput.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			PolytopiaTouch polytopiaTouch = FindOrCreateTouchWithId(((Touch)(ref touch)).fingerId, ((Touch)(ref touch)).position);
			polytopiaTouch.UpdateWithTouch(touch);
			currentTouches.Add(polytopiaTouch);
		}
		if (currentTouches.Count == 0 && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0)))
		{
			AddFakeTouchForMouseButton(0);
		}
	}

	public static Vector2 GetInputPosition()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (PolytopiaInput.touchSupported && PolytopiaInput.touchCount > 0)
		{
			Touch touch = PolytopiaInput.GetTouch(0);
			return ((Touch)(ref touch)).position;
		}
		return PolytopiaInput.mousePosition;
	}

	public void UpdateReleasedPointerDatas()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		releasedPointerEventDatas.Clear();
		if ((Object)(object)EventSystem.current == (Object)null)
		{
			return;
		}
		for (int i = 0; i < GetTouchCount(); i++)
		{
			PolytopiaTouch polytopiaTouch = CurrentTouches[i];
			if ((int)polytopiaTouch.phase == 3 || (int)polytopiaTouch.phase == 4)
			{
				releasedPointerEventDatas.Add(RaycastEventDataAtPosition(polytopiaTouch.position));
			}
		}
	}

	private static PointerEventData RaycastEventDataAtPosition(Vector2 position)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)EventSystem.current == (Object)null)
		{
			return null;
		}
		PointerEventData val = new PointerEventData(EventSystem.current);
		val.position = position;
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(val, list);
		object pointerEnter;
		if (list.Count != 0)
		{
			RaycastResult val2 = list[0];
			pointerEnter = ((RaycastResult)(ref val2)).gameObject;
		}
		else
		{
			pointerEnter = null;
		}
		val.pointerEnter = (GameObject)pointerEnter;
		return val;
	}

	public static bool IsPositionOverUIObject(Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return IsPositionOverUIObject(position, -1);
	}

	public static bool IsPositionOverUIObject(Vector2 position, int excludeLayer = -1)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		PolytopiaInputModule polytopiaInputModule = (PolytopiaInputModule)(object)EventSystem.current.currentInputModule;
		_ = Vector2.zero;
		if ((Object)(object)polytopiaInputModule != (Object)null)
		{
			Vector2 val;
			for (int i = -1; i < 1; i++)
			{
				PointerEventData pointerEventData = polytopiaInputModule.GetPointerEventData(i);
				if (pointerEventData != null)
				{
					_ = pointerEventData.position;
				}
				if (pointerEventData == null)
				{
					continue;
				}
				val = pointerEventData.position - position;
				if (((Vector2)(ref val)).sqrMagnitude < 0.01f && (Object)(object)pointerEventData.pointerEnter != (Object)null && excludeLayer != -1)
				{
					if ((Object)(object)pointerEventData.pointerEnter != (Object)null)
					{
						return pointerEventData.pointerEnter.layer != excludeLayer;
					}
					return false;
				}
			}
			for (int j = 0; j < releasedPointerEventDatas.Count; j++)
			{
				PointerEventData val2 = releasedPointerEventDatas[j];
				val = val2.position - position;
				if (((Vector2)(ref val)).sqrMagnitude < 0.01f)
				{
					if ((Object)(object)val2.pointerEnter != (Object)null)
					{
						return val2.pointerEnter.layer != excludeLayer;
					}
					return false;
				}
			}
		}
		PointerEventData val3 = RaycastEventDataAtPosition(position);
		if ((Object)(object)val3.pointerEnter != (Object)null)
		{
			return val3.pointerEnter.layer != excludeLayer;
		}
		return false;
	}

	public static bool HasTouches()
	{
		return GetTouchCount() > 0;
	}

	public static int GetTouchCount()
	{
		if (currentTouches == null)
		{
			return 0;
		}
		return currentTouches.Count;
	}

	public static bool HasEndedTouch()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		foreach (PolytopiaTouch currentTouch in currentTouches)
		{
			if ((int)currentTouch.phase == 3 || (int)currentTouch.phase == 4)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasNewTouch()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (PolytopiaTouch currentTouch in currentTouches)
		{
			if ((int)currentTouch.phase == 0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasActiveTouch()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I4
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Invalid comparison between Unknown and I4
		foreach (PolytopiaTouch currentTouch in currentTouches)
		{
			if ((int)currentTouch.phase != 3 && (int)currentTouch.phase != 4)
			{
				return true;
			}
		}
		return false;
	}
}
