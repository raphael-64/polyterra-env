using System;
using UnityEngine;
using UnityEngine.UI;

public class LoginOverlay : MonoBehaviour
{
	[SerializeField]
	protected LoginDetails loginDetails;

	[SerializeField]
	protected Image background;

	protected Action onCancelAction;

	public void Back()
	{
		onCancelAction?.Invoke();
		onCancelAction = null;
		Hide();
	}

	public void Hide()
	{
		onCancelAction = null;
		loginDetails.ClearList();
		((Component)this).gameObject.SetActive(false);
	}

	public void Show(Action onCancel = null)
	{
		if (PolytopiaBackendAdapter.Instance.IsAuthenticated && GameManager.IsMultiplayerEnabled)
		{
			Log.Info("Hide LoginOverlay", Array.Empty<object>());
			Hide();
			return;
		}
		Log.Info("Show LoginOverlay", Array.Empty<object>());
		((Component)this).gameObject.SetActive(true);
		onCancelAction = onCancel;
		ShowLoginDetails();
	}

	private void ShowLoginDetails()
	{
		loginDetails.ClearList();
		loginDetails.ShowLoginDetails();
	}
}
