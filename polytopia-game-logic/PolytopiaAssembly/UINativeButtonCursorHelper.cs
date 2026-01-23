using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class UINativeButtonCursorHelper : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	protected bool m_hoverRegistered;

	protected bool m_canRegisterHover = true;

	protected Selectable m_selectable;

	public virtual bool CanRegisterHover
	{
		get
		{
			if (((Behaviour)Selectable).enabled && Selectable.interactable)
			{
				return m_canRegisterHover;
			}
			return false;
		}
		set
		{
			m_canRegisterHover = value;
		}
	}

	protected Selectable Selectable
	{
		get
		{
			if ((Object)(object)m_selectable == (Object)null)
			{
				m_selectable = ((Component)this).GetComponent<Selectable>();
			}
			return m_selectable;
		}
	}

	protected virtual void OnDisable()
	{
		if (m_hoverRegistered)
		{
			SystemManager.DecreaseHoveredCounter();
			m_hoverRegistered = false;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (CanRegisterHover && !m_hoverRegistered)
		{
			m_hoverRegistered = true;
			SystemManager.IncreaseHoveredCounter();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (m_hoverRegistered)
		{
			m_hoverRegistered = false;
			SystemManager.DecreaseHoveredCounter();
		}
	}
}
