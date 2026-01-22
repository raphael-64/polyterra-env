using System.Collections.Generic;
using Polytopia.Data;

public class EndTurnCommand : CommandBase
{
	public EndTurnCommand()
	{
	}

	public EndTurnCommand(byte playerId)
		: base(playerId)
	{
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 40)
		{
			ExecuteV40(state);
		}
		else if (state.Version <= 42)
		{
			ExecuteV42(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new EndTurnAction(base.PlayerId));
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == base.PlayerId)
			{
				if (tileData.unit.CanRecover(state))
				{
					state.ActionStack.Add(new HealAction(base.PlayerId, tileData.coordinates, 0));
				}
				state.GameLogicData.TryGetData(tileData.unit.type, out var data);
				if (tileData.unit.HasAbility(UnitAbility.Type.Grow, state))
				{
					state.TryGetPlayer(base.PlayerId, out var playerState);
					List<UnitData> unlockedUpgradesForUnit = state.GameLogicData.GetUnlockedUpgradesForUnit(playerState, state, data);
					if (unlockedUpgradesForUnit.Count > 0 && tileData.unit.GetAge(state) >= data.growthRate)
					{
						state.ActionStack.Add(new UpgradeAction(base.PlayerId, unlockedUpgradesForUnit[0].type, tileData.coordinates, 0));
					}
				}
				tileData.unit.attacked = false;
				tileData.unit.moved = false;
				tileData.unit.RemoveEffect(UnitEffect.Frozen);
				tileData.unit.previousTurnEndCoordinates = tileData.unit.coordinates;
			}
			if (tileData.owner != base.PlayerId || tileData.improvement == null)
			{
				continue;
			}
			state.GameLogicData.TryGetData(tileData.improvement.type, out var data2);
			if (data2.HasAbility(ImprovementAbility.Type.Heal))
			{
				List<TileData> healOptions = tileData.GetHealOptions(base.PlayerId, state, includeCenter: true);
				if (healOptions.Contains(tileData))
				{
					state.ActionStack.Add(new HealAction(base.PlayerId, tileData.coordinates, 40));
				}
				if (healOptions.Count > 0)
				{
					state.ActionStack.Add(new HealOthersAction(base.PlayerId, tileData.coordinates));
				}
			}
			if (tileData.improvement.HasEffect(ImprovementEffect.decomposing) && (tileData.unit == null || !data2.HasAbility(ImprovementAbility.Type.Bridge)))
			{
				for (int j = 0; j < data2.cost; j++)
				{
					state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, 40));
				}
				state.ActionStack.Add(new DestroyImprovementAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	private void ExecuteV42(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new EndTurnAction(base.PlayerId));
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == base.PlayerId)
			{
				if (tileData.unit.CanRecover(state))
				{
					state.ActionStack.Add(new HealAction(base.PlayerId, tileData.coordinates, 0));
				}
				state.GameLogicData.TryGetData(tileData.unit.type, out var data);
				if (tileData.unit.HasAbility(UnitAbility.Type.Grow, state))
				{
					state.TryGetPlayer(base.PlayerId, out var playerState);
					List<UnitData> unlockedUpgradesForUnit = state.GameLogicData.GetUnlockedUpgradesForUnit(playerState, state, data);
					if (unlockedUpgradesForUnit.Count > 0 && tileData.unit.GetAge(state) >= data.growthRate)
					{
						state.ActionStack.Add(new UpgradeAction(base.PlayerId, unlockedUpgradesForUnit[0].type, tileData.coordinates, 0));
					}
				}
				tileData.unit.attacked = false;
				tileData.unit.moved = false;
				tileData.unit.RemoveEffect(UnitEffect.Frozen);
				tileData.unit.previousTurnEndCoordinates = tileData.unit.coordinates;
			}
			if (tileData.owner != base.PlayerId || tileData.improvement == null)
			{
				continue;
			}
			state.GameLogicData.TryGetData(tileData.improvement.type, out var data2);
			if (data2.HasAbility(ImprovementAbility.Type.Heal) && tileData.GetHealOptions(base.PlayerId, state).Count > 0)
			{
				state.ActionStack.Add(new HealOthersAction(base.PlayerId, tileData.coordinates));
			}
			if (tileData.improvement.HasEffect(ImprovementEffect.decomposing) && (tileData.unit == null || !data2.HasAbility(ImprovementAbility.Type.Bridge)))
			{
				for (int j = 0; j < data2.cost; j++)
				{
					state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, 40));
				}
				state.ActionStack.Add(new DestroyImprovementAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	private void ExecuteV40(GameState state)
	{
		base.Execute(state);
		state.ActionStack.Add(new EndTurnAction(base.PlayerId));
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == base.PlayerId)
			{
				if (tileData.unit.CanRecover(state))
				{
					state.ActionStack.Add(new HealAction(base.PlayerId, tileData.coordinates, 0));
				}
				state.GameLogicData.TryGetData(tileData.unit.type, out var data);
				if (tileData.unit.HasAbility(UnitAbility.Type.Grow, state))
				{
					state.TryGetPlayer(base.PlayerId, out var playerState);
					List<UnitData> unlockedUpgradesForUnit = state.GameLogicData.GetUnlockedUpgradesForUnit(playerState, state, data);
					if (unlockedUpgradesForUnit.Count > 0 && tileData.unit.GetAge(state) >= data.growthRate)
					{
						state.ActionStack.Add(new UpgradeAction(base.PlayerId, unlockedUpgradesForUnit[0].type, tileData.coordinates, 0));
					}
				}
				tileData.unit.attacked = false;
				tileData.unit.moved = false;
				tileData.unit.RemoveEffect(UnitEffect.Frozen);
				tileData.unit.previousTurnEndCoordinates = tileData.unit.coordinates;
			}
			if (tileData.owner != base.PlayerId || tileData.improvement == null)
			{
				continue;
			}
			state.GameLogicData.TryGetData(tileData.improvement.type, out var data2);
			if (data2.HasAbility(ImprovementAbility.Type.Heal) && tileData.GetHealOptions(base.PlayerId, state).Count > 0)
			{
				state.ActionStack.Add(new HealOthersAction(base.PlayerId, tileData.coordinates));
			}
			if (tileData.improvement.HasEffect(ImprovementEffect.decomposing))
			{
				for (int j = 0; j < data2.cost; j++)
				{
					state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, tileData.coordinates, 40));
				}
				state.ActionStack.Add(new DestroyImprovementAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.EndTurn;
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId})";
	}
}
