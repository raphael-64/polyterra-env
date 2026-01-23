using System;

namespace Tesla;

[Flags]
public enum WantedDevices
{
	leftScrollWheel = 1,
	rightScrollWheel = 2,
	steeringWheel = 4,
	touchScreen = 8,
	all = 0xF
}
