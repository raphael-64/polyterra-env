using System;
using UnityEngine;

public class AttackReaction : ReactionBase
{
	private readonly AttackAction action;

	public AttackReaction(AttackAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Target;
	}

	public override void Execute(Action onComplete)
	{
		GameState gameState = GameManager.GameState;
		_ = GameManager.LocalPlayer;
		TileData tile = gameState.Map.GetTile(action.Origin);
		TileData tile2 = gameState.Map.GetTile(action.Target);
		ReactionUtils.NullEmbassyIncome(otherPlayerId: tile2?.unit?.owner, playerId: action.PlayerId);
		gameState.TryGetPlayer(tile.unit.owner, out var _);
		gameState.TryGetPlayer(tile2.unit.owner, out var _);
		switch (action.Animation)
		{
		case AttackAction.AnimationType.Normal:
			NormalAnimation(onComplete);
			break;
		case AttackAction.AnimationType.Splash:
			SplashAnimation(onComplete);
			break;
		default:
			onComplete();
			break;
		}
	}

	private void NormalAnimation(Action onComplete)
	{
		Tile originTile = MapRenderer.Current.GetTileInstance(action.Origin);
		Tile targetTile = MapRenderer.Current.GetTileInstance(action.Target);
		if (Object.op_Implicit((Object)(object)originTile) && Object.op_Implicit((Object)(object)originTile.Unit) && (!originTile.IsHidden || !targetTile.IsHidden))
		{
			originTile.Unit.Attack(action.Target, action.ShouldMoveToTarget, delegate
			{
				if (Object.op_Implicit((Object)(object)targetTile) && Object.op_Implicit((Object)(object)targetTile.Unit) && !targetTile.IsHidden)
				{
					targetTile.Damage(action.Damage);
					targetTile.RenderUnit();
					if (!action.ShouldMoveToTarget)
					{
						originTile.RenderUnit();
					}
				}
				if (action.ShouldMoveToTarget)
				{
					onComplete();
				}
				else
				{
					GameManager.DelayCall(action.Delay, onComplete);
				}
			});
		}
		else
		{
			onComplete();
		}
	}

	private void SplashAnimation(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(action.Target);
		bool flag = Object.op_Implicit((Object)(object)tileInstance.Unit) && tileInstance.Unit.State.IsInvisible(GameManager.GameState, GameManager.LocalPlayer.Id);
		if (!tileInstance.IsHidden && !flag)
		{
			tileInstance.SpawnDarkPuff();
			tileInstance.Damage(action.Damage);
			tileInstance.Render();
		}
		GameManager.DelayCall(action.Delay, onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
