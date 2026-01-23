using System;
using System.Collections.Generic;
using UnityEngine;

public static class ReactionUtils
{
	public const float CAMERA_FOCUS_DURATION = 0.8f;

	public static void UpdateSurroundingBordersAndTransportPaths(byte playerId, TileData cityTile)
	{
		List<TileData> area = GameManager.GameState.Map.GetArea(cityTile.coordinates, cityTile.improvement.borderSize + 1, allowDiagonal: true, includeCenter: false);
		if (area == null || area.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < area.Count; i++)
		{
			Tile tile = area[i]?.GetInstance();
			if (Object.op_Implicit((Object)(object)tile) && !tile.IsHidden)
			{
				tile.Render();
			}
		}
	}

	public static void CameraFocusIfExplored(byte playerId, WorldCoordinates coordinates, bool shouldNudgeToCenter, float duration, Action completion)
	{
		TileData tile = GameManager.GameState.Map.GetTile(coordinates);
		if (tile != null && tile.GetExplored(playerId))
		{
			CameraController.Instance.RevealTile(tile.GetInstance(), shouldAccountForHUD: true, checkEdges: true, duration, forceChange: false, shouldNudgeToCenter, completion);
		}
		else
		{
			completion();
		}
	}

	public static void NullEmbassyIncome(byte playerId, byte? otherPlayerId)
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		bool flag = false;
		byte? b = null;
		if (playerId == localPlayer.Id)
		{
			b = otherPlayerId;
			flag = true;
		}
		else if (otherPlayerId.HasValue && otherPlayerId == localPlayer.Id)
		{
			b = playerId;
			flag = true;
		}
		if (flag)
		{
			ResourceEvents.IncomeChanged(localPlayer.Id);
		}
		if (b.HasValue && b.Value != 0 && b.Value != byte.MaxValue)
		{
			GameManager.GameState.TryGetPlayer(b.Value, out var playerState);
			MapRenderer.Current.GetTileInstance(playerState.startTile).RenderImprovement();
		}
	}
}
