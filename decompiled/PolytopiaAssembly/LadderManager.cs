using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Challengermode.GameIntegration.BindingModels;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class LadderManager
{
	public delegate void OnLadderStateUpdatedEvent(LadderViewModel ladder);

	public struct CachedLadderData
	{
		public LadderViewModel ladderViewModel;

		public List<LadderPlacementViewModel> ladderPlacements;

		public LadderPlacementViewModel localPlayerPlacement;

		public bool hasLocalPlayerJoined;
	}

	private Guid? challengermodeAccountId;

	private Dictionary<Guid, CachedLadderData> cachedLadderData;

	public static event OnLadderStateUpdatedEvent OnLadderStateUpdated;

	public static void LadderStateUpdated(LadderViewModel ladder)
	{
		LadderManager.OnLadderStateUpdated?.Invoke(ladder);
	}

	public async Task<bool> IsAccountConnected()
	{
		return (await GetConnectionStatus())?.IsConnected ?? false;
	}

	public async Task<bool> ConnectAccount(Guid? ladderId = null)
	{
		Log.Info("LadderManager.ConnectAccount({0})...", new object[1] { ladderId.HasValue ? ladderId.Value.ToString() : "" });
		ServerResponse<CmStartOAuthFlowResponse> serverResponse = await PolytopiaBackendAdapter.Instance.StartOauthFlow(new CmStartOAuthFlowBindingModel
		{
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform(),
			LadderId = ladderId
		});
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

	public async Task<bool> IsLadderJoined(Guid ladderId)
	{
		if (cachedLadderData == null)
		{
			cachedLadderData = new Dictionary<Guid, CachedLadderData>();
		}
		if (cachedLadderData.TryGetValue(ladderId, out var value))
		{
			return value.hasLocalPlayerJoined;
		}
		value = await UpdateLadderData(ladderId);
		cachedLadderData.Add(ladderId, value);
		return value.hasLocalPlayerJoined;
	}

	public async Task<bool> JoinLadder(Guid ladderId)
	{
		if (!(await IsAccountConnected()))
		{
			return await ConnectAccount(ladderId);
		}
		Log.Info("LadderManager.JoinLadder({0})", new object[1] { ladderId });
		ServerResponse<ResponseViewModel> response = await PolytopiaBackendAdapter.Instance.JoinLadder(new JoinLadderBindingModel
		{
			LadderId = ladderId
		});
		if (response.Success)
		{
			if (cachedLadderData == null)
			{
				await UpdateLadderData(ladderId);
			}
			if (cachedLadderData.TryGetValue(ladderId, out var value))
			{
				value.hasLocalPlayerJoined = true;
				cachedLadderData[ladderId] = value;
			}
			LadderStateUpdated(value.ladderViewModel);
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(response.ErrorMessage);
		}
		return response.Success;
	}

	public async Task<bool> LeaveLadder(Guid ladderId)
	{
		if (cachedLadderData == null)
		{
			await UpdateLadderData(ladderId);
		}
		if (cachedLadderData.TryGetValue(ladderId, out var value))
		{
			value.hasLocalPlayerJoined = false;
			cachedLadderData[ladderId] = value;
		}
		LadderStateUpdated(value.ladderViewModel);
		return true;
	}

	public async Task<ChallengermodeConnectionStatus> GetConnectionStatus()
	{
		ServerResponse<ChallengermodeConnectionStatus> serverResponse = await PolytopiaBackendAdapter.Instance.GetChallengermodeConnectionStatus();
		if (serverResponse.Success)
		{
			challengermodeAccountId = serverResponse.Data.ChallengermodeUserId;
			return serverResponse.Data;
		}
		PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		return null;
	}

	public async Task<Guid?> GetRecurringLadderId()
	{
		Log.Verbose("LadderManager.GetRecurringLadderId...", Array.Empty<object>());
		ServerResponse<RecurringLadderViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetRecurringLadder();
		if (serverResponse.Success)
		{
			if (serverResponse.Data == null || serverResponse.Data.Current == null)
			{
				return null;
			}
			return serverResponse.Data.Current.Id;
		}
		PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		return null;
	}

	public async Task<Guid?> GetUpcomingLadderId()
	{
		Log.Verbose("LadderManager.GetUpcomingLadderId...", Array.Empty<object>());
		ServerResponse<RecurringLadderViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetRecurringLadder();
		if (serverResponse.Success)
		{
			if (serverResponse.Data == null || serverResponse.Data.Upcoming == null)
			{
				return null;
			}
			return serverResponse.Data.Upcoming.Id;
		}
		PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		return null;
	}

	public async Task<LadderViewModel> GetLadderData(Guid ladderId)
	{
		Log.Verbose("Laddermanager.GetLadderData: {0}", new object[1] { ladderId });
		if (cachedLadderData == null)
		{
			cachedLadderData = new Dictionary<Guid, CachedLadderData>();
		}
		if (cachedLadderData.TryGetValue(ladderId, out var value))
		{
			return value.ladderViewModel;
		}
		value = await UpdateLadderData(ladderId);
		cachedLadderData.Add(ladderId, value);
		return value.ladderViewModel;
	}

	public async Task<List<LadderPlacementViewModel>> GetLadderPlacements(Guid ladderId)
	{
		if (cachedLadderData == null)
		{
			cachedLadderData = new Dictionary<Guid, CachedLadderData>();
		}
		if (cachedLadderData.TryGetValue(ladderId, out var value))
		{
			return value.ladderPlacements;
		}
		value = await UpdateLadderData(ladderId);
		cachedLadderData.Add(ladderId, value);
		return value.ladderPlacements;
	}

	public async Task<LadderPlacementViewModel> GetMyLadderPlacement(Guid ladderId)
	{
		if (cachedLadderData == null)
		{
			cachedLadderData = new Dictionary<Guid, CachedLadderData>();
		}
		if (cachedLadderData.TryGetValue(ladderId, out var value))
		{
			return value.localPlayerPlacement;
		}
		value = await UpdateLadderData(ladderId);
		cachedLadderData.Add(ladderId, value);
		return value.localPlayerPlacement;
	}

	public async Task<CachedLadderData> UpdateLadderData(Guid ladderId)
	{
		Log.Info("LadderManager.UpdateLadderData: {0}", new object[1] { ladderId });
		if (!challengermodeAccountId.HasValue)
		{
			await GetConnectionStatus();
		}
		if (cachedLadderData == null)
		{
			cachedLadderData = new Dictionary<Guid, CachedLadderData>();
		}
		if (!cachedLadderData.TryGetValue(ladderId, out var ladderData))
		{
			ladderData = default(CachedLadderData);
			cachedLadderData.Add(ladderId, ladderData);
		}
		if (ladderId == Guid.Empty)
		{
			ladderData.ladderViewModel = GetDebugViewModel();
			ladderData.ladderPlacements = GetDebugPlacements();
		}
		else
		{
			ServerResponse<LadderViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.GetLadderDetails(ladderId);
			if (serverResponse.Success)
			{
				ladderData.ladderViewModel = serverResponse.Data;
			}
			else
			{
				PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
			}
			ServerResponse<LadderPlacementsViewModel> serverResponse2 = await PolytopiaBackendAdapter.Instance.GetLadderPlacements(ladderId);
			if (serverResponse2.Success)
			{
				ladderData.ladderPlacements = serverResponse2.Data.Placements;
			}
			else
			{
				PopupManager.ShowBackendErrorPopup(serverResponse2.ErrorMessage);
				ladderData.ladderPlacements = new List<LadderPlacementViewModel>();
			}
		}
		if (ladderData.ladderViewModel != null)
		{
			for (int i = 0; i < ladderData.ladderViewModel.ParticipantUserIds.Count; i++)
			{
				Guid value = ladderData.ladderViewModel.ParticipantUserIds[i];
				Guid? guid = challengermodeAccountId;
				if (value == guid)
				{
					ladderData.hasLocalPlayerJoined = true;
					break;
				}
			}
		}
		if (ladderData.ladderPlacements != null)
		{
			for (int j = 0; j < ladderData.ladderPlacements.Count; j++)
			{
				if (Guid.TryParse(ladderData.ladderPlacements[j].PolytopiaId, out var result) && result == AccountManager.PlayerAccountId)
				{
					ladderData.localPlayerPlacement = ladderData.ladderPlacements[j];
					break;
				}
			}
		}
		cachedLadderData[ladderId] = ladderData;
		return ladderData;
	}

	public LadderPlacementViewModel GetPlayerPlacement(List<LadderPlacementViewModel> placements, Guid playerId)
	{
		for (int i = 0; i < placements.Count; i++)
		{
			if (placements[i].UserId == playerId)
			{
				return placements[i];
			}
		}
		return null;
	}

	public SubmitMatchmakingBindingModel GetMatchSettingsPreset(Guid ladderid)
	{
		return new SubmitMatchmakingBindingModel
		{
			GameMode = GameMode.None,
			MapPreset = MapPreset.None,
			MapSize = 0,
			Platform = PolytopiaBackendAdapter.GetCurrentPlatform(),
			TimeLimit = -1,
			OpponentCount = -1,
			Version = VersionManager.GameVersion,
			AllowCrossPlay = true,
			UseLobbies = true
		};
	}

	private LadderViewModel GetDebugViewModel()
	{
		return new LadderViewModel
		{
			Id = Guid.Empty,
			Name = "Polyladder",
			Description = "This is the official Polytopia ladder, join now for a chance to play more polytopia!",
			StartDate = new DateTime(2022, 3, 5),
			EndDate = new DateTime(2022, 6, 5)
		};
	}

	private List<LadderPlacementViewModel> GetDebugPlacements()
	{
		List<LadderPlacementViewModel> list = new List<LadderPlacementViewModel>();
		int num = Random.Range(0, 1000);
		int num2 = Random.Range(0, num);
		int num3 = num * 1000;
		for (int i = 0; i < num; i++)
		{
			list.Add(new LadderPlacementViewModel
			{
				Placement = i,
				Score = num3,
				UserId = ((i == num2) ? AccountManager.PlayerAccountId : Guid.Empty)
			});
			num3 -= Random.Range(1, 100);
		}
		return list;
	}
}
