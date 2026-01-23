using System;

public class RuleAreaReaction : ReactionBase
{
	private readonly RuleAreaAction action;

	public RuleAreaReaction(RuleAreaAction action)
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
