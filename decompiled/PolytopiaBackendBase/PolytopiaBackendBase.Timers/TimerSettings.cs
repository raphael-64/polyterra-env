using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolytopiaBackendBase.Timers;

public class TimerSettings
{
	[Column("TimerSettings_BaseTimeTicks", TypeName = "bigint")]
	public TimeSpan BaseTime { get; set; }

	public bool UseDynamicTimeIncrements { get; set; }

	public bool UseTimebanks { get; set; }

	public bool UseBaseTimeInLobby { get; set; }

	[Column("TimerSettings_BaseTime", TypeName = "time")]
	public TimeSpan BaseTimeOld { get; set; }
}
