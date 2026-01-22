using System;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode;
using UnityEngine;

public class LinkCmAccountIntent : Intent
{
	private const string CM_LOGIN_POPUP_ID = "CMLoginPopup";

	private const string CM_VERIFICATION_POPUP_ID = "CMVerificationPopup";

	public LinkCmAccountIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return !(intent is LinkCmAccountIntent);
	}

	public override async Task HandleAsync()
	{
		base.State = ProcessState.Processing;
		if (!base.QueryMap.TryGetValue("ott", out var ott))
		{
			base.State = ProcessState.Failed;
			return;
		}
		if (PolytopiaBackendAdapter.Instance.ConnectionStatus == ConnectionStatus.None)
		{
			PolytopiaPlayerPrefs.SetString("cm_verification_uri", base.Uri.OriginalString);
			base.State = ProcessState.Failed;
			return;
		}
		if (PopupManager.IsPopupShowing<BasicPopup>("CMLoginPopup"))
		{
			base.State = ProcessState.Failed;
			return;
		}
		if (!PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			await GameManager.GetLoginManager().LoginAsync(silent: false);
			if (!PolytopiaBackendAdapter.Instance.IsAuthenticated)
			{
				base.State = ProcessState.Failed;
				PolytopiaPlayerPrefs.SetString("cm_verification_uri", base.Uri.OriginalString);
				BasicPopup basicPopup = PopupManager.GetBasicPopup();
				basicPopup.identifier = "CMLoginPopup";
				basicPopup.Header = Localization.Get("onlineview.nogames.intro", AccountManager.Alias);
				basicPopup.Description = Localization.Get("onlineview.loaderror");
				basicPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected, delegate
					{
						if (PolytopiaPlayerPrefs.HasKey("cm_verification_uri"))
						{
							PolytopiaPlayerPrefs.DeleteKey("cm_verification_uri");
							PolytopiaPlayerPrefs.Save();
						}
						base.State = ProcessState.Cancelled;
					})
				};
				basicPopup.Show();
				return;
			}
		}
		if (PopupManager.IsPopupShowing<BasicPopup>("CMVerificationPopup") || (!GameManager.IsMultiplayerEnabled && ((Component)UIManager.Instance.loginOverlay).gameObject.activeInHierarchy))
		{
			base.State = ProcessState.Failed;
		}
		else if (!GameManager.IsMultiplayerEnabled)
		{
			base.State = ProcessState.Failed;
			PolytopiaPlayerPrefs.SetString("cm_verification_uri", base.Uri.OriginalString);
			BasicPopup basicPopup2 = PopupManager.GetBasicPopup();
			basicPopup2.identifier = "CMVerificationPopup";
			basicPopup2.Header = Localization.Get("esport.verifyaccount.title");
			basicPopup2.Description = Localization.Get("esport.verifyaccount.description.login");
			basicPopup2.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected, delegate
				{
					if (PolytopiaPlayerPrefs.HasKey("cm_verification_uri"))
					{
						PolytopiaPlayerPrefs.DeleteKey("cm_verification_uri");
						PolytopiaPlayerPrefs.Save();
					}
					base.State = ProcessState.Cancelled;
				}),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					if (!GameManager.IsMultiplayerEnabled && UIManager.Instance.CurrentScreen != UIConstants.Screens.MultiplayerScreen)
					{
						UIManager.Instance.loginOverlay.Show(delegate
						{
							if (PolytopiaPlayerPrefs.HasKey("cm_verification_uri"))
							{
								PolytopiaPlayerPrefs.DeleteKey("cm_verification_uri");
								PolytopiaPlayerPrefs.Save();
							}
							base.State = ProcessState.Cancelled;
						});
					}
				})
			};
			basicPopup2.Show();
		}
		else
		{
			if (((Component)UIManager.Instance.loginOverlay).gameObject.activeInHierarchy)
			{
				UIManager.Instance.loginOverlay.Hide();
			}
			if (PolytopiaPlayerPrefs.HasKey("cm_verification_uri"))
			{
				PolytopiaPlayerPrefs.DeleteKey("cm_verification_uri");
				PolytopiaPlayerPrefs.Save();
			}
			BasicPopup basicPopup3 = PopupManager.GetBasicPopup();
			basicPopup3.Header = Localization.Get("esport.verifyaccount.title");
			basicPopup3.Description = Localization.Get("esport.verifyaccount.description");
			basicPopup3.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected, delegate
				{
					base.State = ProcessState.Cancelled;
				}),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, async delegate
				{
					await VerifyAccountsAsync(ott);
				})
			};
			basicPopup3.Show();
		}
	}

	private async Task VerifyAccountsAsync(string ott)
	{
		ServerResponse<CmLinkAccountResponse> serverResponse = await PolytopiaBackendAdapter.Instance.LinkAccountAsync(ott);
		if (!serverResponse.Success || !serverResponse.Data.SuccessfullyLinkedAccount)
		{
			base.State = ProcessState.Failed;
			return;
		}
		Log.Info("[DeepLinking] Successfully linked account: {0}", new object[1] { base.Uri });
		NotificationManager.Notify(Localization.Get("esport.verifyaccount.success"), Localization.Get("esport.verifyaccount.service.cm"));
		base.State = ProcessState.Processed;
	}
}
