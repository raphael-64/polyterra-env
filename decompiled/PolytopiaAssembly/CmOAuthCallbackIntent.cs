using System;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode.GameIntegration.BindingModels;

public class CmOAuthCallbackIntent : Intent
{
	public CmOAuthCallbackIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return !(intent is CmOAuthCallbackIntent);
	}

	public override async Task HandleAsync()
	{
		base.State = ProcessState.Processing;
		if (!base.QueryMap.TryGetValue("code", out var code))
		{
			base.State = ProcessState.Failed;
			return;
		}
		BasicPopup popup = PopupManager.GetBasicPopup();
		popup.Header = Localization.Get("challengermode.connect.process.header");
		popup.Show();
		if (!PolytopiaBackendAdapter.Instance.IsAuthenticated)
		{
			popup.Description = Localization.Get("challengermode.connect.process.login");
			await GameManager.GetLoginManager().LoginAsync(silent: false);
		}
		if (PolytopiaBackendAdapter.Instance.IsConnected)
		{
			popup.Description = Localization.Get("challengermode.connect.process.finishing");
			if (await SubmitAuthcodeAsync(code) && base.QueryMap.TryGetValue("state", out var value))
			{
				Log.Info("[DeepLinking] Got state parameter: {0}", new object[1] { value });
				string[] array = value.Split(new string[1] { "%3A" }, StringSplitOptions.None);
				if (array.Length >= 2 && array[0] == "tournament" && Guid.TryParse(array[1], out var tournamentId))
				{
					UIManager.OpenTournamentsScreen();
					if (!(await GameManager.GetTournamentManager().IsTournamentJoined(tournamentId)))
					{
						await GameManager.GetTournamentManager().JoinTournament(tournamentId);
					}
					else
					{
						await GameManager.GetTournamentManager().ConfirmTournamentParticipation(tournamentId);
					}
				}
			}
		}
		else
		{
			base.State = ProcessState.Processed;
		}
		popup.Hide();
	}

	private async Task<bool> SubmitAuthcodeAsync(string authCode)
	{
		Log.Info("[DeepLinking] processing oauth code", Array.Empty<object>());
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.ConnectChallengermode(new CmConnectChallengermodeBindingModel
		{
			AuthCode = authCode,
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform()
		});
		if (serverResponse.Success)
		{
			Log.Info("[DeepLinking] Successfully linked account.", Array.Empty<object>());
			NotificationManager.Notify(Localization.Get("esport.verifyaccount.success"), Localization.Get("esport.verifyaccount.service.cm"));
			BackendEvents.AccountLinked(success: true);
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(Localization.GetErrorMessage(serverResponse.ErrorCode, serverResponse.ErrorMessage));
		}
		base.State = ProcessState.Processed;
		return serverResponse.Success;
	}
}
