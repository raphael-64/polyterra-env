using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using UnityEngine.SceneManagement;

public class JoinLobbyIntent : Intent
{
	private TaskCompletionSource<bool> taskCompletion;

	private Guid lobbyId;

	protected bool shouldFadeOut;

	public JoinLobbyIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return intent.Uri != base.Uri;
	}

	public override async Task HandleAsync()
	{
		await DeepLinkJoinLobby();
		Log.Info("[DeepLinking] processed intent.", Array.Empty<object>());
		base.State = ProcessState.Processed;
	}

	private bool IsCancelled()
	{
		return base.State == ProcessState.Cancelled;
	}

	private bool ShouldStopProcessing()
	{
		if (base.State != ProcessState.Cancelled)
		{
			return base.State == ProcessState.Processed;
		}
		return true;
	}

	private async Task<bool> DeepLinkJoinLobby()
	{
		Dictionary<string, string> queryMap = base.QueryMap;
		lobbyId = ParseLobbyId(queryMap);
		if (lobbyId == Guid.Empty)
		{
			Log.Warning("[DeepLinking] Failed to open lobby: invalid id {0}", new object[1] { lobbyId });
			return false;
		}
		Scene activeScene = SceneManager.GetActiveScene();
		bool num = ((Scene)(ref activeScene)).name == "Level";
		taskCompletion = new TaskCompletionSource<bool>();
		if (num)
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			GameManager.ReturnToMenu();
		}
		else
		{
			bool result = await JoinLobby(lobbyId);
			taskCompletion.SetResult(result);
		}
		return taskCompletion.Task.Result;
	}

	private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (ShouldStopProcessing())
		{
			Log.Verbose("[DeepLinking] Cancelled while loading start scene", Array.Empty<object>());
			taskCompletion.SetResult(result: false);
		}
		else if (((Scene)(ref scene)).name != "StartScene")
		{
			Log.Error("[DeepLinking] Failed to join lobby: Failed to return to start scene", Array.Empty<object>());
			taskCompletion.SetResult(result: false);
		}
		else
		{
			bool result = await JoinLobby(lobbyId);
			taskCompletion.SetResult(result);
		}
	}

	protected async Task<bool> EnsureLoggedIn()
	{
		if (!PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await GameManager.GetLoginManager().LoginAsync();
			if (ShouldStopProcessing())
			{
				Log.Verbose("[DeepLinking] Cancelled while logging in", Array.Empty<object>());
				return false;
			}
			if (!PolytopiaBackendAdapter.Instance.IsConnected)
			{
				Log.Warning("[DeepLinking] Failed to join lobby: Could not log in", Array.Empty<object>());
				return false;
			}
		}
		return true;
	}

	protected virtual async Task<bool> JoinLobby(Guid lobbyId)
	{
		UIManager.OpenMultiplayerScreen();
		if (!(await EnsureLoggedIn()))
		{
			return false;
		}
		Log.Verbose("[DeepLinking] Getting lobby data...", Array.Empty<object>());
		NetworkUtils.ShowLoader(Localization.Get("onlineview.lobby.joining"));
		ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetLobby(new GetLobbyBindingModel
		{
			LobbyId = lobbyId
		});
		NetworkUtils.HideLoader();
		if (!serverResponse.Success)
		{
			Log.Warning("[DeepLinking] Unable to find lobby data", Array.Empty<object>());
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
			return false;
		}
		LobbyGameViewModel data = serverResponse.Data;
		if (data != null && data.Id != Guid.Empty)
		{
			ParticipatorViewModel localParticipator = LobbyManager.GetLocalParticipator(data);
			if (localParticipator != null && localParticipator.InvitationState == PlayerInvitationState.Done)
			{
				BasicPopup basicPopup = PopupManager.GetBasicPopup();
				basicPopup.Header = Localization.Get("onlineview.lobby.kicked.title");
				basicPopup.Description = Localization.Get("onlineview.lobby.kicked", data.Name);
				basicPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
				};
				basicPopup.Show();
			}
			else
			{
				LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
				lobbyPopup.SetData(data);
				lobbyPopup.Show();
			}
			return true;
		}
		Log.Warning("[DeepLinking] Unable to find lobby data", Array.Empty<object>());
		return false;
	}

	private Guid ParseLobbyId(Dictionary<string, string> queryVariables)
	{
		if (queryVariables.TryGetValue("id", out var value) && Guid.TryParse(value, out var result))
		{
			return result;
		}
		return Guid.Empty;
	}
}
