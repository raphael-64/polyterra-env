using System;
using PolytopiaBackendBase.Game.ViewModels;
using PolytopiaBackendBase.Timers;

namespace PolytopiaBackendBase.Game;

public class GameViewModel : IServerResponseData
{
	public Guid Id { get; set; }

	public Guid? OwnerId { get; set; }

	public DateTime? DateCreated { get; set; }

	public DateTime? DateLastCommand { get; set; }

	public GameSessionState State { get; set; }

	public string GameSettingsJson { get; set; }

	public byte[] InitialGameStateData { get; set; }

	public byte[] CurrentGameStateData { get; set; }

	public TimerSettings TimerSettings { get; set; }

	public DateTime? DateCurrentTurnDeadline { get; set; }

	public GameContext GameContext { get; set; }
}
