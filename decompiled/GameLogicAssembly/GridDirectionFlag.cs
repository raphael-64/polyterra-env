using System;

[Flags]
public enum GridDirectionFlag
{
	SW = 1,
	W = 2,
	NW = 4,
	N = 8,
	NE = 0x10,
	E = 0x20,
	SE = 0x40,
	S = 0x80
}
