using System;
using Polytopia.Data;
using UnityEngine;

public class InfiltrateReaction : ReactionBase
{
	private readonly InfiltrateAction action;

	public InfiltrateReaction(InfiltrateAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.GameState.GameLogicData.TryGetData(ImprovementData.Type.City, out var _))
		{
			onComplete();
			return;
		}
		Tile fromTile = MapRenderer.Current.GetTileInstance(action.Origin);
		MapRenderer.Current.GetTileInstance(action.Target);
		if ((Object)(object)fromTile.Unit == (Object)null || (fromTile.Unit.State.HasEffect(UnitEffect.Invisible) && fromTile.Unit.IsInvisibleForLocalPlayer))
		{
			onComplete();
			return;
		}
		ReactionUtils.CameraFocusIfExplored(GameManager.LocalPlayer.Id, action.Target, shouldNudgeToCenter: false, 0.8f, delegate
		{
			fromTile.Unit.Attack(action.Target, moveToTarget: false, delegate
			{
				fromTile.Unit.Destroy();
				onComplete();
			});
		});
	}
}
