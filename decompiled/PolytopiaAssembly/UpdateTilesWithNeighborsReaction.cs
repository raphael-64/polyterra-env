using System;
using System.Collections.Generic;

public class UpdateTilesWithNeighborsReaction : ReactionBase
{
	private List<TileData> tilesToUpdate;

	public UpdateTilesWithNeighborsReaction(List<TileData> newTransportPathTiles)
	{
		tilesToUpdate = newTransportPathTiles;
	}

	public override void Execute(Action onComplete)
	{
		if (tilesToUpdate != null && tilesToUpdate.Count > 0)
		{
			List<TileData> list = new List<TileData>();
			foreach (TileData item in tilesToUpdate)
			{
				foreach (TileData item2 in GameManager.GameState.Map.GetArea(item.coordinates, 1, allowDiagonal: true))
				{
					if (!list.Contains(item2))
					{
						list.Add(item2);
					}
				}
			}
			foreach (TileData item3 in list)
			{
				Tile instance = item3.GetInstance();
				if (!instance.IsHidden)
				{
					instance.Render();
				}
			}
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
