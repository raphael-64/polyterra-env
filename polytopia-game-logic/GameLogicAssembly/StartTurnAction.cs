using System.IO;
using Polytopia.Data;

public class StartTurnAction : ActionBase
{
	public const int START_TURN_DELAY = 500;

	public StartTurnAction()
	{
	}

	public StartTurnAction(byte playerId)
		: base(playerId)
	{
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 8)
		{
			ExecuteV8(state);
		}
		else if (state.Version < 60)
		{
			ExecuteV9(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState gameState)
	{
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		foreach (PlayerState playerState2 in gameState.PlayerStates)
		{
			ActionUtils.CalculateEmbassyLevel(gameState, playerState, playerState2);
		}
		for (int i = 0; i < gameState.Map.Tiles.Length; i++)
		{
			TileData tileData = gameState.Map.Tiles[i];
			if (tileData.owner != base.PlayerId)
			{
				int improvementLevel = ((tileData.improvement != null) ? tileData.improvement.level : 0);
				int num = tileData.CalculateWork(gameState, playerState, improvementLevel);
				if (num > 0)
				{
					gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, num, 10));
				}
			}
			else
			{
				if (tileData.improvement == null || gameState.CurrentTurn == 0)
				{
					continue;
				}
				gameState.GameLogicData.TryGetData(tileData.improvement.type, out var data);
				if (data == null)
				{
					continue;
				}
				if (data.HasAbility(ImprovementAbility.Type.Attract) && tileData.improvement.GetAge(gameState) % data.growthRate == 0)
				{
					foreach (TileData item in gameState.Map.GetArea(tileData.coordinates, 1, allowDiagonal: true, includeCenter: false))
					{
						if (item.owner == base.PlayerId && item.improvement == null && item.resource == null && item.terrain == TerrainData.Type.Forest)
						{
							gameState.ActionStack.Add(new CreateResourceAction(base.PlayerId, ResourceData.Type.Game, item.coordinates, CreateResourceAction.CreateReason.Attract));
							break;
						}
					}
				}
				if (data.HasAbility(ImprovementAbility.Type.Spread) && tileData.improvement.GetAge(gameState) % data.growthRate == 0 && tileData.improvement.level >= data.maxLevel)
				{
					foreach (TileData item2 in gameState.Map.GetArea(tileData.coordinates, 1, allowDiagonal: true, includeCenter: false))
					{
						if (item2.improvement == null && item2.terrain == TerrainData.Type.Field)
						{
							gameState.ActionStack.Add(new BuildAction(base.PlayerId, data.type, item2.coordinates));
							break;
						}
					}
				}
				if (tileData.improvement.ShouldLevelUp() && gameState.Version > 40 && tileData.owner == gameState.CurrentPlayer)
				{
					gameState.ActionStack.Add(new CityLevelUpAction(base.PlayerId, tileData.coordinates));
				}
				int improvementLevel2 = ActionUtils.UpdateImprovementLevel(gameState, base.PlayerId, tileData);
				int num2 = tileData.CalculateWork(gameState, improvementLevel2);
				if (num2 > 0)
				{
					gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, num2, 10));
				}
			}
		}
		gameState.CheckTask(playerState, TaskData.Type.Pacifist);
		gameState.CheckTask(playerState, TaskData.Type.Explorer);
		ActionUtils.CheckTribeConnection(gameState, playerState, gameState.Map.Tiles);
		if (playerState.AutoPlay)
		{
			playerState.opinions.UpdateOpinions(gameState, playerState);
		}
		CheckDiplomacy(gameState, playerState);
		ActionUtils.TryRevealAllCapitals(gameState, playerState);
	}

	private void ExecuteV9(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (playerState2.Id != base.PlayerId)
			{
				playerState.NormalizeAggression(playerState2.Id);
				if (playerState.KnowsPlayer(playerState2.Id))
				{
					int aggressionModifier = (int)((playerState.score > playerState2.score) ? (-100f) : 100f);
					playerState.ModifyAggression(playerState2.Id, aggressionModifier);
				}
				if (!playerState2.AutoPlay)
				{
					int aggressionModifier2 = (playerState.handicap - 2) * 100;
					playerState.ModifyAggression(playerState2.Id, aggressionModifier2);
				}
			}
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.owner != base.PlayerId)
			{
				continue;
			}
			if (tileData.improvement != null && state.CurrentTurn != 0)
			{
				state.GameLogicData.TryGetData(tileData.improvement.type, out var data);
				if (data != null)
				{
					if (data.HasAbility(ImprovementAbility.Type.Attract) && tileData.improvement.GetAge(state) % data.growthRate == 0)
					{
						foreach (TileData item in state.Map.GetArea(tileData.coordinates, 1, allowDiagonal: true, includeCenter: false))
						{
							if (item.owner == base.PlayerId && item.improvement == null && item.resource == null && item.terrain == TerrainData.Type.Forest)
							{
								state.ActionStack.Add(new CreateResourceAction(base.PlayerId, ResourceData.Type.Game, item.coordinates, CreateResourceAction.CreateReason.Attract));
								break;
							}
						}
					}
					if (data.HasAbility(ImprovementAbility.Type.Spread) && tileData.improvement.GetAge(state) % data.growthRate == 0 && tileData.improvement.level >= data.maxLevel)
					{
						foreach (TileData item2 in state.Map.GetArea(tileData.coordinates, 1, allowDiagonal: true, includeCenter: false))
						{
							if (item2.improvement == null && item2.terrain == TerrainData.Type.Field)
							{
								state.ActionStack.Add(new BuildAction(base.PlayerId, data.type, item2.coordinates));
								break;
							}
						}
					}
					if (tileData.improvement.ShouldLevelUp() && state.Version > 40 && tileData.owner == state.CurrentPlayer)
					{
						state.ActionStack.Add(new CityLevelUpAction(base.PlayerId, tileData.coordinates));
					}
					int improvementLevel = ActionUtils.UpdateImprovementLevel(state, base.PlayerId, tileData);
					int num = tileData.CalculateWork(state, improvementLevel);
					if (num > 0)
					{
						state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, num, 10));
					}
				}
			}
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId)
			{
				playerState.ModifyAggression(tileData.unit.owner, 500);
				if (tileData.HasImprovement(ImprovementData.Type.City))
				{
					playerState.RemoveNonagression(tileData.unit.owner);
					playerState.ModifyAggression(tileData.unit.owner, 2000);
				}
			}
		}
		state.CheckTask(playerState, TaskData.Type.Pacifist);
		state.CheckTask(playerState, TaskData.Type.Explorer);
		ActionUtils.CheckTribeConnection(state, playerState, state.Map.Tiles);
	}

	private void CheckDiplomacy(GameState gameState, PlayerState player)
	{
		for (int num = player.messages.Count - 1; num >= 0; num--)
		{
			DiplomacyMessage diplomacyMessage = player.messages[num];
			if (gameState.Version > 81)
			{
				gameState.TryGetPlayer(diplomacyMessage.Sender, out var playerState);
				if (!playerState.IsAlive(gameState))
				{
					player.TryRemoveMessage(diplomacyMessage.Type, diplomacyMessage.Sender);
					continue;
				}
			}
			gameState.ActionStack.Add(new ReceiveDiplomacyMessageAction(player.Id, diplomacyMessage.Sender, diplomacyMessage.Type));
			if (!player.KnowsPlayer(diplomacyMessage.Sender))
			{
				gameState.ActionStack.Add(new MeetAction(player.Id, diplomacyMessage.Sender, player.startTile));
			}
		}
		for (int i = 0; i < gameState.PlayerStates.Count; i++)
		{
			PlayerState playerState2 = gameState.PlayerStates[i];
			if (player.HasBrokenPeaceWith(playerState2.Id))
			{
				DiplomacyRelation relation = player.GetRelation(playerState2.Id);
				int lastPeaceBrokenTurn = relation.LastPeaceBrokenTurn;
				if ((player.Id > playerState2.Id && gameState.CurrentTurn > lastPeaceBrokenTurn) || (playerState2.Id > player.Id && gameState.CurrentTurn > lastPeaceBrokenTurn + 1))
				{
					relation.State = DiplomacyRelationState.Neutral;
				}
			}
		}
	}

	private void ExecuteV8(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (playerState2.Id != base.PlayerId)
			{
				playerState.NormalizeAggression(playerState2.Id);
				if (playerState.KnowsPlayer(playerState2.Id))
				{
					int aggressionModifier = (int)((playerState.score > playerState2.score) ? (-100f) : 100f);
					playerState.ModifyAggression(playerState2.Id, aggressionModifier);
				}
				if (!playerState2.AutoPlay)
				{
					int aggressionModifier2 = (playerState.handicap - 2) * 100;
					playerState.ModifyAggression(playerState2.Id, aggressionModifier2);
				}
			}
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.owner != base.PlayerId)
			{
				continue;
			}
			if (tileData.improvement != null && state.CurrentTurn != 0)
			{
				if (state.GameLogicData.TryGetData(tileData.improvement.type, out var data) && data.HasAbility(ImprovementAbility.Type.Attract) && tileData.improvement.GetAge(state) % data.growthRate == 0)
				{
					foreach (TileData item in state.Map.GetArea(tileData.coordinates, 1, allowDiagonal: true, includeCenter: false))
					{
						if (item.owner == base.PlayerId && item.improvement == null && item.resource == null && item.terrain == TerrainData.Type.Forest)
						{
							state.ActionStack.Add(new CreateResourceAction(base.PlayerId, ResourceData.Type.Game, item.coordinates));
							break;
						}
					}
				}
				int improvementLevel = ActionUtils.UpdateImprovementLevel(state, base.PlayerId, tileData);
				int num = tileData.CalculateWork(state, improvementLevel);
				for (uint num2 = 0u; num2 < num; num2++)
				{
					state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, 40));
				}
			}
			if (tileData.unit != null && tileData.unit.owner != base.PlayerId)
			{
				playerState.ModifyAggression(tileData.unit.owner, 500);
				if (tileData.HasImprovement(ImprovementData.Type.City))
				{
					playerState.RemoveNonagression(tileData.unit.owner);
					playerState.ModifyAggression(tileData.unit.owner, 2000);
				}
			}
		}
		state.CheckTask(playerState, TaskData.Type.Pacifist);
		state.CheckTask(playerState, TaskData.Type.Explorer);
		ActionUtils.CheckTribeConnection(state, playerState, state.Map.Tiles);
	}

	public override ActionType GetActionType()
	{
		return ActionType.StartTurn;
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
		return $"{GetType()} (PlayerId: {base.PlayerId})";
	}
}
