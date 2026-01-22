using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class ExpandCityAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public WorldCoordinates PreviousHomeTown { get; protected set; }

	public uint Score { get; protected set; }

	public ExpandCityAction()
	{
	}

	public ExpandCityAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state)
	{
		return base.IsValid(state);
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 19)
		{
			ExecuteV19(state);
		}
		else if (state.Version <= 42)
		{
			ExecuteV42(state);
		}
		else if (state.Version <= 43)
		{
			ExecuteV43(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		List<TileData> list = new List<TileData>(cityArea.Count);
		for (int i = 0; i < cityArea.Count; i++)
		{
			TileData tileData = cityArea[i];
			if (tileData.owner != base.PlayerId)
			{
				list.Add(tileData);
				int num = ScoreSheet.tileValue;
				if (tileData.improvement != null)
				{
					num += state.CalculateImprovementScore(tileData);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData.coordinates, 100));
				tileData.owner = base.PlayerId;
				tileData.rulingCityCoordinates = tile.coordinates;
			}
		}
		ActionUtils.AddPopulationForTiles(state, list, base.PlayerId, Coordinates);
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData2.coordinates));
				}
			}
			else if (tileData2.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData2.coordinates, playerState.GetTribeClimate(state)));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData2);
		}
		List<TileData> area = state.Map.GetArea(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
		for (int k = 0; k < area.Count; k++)
		{
			TileData tileData3 = area[k];
			if (!tileData3.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData3.coordinates));
			}
		}
	}

	private void ExecuteV43(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		List<TileData> list = new List<TileData>(cityArea.Count);
		for (int i = 0; i < cityArea.Count; i++)
		{
			TileData tileData = cityArea[i];
			if (tileData.owner != base.PlayerId)
			{
				list.Add(tileData);
				int num = ScoreSheet.tileValue;
				if (tileData.improvement != null)
				{
					num += state.CalculateImprovementScore(tileData);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData.coordinates, 100));
				tileData.owner = base.PlayerId;
				tileData.rulingCityCoordinates = tile.coordinates;
			}
		}
		ActionUtils.AddPopulationForTiles(state, list, base.PlayerId, Coordinates);
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData2.coordinates));
				}
			}
			else if (tileData2.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData2.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData2.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData2.coordinates));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData2);
		}
	}

	private void ExecuteV42(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		for (int i = 0; i < cityArea.Count; i++)
		{
			TileData tileData = cityArea[i];
			if (tileData.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData.improvement != null)
				{
					num += state.CalculateImprovementScore(tileData);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData.coordinates, 100));
				tileData.owner = base.PlayerId;
				tileData.rulingCityCoordinates = tile.coordinates;
			}
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData2.coordinates));
				}
			}
			else if (tileData2.IsAlienClimate(state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData2.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData2.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData2.coordinates));
			}
			ActionUtils.CheckSurroundingArea(state, base.PlayerId, tileData2);
		}
	}

	private void ExecuteV19(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		state.TryGetPlayer(base.PlayerId, out var playerState);
		List<TileData> cityArea = ActionUtils.GetCityArea(state, tile);
		if (cityArea == null)
		{
			return;
		}
		for (int i = 0; i < cityArea.Count; i++)
		{
			TileData tileData = cityArea[i];
			if (tileData.owner != base.PlayerId)
			{
				int num = ScoreSheet.tileValue;
				if (tileData.improvement != null)
				{
					num += state.CalculateImprovementScore(tileData);
				}
				state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, num, tileData.coordinates, 100));
				tileData.owner = base.PlayerId;
				tileData.rulingCityCoordinates = tile.coordinates;
			}
		}
		List<TileData> cityAreaSorted = ActionUtils.GetCityAreaSorted(state, tile);
		cityAreaSorted.Reverse();
		bool flag = playerState.HasTribeAbility(TribeAbility.Type.AlienClimate, state);
		for (int j = 0; j < cityAreaSorted.Count; j++)
		{
			TileData tileData2 = cityAreaSorted[j];
			PlayerState playerState2;
			if (flag)
			{
				if (playerState.tribe == TribeData.Type.Polaris)
				{
					state.ActionStack.Add(new FreezeTileAction(base.PlayerId, tileData2.coordinates));
				}
			}
			else if (tileData2.owner != 0 && state.TryGetPlayer(tileData2.owner, out playerState2) && playerState2.HasTribeAbility(TribeAbility.Type.AlienClimate, state))
			{
				state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData2.coordinates, playerState.GetTribeClimate(state)));
			}
			if (!tileData2.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData2.coordinates));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.ExpandCity;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
	}

	public override string ToString()
	{
		return base.ToString();
	}
}
