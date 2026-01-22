using System;

public class ExpandCityReaction : ReactionBase
{
	private readonly ExpandCityAction action;

	public ExpandCityReaction(ExpandCityAction action)
	{
		this.action = action;
	}

	public override void Execute(Action onComplete)
	{
		TileData tile = GameManager.GameState.Map.GetTile(action.Coordinates);
		ReactionUtils.UpdateSurroundingBordersAndTransportPaths(action.PlayerId, tile);
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
