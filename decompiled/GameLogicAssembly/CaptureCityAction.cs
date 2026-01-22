using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class CaptureCityAction : ActionBase
{
	public const int CAPTURE_CITY_DELAY = 1000;

	public WorldCoordinates Coordinates { get; protected set; }

	public byte OldOwnerId { get; protected set; }

	public WorldCoordinates PreviousHomeTown { get; protected set; }

	public int Score { get; protected set; }

	public CaptureCityAction()
	{
	}

	public CaptureCityAction(byte playerId, WorldCoordinates coordinates, byte oldOwner)
		: base(playerId)
	{
		Coordinates = coordinates;
		OldOwnerId = oldOwner;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 11)
		{
			ExecuteV11(state);
		}
		else if (state.Version <= 12)
		{
			ExecuteV12(state);
		}
		else if (state.Version <= 15)
		{
			ExecuteV15(state);
		}
		else if (state.Version <= 19)
		{
			ExecuteV19(state);
		}
		else if (state.Version <= 20)
		{
			ExecuteV20(state);
		}
		else if (state.Version <= 29)
		{
			ExecuteV29(state);
		}
		else if (state.Version <= 42)
		{
			ExecuteV42(state);
		}
		else if (state.Version <= 43)
		{
			ExecuteV43(state);
		}
		else if (state.Version <= 45)
		{
			ExecuteV45(state);
		}
		else if (state.Version < 60)
		{
			ExecuteV59(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && gameState.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(gameState, Coordinates, data);
		}
		if (tile.unit != null && !tile.unit.HasAbility(UnitAbility.Type.Independent, gameState))
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (gameState.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(gameState, tile);
		cityAreaSorted.Reverse();
		if (cityAreaSorted == null)
		{
			return;
		}
		List<TileData> list = new List<TileData>(cityAreaSorted.Count);
		List<ActionBase> list2 = new List<ActionBase>(20);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += gameState.CalculateImprovementScore(tileData2);
				}
				list2.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 50));
				if (tileData2.owner != 0)
				{
					list2.Add(new DecreaseScoreAction(tileData2.owner, num));
				}
				else
				{
					list.Add(tileData2);
				}
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (tile.improvement.level > 1)
		{
			ActionUtils.EnableTask(gameState, playerState, TaskData.Type.Metropolis);
			gameState.CheckTask(playerState, TaskData.Type.Metropolis);
		}
		gameState.ActionStack.AddRange(list2);
		ActionUtils.AddPopulationForTiles(gameState, list, base.PlayerId, Coordinates);
		if (gameState.TryGetWinner(out var winner))
		{
			gameState.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(gameState, gameState.Settings.rules.PlayerDeathCondition))
		{
			gameState.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = gameState.CalculateImprovementScore(tile);
		if (gameState.Version >= 15)
		{
			ActionUtils.AddScore(gameState, playerState, Score);
		}
		List<TileData> cityAreaSorted2 = ActionUtils.GetCityAreaSorted(gameState, tile);
		cityAreaSorted2.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, gameState);
		for (int k = 0; k < cityAreaSorted2.Count; k++)
		{
			TileData tileData3 = cityAreaSorted2[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					gameState.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(gameState))
			{
				gameState.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(gameState)));
			}
			ActionUtils.CheckSurroundingArea(gameState, base.PlayerId, tileData3);
		}
		List<TileData> area = gameState.Map.GetArea(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
		for (int l = 0; l < area.Count; l++)
		{
			TileData tileData4 = area[l];
			if (!tileData4.GetExplored(base.PlayerId))
			{
				gameState.ActionStack.Add(new ExploreAction(base.PlayerId, tileData4.coordinates));
			}
		}
		foreach (TileData item in cityAreaSorted2)
		{
			if (item.unit != null && item.unit.HasAbility(UnitAbility.Type.Disloyal, gameState) && item.unit.owner != owner)
			{
				gameState.ActionStack.Add(new ConvertAction(owner, item.coordinates, item.coordinates));
			}
		}
		RemoveEmbassies(gameState, tile);
		if (owner != 0)
		{
			playerState.SetLastAttack(owner, (int)gameState.CurrentTurn, gameState);
		}
		foreach (PlayerState playerState3 in gameState.PlayerStates)
		{
			if (playerState3.GetEmbassyLevel(playerState) >= 2 && !tile.GetExplored(playerState3.Id))
			{
				gameState.ActionStack.Add(new ExploreAction(playerState3.Id, tile.coordinates));
			}
		}
	}

	private void RemoveEmbassies(GameState gameState, TileData tile)
	{
		if (tile.capitalOf != 0)
		{
			gameState.TryGetPlayer(tile.capitalOf, out var playerState);
			List<byte> embassiesInCapitalOf = playerState.GetEmbassiesInCapitalOf(gameState);
			for (int i = 0; i < embassiesInCapitalOf.Count; i++)
			{
				byte playerId = embassiesInCapitalOf[i];
				gameState.ActionStack.Add(new DestroyEmbassyAction(playerId, playerState.Id));
			}
		}
	}

	private void ExecuteV59(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.unit != null && !tile.unit.HasAbility(UnitAbility.Type.Independent, state))
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		if (cityAreaSorted == null)
		{
			return;
		}
		List<TileData> list = new List<TileData>(cityAreaSorted.Count);
		List<ActionBase> list2 = new List<ActionBase>(20);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				list2.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 50));
				if (tileData2.owner != 0)
				{
					list2.Add(new DecreaseScoreAction(tileData2.owner, num));
				}
				else
				{
					list.Add(tileData2);
				}
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (tile.improvement.level > 1)
		{
			ActionUtils.EnableTask(state, playerState, TaskData.Type.Metropolis);
			state.CheckTask(playerState, TaskData.Type.Metropolis);
		}
		state.ActionStack.AddRange(list2);
		ActionUtils.AddPopulationForTiles(state, list, base.PlayerId, Coordinates);
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			playerState2.wipedAtCommand = state.CommandStack.Count - 1;
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted2 = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted2.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int k = 0; k < cityAreaSorted2.Count; k++)
		{
			TileData tileData3 = cityAreaSorted2[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData3);
		}
		List<TileData> area = state.Map.GetArea(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
		for (int l = 0; l < area.Count; l++)
		{
			TileData tileData4 = area[l];
			if (!tileData4.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData4.coordinates));
			}
		}
	}

	private void ExecuteV45(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.improvement.level > 1)
		{
			ActionUtils.EnableTask(state, playerState, TaskData.Type.Metropolis);
			state.CheckTask(playerState, TaskData.Type.Metropolis);
		}
		if (tile.unit != null && !tile.unit.HasAbility(UnitAbility.Type.Independent, state))
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		if (cityAreaSorted == null)
		{
			return;
		}
		List<TileData> list = new List<TileData>(cityAreaSorted.Count);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 50));
				if (tileData2.owner != 0)
				{
					state.ActionStack.Add(new DecreaseScoreAction(tileData2.owner, num));
				}
				else
				{
					list.Add(tileData2);
				}
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		ActionUtils.AddPopulationForTiles(state, list, base.PlayerId, Coordinates);
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
			playerState.wipedAtCommand = state.CommandStack.Count - 1;
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted2 = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted2.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int k = 0; k < cityAreaSorted2.Count; k++)
		{
			TileData tileData3 = cityAreaSorted2[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData3);
		}
		List<TileData> area = state.Map.GetArea(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
		for (int l = 0; l < area.Count; l++)
		{
			TileData tileData4 = area[l];
			if (!tileData4.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData4.coordinates));
			}
		}
	}

	private void ExecuteV43(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.improvement.level > 1)
		{
			ActionUtils.EnableTask(state, playerState, TaskData.Type.Metropolis);
			state.CheckTask(playerState, TaskData.Type.Metropolis);
		}
		if (tile.unit != null && !tile.unit.HasAbility(UnitAbility.Type.Independent, state))
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		if (cityAreaSorted == null)
		{
			return;
		}
		List<TileData> list = new List<TileData>(cityAreaSorted.Count);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 50));
				if (tileData2.owner != 0)
				{
					state.ActionStack.Add(new DecreaseScoreAction(tileData2.owner, num));
				}
				else
				{
					list.Add(tileData2);
				}
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		ActionUtils.AddPopulationForTiles(state, list, base.PlayerId, Coordinates);
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted2 = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted2.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int k = 0; k < cityAreaSorted2.Count; k++)
		{
			TileData tileData3 = cityAreaSorted2[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData3);
		}
	}

	private void ExecuteV42(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.improvement.level > 1)
		{
			ActionUtils.EnableTask(state, playerState, TaskData.Type.Metropolis);
			state.CheckTask(playerState, TaskData.Type.Metropolis);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		if (cityAreaSorted == null)
		{
			return;
		}
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 50));
				if (tileData2.owner != 0)
				{
					state.ActionStack.Add(new DecreaseScoreAction(tileData2.owner, num));
				}
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted2 = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted2.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int k = 0; k < cityAreaSorted2.Count; k++)
		{
			TileData tileData3 = cityAreaSorted2[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData3);
		}
	}

	private void ExecuteV29(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		if (cityAreaSorted == null)
		{
			return;
		}
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 50));
				if (tileData2.owner != 0)
				{
					state.ActionStack.Add(new DecreaseScoreAction(tileData2.owner, num));
				}
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted2 = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted2.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int k = 0; k < cityAreaSorted2.Count; k++)
		{
			TileData tileData3 = cityAreaSorted2[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData3);
		}
	}

	private void ExecuteV20(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		for (int j = 0; j < cityArea.Count; j++)
		{
			TileData tileData2 = cityArea[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 100));
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int k = 0; k < cityAreaSorted.Count; k++)
		{
			TileData tileData3 = cityAreaSorted[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (tileData3.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData3);
		}
	}

	private void ExecuteV19(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		for (int j = 0; j < cityArea.Count; j++)
		{
			TileData tileData2 = cityArea[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 100));
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		bool flag2 = playerState2?.HasTribeAbility(TribeAbility.Type.AlienClimate, state) ?? false;
		for (int k = 0; k < cityAreaSorted.Count; k++)
		{
			TileData tileData3 = cityAreaSorted[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (flag2)
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
		}
	}

	private void ExecuteV15(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data.language);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		playerState.cities++;
		byte owner = tile.owner;
		if (state.TryGetPlayer(owner, out var playerState2))
		{
			playerState2.cities--;
		}
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		for (int j = 0; j < cityArea.Count; j++)
		{
			TileData tileData2 = cityArea[j];
			if (tileData2.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData2.improvement != null && tileData2.coordinates != tile.coordinates)
				{
					num += state.CalculateImprovementScore(tileData2);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData2.coordinates, 100));
			}
			tileData2.owner = base.PlayerId;
			tileData2.rulingCityCoordinates = tile.coordinates;
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (playerState2 != null && !playerState2.IsAlive(state, state.Settings.rules.PlayerDeathCondition))
		{
			state.ActionStack.Add(new WipePlayerAction(base.PlayerId, playerState2.Id));
		}
		Score = state.CalculateImprovementScore(tile);
		if (state.Version >= 15)
		{
			ActionUtils.AddScore(state, playerState, Score);
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		bool flag2 = playerState2?.HasTribeAbility(TribeAbility.Type.AlienClimate, state) ?? false;
		for (int k = 0; k < cityAreaSorted.Count; k++)
		{
			TileData tileData3 = cityAreaSorted[k];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData3.coordinates));
				}
			}
			else if (flag2)
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData3.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
		}
	}

	private void ExecuteV12(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data.language);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		int count = state.ActionStack.Count;
		ActionUtils.RuleArea(state, playerState, tile, shouldUseActions: true);
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Insert(count, new GameOverAction(base.PlayerId, winner.Id));
		}
	}

	private void ExecuteV11(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (string.IsNullOrEmpty(tile.improvement.name) && state.TryGetPlayer(base.PlayerId, out var playerState) && state.GameLogicData.TryGetData(playerState.tribe, out var data))
		{
			tile.improvement.name = MapDataExtensions.GenerateCityName(state, Coordinates, data.language);
		}
		if (tile.unit != null)
		{
			PreviousHomeTown = tile.unit.home;
			tile.unit.home = Coordinates;
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId && !(tileData.unit.home != Coordinates))
			{
				tileData.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.CaptureCity;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write(OldOwnerId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		OldOwnerId = reader.ReadByte();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, PreviousHome: {PreviousHomeTown})";
	}
}
