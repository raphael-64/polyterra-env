using System.Collections.Generic;
using UnityEngine;

public class ButtonSort : IComparer<UIRoundButton>
{
	public int Compare(UIRoundButton a, UIRoundButton b)
	{
		if ((Object)(object)a == (Object)null || (Object)(object)b == (Object)null)
		{
			return 0;
		}
		return a.Cost.CompareTo(b.Cost);
	}
}
