using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class UIWorldScoreBase : UIWorldIconBase
{
	public enum MovementType
	{
		None,
		ResourceBar,
		Tile
	}

	public const float SCALE_UP_MULTIPLIER = 3.3333333f;

	public const float POST_SCALE_DELAY = 5f;

	public const float WORLD_TO_UI_MULTIPLIER = 2f;

	[SerializeField]
	protected CanvasGroup canvasGroup;

	private ResourceEvents.ResourceEventData m_data;

	protected MovementType m_movementType;

	protected float progress;

	protected Vector2 uiStartPosition;

	protected Vector2 uiTargetPosition;

	protected WorldCoordinates startCoordinates;

	protected WorldCoordinates targetCoordinates;

	protected bool doScaleUp;

	protected bool doDelay;

	protected bool endPosSet;

	protected byte playerId;

	protected ResourceManager.Type resourceType;

	protected float amount;

	protected bool updateIncome;

	protected string reason = string.Empty;

	protected Action onCompleteCallback;

	protected Vector2 cameraPositionAtStart;

	public virtual ResourceEvents.ResourceEventData Data
	{
		protected get
		{
			return m_data;
		}
		set
		{
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0165: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			m_data = value;
			if (m_data == null)
			{
				return;
			}
			PlayerID = m_data.playerId;
			ResourceType = m_data.resourceType;
			Amount = m_data.amount;
			Reason = m_data.reason;
			if (m_data.from != null)
			{
				switch (m_data.from.type)
				{
				case ResourceEvents.ResourceEventPositionData.PositionType.WorldCoordinate:
					startCoordinates = m_data.from.worldCoordinate;
					break;
				case ResourceEvents.ResourceEventPositionData.PositionType.UIElement:
					uiStartPosition = RectTransformUtility.PixelAdjustPoint(m_data.from.UIElement.anchoredPosition, (Transform)(object)m_data.from.UIElement, UIManager.WorldCanvas);
					break;
				}
			}
			if (m_data.to != null)
			{
				endPosSet = true;
				switch (m_data.to.type)
				{
				case ResourceEvents.ResourceEventPositionData.PositionType.WorldCoordinate:
					movementType = MovementType.Tile;
					targetCoordinates = m_data.to.worldCoordinate;
					break;
				case ResourceEvents.ResourceEventPositionData.PositionType.UIElement:
					movementType = MovementType.ResourceBar;
					uiTargetPosition = RectTransformUtility.PixelAdjustPoint(Vector2.op_Implicit(((Transform)Data.to.UIElement).position), (Transform)(object)Data.to.UIElement, UIManager.WorldCanvas) / UIManager.WorldCanvasScaler.scaleFactor;
					break;
				case ResourceEvents.ResourceEventPositionData.PositionType.None:
					break;
				}
			}
		}
	}

	public virtual ResourceManager.Type ResourceType
	{
		get
		{
			return resourceType;
		}
		set
		{
			resourceType = value;
		}
	}

	public virtual float Amount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = value;
		}
	}

	public MovementType movementType
	{
		get
		{
			return m_movementType;
		}
		set
		{
			m_movementType = value;
		}
	}

	public byte PlayerID
	{
		get
		{
			return playerId;
		}
		set
		{
			playerId = value;
		}
	}

	public bool UpdateIncome
	{
		get
		{
			return updateIncome;
		}
		set
		{
			updateIncome = value;
		}
	}

	public string Reason
	{
		get
		{
			return reason;
		}
		set
		{
			reason = value;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		ResetItem();
	}

	public override void ResetItem()
	{
		base.ResetItem();
		m_data = null;
		endPosSet = false;
		updateIncome = false;
		movementType = MovementType.None;
		isAnimating = false;
		if ((Object)(object)canvasGroup != (Object)null)
		{
			canvasGroup.alpha = 1f;
		}
	}

	protected override void InternalShow()
	{
		((Component)this).gameObject.SetActive(true);
		switch (movementType)
		{
		case MovementType.None:
			DoSimpleFadeAnimation();
			break;
		case MovementType.ResourceBar:
			DoMoveToResourceBarAnimation();
			break;
		case MovementType.Tile:
			DoJumpAnimation();
			break;
		}
	}

	protected void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)MapRenderer.Current == (Object)null || !isAnimating)
		{
			return;
		}
		if (doScaleUp)
		{
			Vector2 val = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(startCoordinates).VisualCenter);
			uiStartPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val)) / UIManager.WorldCanvasScaler.scaleFactor);
			base.rectTransform.anchoredPosition = uiStartPosition;
			((Transform)base.rectTransform).localScale = Vector3.Lerp(Vector3.zero, Vector3.one, EasingExtensions.OutBack(progress)) * CameraController.CurrentZoom;
			if (progress >= 1f)
			{
				progress = 0f;
				doScaleUp = false;
				doDelay = true;
			}
			progress += Time.deltaTime * 3.3333333f;
		}
		else if (doDelay)
		{
			Vector2 val2 = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(startCoordinates).VisualCenter);
			uiStartPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val2)) / UIManager.WorldCanvasScaler.scaleFactor);
			base.rectTransform.anchoredPosition = uiStartPosition;
			if (progress >= 1f)
			{
				progress = 0f;
				doDelay = false;
			}
			progress += Time.deltaTime * 5f;
		}
		else if (movementType == MovementType.Tile)
		{
			Vector2 val3 = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(startCoordinates).VisualCenter);
			Vector2 val4 = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(targetCoordinates).VisualCenter);
			uiStartPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val3)) / UIManager.WorldCanvasScaler.scaleFactor);
			uiTargetPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val4)) / UIManager.WorldCanvasScaler.scaleFactor);
			float num = Vector3.Distance(Vector2.op_Implicit(val3), Vector2.op_Implicit(val4));
			_ = CameraController.NormalizedZoom;
			float num2 = EasingExtensions.Linear(progress);
			float num3 = Vector2.Distance(uiStartPosition, uiTargetPosition) * 3f;
			Vector2 val5 = Vector2.Lerp(uiStartPosition, uiTargetPosition, 0.5f) + new Vector2(0f, num3);
			base.rectTransform.anchoredPosition = Vector2.Lerp(uiStartPosition, Vector2.Lerp(val5, uiTargetPosition, num2), num2);
			((Transform)base.rectTransform).localScale = Vector3.one * CameraController.CurrentZoom;
			float num4 = 1f / (0.2f + num / 8f);
			progress += Time.deltaTime * num4;
			if (progress >= 1f)
			{
				ShowAnimComplete();
			}
		}
		else if (movementType == MovementType.ResourceBar)
		{
			Vector2 val6 = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(startCoordinates).VisualCenter);
			uiStartPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val6)) / UIManager.WorldCanvasScaler.scaleFactor);
			float num5 = EasingExtensions.InSine(progress);
			Vector2 val7 = default(Vector2);
			((Vector2)(ref val7))._002Ector((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			base.rectTransform.anchoredPosition = Vector2.Lerp(uiStartPosition, Vector2.Lerp(val7, uiTargetPosition, num5), num5);
			((Transform)base.rectTransform).localScale = Vector3.one * CameraController.CurrentZoom;
			progress += Time.deltaTime * 2f;
			if (progress >= 1f)
			{
				ShowAnimComplete();
			}
		}
	}

	protected virtual void DoSimpleFadeAnimation()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Expected O, but got Unknown
		((Transform)base.rectTransform).localScale = Vector3.zero;
		Sequence val = DOTween.Sequence();
		TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)base.rectTransform, 1f, 0.3f), (Ease)27));
		TweenSettingsExtensions.AppendInterval(val, 0.2f);
		TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.SetRelative<TweenerCore<Vector2, Vector2, VectorOptions>>(base.rectTransform.DOAnchorPosY(40f, 0.3f)));
		if ((Object)(object)canvasGroup != (Object)null)
		{
			TweenSettingsExtensions.Join(val, (Tween)(object)canvasGroup.DOFade(0f, 0.3f));
		}
		TweenSettingsExtensions.AppendCallback(val, new TweenCallback(ShowAnimComplete));
	}

	protected virtual void DoMoveToResourceBarAnimation()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (!endPosSet || (Object)(object)MapRenderer.Current == (Object)null || (Object)(object)CameraController.Camera == (Object)null)
		{
			DoSimpleFadeAnimation();
			return;
		}
		Vector2 val = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(startCoordinates).VisualCenter);
		uiStartPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val)) / UIManager.WorldCanvasScaler.scaleFactor);
		base.rectTransform.anchoredPosition = uiStartPosition;
		((Transform)base.rectTransform).localScale = Vector3.zero;
		progress = 0f;
		isAnimating = true;
		doScaleUp = true;
	}

	protected virtual void DoJumpAnimation()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (!endPosSet)
		{
			DoSimpleFadeAnimation();
			return;
		}
		Vector2 val = Vector2.op_Implicit(MapRenderer.Current.GetTileInstance(startCoordinates).VisualCenter);
		uiStartPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(Vector2.op_Implicit(val)) / UIManager.WorldCanvasScaler.scaleFactor);
		base.rectTransform.anchoredPosition = uiStartPosition;
		((Transform)base.rectTransform).localScale = Vector3.one * CameraController.CurrentZoom;
		progress = 0f;
		isAnimating = true;
	}

	protected virtual void ShowAnimComplete()
	{
		isAnimating = false;
		progress = 0f;
		if (Data.resourceType == ResourceManager.Type.Score && Data.amount > 100f)
		{
			AudioManager.PlaySFX(SFXTypes.Score);
		}
		if (Data.resourceType == ResourceManager.Type.Currency && GameManager.GameState.CurrentState == GameState.State.Started)
		{
			AudioManager.PlaySFX(AudioUtils.RandomCoin);
		}
		if (m_data != null && m_data.result != null)
		{
			m_data.result.onResourceAdded?.Invoke();
			switch (m_data.result.type)
			{
			case ResourceEvents.ResourceEventResultData.ResultType.ReturnToResourceManager:
				ResourceManager.AddResourceOfType(m_data.playerId, m_data.resourceType, m_data.amount, m_data.result.onComplete, m_data.reason);
				break;
			case ResourceEvents.ResourceEventResultData.ResultType.AddToBuilding:
			{
				for (int i = 0; (float)i < Amount; i++)
				{
					m_data.result.onComplete?.Invoke();
				}
				break;
			}
			case ResourceEvents.ResourceEventResultData.ResultType.UpdateIncome:
				ResourceManager.IncomeChanged(m_data.playerId);
				break;
			}
		}
		else
		{
			ResourceManager.AddResourceOfType(PlayerID, ResourceType, Amount, null, Reason);
			if (UpdateIncome)
			{
				ResourceManager.IncomeChanged(PlayerID);
			}
		}
		WorldIconContainer.HideScore(this);
	}
}
