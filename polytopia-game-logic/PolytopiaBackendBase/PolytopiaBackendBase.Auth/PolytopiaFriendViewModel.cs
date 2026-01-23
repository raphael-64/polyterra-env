using Newtonsoft.Json;
using PolytopiaBackendBase.Game;

namespace PolytopiaBackendBase.Auth;

public class PolytopiaFriendViewModel : IServerResponseData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public PolytopiaUserViewModel User { get; set; }

	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public FriendshipStatus FriendshipStatus { get; set; }
}
