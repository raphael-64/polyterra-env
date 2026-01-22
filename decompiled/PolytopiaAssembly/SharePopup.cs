using System;
using TMPro;
using UnityEngine;

public class SharePopup : BasicPopup
{
	[Header("Search Friend Popup")]
	[SerializeField]
	protected TextMeshProUGUI linkText;

	private string link;

	private const string SHARE_BASE_URL = "https://share.polytopia.io/g/{0}";

	public void OnCopyUrlButton()
	{
		GUIUtility.systemCopyBuffer = link;
		NotificationManager.Notify(Localization.Get("replay.clipboard"), Localization.Get("replay.clipboard.title"));
	}

	public void SetData(Guid gameId)
	{
		link = $"https://share.polytopia.io/g/{gameId.ToString()}";
		((TMP_Text)linkText).text = link;
	}
}
