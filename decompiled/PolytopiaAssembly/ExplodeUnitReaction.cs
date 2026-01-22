using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class ExplodeUnitReaction : ReactionBase
{
	private readonly ExplodeUnitAction action;

	public ExplodeUnitReaction(ExplodeUnitAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		Tile instance = tile.GetInstance();
		Tile instance2 = GameManager.GameState.Map.GetTile(action.HomeCoordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance2) && !instance2.IsHidden)
		{
			instance2.RenderImprovement();
		}
		if (Object.op_Implicit((Object)(object)instance))
		{
			if (!instance.IsHidden)
			{
				AnimateGrow(instance.Unit, DoExplode);
				GameManager.DelayCall(600, onComplete);
			}
			else
			{
				onComplete();
			}
			instance.StopFire();
		}
		else
		{
			onComplete();
		}
		void DoExplode()
		{
			instance.Unit.Destroy();
			instance.RenderUnit();
			instance.SpawnPuff();
			instance.SpawnEmbers(0.5f);
			AudioManager.PlaySFXAtTile(SFXTypes.Explode, tile.coordinates);
		}
	}

	public static Tween AnimateGrow(Unit unit, Action OnLevelUp)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		AudioManager.PlaySFXAtTile(SFXTypes.Grow, unit.Tile.Coordinates);
		return (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)unit).transform, ((Component)unit).transform.localPosition.y - 0.1f, 0.5f, false), TweenUtils.GetRoughEase()), (TweenCallback)delegate
		{
			OnLevelUp?.Invoke();
		});
	}
}
