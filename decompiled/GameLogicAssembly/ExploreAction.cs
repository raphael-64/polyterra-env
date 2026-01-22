using System.IO;
using Polytopia.Data;

public class ExploreAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public ExploreAction()
	{
	}

	public ExploreAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile == null)
		{
			return false;
		}
		if (tile.GetExplored(base.PlayerId))
		{
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 29)
		{
			Execute29(state);
		}
		else if (state.Version < 80)
		{
			Execute30(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		tile.SetExplored(base.PlayerId, explored: true);
		gameState.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, ScoreSheet.exploreValue, tile.coordinates, 20));
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		UnitState unit = tile.GetUnit(gameState, base.PlayerId);
		bool flag = unit != null && !playerState.KnowsPlayer(unit.owner);
		bool num = tile.HasImprovement(ImprovementData.Type.City) && tile.owner != 0 && tile.owner != playerState.Id && !playerState.KnowsPlayer(tile.owner);
		if (flag)
		{
			gameState.ActionStack.Add(new MeetAction(base.PlayerId, unit.owner, tile.coordinates));
		}
		if (num)
		{
			if (flag && tile.unit.owner != tile.owner)
			{
				gameState.ActionStack.Add(new MeetAction(playerState.Id, tile.owner, tile.coordinates));
			}
			else if (!flag)
			{
				gameState.ActionStack.Add(new MeetAction(playerState.Id, tile.owner, tile.coordinates));
			}
		}
		if (gameState.TryGetTask(playerState, TaskData.Type.Explorer, out var task) && task.Bump(gameState))
		{
			gameState.CheckTask(playerState, task);
		}
	}

	public void Execute29(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		tile.SetExplored(base.PlayerId, explored: true);
		state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, ScoreSheet.exploreValue, tile.coordinates, 20));
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (tile.unit != null && !playerState.KnowsPlayer(tile.unit.owner))
		{
			state.ActionStack.Add(new MeetAction(base.PlayerId, tile.unit.owner, tile.coordinates));
		}
		if (state.TryGetTask(playerState, TaskData.Type.Explorer, out var task) && task.Bump(state))
		{
			state.CheckTask(playerState, task);
		}
	}

	public void Execute30(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		tile.SetExplored(base.PlayerId, explored: true);
		gameState.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, ScoreSheet.exploreValue, tile.coordinates, 20));
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		UnitState unit = tile.GetUnit(gameState, base.PlayerId);
		bool flag = unit != null && !playerState.KnowsPlayer(unit.owner);
		bool num = tile.HasImprovement(ImprovementData.Type.City) && tile.owner != 0 && tile.owner != playerState.Id && !playerState.KnowsPlayer(tile.owner);
		if (flag)
		{
			gameState.ActionStack.Add(new MeetAction(base.PlayerId, unit.owner, tile.coordinates));
		}
		if (num)
		{
			if (flag && tile.unit.owner != tile.owner)
			{
				gameState.ActionStack.Add(new MeetAction(playerState.Id, tile.owner, tile.coordinates));
			}
			else if (!flag)
			{
				gameState.ActionStack.Add(new MeetAction(playerState.Id, tile.owner, tile.coordinates));
			}
		}
		if (gameState.TryGetTask(playerState, TaskData.Type.Explorer, out var task) && task.Bump(gameState))
		{
			gameState.CheckTask(playerState, task);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Explore;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
