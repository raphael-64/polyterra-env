using System;
using UnityEngine;

public class ExploreReaction : ReactionBase
{
	private readonly ExploreAction action;

	public ExploreReaction(ExploreAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		Tile instance = tile.GetInstance();
		if ((Object)(object)instance != (Object)null)
		{
			instance.OnExplored(action.PlayerId);
			AudioManager.PlaySFXAtTile(SFXTypes.Explore, tile.coordinates, Random.value * 0.1f);
		}
		if (tile.unit != null && tile.unit.owner != action.PlayerId)
		{
			for (int i = 0; i < GameManager.GameState.Map.Tiles.Length; i++)
			{
				TileData tileData = GameManager.GameState.Map.Tiles[i];
				if (tileData.unit != null && tileData.unit.owner == action.PlayerId && tileData.unit.CanAttack())
				{
					tileData.GetInstance().Unit.UpdateObject();
				}
			}
		}
		onComplete?.Invoke();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
