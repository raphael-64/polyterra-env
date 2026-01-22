using System;
using UnityEngine;
using UnityEngine.UI;

public class DiplomacyGraphics : MonoBehaviour
{
	public enum Type
	{
		PeaceTreatyAccepted,
		PeaceTreatyRejected,
		EmbassyEstablished,
		EmbassyDestroyed
	}

	[SerializeField]
	protected Image diplomacyIcon;

	[SerializeField]
	protected PlayerInfoIcon leftPlayerIcon;

	[SerializeField]
	protected PlayerInfoIcon rightPlayerIcon;

	public void SetData(PlayerState player, PlayerState otherPlayer, Type type)
	{
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		((Component)leftPlayerIcon).gameObject.SetActive(true);
		((Component)rightPlayerIcon).gameObject.SetActive(true);
		bool flag;
		switch (type)
		{
		case Type.PeaceTreatyAccepted:
			diplomacyIcon.sprite = UIManager.Instance.iconData.GetSprite("peacetreaty");
			leftPlayerIcon.SetData(player, otherPlayer, PlayerInfoIcon.Mood.None, shouldShowDiplomaticState: false);
			rightPlayerIcon.SetData(otherPlayer, player, PlayerInfoIcon.Mood.Happy, shouldShowDiplomaticState: false);
			flag = true;
			break;
		case Type.PeaceTreatyRejected:
			diplomacyIcon.sprite = UIManager.Instance.iconData.GetSprite("peacetreatyrejected");
			leftPlayerIcon.SetData(player, otherPlayer, PlayerInfoIcon.Mood.None, shouldShowDiplomaticState: false);
			rightPlayerIcon.SetData(otherPlayer, player, PlayerInfoIcon.Mood.Angry, shouldShowDiplomaticState: false);
			flag = false;
			break;
		case Type.EmbassyEstablished:
			diplomacyIcon.sprite = UIManager.Instance.iconData.GetSprite("embassyestablished");
			leftPlayerIcon.SetData(player, otherPlayer, PlayerInfoIcon.Mood.None, shouldShowDiplomaticState: false);
			((Component)rightPlayerIcon).gameObject.SetActive(false);
			flag = true;
			break;
		case Type.EmbassyDestroyed:
			diplomacyIcon.sprite = UIManager.Instance.iconData.GetSprite("embassydestroyed");
			((Component)leftPlayerIcon).gameObject.SetActive(false);
			rightPlayerIcon.SetData(otherPlayer, player, PlayerInfoIcon.Mood.None, shouldShowDiplomaticState: false);
			flag = false;
			break;
		default:
			throw new Exception("Not implemented");
		}
		((Component)leftPlayerIcon).transform.localScale = new Vector3((float)(flag ? 1 : (-1)), 1f, 1f);
		((Component)rightPlayerIcon).transform.localScale = new Vector3((float)((!flag) ? 1 : (-1)), 1f, 1f);
	}
}
