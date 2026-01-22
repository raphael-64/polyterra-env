using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PullToRefresh;

public class UIRefreshControl : MonoBehaviour
{
	[Serializable]
	public class RefreshControlEvent : UnityEvent
	{
	}

	[SerializeField]
	private ScrollRect m_ScrollRect;

	[SerializeField]
	private float m_PullDistanceRequiredRefresh = 150f;

	[SerializeField]
	private Animator m_LoadingAnimator;

	[SerializeField]
	private RefreshControlEvent m_OnRefresh = new RefreshControlEvent();

	[SerializeField]
	private RefreshControlEvent onTriggerHit = new RefreshControlEvent();

	[SerializeField]
	private RefreshControlEvent onCancel = new RefreshControlEvent();

	private float m_InitialPosition;

	private float m_Progress;

	private bool m_IsPulled;

	private bool m_IsRefreshing;

	private Vector2 m_PositionStop;

	private IScrollable m_ScrollView;

	private const string _activityIndicatorStartLoadingName = "Loading";

	public float Progress => m_Progress;

	public bool IsRefreshing => m_IsRefreshing;

	public RefreshControlEvent OnRefresh
	{
		get
		{
			return m_OnRefresh;
		}
		set
		{
			m_OnRefresh = value;
		}
	}

	public void EndRefreshing()
	{
		m_IsPulled = false;
		m_IsRefreshing = false;
		if ((Object)(object)m_LoadingAnimator != (Object)null)
		{
			m_LoadingAnimator.SetBool("Loading", false);
		}
	}

	private void Start()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		m_InitialPosition = GetContentAnchoredPosition();
		m_PositionStop = new Vector2(m_ScrollRect.content.anchoredPosition.x, m_InitialPosition - m_PullDistanceRequiredRefresh);
		m_ScrollView = ((Component)m_ScrollRect).GetComponent<IScrollable>();
		((UnityEvent<Vector2>)(object)m_ScrollRect.onValueChanged).AddListener((UnityAction<Vector2>)OnScroll);
	}

	private void LateUpdate()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (m_IsPulled && m_IsRefreshing)
		{
			m_ScrollRect.content.anchoredPosition = m_PositionStop;
		}
	}

	private void OnScroll(Vector2 normalizedPosition)
	{
		float num = m_InitialPosition - GetContentAnchoredPosition();
		if (!(num < 0f))
		{
			OnPull(num);
		}
	}

	private void OnPull(float distance)
	{
		if (m_IsRefreshing && Math.Abs(distance) < 1f)
		{
			m_IsRefreshing = false;
		}
		m_Progress = distance / m_PullDistanceRequiredRefresh;
		if (m_IsPulled && m_ScrollView.Dragging)
		{
			if (m_Progress < 1f)
			{
				m_IsPulled = false;
				RefreshControlEvent refreshControlEvent = onCancel;
				if (refreshControlEvent != null)
				{
					((UnityEvent)refreshControlEvent).Invoke();
				}
			}
		}
		else
		{
			if (m_Progress < 1f)
			{
				return;
			}
			if (m_ScrollView.Dragging)
			{
				m_IsPulled = true;
				RefreshControlEvent refreshControlEvent2 = onTriggerHit;
				if (refreshControlEvent2 != null)
				{
					((UnityEvent)refreshControlEvent2).Invoke();
				}
				if ((Object)(object)m_LoadingAnimator != (Object)null)
				{
					m_LoadingAnimator.SetBool("Loading", true);
				}
			}
			if (m_IsPulled && !m_ScrollView.Dragging)
			{
				m_IsRefreshing = true;
				((UnityEvent)m_OnRefresh).Invoke();
				m_Progress = 0f;
			}
		}
	}

	private float GetContentAnchoredPosition()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return m_ScrollRect.content.anchoredPosition.y;
	}
}
