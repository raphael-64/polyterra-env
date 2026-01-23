using System;
using UnityEngine;

public class KillUnitReaction : ReactionBase
{
	private readonly WorldCoordinates coordinates;

	public KillUnitReaction(WorldCoordinates coordinates)
	{
		this.coordinates = coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile tileInstance = MapRenderer.Current.GetTileInstance(coordinates);
		if ((Object)(object)tileInstance == (Object)null)
		{
			onComplete();
			return;
		}
		if ((Object)(object)tileInstance.Unit != (Object)null)
		{
			if (!tileInstance.IsHidden)
			{
				if (tileInstance.Unit.State.HasEffect(UnitEffect.Poisoned))
				{
					tileInstance.SpawnPoison();
					AudioManager.PlaySFXAtTile(SFXTypes.Explode, tileInstance.Coordinates);
				}
				else
				{
					tileInstance.SpawnPuff();
					AudioManager.PlaySFXAtTile(SFXTypes.Kill, tileInstance.Coordinates);
				}
			}
			Tile tileInstance2 = MapRenderer.Current.GetTileInstance(tileInstance.Unit.GetHomeTile());
			if ((Object)(object)tileInstance2 != (Object)null && !tileInstance2.IsHidden)
			{
				tileInstance2.RenderImprovement();
			}
			tileInstance.Unit.Destroy();
		}
		tileInstance.StopFire();
		if (tileInstance.IsHidden)
		{
			onComplete();
			return;
		}
		tileInstance.RenderUnit();
		for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
		{
			Tile tileInstance3 = MapRenderer.Current.GetTileInstance(GameManager.GameState.Map.Tiles[i].coordinates);
			if ((Object)(object)tileInstance3.Unit != (Object)null)
			{
				tileInstance3.Unit.UpdateObject();
			}
		}
		GameManager.DelayCall(400, onComplete);
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
