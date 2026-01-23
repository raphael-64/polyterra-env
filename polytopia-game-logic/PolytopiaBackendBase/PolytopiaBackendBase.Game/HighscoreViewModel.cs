using System;
using Newtonsoft.Json;

namespace PolytopiaBackendBase.Game;

public class HighscoreViewModel : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public Guid PolytopiaUserId { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public string Username { get; set; }

	public uint Score { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int TribeType { get; set; }

	public byte[] AvatarStateData { get; set; }
}
