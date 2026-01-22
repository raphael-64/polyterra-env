using System.Collections.Generic;

public class UpdateRoutesAction : ActionBase
{
	public List<TileData> newTransportPathTiles;

	public UpdateRoutesAction()
	{
	}

	public UpdateRoutesAction(byte playerId)
		: base(playerId)
	{
	}

	public override void Execute(GameState state)
	{
		newTransportPathTiles = new List<TileData>();
		state.UpdateRoutes(newTransportPathTiles);
	}

	public override ActionType GetActionType()
	{
		return ActionType.UpdateRoutes;
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId})";
	}
}
