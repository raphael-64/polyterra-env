using System;
using System.Collections.Generic;
using IchiGamepad;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OmnicursorController
{
	private enum AffixMode
	{
		ScreenCanvas,
		WorldCanvas,
		Tile
	}

	private struct AffixState
	{
		public Transform Transform;

		public AffixMode Mode;

		public Camera WorldCanvasCamera;

		public Canvas Canvas;
	}

	private struct UICandidate
	{
		public Graphic Item;

		public RectTransform ItemTransform;

		public RectTransform ButtonTransform;

		public float DistanceMetric;

		public Canvas Canvas;

		public Camera Camera;

		public IList<Graphic> CanvasItems;
	}

	private class UICandidateComparer : IComparer<UICandidate>
	{
		public int Compare(UICandidate x, UICandidate y)
		{
			return x.DistanceMetric.CompareTo(y.DistanceMetric);
		}
	}

	private Vector2 _position = new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2));

	private Vector2 _simulatedMousePosition = new Vector2((float)(Screen.width / 2), (float)(Screen.height / 2));

	private Vector2 _smoothedDirection;

	private const float stickThreshold = 5f;

	private float _smoothedSpeed;

	private float _controllerSpeedup;

	private bool _wasControllerMoving;

	private bool _isPrimaryButtonDown;

	private bool _isPrimaryButtonUp;

	private bool _isPrimaryButtonHeld;

	private Vector2 _idleSnapStartPosition;

	private float _idleSnapStartTime;

	private List<RaycastResult> _raycastCache = new List<RaycastResult>();

	private List<Transform> _raycastTransformCache = new List<Transform>();

	private List<UICandidate> _uiCandidates = new List<UICandidate>();

	private bool _quickActionsOpen;

	private Transform _lastTileAffix;

	private AffixState? _currentAffix;

	private static readonly Vector3[] CORNERS = (Vector3[])(object)new Vector3[4];

	public bool IsPrimaryButtonDown => _isPrimaryButtonDown;

	public bool IsPrimaryButtonUp => _isPrimaryButtonUp;

	public bool IsPrimaryButtonHeld => _isPrimaryButtonHeld;

	public Vector2 Position => _position;

	public Vector2 SimulatedMousePosition => _simulatedMousePosition;

	public OmnicursorController()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		UIEvents.OnQuickActionsOpen += OnQuickActionsOpen;
	}

	public void AffixToUIElement(RectTransform element)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I4
		Canvas componentInParent = ((Component)element).GetComponentInParent<Canvas>();
		if ((Object)(object)componentInParent == (Object)null)
		{
			Log.Warning("Canvas not found", Array.Empty<object>());
			return;
		}
		InvokeAffixCallback(element);
		if ((int)componentInParent.renderMode == 2)
		{
			_currentAffix = new AffixState
			{
				Transform = (Transform)(object)element,
				Mode = AffixMode.WorldCanvas,
				WorldCanvasCamera = componentInParent.worldCamera,
				Canvas = componentInParent
			};
		}
		else
		{
			_currentAffix = new AffixState
			{
				Transform = (Transform)(object)element,
				Mode = AffixMode.ScreenCanvas,
				Canvas = componentInParent
			};
		}
	}

	public void OverrideToPosition(Vector2 position)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		_currentAffix = null;
		_position = position;
		_simulatedMousePosition = position;
	}

	public bool RestoreTileAffix()
	{
		if (Object.op_Implicit((Object)(object)_lastTileAffix) && (!_currentAffix.HasValue || _currentAffix.Value.Mode != AffixMode.Tile) && !IsOverlayActive())
		{
			_currentAffix = new AffixState
			{
				Mode = AffixMode.Tile,
				Transform = _lastTileAffix
			};
			return true;
		}
		return false;
	}

	public void UpdateControllerOverridePosition(EventSystem eventSystem, Canvas[] canvases)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		//IL_053c: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0544: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Expected O, but got Unknown
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Expected O, but got Unknown
		//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Expected O, but got Unknown
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0515: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.gamepadInputManager == null)
		{
			return;
		}
		_isPrimaryButtonDown = InputManager.gamepadInputManager.IsPressedThisFrame((LogicalButton)1);
		_isPrimaryButtonUp = InputManager.gamepadInputManager.IsReleasedThisFrame((LogicalButton)1);
		_isPrimaryButtonHeld = InputManager.gamepadInputManager.IsHeldDown((LogicalButton)1);
		Vector2 val = InputManager.gamepadInputManager.GetPrimaryStick();
		AnimationCurves sharedAnimationCurves = GameManager.GetSharedAnimationCurves();
		Vector2 zero;
		bool flag;
		Rect val2;
		if (((Vector2)(ref val)).sqrMagnitude <= 0.001f)
		{
			val = Vector2.zero;
			zero = Vector2.zero;
			if (InputManager.gamepadInputManager.IsPressedThisFrame((LogicalButton)128))
			{
				zero.y = -1f;
			}
			else if (InputManager.gamepadInputManager.IsPressedThisFrame((LogicalButton)32))
			{
				zero.y = 1f;
			}
			if (InputManager.gamepadInputManager.IsPressedThisFrame((LogicalButton)256))
			{
				zero.x = -1f;
			}
			else if (InputManager.gamepadInputManager.IsPressedThisFrame((LogicalButton)64))
			{
				zero.x = 1f;
			}
			flag = false;
			if (_currentAffix.HasValue && _currentAffix.Value.Mode == AffixMode.Tile)
			{
				Tile component = ((Component)_currentAffix.Value.Transform).GetComponent<Tile>();
				GridDirection? gridDirection = WorldCoordinates.ToDirection(new WorldCoordinates(Mathf.RoundToInt(zero.y), Mathf.RoundToInt(0f - zero.x)));
				if (gridDirection.HasValue)
				{
					flag = true;
					Tile neighbor = component.GetNeighbor(gridDirection.Value);
					if (Object.op_Implicit((Object)(object)neighbor))
					{
						Camera main = Camera.main;
						HudScreen hud = (HudScreen)UIManager.Instance.GetScreen(UIConstants.Screens.Hud);
						if (!HitTestHud(main, component, hud) && CanClickTile(canvases, main, neighbor))
						{
							val2 = main.pixelRect;
							if (((Rect)(ref val2)).Contains(main.WorldToScreenPoint(Vector2.op_Implicit(component.Coordinates.ToPosition()))))
							{
								LevelManager.GetClientInteraction().SetMarkedTile(neighbor);
								_currentAffix = new AffixState
								{
									Transform = ((Component)neighbor).transform,
									Mode = AffixMode.Tile
								};
								InputEvents.OmnicursorSnapToTile(neighbor);
								AudioManager.PlaySFX(SFXTypes.Snap);
								goto IL_022d;
							}
						}
						flag = false;
					}
				}
			}
			goto IL_022d;
		}
		float magnitude = ((Vector2)(ref val)).magnitude;
		float num = sharedAnimationCurves.stickSensitivityCurve.Evaluate(magnitude) / magnitude;
		val *= num;
		goto IL_0263;
		IL_0263:
		float num2 = (float)Mathf.Max(Screen.width, Screen.height) * 0.08f;
		if (((Vector2)(ref val)).sqrMagnitude <= 0.001f)
		{
			HandleControllerIdle(eventSystem, canvases);
		}
		else if (((Vector2)(ref val)).sqrMagnitude >= 5f)
		{
			bool flag2 = !_wasControllerMoving;
			InputSystemCursorOverride.INSTANCE.UserIsMovingCursor();
			float num3 = _controllerSpeedup * Time.deltaTime * num2;
			if (!_isPrimaryButtonHeld)
			{
				if (flag2)
				{
					_position = _simulatedMousePosition;
					_smoothedDirection = Vector2.op_Implicit(default(Vector3));
					_smoothedSpeed = ((Vector2)(ref val)).magnitude * num3;
					_raycastCache.Clear();
					_raycastTransformCache.Clear();
					PointerEventData val3 = new PointerEventData(eventSystem);
					val3.position = _position;
					eventSystem.RaycastAll(val3, _raycastCache);
					foreach (RaycastResult item in _raycastCache)
					{
						RaycastResult current = item;
						RectTransform val4 = FindSnapTarget(((RaycastResult)(ref current)).gameObject.GetComponent<RectTransform>());
						if (Object.op_Implicit((Object)(object)val4))
						{
							_raycastTransformCache.Add((Transform)(object)val4);
						}
					}
					if (_currentAffix.HasValue && _currentAffix.Value.Mode == AffixMode.Tile)
					{
						_raycastTransformCache.Add(_currentAffix.Value.Transform);
					}
					InputEvents.OmnicursorStartMoving();
				}
				_smoothedDirection = Vector2.op_Implicit(Vector3.Lerp(Vector2.op_Implicit(_smoothedDirection), Vector2.op_Implicit(val), 0.5f));
				_smoothedSpeed = Mathf.Lerp(_smoothedSpeed, ((Vector2)(ref val)).magnitude * num3, 0.95f);
				if (((Vector2)(ref _smoothedDirection)).magnitude >= 0.5f)
				{
					PerformDirectionSnapping(eventSystem, canvases, _smoothedDirection);
				}
				AffixState? currentAffix = _currentAffix;
				if (currentAffix.HasValue)
				{
					AffixState valueOrDefault = currentAffix.GetValueOrDefault();
					if (Object.op_Implicit((Object)(object)valueOrDefault.Transform))
					{
						switch (valueOrDefault.Mode)
						{
						case AffixMode.ScreenCanvas:
							val2 = RectTransformExtensions.WorldRect((RectTransform)valueOrDefault.Transform);
							_simulatedMousePosition = ((Rect)(ref val2)).center;
							break;
						case AffixMode.WorldCanvas:
							val2 = RectTransformExtensions.ScreenRect((RectTransform)valueOrDefault.Transform, valueOrDefault.Canvas);
							_simulatedMousePosition = ((Rect)(ref val2)).center;
							break;
						case AffixMode.Tile:
							if (Object.op_Implicit((Object)(object)MapRenderer.Current))
							{
								_simulatedMousePosition = Vector2.op_Implicit(Camera.main.WorldToScreenPoint(valueOrDefault.Transform.position + Vector3.up * TileTypeYOffset(((Component)valueOrDefault.Transform).GetComponent<Tile>())));
							}
							break;
						}
					}
				}
			}
			_wasControllerMoving = true;
			_position += val * num3;
			_position.x = Mathf.Clamp(_position.x, 0f, (float)Screen.width);
			_position.y = Mathf.Clamp(_position.y, 0f, (float)Screen.height);
			if (!_currentAffix.HasValue)
			{
				_simulatedMousePosition = _position;
			}
			_controllerSpeedup = Mathf.Min(_controllerSpeedup + Time.deltaTime, 2f);
		}
		if (Object.op_Implicit((Object)(object)InputSystemCursorOverride.INSTANCE))
		{
			InputSystemCursorOverride.INSTANCE.SetPosition(Position);
			InputSystemCursorOverride.INSTANCE.SetHover(_currentAffix.HasValue && !_wasControllerMoving && Object.op_Implicit((Object)(object)_currentAffix.Value.Transform));
		}
		return;
		IL_022d:
		if (!flag)
		{
			val = zero * 5.1f;
		}
		goto IL_0263;
	}

	private void HandleControllerIdle(EventSystem eventSystem, Canvas[] canvases)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		_controllerSpeedup = 1f;
		AffixState? currentAffix;
		if (_wasControllerMoving)
		{
			_idleSnapStartPosition = _position;
			_idleSnapStartTime = Time.realtimeSinceStartup;
			currentAffix = _currentAffix;
			if (currentAffix.HasValue)
			{
				AffixState valueOrDefault = currentAffix.GetValueOrDefault();
				if (valueOrDefault.Mode == AffixMode.Tile && Object.op_Implicit((Object)(object)valueOrDefault.Transform))
				{
					InputEvents.OmnicursorSnapToTile(((Component)valueOrDefault.Transform).GetComponent<Tile>());
				}
				else if (valueOrDefault.Mode != AffixMode.Tile)
				{
					InvokeAffixCallback((RectTransform)valueOrDefault.Transform);
				}
			}
			AudioManager.PlaySFX(SFXTypes.Snap);
		}
		currentAffix = _currentAffix;
		if (currentAffix.HasValue)
		{
			AffixState valueOrDefault2 = currentAffix.GetValueOrDefault();
			if (Object.op_Implicit((Object)(object)valueOrDefault2.Transform))
			{
				switch (valueOrDefault2.Mode)
				{
				case AffixMode.ScreenCanvas:
				{
					RectTransform val2 = (RectTransform)valueOrDefault2.Transform;
					if (!((Component)valueOrDefault2.Transform).gameObject.activeInHierarchy)
					{
						PerformDistanceSnapping(eventSystem, canvases, _position);
						break;
					}
					Rect val3 = val2.WorldRect();
					_simulatedMousePosition = ((Rect)(ref val3)).center;
					if (_wasControllerMoving && !valueOrDefault2.Canvas.pixelRect.Contains(val2.WorldRect()))
					{
						ScrollElementIntoView(val2, valueOrDefault2.Canvas);
					}
					break;
				}
				case AffixMode.WorldCanvas:
				{
					if (!((Component)valueOrDefault2.Transform).gameObject.activeInHierarchy)
					{
						PerformDistanceSnapping(eventSystem, canvases, _position);
						break;
					}
					Camera worldCanvasCamera = valueOrDefault2.WorldCanvasCamera;
					Rect val = RectTransformExtensions.WorldRect((RectTransform)valueOrDefault2.Transform);
					_simulatedMousePosition = Vector2.op_Implicit(worldCanvasCamera.WorldToScreenPoint(Vector2.op_Implicit(((Rect)(ref val)).center)));
					break;
				}
				case AffixMode.Tile:
					if (Object.op_Implicit((Object)(object)MapRenderer.Current) && !IsOverlayActive())
					{
						_simulatedMousePosition = Vector2.op_Implicit(Camera.main.WorldToScreenPoint(valueOrDefault2.Transform.position + Vector3.up * TileTypeYOffset(((Component)valueOrDefault2.Transform).GetComponent<Tile>())));
					}
					else
					{
						PerformDistanceSnapping(eventSystem, canvases, _position);
					}
					break;
				}
				goto IL_0256;
			}
		}
		if (Object.op_Implicit((Object)(object)MapRenderer.Current) && !_quickActionsOpen && !RestoreTileAffix())
		{
			PerformDistanceSnapping(eventSystem, canvases, _position);
		}
		goto IL_0256;
		IL_0256:
		if (_smoothedSpeed > 0f)
		{
			_idleSnapStartPosition += _smoothedDirection * _smoothedSpeed;
			_smoothedSpeed *= 0.9f;
			if (_smoothedSpeed < 0.1f)
			{
				_smoothedSpeed = 0f;
			}
		}
		float num = Mathf.InverseLerp(_idleSnapStartTime, _idleSnapStartTime + 0.05f, Time.realtimeSinceStartup);
		num = Mathf.Pow(num, 2f);
		_position = Vector2.Lerp(_idleSnapStartPosition, _simulatedMousePosition, num);
		_wasControllerMoving = false;
	}

	public bool IsCursorAffixedToChildOf(RectTransform transform)
	{
		if (!_currentAffix.HasValue)
		{
			return false;
		}
		AffixState value = _currentAffix.Value;
		switch (value.Mode)
		{
		case AffixMode.ScreenCanvas:
		case AffixMode.WorldCanvas:
		{
			Transform val = value.Transform;
			while ((Object)(object)val != (Object)null)
			{
				if ((Object)(object)val == (Object)(object)transform)
				{
					return true;
				}
				val = val.parent;
			}
			break;
		}
		case AffixMode.Tile:
			return false;
		}
		return false;
	}

	private void ScrollElementIntoView(RectTransform element, Canvas canvas)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		ScrollRect val = null;
		ScrollRect val2 = null;
		Transform parent = ((Transform)element).parent;
		while (Object.op_Implicit((Object)(object)parent))
		{
			ScrollRect component = ((Component)parent).GetComponent<ScrollRect>();
			if (Object.op_Implicit((Object)(object)component))
			{
				if (component.horizontal)
				{
					val2 = component;
				}
				if (component.vertical)
				{
					val = component;
				}
				if (Object.op_Implicit((Object)(object)val2) && Object.op_Implicit((Object)(object)val))
				{
					break;
				}
			}
			parent = parent.parent;
		}
		if (Object.op_Implicit((Object)(object)val) || Object.op_Implicit((Object)(object)val2))
		{
			if ((Object)(object)val2 == (Object)(object)val)
			{
				val = null;
			}
			element.WorldRect();
			_ = canvas.pixelRect;
			ScrollRectHighlightHelper scrollRectHighlightHelper = default(ScrollRectHighlightHelper);
			if (Object.op_Implicit((Object)(object)val) && ((Component)val).TryGetComponent<ScrollRectHighlightHelper>(ref scrollRectHighlightHelper))
			{
				scrollRectHighlightHelper.ScrollToObject(element);
			}
			if (Object.op_Implicit((Object)(object)val2) && ((Component)val2).TryGetComponent<ScrollRectHighlightHelper>(ref scrollRectHighlightHelper))
			{
				scrollRectHighlightHelper.ScrollToObject(element);
			}
		}
	}

	private void PerformDistanceSnapping(EventSystem eventSystem, Canvas[] canvases, Vector2 origin)
	{
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Invalid comparison between Unknown and I4
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		_uiCandidates.Clear();
		float num = float.PositiveInfinity;
		UICandidate uICandidate = default(UICandidate);
		foreach (Canvas val in canvases)
		{
			IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(val);
			Camera worldCamera = val.worldCamera;
			int count = graphicsForCanvas.Count;
			for (int j = 0; j < count; j++)
			{
				Graphic val2 = graphicsForCanvas[j];
				if (!val2.raycastTarget || val2.canvasRenderer.cull || val2.depth == -1 || ((Object)(object)worldCamera != (Object)null && worldCamera.WorldToScreenPoint(((Transform)val2.rectTransform).position).z > worldCamera.farClipPlane))
				{
					continue;
				}
				RectTransform val3 = FindSnapTarget(val2.rectTransform);
				if (!((Object)(object)val3 == (Object)null))
				{
					Rect val4 = val3.ScreenRect(val);
					Vector2 val5 = origin - ((Rect)(ref val4)).center;
					float sqrMagnitude = ((Vector2)(ref val5)).sqrMagnitude;
					UICandidate uICandidate2 = new UICandidate
					{
						Item = val2,
						ItemTransform = val2.rectTransform,
						ButtonTransform = val3,
						DistanceMetric = sqrMagnitude,
						Canvas = val,
						Camera = worldCamera,
						CanvasItems = graphicsForCanvas
					};
					if (IsClickableCandidate(val, worldCamera, graphicsForCanvas, uICandidate2) && sqrMagnitude < num)
					{
						uICandidate = uICandidate2;
						num = sqrMagnitude;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)uICandidate.Item))
		{
			if ((int)uICandidate.Canvas.renderMode == 2)
			{
				_currentAffix = new AffixState
				{
					Mode = AffixMode.WorldCanvas,
					WorldCanvasCamera = uICandidate.Camera,
					Transform = (Transform)(object)uICandidate.ButtonTransform,
					Canvas = uICandidate.Canvas
				};
			}
			else
			{
				_currentAffix = new AffixState
				{
					Mode = AffixMode.ScreenCanvas,
					Transform = (Transform)(object)uICandidate.ButtonTransform,
					Canvas = uICandidate.Canvas
				};
			}
		}
	}

	private void PerformDirectionSnapping(EventSystem eventSystem, Canvas[] canvases, Vector2 direction)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		Ray ray = default(Ray);
		((Ray)(ref ray))._002Ector(Vector2.op_Implicit(_position), Vector2.op_Implicit(direction));
		FindElementInDirection(canvases, ray, _raycastTransformCache, out var _, out var go, out var foundCanvas);
		if (!Object.op_Implicit((Object)(object)go))
		{
			return;
		}
		Tile tile = default(Tile);
		if (go.TryGetComponent<Tile>(ref tile))
		{
			LevelManager.GetClientInteraction().SetMarkedTile(tile, revealShouldAccountForHUD: false);
			_currentAffix = new AffixState
			{
				Mode = AffixMode.Tile,
				Transform = go.transform
			};
			_lastTileAffix = go.transform;
			return;
		}
		if ((int)foundCanvas.renderMode == 2)
		{
			_currentAffix = new AffixState
			{
				Mode = AffixMode.WorldCanvas,
				WorldCanvasCamera = foundCanvas.worldCamera,
				Transform = go.transform,
				Canvas = foundCanvas
			};
		}
		else
		{
			_currentAffix = new AffixState
			{
				Mode = AffixMode.ScreenCanvas,
				Transform = go.transform,
				Canvas = foundCanvas
			};
		}
		if (Object.op_Implicit((Object)(object)MapRenderer.Current))
		{
			LevelManager.GetClientInteraction().SetMarkedTile(null, revealShouldAccountForHUD: false);
		}
	}

	private static RectTransform FindSnapTarget(RectTransform hit)
	{
		CachedOmnicursorSnapTarget cachedOmnicursorSnapTarget = default(CachedOmnicursorSnapTarget);
		if (((Component)hit).TryGetComponent<CachedOmnicursorSnapTarget>(ref cachedOmnicursorSnapTarget))
		{
			return cachedOmnicursorSnapTarget.cachedSnapTarget;
		}
		RectTransform initial = hit;
		bool hasIgnoreComponent = false;
		IgnoredByOmnicursorRaycast ignoredByOmnicursorRaycast = default(IgnoredByOmnicursorRaycast);
		Slider val = default(Slider);
		PolytopiaInputTextField polytopiaInputTextField = default(PolytopiaInputTextField);
		while (Object.op_Implicit((Object)(object)hit))
		{
			bool flag = false;
			if (((Component)hit).TryGetComponent<IgnoredByOmnicursorRaycast>(ref ignoredByOmnicursorRaycast))
			{
				hasIgnoreComponent = true;
				flag = ignoredByOmnicursorRaycast.ignore;
			}
			if (!flag)
			{
				if (Object.op_Implicit((Object)(object)((Component)hit).GetComponent<UIButtonBase>()) || Object.op_Implicit((Object)(object)((Component)hit).GetComponent<CustomOmnicursorSnapTarget>()))
				{
					CacheSnapTarget(initial, hit, hasIgnoreComponent);
					return hit;
				}
				if (((Component)hit).TryGetComponent<Slider>(ref val))
				{
					CacheSnapTarget(initial, val.handleRect, hasIgnoreComponent);
					return val.handleRect;
				}
				if (((Component)hit).TryGetComponent<PolytopiaInputTextField>(ref polytopiaInputTextField))
				{
					CacheSnapTarget(initial, hit, hasIgnoreComponent);
					return hit;
				}
			}
			Transform parent = ((Transform)hit).parent;
			hit = ((parent != null) ? ((Component)parent).GetComponent<RectTransform>() : null);
			if (!Object.op_Implicit((Object)(object)hit))
			{
				break;
			}
		}
		CacheSnapTarget(initial, null, hasIgnoreComponent);
		return null;
	}

	private bool IsOverlayActive()
	{
		UIScreenBase currentScreen = UIManager.Instance.GetCurrentScreen();
		if (!UIBlackFader.IsShowing() && !ResultScreen.IsShowing() && !PopupManager.IsUnskippablePopupShowing())
		{
			if ((Object)(object)currentScreen != (Object)null)
			{
				return currentScreen.screenType != UIConstants.Screens.Hud;
			}
			return false;
		}
		return true;
	}

	private static void CacheSnapTarget(RectTransform initial, RectTransform target, bool hasIgnoreComponent)
	{
		if (!hasIgnoreComponent)
		{
			((Component)initial).gameObject.AddComponent<CachedOmnicursorSnapTarget>().cachedSnapTarget = target;
		}
	}

	private void FindElementInDirection(Canvas[] canvases, Ray ray, List<Transform> exclude, out Vector2 screenPosition, out GameObject go, out Canvas foundCanvas)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		RefreshUICandidates(canvases, ray, exclude, out var closestTransform, out var closestDist, out var closestTransformCamera, out var closestCanvas);
		UIManager.Instance.GetCurrentScreen();
		if (Object.op_Implicit((Object)(object)MapRenderer.Current) && !IsOverlayActive())
		{
			Camera main = Camera.main;
			ClosestTile(canvases, main, main.pixelRect, ray, exclude, out var closestDist2, out var closestTile);
			if (closestDist2 < closestDist && Object.op_Implicit((Object)(object)closestTile))
			{
				Vector3 val = main.WorldToScreenPoint(Vector2.op_Implicit(closestTile.Coordinates.ToPosition() + Vector2.up * TileTypeYOffset(closestTile)));
				go = ((Component)closestTile).gameObject;
				screenPosition = Vector2.op_Implicit(val);
				foundCanvas = null;
				return;
			}
		}
		if (Object.op_Implicit((Object)(object)closestTransform))
		{
			RectTransform obj = closestTransform;
			Rect rect = closestTransform.rect;
			screenPosition = Vector2.op_Implicit(((Transform)obj).TransformPoint(Vector2.op_Implicit(((Rect)(ref rect)).center)));
			if (Object.op_Implicit((Object)(object)closestTransformCamera))
			{
				screenPosition = Vector2.op_Implicit(closestTransformCamera.WorldToScreenPoint(Vector2.op_Implicit(screenPosition)));
			}
			go = ((Component)closestTransform).gameObject;
			foundCanvas = closestCanvas;
		}
		else
		{
			screenPosition = default(Vector2);
			go = null;
			foundCanvas = null;
		}
	}

	private void RefreshUICandidates(Canvas[] canvases, Ray ray, List<Transform> exclude, out RectTransform closestTransform, out float closestDist, out Camera closestTransformCamera, out Canvas closestCanvas)
	{
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		_uiCandidates.Clear();
		foreach (Canvas val in canvases)
		{
			IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(val);
			Camera worldCamera = val.worldCamera;
			int count = graphicsForCanvas.Count;
			for (int j = 0; j < count; j++)
			{
				Graphic val2 = graphicsForCanvas[j];
				if (!val2.raycastTarget || val2.canvasRenderer.cull || val2.depth == -1 || ((Object)(object)worldCamera != (Object)null && worldCamera.WorldToScreenPoint(((Transform)val2.rectTransform).position).z > worldCamera.farClipPlane))
				{
					continue;
				}
				RectTransform val3 = FindSnapTarget(val2.rectTransform);
				if ((Object)(object)val3 == (Object)null || (exclude != null && exclude.Contains((Transform)(object)val3)))
				{
					continue;
				}
				float sqrDistance;
				Vector2 val4 = ClosestPointToTransform(ray, val, val3, out sqrDistance);
				if (sqrDistance != float.PositiveInfinity)
				{
					float num = Vector2.Angle(Vector2.op_Implicit(((Ray)(ref ray)).direction), val4 - Vector2.op_Implicit(((Ray)(ref ray)).origin));
					sqrDistance = Mathf.Sqrt(sqrDistance);
					sqrDistance *= Mathf.Clamp(num / 180f, 0.1f, 1f);
					if (!(num >= 30f) && !((Object)(object)val3 == (Object)null) && (exclude == null || !exclude.Contains((Transform)(object)val3)))
					{
						_uiCandidates.Add(new UICandidate
						{
							Item = val2,
							ItemTransform = val2.rectTransform,
							ButtonTransform = val3,
							DistanceMetric = sqrDistance,
							Canvas = val,
							Camera = worldCamera,
							CanvasItems = graphicsForCanvas
						});
					}
				}
			}
		}
		closestTransform = null;
		closestDist = float.PositiveInfinity;
		closestTransformCamera = null;
		closestCanvas = null;
		if (_uiCandidates.Count <= 0)
		{
			return;
		}
		_uiCandidates.Sort(new UICandidateComparer());
		foreach (UICandidate uiCandidate in _uiCandidates)
		{
			if (IsClickableCandidate(uiCandidate.Canvas, uiCandidate.Camera, uiCandidate.CanvasItems, uiCandidate))
			{
				closestTransform = uiCandidate.ButtonTransform;
				closestDist = uiCandidate.DistanceMetric;
				closestCanvas = uiCandidate.Canvas;
				if ((int)uiCandidate.Canvas.renderMode == 2)
				{
					closestTransformCamera = uiCandidate.Camera;
				}
				break;
			}
		}
	}

	private static bool IsClickableCandidate(Canvas canvas, Camera camera, IList<Graphic> items, UICandidate candidate)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Rect val = candidate.ItemTransform.ScreenRect(canvas);
		Vector2 center = ((Rect)(ref val)).center;
		int depth = candidate.Item.depth;
		int count = items.Count;
		for (int i = 0; i < count; i++)
		{
			Graphic val2 = items[i];
			if (val2.depth != -1 && val2.depth >= depth && val2.raycastTarget && !val2.canvasRenderer.cull && !((Object)(object)val2 == (Object)(object)candidate.Item) && !((Object)(object)FindSnapTarget(val2.rectTransform) == (Object)(object)candidate.ButtonTransform))
			{
				val = val2.rectTransform.ScreenRect(canvas);
				if (((Rect)(ref val)).Contains(center) && val2.Raycast(center, camera))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static float TileTypeYOffset(Tile tile)
	{
		TerrainData.Type terrain = tile.Data.terrain;
		if ((uint)(terrain - 3) <= 3u)
		{
			return 0.08f;
		}
		return 0f;
	}

	private static bool CanClickTile(Canvas[] canvases, Camera camera, Tile tile)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Invalid comparison between Unknown and I4
		Vector3 val = camera.WorldToScreenPoint(Vector2.op_Implicit(tile.Coordinates.ToPosition()));
		IgnoredByOmnicursorRaycast ignoredByOmnicursorRaycast = default(IgnoredByOmnicursorRaycast);
		foreach (Canvas val2 in canvases)
		{
			IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(val2);
			int count = graphicsForCanvas.Count;
			for (int j = 0; j < count; j++)
			{
				Graphic val3 = graphicsForCanvas[j];
				if (val3.depth != -1 && val3.raycastTarget && !val3.canvasRenderer.cull)
				{
					Rect val4 = val3.rectTransform.ScreenRect(val2);
					if (((Rect)(ref val4)).Contains(val) && (!((Component)val3).TryGetComponent<IgnoredByOmnicursorRaycast>(ref ignoredByOmnicursorRaycast) || !ignoredByOmnicursorRaycast.ignore) && val3.Raycast(Vector2.op_Implicit(val), ((int)val2.renderMode == 2) ? val2.worldCamera : null))
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	private void OnQuickActionsOpen(WorldCoordinates coordinates, bool open)
	{
		_quickActionsOpen = open;
	}

	private static Vector2 ClosestPointToTransform(Ray ray, Canvas canvas, RectTransform transform, out float sqrDistance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Ray)(ref ray)).origin + ((Ray)(ref ray)).direction * 5000f;
		transform.GetWorldCorners(CORNERS);
		Vector3 val2 = CORNERS[0];
		Vector3 val3 = CORNERS[1];
		Vector3 val4 = CORNERS[2];
		Vector3 val5 = CORNERS[3];
		if ((int)canvas.renderMode == 2)
		{
			val2 = canvas.worldCamera.WorldToScreenPoint(val2);
			val3 = canvas.worldCamera.WorldToScreenPoint(val3);
			val4 = canvas.worldCamera.WorldToScreenPoint(val4);
			val5 = canvas.worldCamera.WorldToScreenPoint(val5);
		}
		Vector2? val6 = null;
		float num = float.PositiveInfinity;
		Vector2 result;
		if (Test2DSegmentSegment(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val2), Vector2.op_Implicit(val3), out var t, out var p))
		{
			val6 = p;
			result = p - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			num = ((Vector2)(ref result)).sqrMagnitude;
		}
		if (Test2DSegmentSegment(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val3), Vector2.op_Implicit(val4), out t, out p))
		{
			result = p - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			float sqrMagnitude = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				val6 = p;
				num = sqrMagnitude;
			}
		}
		if (Test2DSegmentSegment(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val4), Vector2.op_Implicit(val5), out t, out p))
		{
			result = p - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			float sqrMagnitude2 = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude2 < num)
			{
				val6 = p;
				num = sqrMagnitude2;
			}
		}
		if (Test2DSegmentSegment(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val5), Vector2.op_Implicit(val2), out t, out p))
		{
			result = p - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			float sqrMagnitude3 = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude3 < num)
			{
				val6 = p;
				num = sqrMagnitude3;
			}
		}
		if (!val6.HasValue)
		{
			ClosestPointsBetweenLineSegments(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val2), Vector2.op_Implicit(val3), out t, out var t2, out p, out var end);
			result = end - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			float sqrMagnitude4 = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude4 < num)
			{
				val6 = end;
				num = sqrMagnitude4;
			}
			ClosestPointsBetweenLineSegments(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val3), Vector2.op_Implicit(val4), out t, out t2, out p, out end);
			result = end - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			sqrMagnitude4 = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude4 < num)
			{
				val6 = end;
				num = sqrMagnitude4;
			}
			ClosestPointsBetweenLineSegments(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val4), Vector2.op_Implicit(val5), out t, out t2, out p, out end);
			result = end - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			sqrMagnitude4 = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude4 < num)
			{
				val6 = end;
				num = sqrMagnitude4;
			}
			ClosestPointsBetweenLineSegments(Vector2.op_Implicit(((Ray)(ref ray)).origin), Vector2.op_Implicit(val), Vector2.op_Implicit(val5), Vector2.op_Implicit(val2), out t, out t2, out p, out end);
			result = end - Vector2.op_Implicit(((Ray)(ref ray)).origin);
			sqrMagnitude4 = ((Vector2)(ref result)).sqrMagnitude;
			if (sqrMagnitude4 < num)
			{
				val6 = end;
				num = sqrMagnitude4;
			}
		}
		if (val6.HasValue)
		{
			sqrDistance = num;
			return val6.Value;
		}
		sqrDistance = float.PositiveInfinity;
		result = default(Vector2);
		return result;
	}

	private static void ClosestTile(Canvas[] canvases, Camera camera, Rect pixelRect, Ray ray, List<Transform> exclude, out float closestDist, out Tile closestTile)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2.op_Implicit(camera.ScreenToWorldPoint(((Ray)(ref ray)).origin));
		WorldCoordinates worldCoordinates = val.ToWorldCoordinates();
		Tile tileInstance = MapRenderer.Current.GetTileInstance(worldCoordinates);
		HudScreen hud = (HudScreen)UIManager.Instance.GetScreen(UIConstants.Screens.Hud);
		if (Object.op_Implicit((Object)(object)tileInstance) && !HitTestHud(camera, tileInstance, hud) && CanClickTile(canvases, camera, tileInstance) && (exclude == null || !exclude.Contains(((Component)tileInstance).transform)))
		{
			Rect val2 = default(Rect);
			((Rect)(ref val2))._002Ector(Vector2.zero, new Vector2(0.9622f, 0.576f) * 0.7f);
			((Rect)(ref val2)).center = Vector2.op_Implicit(((Component)tileInstance).transform.position);
			if (((Rect)(ref val2)).Contains(val))
			{
				Vector3 val3 = camera.WorldToScreenPoint(Vector2.op_Implicit(worldCoordinates.ToPosition())) - ((Ray)(ref ray)).origin;
				closestDist = 0.1f * ((Vector3)(ref val3)).magnitude;
				closestTile = tileInstance;
				return;
			}
		}
		closestDist = float.PositiveInfinity;
		closestTile = null;
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				DistanceMetricToTile(camera, ray, worldCoordinates + new WorldCoordinates(i, j), out var distance, out var tile);
				if (distance < closestDist && !HitTestHud(camera, tile, hud) && CanClickTile(canvases, camera, tile) && ((Rect)(ref pixelRect)).Contains(camera.WorldToScreenPoint(Vector2.op_Implicit(tile.Coordinates.ToPosition()))) && (exclude == null || !exclude.Contains(((Component)tile).transform)))
				{
					closestDist = distance;
					closestTile = tile;
				}
			}
		}
	}

	private static bool HitTestHud(Camera camera, Tile tile, HudScreen hud)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = camera.WorldToScreenPoint(Vector2.op_Implicit(tile.Coordinates.ToPosition()));
		Rect val2 = hud.InteractionBar.rectTransform.WorldRect();
		if (((Rect)(ref val2)).Contains(val))
		{
			return true;
		}
		Rect val3 = hud.buttonBar.rectTransform.WorldRect();
		if (((Rect)(ref val3)).Contains(val))
		{
			Rect? val4 = null;
			foreach (RectTransform item in (Transform)hud.buttonBar.rectTransform)
			{
				Rect value = RectTransformExtensions.WorldRect(item);
				if (val4.HasValue)
				{
					Rect value2 = val4.Value;
					((Rect)(ref value2)).xMin = Mathf.Min(((Rect)(ref value2)).xMin, ((Rect)(ref value)).xMin);
					((Rect)(ref value2)).yMin = Mathf.Min(((Rect)(ref value2)).yMin, ((Rect)(ref value)).yMin);
					((Rect)(ref value2)).xMax = Mathf.Max(((Rect)(ref value2)).xMax, ((Rect)(ref value)).xMax);
					((Rect)(ref value2)).yMax = Mathf.Max(((Rect)(ref value2)).yMax, ((Rect)(ref value)).yMax);
					val4 = value2;
				}
				else
				{
					val4 = value;
				}
			}
			Rect value3 = val4.Value;
			if (val.x >= ((Rect)(ref value3)).xMin)
			{
				return val.x <= ((Rect)(ref value3)).xMax;
			}
			return false;
		}
		return false;
	}

	private static void DistanceMetricToTile(Camera camera, Ray ray, WorldCoordinates coords, out float distance, out Tile tile)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		tile = MapRenderer.Current.GetTileInstance(coords);
		if (!Object.op_Implicit((Object)(object)tile))
		{
			distance = float.PositiveInfinity;
			return;
		}
		Vector3 val = camera.WorldToScreenPoint(Vector2.op_Implicit(tile.Coordinates.ToPosition())) - ((Ray)(ref ray)).origin;
		float num = Vector2.Angle(Vector2.op_Implicit(((Ray)(ref ray)).direction), Vector2.op_Implicit(val));
		if (Mathf.Abs(num) > 40f)
		{
			distance = float.PositiveInfinity;
		}
		else
		{
			distance = ((Vector3)(ref val)).magnitude * Mathf.Clamp(num / 180f, 0.07f, 1f);
		}
	}

	private static float Signed2DTriArea(Vector2 a, Vector2 b, Vector2 c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return (a.x - c.x) * (b.y - c.y) - (a.y - c.y) * (b.x - c.x);
	}

	private static bool Test2DSegmentSegment(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out float t, out Vector2 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		float num = Signed2DTriArea(a, b, d);
		float num2 = Signed2DTriArea(a, b, c);
		if (num * num2 < 0f)
		{
			float num3 = Signed2DTriArea(c, d, a);
			float num4 = num3 + num2 - num;
			if (num3 * num4 < 0f)
			{
				t = num3 / (num3 - num4);
				p = a + t * (b - a);
				return true;
			}
		}
		t = 0f;
		p = default(Vector2);
		return false;
	}

	private static float ClosestPointsBetweenLineSegments(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out float t1, out float t2, out Vector2 start3, out Vector2 end3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = end1 - start1;
		Vector2 val2 = end2 - start2;
		Vector2 val3 = start1 - start2;
		float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
		float sqrMagnitude2 = ((Vector2)(ref val2)).sqrMagnitude;
		float num = Vector2.Dot(val2, val3);
		Vector2 val4;
		if (Mathf.Approximately(sqrMagnitude, 0f) && Mathf.Approximately(sqrMagnitude2, 0f))
		{
			t1 = (t2 = 0f);
			start3 = start1;
			end3 = end1;
			val4 = end3 - start3;
			return ((Vector2)(ref val4)).sqrMagnitude;
		}
		if (Mathf.Approximately(sqrMagnitude, 0f))
		{
			t1 = 0f;
			t2 = Mathf.Clamp01(num / sqrMagnitude2);
		}
		else if (Mathf.Approximately(sqrMagnitude2, 0f))
		{
			float num2 = Vector2.Dot(val, val3);
			t2 = 0f;
			t1 = Mathf.Clamp01((0f - num2) / sqrMagnitude);
		}
		else
		{
			float num3 = Vector2.Dot(val, val2);
			float num4 = Vector2.Dot(val, val3);
			float num5 = sqrMagnitude * sqrMagnitude2 - num3 * num3;
			if (num5 != 0f)
			{
				t1 = Mathf.Clamp01((num3 * num - num4 * sqrMagnitude2) / num5);
			}
			else
			{
				t1 = 0f;
			}
			t2 = (num3 * t1 + num) / sqrMagnitude2;
			if (t2 < 0f)
			{
				t2 = 0f;
				t1 = Mathf.Clamp01((0f - num4) / sqrMagnitude);
			}
			else if (t2 > 1f)
			{
				t2 = 1f;
				t1 = Mathf.Clamp01((num3 - num4) / sqrMagnitude);
			}
		}
		start3 = Vector2.LerpUnclamped(start1, end1, t1);
		end3 = Vector2.LerpUnclamped(start2, end2, t2);
		val4 = end3 - start3;
		return ((Vector2)(ref val4)).sqrMagnitude;
	}

	private static void InvokeAffixCallback(RectTransform obj)
	{
		if (Object.op_Implicit((Object)(object)obj))
		{
			InputEvents.OmnicursorSnapToUIElement(obj);
			IOmnicursorAffixCallback[] components = ((Component)obj).GetComponents<IOmnicursorAffixCallback>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnOmnicursorAffixToGameObject();
			}
		}
	}
}
