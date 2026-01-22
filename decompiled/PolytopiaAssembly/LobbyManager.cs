using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using PolytopiaBackendBase.Game.BindingModels;

public class LobbyManager
{
	public struct CachedLobbyData
	{
		public LobbyGameViewModel viewModel;
	}

	private Dictionary<Guid, CachedLobbyData> cachedLobbies;

	private Task<ServerResponse<GetLobbyInvitationsViewModel>> updateTask;

	public bool HasLoadedCache => cachedLobbies != null;

	public void Initialize()
	{
		BackendEvents.OnReceivedLobbyUpdate += OnReceivedLobbyUpdate;
	}

	public void Destroy()
	{
		BackendEvents.OnReceivedLobbyUpdate -= OnReceivedLobbyUpdate;
	}

	private void OnReceivedLobbyUpdate(LobbyGameViewModel lobbyGameViewModel)
	{
		Log.Verbose("[LobbyManager] Lobby updated: {0}", new object[1] { lobbyGameViewModel.Id });
		if (lobbyGameViewModel.Participators != null && lobbyGameViewModel.Participators.Count == 0)
		{
			RemoveLobby(lobbyGameViewModel.Id);
		}
		else
		{
			AddOrUpdateLobby(lobbyGameViewModel);
		}
	}

	public async Task<List<LobbyGameViewModel>> GetLobbies(bool forceUpdate = false)
	{
		if (cachedLobbies == null || forceUpdate)
		{
			await UpdateLobbies();
		}
		List<LobbyGameViewModel> list = new List<LobbyGameViewModel>(cachedLobbies.Count);
		foreach (KeyValuePair<Guid, CachedLobbyData> cachedLobby in cachedLobbies)
		{
			if (cachedLobby.Value.viewModel != null)
			{
				list.Add(cachedLobby.Value.viewModel);
			}
		}
		return list;
	}

	public async Task<bool> UpdateLobbies()
	{
		if (cachedLobbies == null)
		{
			cachedLobbies = new Dictionary<Guid, CachedLobbyData>();
		}
		else
		{
			cachedLobbies.Clear();
		}
		if (updateTask == null || updateTask.IsCompleted)
		{
			Log.Info("[LobbyManager] Updating Lobbies...", Array.Empty<object>());
			updateTask = PolytopiaBackendAdapter.Instance.GetLobbiesInvitations();
		}
		else
		{
			Log.Verbose("[LobbyManager] Waiting for lobbies to update...", Array.Empty<object>());
		}
		ServerResponse<GetLobbyInvitationsViewModel> serverResponse = await updateTask;
		if (!serverResponse.Success)
		{
			NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponse));
			return false;
		}
		List<Guid> list = new List<Guid>(serverResponse.Data.Lobbies.Count);
		List<Guid> list2 = new List<Guid>(serverResponse.Data.Lobbies.Count);
		foreach (LobbyGameViewModel lobby in serverResponse.Data.Lobbies)
		{
			if (!IsUserParticipant(lobby, AccountManager.PlayerAccountId))
			{
				if (cachedLobbies.ContainsKey(lobby.Id))
				{
					list2.Add(lobby.Id);
				}
				continue;
			}
			if (cachedLobbies.TryGetValue(lobby.Id, out var value))
			{
				value.viewModel = lobby;
				cachedLobbies[lobby.Id] = value;
				Log.Verbose("[LobbyManager] Updated lobby: {0}", new object[1] { lobby.Id });
			}
			else
			{
				cachedLobbies.Add(lobby.Id, new CachedLobbyData
				{
					viewModel = lobby
				});
				Log.Verbose("[LobbyManager] Added lobby: {0}", new object[1] { lobby.Id });
			}
			list.Add(lobby.Id);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			if (cachedLobbies.ContainsKey(list2[i]))
			{
				Log.Verbose("[LobbyManager] Removing cached lobby: {0}", new object[1] { list2[i] });
				cachedLobbies.Remove(list2[i]);
			}
		}
		if (list.Count > 0)
		{
			PolytopiaBackendAdapter.Instance.SubscribeToLobby(new SubscribeToLobbyBindingModel
			{
				LobbyIds = list,
				Subscribe = true
			}).WrapErrors();
		}
		if (list2.Count > 0)
		{
			PolytopiaBackendAdapter.Instance.SubscribeToLobby(new SubscribeToLobbyBindingModel
			{
				LobbyIds = list2,
				Subscribe = false
			}).WrapErrors();
		}
		await new WaitForUpdate();
		return true;
	}

	public void AddOrUpdateLobby(LobbyGameViewModel lobby)
	{
		if (cachedLobbies == null)
		{
			cachedLobbies = new Dictionary<Guid, CachedLobbyData>();
		}
		if (!IsUserParticipant(lobby, AccountManager.PlayerAccountId))
		{
			RemoveLobby(lobby.Id);
			return;
		}
		if (cachedLobbies.TryGetValue(lobby.Id, out var value))
		{
			value.viewModel = lobby;
			cachedLobbies[lobby.Id] = value;
			Log.Verbose("[LobbyManager] Updated lobby: {0}", new object[1] { lobby.Id });
		}
		else
		{
			cachedLobbies.Add(lobby.Id, new CachedLobbyData
			{
				viewModel = lobby
			});
			PolytopiaBackendAdapter.Instance.SubscribeToLobby(new SubscribeToLobbyBindingModel
			{
				LobbyIds = new List<Guid> { lobby.Id },
				Subscribe = true
			}).WrapErrors();
			Log.Verbose("[LobbyManager] Added lobby: {0}", new object[1] { lobby.Id });
		}
		Log.Spam("[LobbyManager] Participators:", Array.Empty<object>());
		for (int i = 0; i < lobby.Participators.Count; i++)
		{
			ParticipatorViewModel participatorViewModel = lobby.Participators[i];
			Log.Spam(" - {0} (State: {1}, Id: {2})", new object[3] { participatorViewModel.Name, participatorViewModel.InvitationState, participatorViewModel.UserId });
		}
		BackendEvents.LobbiesUpdated();
	}

	public bool TryGetCachedLobby(Guid lobbyId, out LobbyGameViewModel lobbyGameViewModel)
	{
		lobbyGameViewModel = null;
		if (cachedLobbies != null && cachedLobbies.TryGetValue(lobbyId, out var value))
		{
			lobbyGameViewModel = value.viewModel;
			return true;
		}
		return false;
	}

	public void RemoveLobby(Guid lobbyId)
	{
		if (cachedLobbies != null && cachedLobbies.Count != 0)
		{
			if (cachedLobbies.ContainsKey(lobbyId))
			{
				Log.Verbose("[LobbyManager] Removing cached lobby: {0}", new object[1] { lobbyId });
				cachedLobbies.Remove(lobbyId);
			}
			PolytopiaBackendAdapter.Instance.SubscribeToLobby(new SubscribeToLobbyBindingModel
			{
				LobbyIds = new List<Guid> { lobbyId },
				Subscribe = false
			}).WrapErrors();
			BackendEvents.LobbiesUpdated();
		}
	}

	public static bool IsUserParticipant(LobbyGameViewModel lobby, Guid userId)
	{
		if (lobby != null && lobby.Participators != null && lobby.Participators.Count > 0)
		{
			for (int i = 0; i < lobby.Participators.Count; i++)
			{
				ParticipatorViewModel participatorViewModel = lobby.Participators[i];
				if (participatorViewModel.UserId == userId && participatorViewModel.InvitationState != PlayerInvitationState.Declined && participatorViewModel.InvitationState != PlayerInvitationState.Done)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static ParticipatorViewModel GetLocalParticipator(LobbyGameViewModel lobby)
	{
		for (int i = 0; i < lobby.Participators.Count; i++)
		{
			ParticipatorViewModel participatorViewModel = lobby.Participators[i];
			if (participatorViewModel.UserId == AccountManager.PlayerAccountId)
			{
				return participatorViewModel;
			}
		}
		return null;
	}

	public static ParticipatorViewModel GetOwnerParticipator(LobbyGameViewModel lobby)
	{
		for (int i = 0; i < lobby.Participators.Count; i++)
		{
			ParticipatorViewModel participatorViewModel = lobby.Participators[i];
			if (participatorViewModel.UserId == lobby.OwnerId)
			{
				return participatorViewModel;
			}
		}
		return null;
	}

	public static int GetActiveParticipatorsCount(LobbyGameViewModel lobby)
	{
		int num = lobby.Bots.Count;
		for (int i = 0; i < lobby.Participators.Count; i++)
		{
			ParticipatorViewModel participatorViewModel = lobby.Participators[i];
			if (participatorViewModel.InvitationState == PlayerInvitationState.Accepted || participatorViewModel.InvitationState == PlayerInvitationState.Invited)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetAcceptedPlayersCount(LobbyGameViewModel lobby)
	{
		int num = 0;
		for (int i = 0; i < lobby.Participators.Count; i++)
		{
			if (lobby.Participators[i].InvitationState == PlayerInvitationState.Accepted)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetWaitingForPlayersCount(LobbyGameViewModel lobby)
	{
		int num = 0;
		for (int i = 0; i < lobby.Participators.Count; i++)
		{
			if (lobby.Participators[i].InvitationState == PlayerInvitationState.Invited)
			{
				num++;
			}
		}
		return num;
	}

	public static bool CanBeStartedByPlayer(LobbyGameViewModel lobby, Guid userId)
	{
		int acceptedPlayersCount = GetAcceptedPlayersCount(lobby);
		if (acceptedPlayersCount < 2)
		{
			return false;
		}
		if (!HasOwner(lobby))
		{
			bool flag = GetWaitingForPlayersCount(lobby) != 0;
			bool flag2 = acceptedPlayersCount == lobby.OpponentCount + 1;
			if (lobby.StartTime.HasValue && !(DateTime.UtcNow >= lobby.StartTime.Value))
			{
				if (!flag)
				{
					return !lobby.MatchmakingGameId.HasValue || flag2;
				}
				return false;
			}
			return true;
		}
		return lobby.OwnerId == userId;
	}

	public static string GetLobbyDescription(LobbyGameViewModel lobby, Guid userId)
	{
		ParticipatorViewModel localParticipator = GetLocalParticipator(lobby);
		if (localParticipator == null || localParticipator.InvitationState != PlayerInvitationState.Accepted)
		{
			return Localization.Get("gameitem.join");
		}
		int waitingForPlayersCount = GetWaitingForPlayersCount(lobby);
		int acceptedPlayersCount = GetAcceptedPlayersCount(lobby);
		bool flag = acceptedPlayersCount + waitingForPlayersCount == lobby.OpponentCount + 1;
		if (lobby.MatchmakingGameId.HasValue && !flag)
		{
			return Localization.Get("matchmakinggameinfo.waitingforplayers2");
		}
		if (waitingForPlayersCount > 0)
		{
			string arg = ((waitingForPlayersCount == 1) ? Localization.Get("misc.player") : Localization.Get("misc.players"));
			return Localization.Get("onlineview.lobby.waitingforplayers", waitingForPlayersCount, arg);
		}
		if (acceptedPlayersCount > 1)
		{
			if (CanBeStartedByPlayer(lobby, userId))
			{
				return Localization.Get("gameitem.ready");
			}
			ParticipatorViewModel ownerParticipator = GetOwnerParticipator(lobby);
			if (ownerParticipator == null)
			{
				return Localization.Get("gameitem.ready.wait.noowner");
			}
			return Localization.Get("gameitem.ready.wait", ownerParticipator.Name);
		}
		return Localization.Get("gamesettings.notenoughplayers");
	}

	public static bool HasOwner(LobbyGameViewModel lobby)
	{
		_ = lobby.OwnerId;
		return lobby.OwnerId != Guid.Empty;
	}
}
