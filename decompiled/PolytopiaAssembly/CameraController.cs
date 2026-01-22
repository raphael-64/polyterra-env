using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using IchiGamepad;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	private const float MIN_SPEED_THRESHOLD = 0.047348484f;

	private const float DEFAULT_ZOOM = 2f;

	private static CameraController instance;

	[SerializeField]
	private Camera camera;

	[Header("Camera Settings")]
	[SerializeField]
	[Tooltip("Move speed when fully zoomed in")]
	private float moveSpeedMinZoom = 5f;

	[SerializeField]
	[Tooltip("Move speed when fully zoomed out")]
	private float moveSpeedMaxZoom = 1f;

	[SerializeField]
	[Tooltip("Viewport size when fully zoomed in")]
	private float minZoom = 2f;

	[SerializeField]
	[Tooltip("Viewport size when fully zoomed out")]
	private float maxZoom = 12f;

	[SerializeField]
	private float elasticity = 0.1f;

	[SerializeField]
	private float decelarationRate = 0.135f;

	[SerializeField]
	private float stoppingThreshold = 0.0001f;

	[Range(0f, 1f)]
	public float zoom;

	[Header("Tech view camera settings")]
	[SerializeField]
	private Bounds techViewBounds;

	private Vector3 worldPositionOnStart;

	private Vector2 startPosition;

	private Vector2 panningDelta;

	private Vector2 currentPosition;

	private Vector2 zoomFocus;

	private Vector2 lastCameraFocus;

	private bool isPanning;

	private bool isDragging;

	private bool hasTouch;

	private bool isAutoFocusEnabled = true;

	private int touchCount;

	private Vector3 previousPosition;

	private Vector2 velocity = Vector2.zero;

	private Bounds scrollBounds;

	private Vector2? touch0PreviousScreen;

	private Vector2? touch1PreviousScreen;

	private Tween centerCameraTween;

	private Transform inputQuad;

	private Transform mapTargetQuad;

	private Transform fixedInputQuad;

	private bool isTechViewEnabled;

	private Vector3 cameraPositionBeforeTech = Vector3.zero;

	private float cameraZoomBeforeTech = 2f;

	public static CameraController Instance => instance;

	public static bool IsDragging
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)Instance))
			{
				return false;
			}
			return Instance.isDragging;
		}
	}

	public static bool IsBlocked
	{
		get
		{
			if (InputManager.IsEnabled(InputManager.InputType.Camera))
			{
				return DebugConsole.IsOpen;
			}
			return true;
		}
	}

	public bool ScrollButtonPress
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			if (!InputManager.HasTouches())
			{
				return false;
			}
			return (int)InputManager.CurrentTouches[0].phase == 0;
		}
	}

	public bool ScrollButtonRelease
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Invalid comparison between Unknown and I4
			if (!InputManager.HasTouches())
			{
				return false;
			}
			if ((int)InputManager.CurrentTouches[0].phase != 3)
			{
				return (int)InputManager.CurrentTouches[0].phase == 4;
			}
			return true;
		}
	}

	public bool ScrollButtonDown
	{
		get
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Invalid comparison between Unknown and I4
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Invalid comparison between Unknown and I4
			if (IsBlocked)
			{
				return false;
			}
			if (!InputManager.HasTouches())
			{
				return false;
			}
			if ((int)InputManager.CurrentTouches[0].phase != 1)
			{
				return (int)InputManager.CurrentTouches[0].phase == 2;
			}
			return true;
		}
	}

	public static Camera Camera => instance.camera;

	public static float NormalizedZoom
	{
		get
		{
			if (!((Object)(object)instance != (Object)null))
			{
				return 2f;
			}
			return instance.zoom;
		}
	}

	public static float CurrentZoom => instance.minZoom / (Mathf.Lerp(instance.minZoom, instance.maxZoom, instance.zoom) * UICanvasScalerHelper.GetInvertedUIScale());

	public static WorldCoordinates Coordinates
	{
		get
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)instance == (Object)null)
			{
				return WorldCoordinates.NULL_COORDINATES;
			}
			return MapExtensions.ToWorldCoordinates(new Vector2(((Component)instance).transform.position.x, ((Component)instance).transform.position.y));
		}
	}

	public static float MinimumMovementThreshold => ScalingUtils.ScaledDragThreshold();

	private void Awake()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		instance = this;
		lastCameraFocus = Vector2.zero;
		if (!Object.op_Implicit((Object)(object)camera))
		{
			camera = ((Component)this).GetComponent<Camera>();
		}
		zoom = 0f;
		UpdateCameraSize();
		previousPosition = ((Component)this).transform.localPosition;
		isTechViewEnabled = false;
		SunriseBg.Refresh();
		WorldMouseCatcher.Refresh();
	}

	private void OnDestroy()
	{
		if (centerCameraTween != null && !centerCameraTween.active)
		{
			TweenUtils.KillTween(centerCameraTween);
		}
	}

	private void Update()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!IsBlocked)
		{
			UpdateInput();
			bool isZoomFromGamepad;
			float zoomDelta = GetZoomDelta(out isZoomFromGamepad);
			if (zoomDelta != 0f && (!InputManager.OutsideWindow || isZoomFromGamepad))
			{
				AdjustZoom(Vector2.op_Implicit(zoomFocus), zoomDelta);
			}
			else
			{
				UpdateCameraSize();
			}
			currentPosition = GetCurrentPosition();
			UpdateBounds();
			UpdatePosition();
		}
	}

	private void LateUpdate()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if (IsBlocked || ((Bounds)(ref scrollBounds)).size == Vector3.zero)
		{
			return;
		}
		Vector2 scrollOffset = GetScrollOffset(Vector2.op_Implicit(((Component)this).transform.localPosition));
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (!isDragging && (scrollOffset != Vector2.zero || velocity != Vector2.zero))
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			for (int i = 0; i < 2; i++)
			{
				if (((Vector2)(ref scrollOffset))[i] != 0f)
				{
					float num = ((Vector2)(ref velocity))[i];
					int num2 = i;
					Vector3 localPosition2 = ((Component)this).transform.localPosition;
					float num3 = ((Vector3)(ref localPosition2))[i];
					localPosition2 = ((Component)this).transform.localPosition;
					((Vector3)(ref localPosition))[num2] = Mathf.SmoothDamp(num3, ((Vector3)(ref localPosition2))[i] + ((Vector2)(ref scrollOffset))[i], ref num, elasticity, float.PositiveInfinity, unscaledDeltaTime);
					((Vector2)(ref velocity))[i] = num;
					continue;
				}
				ref Vector2 reference = ref velocity;
				int num4 = i;
				((Vector2)(ref reference))[num4] = ((Vector2)(ref reference))[num4] * Mathf.Pow(decelarationRate, unscaledDeltaTime);
				if (Mathf.Abs(((Vector2)(ref velocity))[i]) < stoppingThreshold)
				{
					((Vector2)(ref velocity))[i] = 0f;
				}
				num4 = i;
				((Vector3)(ref localPosition))[num4] = ((Vector3)(ref localPosition))[num4] + ((Vector2)(ref velocity))[i] * unscaledDeltaTime;
			}
			if (velocity != Vector2.zero)
			{
				((Component)this).transform.localPosition = localPosition;
			}
		}
		if (isDragging)
		{
			velocity = Vector2.Lerp(velocity, Vector2.op_Implicit((((Component)this).transform.localPosition - previousPosition) / unscaledDeltaTime), unscaledDeltaTime * 10f);
		}
		previousPosition = ((Component)this).transform.localPosition;
	}

	private void UpdateBounds()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		scrollBounds = (isTechViewEnabled ? GetTechCameraBounds() : GetWorldCameraBounds());
	}

	private void NullVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		velocity = Vector2.zero;
	}

	private void AdjustZoom(Vector3 target, float delta)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = camera.WorldToViewportPoint(target);
		float num = Mathf.Lerp(minZoom, maxZoom, zoom) * (1f + delta);
		zoom = Mathf.Clamp01(Mathf.InverseLerp(minZoom, maxZoom, num));
		UpdateCameraSize();
		Vector3 val2 = camera.ViewportToWorldPoint(val);
		target.z = 0f;
		val2.z = 0f;
		Transform transform = ((Component)this).transform;
		transform.position += target - val2;
		AudioManager.SetZoomLevel(zoom);
		SunriseBg.Refresh();
		WorldMouseCatcher.Refresh();
	}

	private void UpdateCameraSize()
	{
		float num = Mathf.Lerp(minZoom, maxZoom, zoom) * UICanvasScalerHelper.GetInvertedUIScale();
		camera.orthographicSize = num * ((float)Screen.height / 1056f);
	}

	private void UpdatePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.localPosition;
		if (isDragging)
		{
			Vector3 val2 = camera.ScreenToWorldPoint(Vector2.op_Implicit(currentPosition));
			Vector3 val3 = val + (worldPositionOnStart - val2);
			val3.z = val.z;
			Vector3 val4 = Vector2.op_Implicit(GetScrollOffset(Vector2.op_Implicit(val3)));
			val = val3 + val4;
			if (val4.x != 0f)
			{
				val.x -= GetRubberDelta(val4.x, ((Bounds)(ref scrollBounds)).size.x);
			}
			if (val4.y != 0f)
			{
				val.y -= GetRubberDelta(val4.y, ((Bounds)(ref scrollBounds)).size.y);
			}
		}
		else if (isPanning)
		{
			Vector3 val5 = new Vector3(panningDelta.x, panningDelta.y, 0f);
			Vector3 normalized = ((Vector3)(ref val5)).normalized;
			float num = Mathf.Max(Mathf.Abs(panningDelta.x), Mathf.Abs(panningDelta.y));
			float num2 = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * num * Time.unscaledDeltaTime;
			Vector3 val6 = val + normalized * num2;
			Vector3 val7 = Vector2.op_Implicit(GetScrollOffset(Vector2.op_Implicit(val6)));
			val = val6 + val7;
		}
		((Component)this).transform.localPosition = val;
	}

	private Vector3 ScreenToWorldPointWithoutRubberBanding(Vector3 currentPosition)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = camera.ScreenToWorldPoint(currentPosition);
		Vector3 localPosition = ((Component)this).transform.localPosition;
		Vector3 val2 = Vector2.op_Implicit(GetScrollOffset(Vector2.op_Implicit(localPosition)));
		float num = ((val2.x != 0f) ? GetInverseRubberDelta(val2.x, ((Bounds)(ref scrollBounds)).size.x) : 0f);
		float num2 = ((val2.y != 0f) ? GetInverseRubberDelta(val2.y, ((Bounds)(ref scrollBounds)).size.y) : 0f);
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(num, num2, 0f);
		return val + val2 - val3;
	}

	private void UpdateInput()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		currentPosition = GetCurrentPosition();
		int num = InputManager.GetTouchCount();
		if (touchCount != num && num >= 1)
		{
			startPosition = currentPosition;
			worldPositionOnStart = ScreenToWorldPointWithoutRubberBanding(Vector2.op_Implicit(currentPosition));
			NullVelocity();
			if (num > 1 && !isDragging)
			{
				hasTouch = true;
				isDragging = true;
				isPanning = false;
			}
		}
		if (InputManager.HasActiveTouch())
		{
			bool flag = (hasTouch = !InputManager.IsPositionOverUIObject(startPosition, 9) || isTechViewEnabled);
			Vector2 val = currentPosition - startPosition;
			if (((Vector2)(ref val)).magnitude > MinimumMovementThreshold && !isDragging && flag)
			{
				isDragging = true;
				isPanning = false;
				startPosition = currentPosition;
				worldPositionOnStart = ScreenToWorldPointWithoutRubberBanding(Vector2.op_Implicit(currentPosition));
			}
		}
		else
		{
			hasTouch = false;
			isDragging = false;
			UIScreenBase currentScreen = UIManager.Instance.GetCurrentScreen();
			if (!UIBlackFader.IsShowing() && !ResultScreen.IsShowing() && (!((Object)(object)currentScreen != (Object)null) || currentScreen.screenType == UIConstants.Screens.Hud || currentScreen.screenType == UIConstants.Screens.TechTree))
			{
				panningDelta = new Vector2(InputManager.GetAxis("SecondaryHorizontal"), InputManager.GetAxis("SecondaryVertical"));
				isPanning = ((Vector2)(ref panningDelta)).magnitude > 0.1f;
				if (isPanning)
				{
					NullVelocity();
					panningDelta = ((Vector2)(ref panningDelta)).normalized * (Mathf.Abs(panningDelta.x) + Mathf.Abs(panningDelta.y));
				}
				else
				{
					panningDelta.x = 0f;
					panningDelta.y = 0f;
				}
			}
		}
		if ((isDragging || isPanning) && GameManager.Client.IsSpectating)
		{
			SetIsAutoFocusEnabled(value: false);
		}
		touchCount = num;
	}

	private Vector2 GetScrollOffset(Vector2 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		if (position.x < ((Bounds)(ref scrollBounds)).min.x)
		{
			zero.x = ((Bounds)(ref scrollBounds)).min.x - position.x;
		}
		else if (position.x > ((Bounds)(ref scrollBounds)).max.x)
		{
			zero.x = ((Bounds)(ref scrollBounds)).max.x - position.x;
		}
		if (position.y < ((Bounds)(ref scrollBounds)).min.y)
		{
			zero.y = ((Bounds)(ref scrollBounds)).min.y - position.y;
		}
		else if (position.y > ((Bounds)(ref scrollBounds)).max.y)
		{
			zero.y = ((Bounds)(ref scrollBounds)).max.y - position.y;
		}
		return zero;
	}

	private Vector2 GetCurrentPosition()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.GetTouchCount() > 1)
		{
			return Vector2.Lerp(InputManager.CurrentTouches[0].position, InputManager.CurrentTouches[1].position, 0.5f);
		}
		return InputManager.GetInputPosition();
	}

	private float GetZoomDelta(out bool isZoomFromGamepad)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		isZoomFromGamepad = false;
		float result = (0f - Input.mouseScrollDelta.y) * 0.1f;
		if (InputManager.GetKeyUp((KeyCode)270) || InputManager.GetKeyUp((KeyCode)43) || InputManager.GetKeyUp((KeyCode)280))
		{
			result = -0.1f;
		}
		else if (InputManager.GetKeyUp((KeyCode)269) || InputManager.GetKeyUp((KeyCode)45) || InputManager.GetKeyUp((KeyCode)281))
		{
			result = 0.1f;
		}
		if (InputManager.GetTouchCount() > 1)
		{
			Vector2 val = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(InputManager.CurrentTouches[0].position)));
			Vector2 val2 = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(InputManager.CurrentTouches[1].position)));
			if (touch0PreviousScreen.HasValue && touch1PreviousScreen.HasValue)
			{
				Vector2 val3 = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(touch0PreviousScreen.Value)));
				Vector2 val4 = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(touch1PreviousScreen.Value)));
				Vector2 val5 = val2 - val;
				float num = Mathf.Max(0.0001f, ((Vector2)(ref val5)).magnitude);
				val5 = val4 - val3;
				float num2 = Mathf.Max(0.0001f, ((Vector2)(ref val5)).magnitude);
				result = 1f - num / num2;
			}
			touch0PreviousScreen = InputManager.CurrentTouches[0].position;
			touch1PreviousScreen = InputManager.CurrentTouches[1].position;
			zoomFocus = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(currentPosition)));
		}
		else if (InputManager.gamepadInputManager != null)
		{
			if (InputManager.gamepadInputManager.IsHeldDown((LogicalButton)32768))
			{
				result = 0.05f;
			}
			if (InputManager.gamepadInputManager.IsHeldDown((LogicalButton)65536))
			{
				result = -0.05f;
			}
			isZoomFromGamepad = true;
			touch0PreviousScreen = null;
			touch1PreviousScreen = null;
			zoomFocus = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(new Vector2((float)(camera.pixelWidth / 2), (float)(camera.pixelHeight / 2)))));
		}
		else
		{
			touch0PreviousScreen = null;
			touch1PreviousScreen = null;
			zoomFocus = Vector2.op_Implicit(camera.ScreenToWorldPoint(Vector2.op_Implicit(currentPosition)));
		}
		return result;
	}

	private float GetRubberDelta(float overStretching, float viewSize)
	{
		return (float)(1.0 - 1.0 / ((double)Mathf.Abs(overStretching) * 0.550000011920929 / (double)viewSize + 1.0)) * viewSize * Mathf.Sign(overStretching);
	}

	private float GetInverseRubberDelta(float rubberDelta, float viewSize)
	{
		return Mathf.Sign(rubberDelta) * (float)((1.0 / (1.0 - (double)(Mathf.Sign(rubberDelta) * rubberDelta / viewSize)) - 1.0) * (double)viewSize / 0.550000011920929);
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
		previousPosition = ((Component)this).transform.localPosition;
	}

	public void SetWorldCameraBounds(Action onComplete = null)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		isTechViewEnabled = false;
		SetCameraZoom(cameraZoomBeforeTech);
		UpdateBounds();
		CenterOnPosition(Vector2.op_Implicit(cameraPositionBeforeTech), 0f, onComplete, forceChange: true);
	}

	public void SetTechBoundsState(Action onComplete = null)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		isTechViewEnabled = true;
		TweenUtils.KillTween(centerCameraTween, complete: true);
		cameraPositionBeforeTech = ((Component)this).transform.localPosition;
		cameraZoomBeforeTech = zoom;
		SetCameraZoom(0f);
		UpdateBounds();
		CenterOnPosition(Vector2.op_Implicit(((Bounds)(ref techViewBounds)).center), 0f, onComplete, forceChange: true);
	}

	private void SetCameraZoom(float newZoom)
	{
		zoom = newZoom;
		AudioManager.SetZoomLevel(zoom);
	}

	private Bounds GetTechCameraBounds()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return techViewBounds;
	}

	private Bounds GetWorldCameraBounds()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)MapRenderer.Current == (Object)null)
		{
			return new Bounds(Vector3.zero, Vector3.one);
		}
		return MapRenderer.Current.GetWorldBounds();
	}

	public void CenterOnPosition(Vector2 position, float speed = 1f, Action onComplete = null, bool forceChange = false)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		lastCameraFocus = position;
		if ((isTechViewEnabled || !isAutoFocusEnabled || hasTouch) && !forceChange)
		{
			onComplete?.Invoke();
			return;
		}
		isPanning = false;
		isDragging = false;
		NullVelocity();
		if (centerCameraTween != null && !centerCameraTween.active)
		{
			TweenUtils.KillTween(centerCameraTween);
			previousPosition = ((Component)this).transform.localPosition;
		}
		if (speed <= 0f)
		{
			((Component)this).transform.localPosition = Vector2.op_Implicit(position);
			previousPosition = ((Component)this).transform.localPosition;
			onComplete?.Invoke();
			return;
		}
		float num = 0.4f * speed;
		centerCameraTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, Vector2.op_Implicit(position), num, false), (Ease)4), (TweenCallback)delegate
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			previousPosition = ((Component)this).transform.localPosition;
			onComplete?.Invoke();
		});
	}

	public void RevealTile(Tile tile, bool shouldAccountForHUD = true, bool checkEdges = true, float speed = 1f, bool forceChange = false, bool shouldNudgeToCenter = false, Action onComplete = null)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tile == (Object)null)
		{
			onComplete?.Invoke();
			return;
		}
		Vector3 position = tile.Position;
		Vector3 val = position - ((Component)this).transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		position += new Vector3(Mathf.Sign(normalized.x) * 0.4811f, Mathf.Sign(normalized.y) * 0.288f, 0f);
		Rect val2 = default(Rect);
		((Rect)(ref val2))._002Ector(-1f, -1f, 3f, 3f);
		Rect val3 = default(Rect);
		((Rect)(ref val3))._002Ector(0f, 0.1f, 1f, 0.8f);
		if (checkEdges)
		{
			((Rect)(ref val2))._002Ector(0.05f, 0.15f, 0.9f, 0.7f);
		}
		Vector3 val4 = Camera.WorldToViewportPoint(position);
		if (shouldAccountForHUD)
		{
			float yMin = (((Rect)(ref val2)).yMin = (UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).GetCoveredBottomAreaAt(val4.x));
			((Rect)(ref val3)).yMin = yMin;
		}
		if (((Rect)(ref val3)).Contains(val4))
		{
			onComplete?.Invoke();
			return;
		}
		if (shouldNudgeToCenter)
		{
			CenterOnPosition(Vector2.op_Implicit(position), speed, onComplete, forceChange);
			return;
		}
		val4.x = Mathf.Clamp(val4.x, ((Rect)(ref val2)).xMin, ((Rect)(ref val2)).xMax);
		val4.y = Mathf.Clamp(val4.y, ((Rect)(ref val2)).yMin, ((Rect)(ref val2)).yMax);
		CenterOnPosition(Vector2.op_Implicit(((Component)this).transform.position + (position - Camera.ViewportToWorldPoint(val4))), speed, onComplete, forceChange);
	}

	public static void SetIsAutoFocusEnabled(bool value)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Instance) && Config.replayCameraDecoupling.IntValue != 0 && Instance.isAutoFocusEnabled != value)
		{
			Instance.isAutoFocusEnabled = value;
			UIEvents.AutoCameraFocusEnabled(value);
			if (value && Instance.lastCameraFocus != Vector2.zero)
			{
				Instance.CenterOnPosition(Instance.lastCameraFocus);
			}
		}
	}

	public static bool GetIsAutoFocusEnabled()
	{
		if (!Object.op_Implicit((Object)(object)Instance))
		{
			return false;
		}
		return Instance.isAutoFocusEnabled;
	}
}
