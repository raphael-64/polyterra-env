using System;

namespace PolytopiaBackendBase.Challengermode.Data;

public class ChallengermodeConnectionStatus : IServerResponseData
{
	public bool IsConnected { get; set; }

	public Guid? ChallengermodeUserId { get; set; }

	public bool IsAnotherAccountConnected { get; set; }

	public bool IsRefreshtokenExpired { get; set; }
}
