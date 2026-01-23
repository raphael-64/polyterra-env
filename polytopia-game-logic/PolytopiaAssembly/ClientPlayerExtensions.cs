using UnityEngine;

public static class ClientPlayerExtensions
{
	public static Color GetPlayerColor(this PlayerState playerState, GameState gameState)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (playerState.colorOverride >= 0)
		{
			return ColorUtil.ColorFromInt((uint)playerState.colorOverride);
		}
		if (gameState.GameLogicData.TryGetData(playerState.tribeMix, out var data))
		{
			return ColorUtil.ColorFromInt((uint)data.color);
		}
		if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data2))
		{
			return ColorUtil.ColorFromInt((uint)data2.color);
		}
		return Color.white;
	}

	public static string GetLocalizedTribeName(this PlayerState playerState, GameState gameState)
	{
		string result = null;
		if (playerState == null)
		{
			return "";
		}
		if (gameState.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			result = Localization.Get(data.displayName);
		}
		if (gameState.GameLogicData.TryGetData(playerState.tribeMix, out var data2))
		{
			result = TribeExtensions.GetMixedTribeName(data, data2);
		}
		return result;
	}
}
