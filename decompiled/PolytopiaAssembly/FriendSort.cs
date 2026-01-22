using System.Collections.Generic;

public class FriendSort : IComparer<PlayerData>
{
	public int Compare(PlayerData a, PlayerData b)
	{
		if (a == null || b == null || a.GetName() == null || b.GetName() == null)
		{
			return 0;
		}
		return a.GetName().CompareTo(b.GetName());
	}
}
