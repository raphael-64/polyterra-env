using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HintIcon : UIWorldIconBase
{
	public enum IconTypes
	{
		None,
		CaptureCity,
		Suggestion,
		ExamineRuins,
		Promote,
		Target
	}

	[Serializable]
	public class HintType
	{
		public IconTypes type;

		public Color color = Color.white;

		public Sprite icon;

		public Vector2 iconPos = Vector2.zero;

		public float iconSize = 40f;

		public bool isFloaty;

		public bool showShineOverlay;
	}

	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected RectTransform container;

	[SerializeField]
	protected Image shineOverlay;

	[SerializeField]
	protected RectTransform controllerButtonSprite;

	[Space]
	public HintType[] hintTypes;

	protected IconTypes currType;

	protected HintType currTypeData;

	protected Vector2 containerStartPos;

	protected Vector2 containerStartSize;

	protected int id = -1;

	protected Action callback;

	protected Action<int> idCallback;

	protected Action<WorldCoordinates> coordinateCallback;

	protected Tween showTween;

	protected Tween floatyTween;

	private bool waitingForMouseUp;

	private bool hasControllerFocus;

	public int ID
	{
		get
		{
			return id;
		}
		set
		{
			id = value;
		}
	}

	public IconTypes Type
	{
		get
		{
			return currType;
		}
		set
		{
			currType = value;
			RefreshGraphics();
		}
	}

	public Vector2 IconPosOffset
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return container.anchoredPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			container.anchoredPosition = value;
		}
	}

	public Action Callback
	{
		get
		{
			return callback;
		}
		set
		{
			callback = value;
		}
	}

	public Action<int> IDCallback
	{
		get
		{
			return idCallback;
		}
		set
		{
			idCallback = value;
		}
	}

	public Action<WorldCoordinates> CoordinateCallback
	{
		get
		{
			return coordinateCallback;
		}
		set
		{
			coordinateCallback = value;
		}
	}

	public override Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_position;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			m_position = value;
			KeepWorldPosition = true;
			UIPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(m_position + WorldOffset) * UICanvasScalerHelper.GetInvertedUIScale());
		}
	}

	public override RectTransform UIElement
	{
		get
		{
			return m_uiElement;
		}
		set
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			m_uiElement = value;
			KeepWorldPosition = false;
			UIPosition = RectTransformUtility.PixelAdjustPoint(Vector2.op_Implicit(((Transform)m_uiElement).position), (Transform)(object)m_uiElement, UIManager.Canvas) * UICanvasScalerHelper.GetInvertedUIScale();
		}
	}

	public override bool KeepWorldPosition
	{
		get
		{
			return m_keepWorldPosition;
		}
		set
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			m_keepWorldPosition = value;
			lastCamPos = ((Component)CameraController.Camera).transform.position;
			CameraZoom = CameraController.CurrentZoom;
		}
	}

	public override Transform Target
	{
		get
		{
			return m_target;
		}
		set
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			m_target = value;
			lastCamPos = ((Component)CameraController.Camera).transform.position;
			CameraZoom = CameraController.CurrentZoom;
			Position = m_target.position;
		}
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		containerStartPos = container.anchoredPosition;
		containerStartSize = container.sizeDelta;
		((Component)controllerButtonSprite).gameObject.SetActive(false);
	}

	protected override void OnEnable()
	{
		InputEvents.OnButtonDown += OnButtonDown;
		InputEvents.OnOmnicursorSnapToTile += OnTileMarked;
		InputEvents.OnTileMarkCleared += OnTileMarkCleared;
		InputEvents.OnOmnicursorStartMoving += OnTileMarkCleared;
		base.OnEnable();
	}

	private void OnButtonDown(InputManager.Buttons button)
	{
		if (button == InputManager.Buttons.Hint && currType != IconTypes.None && hasControllerFocus)
		{
			OnClicked();
		}
	}

	protected override void OnDisable()
	{
		InputEvents.OnButtonDown -= OnButtonDown;
		InputEvents.OnOmnicursorSnapToTile -= OnTileMarked;
		InputEvents.OnTileMarkCleared -= OnTileMarkCleared;
		InputEvents.OnOmnicursorStartMoving -= OnTileMarkCleared;
		if (waitingForMouseUp)
		{
			waitingForMouseUp = false;
			InputManager.EnableInput(InputManager.InputType.Camera | InputManager.InputType.Map);
		}
		base.OnDisable();
		ResetItem();
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (PolytopiaInput.GetMouseButtonUp(0) && waitingForMouseUp)
		{
			waitingForMouseUp = false;
			InputManager.EnableInput(InputManager.InputType.Camera | InputManager.InputType.Map);
		}
	}

	public override void ResetItem()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.ResetItem();
		TweenUtils.KillTween(showTween);
		TweenUtils.KillTween(floatyTween);
		Callback = null;
		IDCallback = null;
		CoordinateCallback = null;
		currType = IconTypes.None;
		currTypeData = null;
		container.anchoredPosition = containerStartPos;
		container.sizeDelta = containerStartSize;
		((Transform)container).localScale = Vector3.one;
		waitingForMouseUp = false;
		IconPosOffset = Vector2.op_Implicit(Vector3.zero);
	}

	public override void Show()
	{
		base.Show();
	}

	public void RefreshGraphics()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (currType == IconTypes.None)
		{
			return;
		}
		currTypeData = null;
		HintType[] array = hintTypes;
		foreach (HintType hintType in array)
		{
			if (hintType.type == Type)
			{
				currTypeData = hintType;
				break;
			}
		}
		((Graphic)bg).color = currTypeData.color;
		icon.sprite = currTypeData.icon;
		((Graphic)icon).rectTransform.anchoredPosition = currTypeData.iconPos;
		((Graphic)icon).SetNativeSize();
		float num = currTypeData.iconSize / Mathf.Max(((Graphic)icon).rectTransform.sizeDelta.x, ((Graphic)icon).rectTransform.sizeDelta.y);
		Vector2 sizeDelta = ((Graphic)icon).rectTransform.sizeDelta;
		sizeDelta *= num;
		((Graphic)icon).rectTransform.sizeDelta = sizeDelta;
		((Component)shineOverlay).gameObject.SetActive(currTypeData.showShineOverlay);
	}

	protected override void InternalShow()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		base.InternalShow();
		((Transform)container).localScale = Vector3.zero;
		Vector3 val = (Vector3)(currTypeData.isFloaty ? new Vector3(1f, 0.95f, 1f) : Vector3.one);
		showTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)container, val, 0.3f), (Ease)27, 5f), new TweenCallback(ShowAnimComplete));
		if (currTypeData.showShineOverlay)
		{
			((Component)shineOverlay).gameObject.SetActive(true);
			((Graphic)shineOverlay).color = Color.white;
			TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Color, Color, ColorOptions>>(shineOverlay.DOFade(0f, 0.3f), 0.2f), (TweenCallback)delegate
			{
				((Component)shineOverlay).gameObject.SetActive(false);
			});
		}
	}

	private void ShowAnimComplete()
	{
		TweenUtils.KillTween(showTween, complete: true);
		showTween = null;
		if (currTypeData != null && currTypeData.isFloaty)
		{
			floatyTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScaleY((Transform)(object)container, 1.05f, 1.2f), (Ease)4), -1, (LoopType)1);
		}
	}

	public void OnClicked()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.IsEnabled(InputManager.InputType.Map) || Type == IconTypes.Suggestion)
		{
			if (Type == IconTypes.Suggestion)
			{
				GameManager.GetAnalyticsManager().SendEvent("tips_view", new Dictionary<string, object> { 
				{
					"game_id",
					GameManager.Client?.CurrentGameId
				} });
			}
			PopupManager.HideCurrentPopup();
			AudioManager.PlaySFX(SFXTypes.Press, 1f, 1f, AudioManager.GetPanFromPosition(Vector2.op_Implicit(((Component)this).transform.position)));
			Callback?.Invoke();
			IDCallback?.Invoke(ID);
			CoordinateCallback?.Invoke(Coordinates);
			if (Type != IconTypes.Target)
			{
				ReturnToPool();
			}
		}
	}

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((InputManager.IsEnabled(InputManager.InputType.Map) || Type == IconTypes.Suggestion) && (int)pointerEventData.button == 0)
		{
			if (!waitingForMouseUp)
			{
				InputManager.DisableInput(InputManager.InputType.Camera | InputManager.InputType.Map);
			}
			waitingForMouseUp = true;
		}
	}

	private void OnTileMarked(Tile tile)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.gamepadInputManager == null)
		{
			return;
		}
		if ((Object)(object)tile == (Object)null || tile.Coordinates != Coordinates)
		{
			Image component = ((Component)controllerButtonSprite).GetComponent<Image>();
			ShortcutExtensions.DOKill((Component)(object)component, false);
			TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(component.DOFade(0f, 0.3f), (TweenCallback)delegate
			{
				((Component)controllerButtonSprite).gameObject.SetActive(false);
			});
			((Component)controllerButtonSprite).gameObject.SetActive(false);
			hasControllerFocus = false;
		}
		else if (tile.Coordinates == Coordinates)
		{
			Image component2 = ((Component)controllerButtonSprite).GetComponent<Image>();
			ShortcutExtensions.DOKill((Component)(object)component2, false);
			((Graphic)component2).color = Color.white;
			((Transform)controllerButtonSprite).localScale = Vector3.zero;
			((Component)controllerButtonSprite).gameObject.SetActive(true);
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)controllerButtonSprite, Vector3.one, 0.3f), (Ease)27, 5f);
			hasControllerFocus = true;
		}
	}

	private void OnTileMarkCleared()
	{
		OnTileMarked(null);
	}
}
