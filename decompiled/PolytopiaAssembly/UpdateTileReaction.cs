using System;
using UnityEngine;

public class UpdateTileReaction : ReactionBase
{
	private readonly WorldCoordinates coordinates;

	public UpdateTileReaction(WorldCoordinates coordinates)
	{
		this.coordinates = coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(coordinates).GetInstance();
		if (Object.op_Implicit((Object)(object)instance) && !instance.IsHidden)
		{
			instance.Render();
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
