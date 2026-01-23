using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPLinkHelper : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[Serializable]
	public class TMPLinkEvent : UnityEvent<string, string>
	{
	}

	public TMPLinkEvent OnClick;

	protected bool m_hoverRegistered;

	protected bool m_insideTextfield;

	protected TextMeshProUGUI textField;

	protected bool PointerInsideLink => LinkIndex != -1;

	protected int LinkIndex => TMP_TextUtilities.FindIntersectingLink((TMP_Text)(object)textField, Vector2.op_Implicit(PolytopiaInput.mousePosition), (Camera)null);

	private void Awake()
	{
		textField = ((Component)this).GetComponent<TextMeshProUGUI>();
	}

	protected virtual void OnDisable()
	{
		if (m_hoverRegistered)
		{
			SystemManager.DecreaseHoveredCounter();
			m_hoverRegistered = false;
		}
	}

	private void Update()
	{
		if (m_insideTextfield)
		{
			if (!m_hoverRegistered && PointerInsideLink)
			{
				m_hoverRegistered = true;
				SystemManager.IncreaseHoveredCounter();
			}
			else if (m_hoverRegistered && !PointerInsideLink)
			{
				m_hoverRegistered = false;
				SystemManager.DecreaseHoveredCounter();
			}
		}
		else if (m_hoverRegistered)
		{
			SystemManager.DecreaseHoveredCounter();
			m_hoverRegistered = false;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_insideTextfield = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_insideTextfield = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		int linkIndex = LinkIndex;
		if (linkIndex != -1)
		{
			TMP_LinkInfo val = ((TMP_Text)textField).textInfo.linkInfo[linkIndex];
			((UnityEvent<string, string>)OnClick)?.Invoke(((TMP_LinkInfo)(ref val)).GetLinkID(), ((TMP_LinkInfo)(ref val)).GetLinkText());
		}
	}
}
