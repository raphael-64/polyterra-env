using UnityEngine;

public class DiplomacyNotification : NotificationBase
{
	[SerializeField]
	private DiplomacyGraphics diplomacyGraphics;

	public void SetData(PlayerState player, PlayerState otherPlayer, DiplomacyGraphics.Type type)
	{
		diplomacyGraphics.SetData(player, otherPlayer, type);
	}
}
