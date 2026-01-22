using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PolytopiaBackendBase.Challengermode.Data;

namespace PolytopiaBackendBase.Auth;

public class PolytopiaUserViewModel : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public Guid PolytopiaId { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string UserName { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string Alias { get; set; }

	public string SteamId { get; set; }

	public int? NumFriends { get; set; }

	public int Elo { get; set; }

	public Dictionary<string, int> Victories { get; set; }

	public Dictionary<string, int> Defeats { get; set; }

	public int? NumGames { get; set; }

	public int? NumMultiplayergames { get; set; }

	public int? MultiplayerRating { get; set; }

	public byte[] AvatarStateData { get; set; }

	public bool UserMigrated { get; set; }

	public List<ClientGameVersionViewModel> GameVersions { get; set; }

	public DateTime? LastLoginDate { get; set; }

	public List<int> UnlockedTribes { get; set; }

	public List<int> UnlockedSkins { get; set; }

	public UserViewModel CmUserData { get; set; }
}
