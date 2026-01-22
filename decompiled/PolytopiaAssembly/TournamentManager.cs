using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Challengermode.GameIntegration.BindingModels;
using UnityEngine;

public class TournamentManager
{
	public delegate void OnTournamentUpdatedEvent(TournamentViewModel tournamentViewModel);

	public struct CachedTournamentData
	{
		public TournamentViewModel viewModel;
	}

	private static bool USE_DEBUG_DATA;

	private Guid? challengermodeAccountId;

	private Dictionary<Guid, CachedTournamentData> cachedTournaments;

	private Dictionary<Guid, PlayerData> cachedFriends;

	private Task<ServerResponse<TournamentListViewModel>> updateTask;

	public bool HasLoadedCache => cachedTournaments != null;

	public static event OnTournamentUpdatedEvent OnTournamentUpdated;

	public static void TournamentUpdated(TournamentViewModel tournamentViewModel)
	{
		TournamentManager.OnTournamentUpdated?.Invoke(tournamentViewModel);
	}

	public void Initialize()
	{
	}

	public void Destroy()
	{
	}

	public async Task<bool> ConnectChallengermodeAccount(bool reconnect = false, Guid? tournamentId = null)
	{
		Log.Info("[TournamentManager] Connecting challengermode account...", Array.Empty<object>());
		if (reconnect)
		{
			return await ReconnectChallengermodeAccount(tournamentId);
		}
		return await ConnectChallengermodeAccountOAuth(tournamentId);
	}

	private async Task<bool> ConnectChallengermodeAccountOAuth(Guid? tournamentId = null)
	{
		Log.Info("[TournamentManager] Starting OAuth flow...", Array.Empty<object>());
		NetworkUtils.ShowLoader();
		ServerResponse<CmStartOAuthFlowResponse> serverResponse = await PolytopiaBackendAdapter.Instance.StartOauthFlow(new CmStartOAuthFlowBindingModel
		{
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform(),
			TournamentId = tournamentId
		});
		NetworkUtils.HideLoader();
		if (serverResponse.Success)
		{
			NativeHelpers.OpenURL(serverResponse.Data.Url);
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
		return serverResponse.Success;
	}

	private async Task<bool> ReconnectChallengermodeAccount(Guid? tournamentId = null)
	{
		Log.Info("[TournamentManager] Reconnecting account...", Array.Empty<object>());
		NetworkUtils.ShowLoader();
		ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.ConnectChallengermode(new CmConnectChallengermodeBindingModel());
		NetworkUtils.HideLoader();
		if (serverResponse.Success)
		{
			Log.Info("[TournamentManager] Successfully linked account.", Array.Empty<object>());
			NotificationManager.Notify(Localization.Get("esport.verifyaccount.success"), Localization.Get("esport.verifyaccount.service.cm"));
			BackendEvents.AccountLinked(success: true);
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(Localization.GetErrorMessage(serverResponse.ErrorCode, serverResponse.ErrorMessage));
		}
		return serverResponse.Success;
	}

	public async Task<bool> JoinTournament(Guid tournamentId)
	{
		Log.Info("TournamentManager.JoinTournament({0})", new object[1] { tournamentId });
		TournamentTopMemberViewModel overflowMember = null;
		TemporarilySetHasSignedUp(tournamentId, hasSignedUp: true, ref overflowMember);
		ChallengermodeConnectionStatus connectionStatus = await GetConnectionStatus();
		if (connectionStatus == null)
		{
			TemporarilySetHasSignedUp(tournamentId, hasSignedUp: false, ref overflowMember);
			return false;
		}
		if (!connectionStatus.IsConnected)
		{
			if (!(await ConnectChallengermodeAccount(connectionStatus.IsAnotherAccountConnected, tournamentId)))
			{
				TemporarilySetHasSignedUp(tournamentId, hasSignedUp: false, ref overflowMember);
				return false;
			}
			if (!connectionStatus.IsAnotherAccountConnected)
			{
				Log.Info("[TournamentManager] Waiting for OAuth flow...", Array.Empty<object>());
				TemporarilySetHasSignedUp(tournamentId, hasSignedUp: false, ref overflowMember);
				return false;
			}
		}
		ServerResponse<ResponseViewModel> response = await PolytopiaBackendAdapter.Instance.SignupToTournament(new SignupTournamentBindingModel
		{
			TournamentId = tournamentId
		});
		if (response.Success)
		{
			Log.Info("TournamentManager.JoinTournament: Success", Array.Empty<object>());
			await UpdateTournament(tournamentId);
			if (cachedTournaments != null && cachedTournaments.TryGetValue(tournamentId, out var value))
			{
				TournamentUpdated(value.viewModel);
			}
		}
		else
		{
			TemporarilySetHasSignedUp(tournamentId, hasSignedUp: false, ref overflowMember);
			HandleTournamentError(tournamentId, response);
		}
		return response.Success;
	}

	private void TemporarilySetHasSignedUp(Guid tournamentId, bool hasSignedUp, ref TournamentTopMemberViewModel overflowMember)
	{
		if (!cachedTournaments.TryGetValue(tournamentId, out var value))
		{
			return;
		}
		if (hasSignedUp)
		{
			if (value.viewModel.NumberRegistered > value.viewModel.TopMembers.Count && value.viewModel.TopMembers.Count > 0)
			{
				int index = value.viewModel.TopMembers.Count - 1;
				overflowMember = value.viewModel.TopMembers[index];
				value.viewModel.TopMembers.RemoveAt(index);
			}
			value.viewModel.NumberRegistered++;
			value.viewModel.TopMembers.Insert(0, UserToTournament(AccountManager.UserModel, isConfirmed: false));
		}
		else
		{
			if (overflowMember != null)
			{
				value.viewModel.TopMembers.Add(overflowMember);
			}
			value.viewModel.NumberRegistered--;
			TemporarilyRemoveSelf(value.viewModel.TopMembers);
		}
		value.viewModel.PersonalViewModel.HasSignedUp = hasSignedUp;
		TournamentUpdated(value.viewModel);
	}

	private TournamentTopMemberViewModel TemporarilyRemoveSelf(List<TournamentTopMemberViewModel> topMembers)
	{
		for (int i = 0; i < topMembers.Count; i++)
		{
			if (topMembers[i].PolytopiaUserId == AccountManager.PlayerAccountId)
			{
				TournamentTopMemberViewModel result = topMembers[i];
				topMembers.RemoveAt(i);
				return result;
			}
		}
		return null;
	}

	public async Task<bool> ConfirmTournamentParticipation(Guid tournamentId)
	{
		Log.Info("TournamentManager.ConfirmTournamentParticipation({0})", new object[1] { tournamentId });
		TemporarilySetHasConfirmed(tournamentId, hasConfirmed: true);
		ChallengermodeConnectionStatus connectionStatus = await GetConnectionStatus();
		if (connectionStatus == null)
		{
			TemporarilySetHasConfirmed(tournamentId, hasConfirmed: false);
			return false;
		}
		if (!connectionStatus.IsConnected)
		{
			if (!(await ConnectChallengermodeAccount(connectionStatus.IsAnotherAccountConnected, tournamentId)))
			{
				TemporarilySetHasConfirmed(tournamentId, hasConfirmed: false);
				return false;
			}
			if (!connectionStatus.IsAnotherAccountConnected)
			{
				Log.Info("[TournamentManager] Waiting for OAuth flow...", Array.Empty<object>());
				TemporarilySetHasConfirmed(tournamentId, hasConfirmed: false);
				return false;
			}
		}
		ServerResponse<ResponseViewModel> response = await PolytopiaBackendAdapter.Instance.ConfirmTournament(new ConfirmTournamentBindingModel
		{
			TournamentId = tournamentId
		});
		if (response.Success)
		{
			Log.Info("TournamentManager.ConfirmTournamentParticipation: Success", Array.Empty<object>());
			await UpdateTournament(tournamentId);
			if (cachedTournaments.TryGetValue(tournamentId, out var value))
			{
				TournamentUpdated(value.viewModel);
			}
		}
		else
		{
			TemporarilySetHasConfirmed(tournamentId, hasConfirmed: false);
			HandleTournamentError(tournamentId, response);
		}
		return response.Success;
	}

	private void TemporarilySetHasConfirmed(Guid tournamentId, bool hasConfirmed)
	{
		if (cachedTournaments.TryGetValue(tournamentId, out var value))
		{
			value.viewModel.PersonalViewModel.HasConfirmed = hasConfirmed;
			TemporarilySetHasConfirmed(value.viewModel.TopMembers, hasConfirmed);
			TournamentUpdated(value.viewModel);
		}
	}

	private void TemporarilySetHasConfirmed(List<TournamentTopMemberViewModel> topMembers, bool hasConfirmed)
	{
		for (int i = 0; i < topMembers.Count; i++)
		{
			if (topMembers[i].PolytopiaUserId == AccountManager.PlayerAccountId)
			{
				topMembers[i].IsConfirmed = hasConfirmed;
			}
		}
	}

	public async Task<bool> LeaveTournament(Guid tournamentId)
	{
		TournamentTopMemberViewModel deletedModel = null;
		TemporarilySetSignedUpAndConfirmed(tournamentId, newSignedUp: false, newConfirmed: false, out var wasSignedUp, out var wasConfirmed, ref deletedModel);
		Log.Info("TournamentManager.LeaveTournament({0})", new object[1] { tournamentId });
		ServerResponse<ResponseViewModel> response = await PolytopiaBackendAdapter.Instance.LeaveTournament(new LeaveTournamentBindingModel
		{
			TournamentId = tournamentId
		});
		if (response.Success)
		{
			Log.Info("TournamentManager.LeaveTournament: Success", Array.Empty<object>());
			await UpdateTournament(tournamentId);
			if (cachedTournaments.TryGetValue(tournamentId, out var value))
			{
				TournamentUpdated(value.viewModel);
			}
		}
		else
		{
			TemporarilySetSignedUpAndConfirmed(tournamentId, wasSignedUp, wasConfirmed, out var _, out var _, ref deletedModel);
			HandleTournamentError(tournamentId, response);
		}
		return response.Success;
	}

	private void TemporarilySetSignedUpAndConfirmed(Guid tournamentId, bool newSignedUp, bool newConfirmed, out bool oldSignedUp, out bool oldConfirmed, ref TournamentTopMemberViewModel deletedModel)
	{
		oldSignedUp = false;
		oldConfirmed = false;
		if (cachedTournaments.TryGetValue(tournamentId, out var value))
		{
			oldSignedUp = value.viewModel.PersonalViewModel.HasSignedUp;
			oldConfirmed = value.viewModel.PersonalViewModel.HasConfirmed;
			value.viewModel.PersonalViewModel.HasSignedUp = newSignedUp;
			value.viewModel.PersonalViewModel.HasConfirmed = newConfirmed;
			if (oldSignedUp && !newSignedUp)
			{
				value.viewModel.NumberRegistered--;
				deletedModel = TemporarilyRemoveSelf(value.viewModel.TopMembers);
			}
			else if (!oldSignedUp && (newSignedUp & (deletedModel != null)))
			{
				value.viewModel.NumberRegistered++;
				value.viewModel.TopMembers.Insert(0, deletedModel);
			}
			TournamentUpdated(value.viewModel);
		}
	}

	public async Task<ChallengermodeConnectionStatus> GetConnectionStatus()
	{
		ServerResponse<ChallengermodeConnectionStatus> serverResponse = await PolytopiaBackendAdapter.Instance.GetChallengermodeConnectionStatus();
		if (serverResponse.Success)
		{
			challengermodeAccountId = serverResponse.Data.ChallengermodeUserId;
			return serverResponse.Data;
		}
		NetworkUtils.ShowLoaderError(serverResponse.ErrorMessage);
		return null;
	}

	public async Task<bool> IsTournamentJoined(Guid tournamentId)
	{
		if (cachedTournaments == null)
		{
			cachedTournaments = new Dictionary<Guid, CachedTournamentData>();
		}
		if (cachedTournaments.TryGetValue(tournamentId, out var value))
		{
			return value.viewModel.PersonalViewModel.HasSignedUp;
		}
		await UpdateTournament(tournamentId);
		if (cachedTournaments.TryGetValue(tournamentId, out value))
		{
			return value.viewModel.PersonalViewModel.HasSignedUp;
		}
		return false;
	}

	public async Task<TournamentViewModel> GetTournament(Guid tournamentId, bool forceUpdate = false)
	{
		Log.Verbose("TournamentManager.GetTournamentData: {0}", new object[1] { tournamentId });
		if (cachedTournaments == null)
		{
			cachedTournaments = new Dictionary<Guid, CachedTournamentData>();
		}
		if (!cachedTournaments.TryGetValue(tournamentId, out var value) && !forceUpdate)
		{
			await UpdateTournament(tournamentId);
			cachedTournaments.TryGetValue(tournamentId, out value);
			return value.viewModel;
		}
		return value.viewModel;
	}

	public async Task<List<TournamentViewModel>> GetTournaments(bool forceUpdate = false)
	{
		if (cachedTournaments == null || forceUpdate)
		{
			await UpdateTournaments();
		}
		List<TournamentViewModel> list = new List<TournamentViewModel>(cachedTournaments.Count);
		foreach (KeyValuePair<Guid, CachedTournamentData> cachedTournament in cachedTournaments)
		{
			if (cachedTournament.Value.viewModel != null)
			{
				list.Add(cachedTournament.Value.viewModel);
			}
		}
		return list;
	}

	public async Task<bool> UpdateTournaments()
	{
		if (cachedTournaments == null)
		{
			cachedTournaments = new Dictionary<Guid, CachedTournamentData>();
		}
		if (USE_DEBUG_DATA)
		{
			await PopulateDebugData();
			return true;
		}
		if (updateTask == null || updateTask.IsCompleted)
		{
			Log.Info("[TournamentManager] Updating Tournaments...", Array.Empty<object>());
			updateTask = PolytopiaBackendAdapter.Instance.GetTournamentList(new GetTournamentListBindingModel
			{
				Platform = PolytopiaBackendAdapter.GetCurrentPlatform()
			});
		}
		else
		{
			Log.Verbose("[TournamentManager] Waiting for tournaments to update...", Array.Empty<object>());
		}
		ServerResponse<TournamentListViewModel> serverResponse = await updateTask;
		if (!serverResponse.Success)
		{
			NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(serverResponse));
			return false;
		}
		cachedTournaments.Clear();
		foreach (TournamentViewModel tournament in serverResponse.Data.Tournaments)
		{
			AddOrUpdateTournament(tournament);
		}
		await new WaitForUpdate();
		return true;
	}

	public async Task UpdateTournament(Guid tournamentId)
	{
		Log.Info("[TournamentManager] Updating data for tournament: {0}", new object[1] { tournamentId });
		ServerResponse<TournamentViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetTournament(tournamentId);
		if (serverResponse.Success)
		{
			AddOrUpdateTournament(serverResponse.Data);
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	public void UpdateTournament(TournamentViewModel tournament)
	{
		AddOrUpdateTournament(tournament);
	}

	private void AddOrUpdateTournament(TournamentViewModel tournamentViewModel)
	{
		CachedTournamentData value = new CachedTournamentData
		{
			viewModel = tournamentViewModel
		};
		if (cachedTournaments == null)
		{
			cachedTournaments = new Dictionary<Guid, CachedTournamentData>();
		}
		if (cachedTournaments.ContainsKey(tournamentViewModel.Id))
		{
			cachedTournaments[tournamentViewModel.Id] = value;
		}
		else
		{
			cachedTournaments.Add(tournamentViewModel.Id, value);
		}
	}

	private void HandleTournamentError(Guid tournamentId, ServerResponse<ResponseViewModel> response)
	{
		switch (response.ErrorCode)
		{
		case ErrorCode.CmParseIntent:
		case ErrorCode.CmReportGameFailed:
		case ErrorCode.CmGameAccountAlreadyLinked:
		case ErrorCode.CmRefreshTokenExpired:
		case ErrorCode.CmUserAuthenticationFailed:
		case ErrorCode.CmUserAccountAlreadyUsed:
		case ErrorCode.CmGameAccountNotLinked:
		case ErrorCode.CmTriedToResendAuthCode:
		case ErrorCode.CmStateProhibitsOperation:
			PopupManager.ShowErrorPopup(Localization.GetErrorMessage(response.ErrorCode));
			break;
		case ErrorCode.CmNotPossibleInGame:
		case ErrorCode.CmActionNotAllowed:
		case ErrorCode.CmTechnicalError:
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("misc.error.title");
			basicPopup.Description = Localization.GetErrorMessage(response.ErrorCode);
			if (cachedTournaments.TryGetValue(tournamentId, out var data))
			{
				basicPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
					{
						NativeHelpers.OpenURL(data.viewModel.OverviewUrl);
					})
				};
			}
			else
			{
				basicPopup.buttonData = new PopupBase.PopupButtonData[1]
				{
					new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
				};
			}
			basicPopup.Show();
			break;
		}
		default:
			PopupManager.ShowErrorPopup(response.ErrorMessage);
			break;
		}
	}

	public async Task PopulateDebugData()
	{
		Log.Info("[TournamentManager] Updating Tournaments (Dummy data)...", Array.Empty<object>());
		if (cachedTournaments != null)
		{
			cachedTournaments.Clear();
		}
		else
		{
			cachedTournaments = new Dictionary<Guid, CachedTournamentData>();
		}
		Random rng = new Random((int)Time.time);
		for (int i = 0; i < 8; i++)
		{
			Guid tournamentId = Guid.NewGuid();
			TournamentState state = ((Random.Range(0, 2) != 1) ? TournamentState.Published : TournamentState.Running);
			DateTime startTime = DateTime.UtcNow + TimeSpan.FromMinutes(Random.Range(0.1f, 5f));
			DateTime readyTime = startTime - TimeSpan.FromMinutes(Random.Range(0, 3));
			int totalSlots = ((Random.Range(0, 2) == 1) ? 64 : 128);
			bool flag = Random.Range(0, 2) == 1;
			TournamentPersonalViewModel personalInfo = new TournamentPersonalViewModel
			{
				HasSignedUp = flag,
				HasConfirmed = (flag && Random.Range(0, 2) == 1)
			};
			List<TournamentTopMemberViewModel> topMembers = new List<TournamentTopMemberViewModel>();
			if (personalInfo.HasSignedUp)
			{
				topMembers.Add(UserToTournament(AccountManager.UserModel, personalInfo.HasConfirmed));
			}
			if (Random.Range(0, 2) == 1 && AccountManager.GetFriendCount() > 0)
			{
				foreach (PolytopiaFriendViewModel item in await AccountManager.GetFriendViewModels())
				{
					if (Random.Range(0, 2) != 1)
					{
						topMembers.Add(UserToTournament(item.User, Random.Range(0, 2) == 1));
					}
				}
			}
			TournamentViewModel tournamentViewModel = new TournamentViewModel
			{
				Id = tournamentId,
				Name = PolyLanguage.MakeGameName(makeCrazyName: false, rng),
				DateCreated = DateTime.UtcNow,
				State = state,
				OverviewUrl = "http://guessthe.game",
				ContactUrl = "http://guessthe.game",
				TournamentFormat = TournamentFormat.SingleElimination,
				Members = new List<TournamentMemberViewModel>(),
				TopMembers = topMembers,
				TotalSlots = totalSlots,
				AvailableSlots = Random.Range(0, totalSlots + 1),
				NumberRegistered = Random.Range(0, 1000),
				PersonalViewModel = personalInfo,
				ScheduledStartTime = startTime,
				ReadyTime = readyTime,
				StartTime = startTime + TimeSpan.FromMinutes(Random.Range(1, 10))
			};
			AddOrUpdateTournament(tournamentViewModel);
		}
	}

	private TournamentTopMemberViewModel UserToTournament(PolytopiaUserViewModel user, bool isConfirmed)
	{
		return new TournamentTopMemberViewModel
		{
			Alias = user.Alias,
			AvatarStateData = user.AvatarStateData,
			LastLoginDate = user.LastLoginDate,
			GameVersions = user.GameVersions,
			MultiplayerRating = user.MultiplayerRating,
			NumMultiplayergames = user.NumMultiplayergames,
			NumFriends = user.NumFriends,
			NumGames = user.NumGames,
			UserName = user.UserName,
			PolytopiaUserId = user.PolytopiaId,
			ChallengermodeUserId = Guid.Empty,
			IsConfirmed = isConfirmed
		};
	}
}
