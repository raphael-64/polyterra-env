using UnityEngine;

public class DiplomacyPopup : BasicPopup
{
	[Header("Diplomacy Popup")]
	[SerializeField]
	protected DiplomacyGraphics diplomacyGraphics;

	public void SetData(PlayerState player, PlayerState otherPlayer, DiplomacyGraphics.Type type)
	{
		diplomacyGraphics.SetData(player, otherPlayer, type);
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
	}
}
