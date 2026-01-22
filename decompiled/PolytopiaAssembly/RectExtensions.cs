using UnityEngine;

public static class RectExtensions
{
	public static bool Contains(this Rect outerRect, Rect innerRect)
	{
		if (((Rect)(ref innerRect)).xMin >= ((Rect)(ref outerRect)).xMin && ((Rect)(ref innerRect)).xMax <= ((Rect)(ref outerRect)).xMax && ((Rect)(ref innerRect)).yMin >= ((Rect)(ref outerRect)).yMin)
		{
			return ((Rect)(ref innerRect)).yMax <= ((Rect)(ref outerRect)).yMax;
		}
		return false;
	}
}
