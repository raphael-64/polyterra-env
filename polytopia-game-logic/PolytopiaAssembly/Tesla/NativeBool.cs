namespace Tesla;

public struct NativeBool
{
	public uint value;

	public static implicit operator bool(NativeBool t)
	{
		return (t.value & 1) != 0;
	}
}
