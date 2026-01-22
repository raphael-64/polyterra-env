using UnityEngine;

public class FriendsListPlayerInfo : UIBasicComponent
{
	[SerializeField]
	protected UIPlainButton playerIdButton;

	private void OnEnable()
	{
		playerIdButton.text = AccountManager.PlayerAccountId.ToString();
	}

	public void OnCopyPlayerId()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		TextEditor val = new TextEditor
		{
			text = AccountManager.PlayerAccountId.ToString()
		};
		val.SelectAll();
		val.Copy();
		NotificationManager.Notify(Localization.Get("throne.clipboard"), Localization.Get("throne.clipboard.title"));
	}
}
