using System;
using System.Collections.Generic;

namespace PolytopiaBackendBase.Challengermode.Data;

public class LadderViewModel : IServerResponseData
{
	public Guid Id { get; set; }

	public string Name { get; set; }

	public string Description { get; set; }

	public string DateCreated { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public DateTime LastJoinDate { get; set; }

	public List<Guid> ParticipantUserIds { get; set; }
}
