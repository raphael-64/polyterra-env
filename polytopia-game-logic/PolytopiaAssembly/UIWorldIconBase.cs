using System.Runtime.CompilerServices;
using UnityEngine;

public class UIWorldIconBase : UIBasicComponent, IPooledObject
{
	protected Vector2 m_uiPosition;

	protected Vector3 m_position;

	protected WorldCoordinates m_coordinate = WorldCoordinates.NULL_COORDINATES;

	protected RectTransform m_uiElement;

	protected Vector3 m_worldOffset = Vector3.zero;

	protected Vector2 m_uiOffset = Vector2.zero;

	protected bool m_isUsed;

	protected bool m_keepWorldPosition;

	protected Vector3 lastCamPos;

	protected float lastCameraZoom;

	protected Transform m_target;

	protected bool isAnimating;

	public bool forceUpdatePosition;

	public bool IsUsed
	{
		get
		{
			return m_isUsed;
		}
		set
		{
			m_isUsed = value;
		}
	}

	public virtual WorldCoordinates Coordinates
	{
		get
		{
			return m_coordinate;
		}
		set
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			m_coordinate = value;
			KeepWorldPosition = true;
			Tile tileInstance = MapRenderer.Current.GetTileInstance(m_coordinate);
			if ((Object)(object)tileInstance == (Object)null)
			{
				Log.Error("Could not get tile for coordinate {0}, frame {1}, icon name {2}", new object[3]
				{
					m_coordinate,
					Time.frameCount,
					((Object)this).name
				});
			}
			else
			{
				Position = tileInstance.VisualCenter;
			}
		}
	}

	public virtual Vector3 Position
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
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			m_position = value;
			KeepWorldPosition = true;
			UIPosition = Vector2.op_Implicit(CameraController.Camera.WorldToScreenPoint(m_position + WorldOffset) / UIManager.WorldCanvasScaler.scaleFactor);
		}
	}

	public virtual RectTransform UIElement
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
			m_uiElement = value;
			KeepWorldPosition = false;
			UIPosition = RectTransformUtility.PixelAdjustPoint(Vector2.op_Implicit(((Transform)m_uiElement).position), (Transform)(object)m_uiElement, UIManager.WorldCanvas);
		}
	}

	public virtual Vector2 UIPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_uiPosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			m_uiPosition = value;
			base.rectTransform.anchoredPosition = m_uiPosition + UIOffset;
		}
	}

	public virtual bool KeepWorldPosition
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

	public virtual Transform Target
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

	public virtual Vector3 WorldOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_worldOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			m_worldOffset = value;
			Position = m_position;
		}
	}

	public virtual Vector2 UIOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return m_uiOffset;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			m_uiOffset = value;
			UIPosition = m_uiPosition;
		}
	}

	protected virtual Vector3 CameraPos
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return lastCamPos;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			lastCamPos = value;
		}
	}

	protected virtual float CameraZoom
	{
		get
		{
			return lastCameraZoom;
		}
		set
		{
			lastCameraZoom = value;
		}
	}

	public override void Init()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		base.rectTransform.anchorMin = Vector2.zero;
		base.rectTransform.anchorMax = Vector2.zero;
	}

	protected virtual void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
		forceUpdatePosition = true;
	}

	protected virtual void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		if (!IsUsed)
		{
			ResetItem();
		}
	}

	protected virtual void OnDestroy()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		ResetItem();
	}

	protected virtual void LateUpdate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (m_keepWorldPosition && (((Component)CameraController.Camera).transform.position != lastCamPos || !Mathf.Approximately(CameraController.CurrentZoom, CameraZoom) || forceUpdatePosition))
		{
			lastCamPos = ((Component)CameraController.Camera).transform.position;
			CameraZoom = CameraController.CurrentZoom;
			Position = m_position;
		}
		if ((Object)(object)m_target != (Object)null && (m_target.position != m_position || forceUpdatePosition))
		{
			Position = m_target.position;
		}
		if ((Object)(object)m_uiElement != (Object)null && (((Transform)m_uiElement).position != m_position || forceUpdatePosition))
		{
			UIElement = m_uiElement;
		}
		forceUpdatePosition = false;
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		forceUpdatePosition = true;
	}

	public void ReturnToPool()
	{
		ObjectPool.ReturnObject(((Component)this).gameObject);
	}

	public virtual void ResetItem()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		m_keepWorldPosition = false;
		m_target = null;
		m_uiPosition = Vector2.zero;
		m_position = Vector3.zero;
		m_coordinate = WorldCoordinates.NULL_COORDINATES;
		m_uiElement = null;
		m_worldOffset = Vector3.zero;
		m_uiOffset = Vector2.zero;
	}

	public virtual void Show()
	{
		IsUsed = true;
		InternalShow();
	}

	protected virtual void InternalShow()
	{
		if (!((Object)(object)this == (Object)null))
		{
			((Component)this).gameObject.SetActive(true);
		}
	}

	public virtual void Hide()
	{
		ReturnToPool();
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
