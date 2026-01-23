using UnityEngine;

public class UIBasicComponent : MonoBehaviour
{
	protected RectTransform m_rectTransform;

	protected bool m_searchedRt;

	protected bool initialized;

	public RectTransform rectTransform
	{
		get
		{
			if (!m_searchedRt && (Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
				m_searchedRt = true;
			}
			return m_rectTransform;
		}
	}

	public bool Initialized
	{
		get
		{
			return initialized;
		}
		private set
		{
			initialized = value;
		}
	}

	public virtual void Init()
	{
		Initialized = true;
	}

	public virtual void DeInit()
	{
		Initialized = false;
	}
}
