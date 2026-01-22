using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenGameIntent : Intent
{
	private TaskCompletionSource<bool> taskCompletion;

	private Guid gameId;

	protected bool shouldFadeOut;

	public OpenGameIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return intent.Uri != base.Uri;
	}

	public override async Task HandleAsync()
	{
		bool result = await DeepLinkOpenGame();
		if (shouldFadeOut)
		{
			UIBlackFader.FadeOut(1f, delegate
			{
				if (!result && !IsCancelled())
				{
					NotificationManager.Notify(Localization.Get("misc.errorloadinggame"));
				}
			});
		}
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

	private Task<bool> DeepLinkOpenGame()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, string> queryMap = base.QueryMap;
		gameId = ParseGameId(queryMap);
		if (gameId == Guid.Empty)
		{
			Log.Error("Failed to open game: invalid game id {0}", new object[1] { gameId });
			return Task.FromResult(result: false);
		}
		Scene activeScene = SceneManager.GetActiveScene();
		bool isInLevel = ((Scene)(ref activeScene)).name == "Level";
		if (isInLevel)
		{
			ClientBase client = GameManager.Client;
			if (client != null && client.CurrentGameId.HasValue && GameManager.Client?.CurrentGameId.Value == gameId)
			{
				return Task.FromResult(result: true);
			}
		}
		if ((Object)(object)UIBlackFader.instance == (Object)null)
		{
			Log.Error("Failed to open game: Missing fader instance", Array.Empty<object>());
			return Task.FromResult(result: false);
		}
		taskCompletion = new TaskCompletionSource<bool>();
		UIBlackFader.FadeIn(0.5f, async delegate
		{
			shouldFadeOut = true;
			if (isInLevel)
			{
				SceneManager.sceneLoaded += OnSceneLoaded;
				GameManager.ReturnToMenu();
			}
			else
			{
				bool result = await OpenGame(gameId);
				taskCompletion.SetResult(result);
			}
		}, "gamesettings.loading", delegate
		{
			if (shouldFadeOut)
			{
				UIBlackFader.FadeOut(1f);
			}
			shouldFadeOut = false;
			Log.Verbose("Cancelled opening game intent", Array.Empty<object>());
			base.State = ProcessState.Cancelled;
		});
		return taskCompletion.Task;
	}

	private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		SceneManager.sceneLoaded -= OnSceneLoaded;
		Log.Verbose("Opening game intent finished loading scene", Array.Empty<object>());
		if (ShouldStopProcessing())
		{
			Log.Verbose("Cancelled while loading start scene", Array.Empty<object>());
			taskCompletion.SetResult(result: false);
		}
		else if (((Scene)(ref scene)).name != "StartScene")
		{
			Log.Error("Failed to open game: Failed to return to start scene", Array.Empty<object>());
			taskCompletion.SetResult(result: false);
		}
		else
		{
			bool result = await OpenGame(gameId);
			taskCompletion.SetResult(result);
		}
	}

	protected async Task<bool> EnsureLoggedIn()
	{
		if (!PolytopiaBackendAdapter.Instance.IsConnected)
		{
			await GameManager.GetLoginManager().LoginAsync();
			Log.Verbose("Opening game intent finished logging in", Array.Empty<object>());
			if (ShouldStopProcessing())
			{
				Log.Verbose("Cancelled while logging in", Array.Empty<object>());
				return false;
			}
			if (!PolytopiaBackendAdapter.Instance.IsConnected)
			{
				Log.Warning("Failed to open game: Could not log in", Array.Empty<object>());
				return false;
			}
		}
		return true;
	}

	protected virtual async Task<bool> OpenGame(Guid gameId)
	{
		if (!(await EnsureLoggedIn()))
		{
			return false;
		}
		ServerResponse<LobbyGameViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetLobby(new GetLobbyBindingModel
		{
			LobbyId = gameId
		});
		if (ShouldStopProcessing())
		{
			Log.Verbose("Cancelled while loading lobby", Array.Empty<object>());
			return false;
		}
		if (serverResponse.Success && serverResponse.Data != null && serverResponse.Data.Id != Guid.Empty)
		{
			Log.Verbose("Opening lobby...", Array.Empty<object>());
			LobbyGameViewModel data = serverResponse.Data;
			GameManager.GetLobbyManager().AddOrUpdateLobby(data);
			UIManager.OpenMultiplayerScreen();
			LobbyPopup lobbyPopup = PopupManager.GetLobbyPopup();
			lobbyPopup.SetData(data);
			lobbyPopup.Show();
			return true;
		}
		ServerResponse<GameSummaryViewModel> serverResponse2 = await PolytopiaBackendAdapter.Instance.GetGameSummaryViewModelByIdAsync(gameId);
		if (ShouldStopProcessing())
		{
			Log.Verbose("Cancelled while loading game", Array.Empty<object>());
			return false;
		}
		if (serverResponse2.Success && serverResponse2.Data != null)
		{
			Log.Verbose("Opening game...", Array.Empty<object>());
			GameSummaryViewModel data2 = serverResponse2.Data;
			GameManager.GetRemoteGameDataManager().AddOrUpdateGameSummary(data2);
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < data2.Participators.Count; i++)
			{
				ParticipatorViewModel participatorViewModel = data2.Participators[i];
				if (AccountManager.PlayerAccountId == participatorViewModel.UserId)
				{
					flag = participatorViewModel.InvitationState != PlayerInvitationState.Declined;
					break;
				}
			}
			if (data2.State == GameSessionState.Ended || flag2)
			{
				shouldFadeOut = false;
				if (!(await GameManager.Instance.OpenReplay(gameId, data2)))
				{
					shouldFadeOut = true;
					Log.Error("Failed to open game: OpenReplay failed", Array.Empty<object>());
					return false;
				}
				return true;
			}
			if (data2.State == GameSessionState.Started && flag)
			{
				shouldFadeOut = false;
				if (!(await GameManager.Instance.OpenMultiplayerGame(gameId)))
				{
					shouldFadeOut = true;
					Log.Error("Failed to open game: OpenMultiplayerGame failed", Array.Empty<object>());
					return false;
				}
				return true;
			}
			UIManager.OpenMultiplayerScreen();
			GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
			gameInfoPopup.SetData(data2);
			gameInfoPopup.Show();
			return true;
		}
		PopupManager.ShowErrorPopup($"Failed to open game: could not find game or lobby with id: {gameId}");
		Log.Error("Failed to open game: Could not find game or lobby with id {0}", new object[1] { gameId });
		return false;
	}

	private Guid ParseGameId(Dictionary<string, string> queryVariables)
	{
		if (queryVariables.TryGetValue("id", out var value) && Guid.TryParse(value, out var result))
		{
			return result;
		}
		return Guid.Empty;
	}
}
