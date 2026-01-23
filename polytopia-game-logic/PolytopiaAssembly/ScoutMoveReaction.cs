using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class ScoutMoveReaction : ReactionBase
{
	private readonly ScoutMoveAction action;

	public ScoutMoveReaction(ScoutMoveAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			return action.From != action.To;
		}
		return false;
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.To;
	}

	public override bool ShouldNudgeToCenter()
	{
		return true;
	}

	public override void Execute(Action onComplete)
	{
		if (!GameManager.IsPlayerViewing(action.PlayerId))
		{
			onComplete();
			return;
		}
		Unit unit = MapRenderer.Current.GetUnitInstance(action.UnitId);
		if (action.From != action.To)
		{
			if ((Object)(object)unit == (Object)null)
			{
				GameManager.GameState.GameLogicData.TryGetData(UnitData.Type.Scout, out var data);
				unit = Unit.CreateUnit(data);
				UnitState unitState = UnitState.Create(GameManager.GameState, action.PlayerId, (ushort)GameManager.GameState.CurrentTurn, data, action.From, action.From);
				unitState.id = action.UnitId;
				unit.SetState(unitState);
				unit.SetData(data);
			}
			Tile tileInstance = MapRenderer.Current.GetTileInstance(action.From);
			Tile tileInstance2 = MapRenderer.Current.GetTileInstance(action.To);
			if (!tileInstance.IsHidden && !tileInstance2.IsHidden)
			{
				unit.AnimatePathMove(new List<WorldCoordinates> { action.To, action.From }, delegate
				{
					if (action.RemainingMoves <= 1)
					{
						unit.Destroy();
					}
					onComplete();
				});
			}
			else
			{
				onComplete();
			}
		}
		else
		{
			if (action.RemainingMoves <= 1 && (Object)(object)unit != (Object)null)
			{
				unit.Destroy();
			}
			onComplete();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
