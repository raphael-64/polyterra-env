using System.Collections.Generic;

namespace Polytopia.Data;

public class ColorPalette
{
	public enum Type
	{
		None,
		Default
	}

	public int idx;

	public List<int> colors = new List<int>();

	public Type type => (Type)idx;
}
