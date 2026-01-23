using System;
using UnityEngine;

public static class Awaiters
{
	private static readonly WaitForUpdate _waitForUpdate = new WaitForUpdate();

	private static readonly WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

	private static readonly WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

	public static WaitForUpdate NextFrame => _waitForUpdate;

	public static WaitForFixedUpdate FixedUpdate => _waitForFixedUpdate;

	public static WaitForEndOfFrame EndOfFrame => _waitForEndOfFrame;

	public static WaitForSeconds Seconds(float seconds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new WaitForSeconds(seconds);
	}

	public static WaitForSecondsRealtime SecondsRealtime(float seconds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new WaitForSecondsRealtime(seconds);
	}

	public static WaitUntil Until(Func<bool> predicate)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new WaitUntil(predicate);
	}

	public static WaitWhile While(Func<bool> predicate)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		return new WaitWhile(predicate);
	}
}
