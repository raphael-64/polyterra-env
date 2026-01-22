using System;
using Polytopia.Data;
using UnityEngine;

public class MoveReaction : ReactionBase
{
	private readonly MoveAction action;

	public MoveReaction(MoveAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (GameManager.GameState.TryGetUnit(action.UnitId, out var unit))
		{
			WorldCoordinates coordinates = action.Path[0];
			WorldCoordinates coordinates2 = action.Path[action.Path.Count - 1];
			Tile fromTile = MapRenderer.Current.GetTileInstance(coordinates2);
			Tile toTile = MapRenderer.Current.GetTileInstance(coordinates);
			ReactionUtils.NullEmbassyIncome(action.PlayerId, fromTile.Owner?.Id);
			TileData targetTile = toTile.Data;
			fromTile.StopFire();
			bool shouldAnimate = (!fromTile.IsHidden || !toTile.IsHidden) && action.ShouldAnimate;
			if (Object.op_Implicit((Object)(object)fromTile) && Object.op_Implicit((Object)(object)fromTile.Unit) && !fromTile.Unit.IsInvisibleForLocalPlayer)
			{
				ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, coordinates, shouldNudgeToCenter: false, 0.8f, delegate
				{
					fromTile.Unit.Move(action.Path, shouldAnimate, delegate
					{
						if (toTile.Data.IsBeingCaptured(GameManager.GameState))
						{
							toTile.SpawnFire();
						}
						if (toTile.Data.HasImprovement(ImprovementData.Type.City))
						{
							string text = string.Empty;
							string text2 = string.Empty;
							byte playerId = 0;
							if (toTile.Owner != null)
							{
								toTile.Render();
								ResourceManager.IncomeChanged(toTile.Owner.Id);
								if (toTile.Owner.Id != action.PlayerId && unit.CanCapture(GameManager.GameState, targetTile, includeNextTurn: true))
								{
									if (GameManager.IsPlayerViewing(action.PlayerId))
									{
										playerId = action.PlayerId;
										text = Localization.Get("world.building.capture.ready.title", toTile.Improvement.DisplayName);
										text2 = Localization.Get("world.building.capture.ready");
									}
									else if (GameManager.IsPlayerLocal(toTile.Owner.Id))
									{
										playerId = toTile.Owner.Id;
										text = Localization.Get("world.building.capture.warning.title", toTile.Improvement.DisplayName);
										text2 = Localization.Get("world.building.capture.warning");
									}
								}
							}
							else if (GameManager.IsPlayerViewing(action.PlayerId) && unit.CanCapture(GameManager.GameState, targetTile, includeNextTurn: true))
							{
								playerId = action.PlayerId;
								text = Localization.Get("world.building.capture.ready.title", Localization.Get("world.building.village"));
								text2 = Localization.Get("world.building.capture.ready");
							}
							if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2) && !GameManager.Client.IsSpectating && GameManager.GameState.TryGetPlayer(playerId, out var playerState))
							{
								NotificationManager.Notify(text2, text, null, playerState);
							}
						}
						onComplete();
					});
				});
				return;
			}
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
