using System;
using System.Collections.Generic;
using Polytopia.Data;

public class BuildReaction : ReactionBase
{
	private readonly BuildAction action;

	public BuildReaction(BuildAction action)
	{
		this.action = action;
	}

	public override bool ShouldFocusCamera()
	{
		return IsRecapOrOpponentAction(action);
	}

	public override WorldCoordinates GetCameraFocusCoordinates()
	{
		return action.Coordinates;
	}

	public override void Execute(Action onComplete)
	{
		Tile instance = GameManager.GameState.Map.GetTile(action.Coordinates).GetInstance();
		if (instance.IsHidden)
		{
			onComplete();
			return;
		}
		GameManager.GameState.GameLogicData.TryGetData(action.Type, out var data);
		if (GameManager.IsPlayerViewing(action.PlayerId) && !GameManager.Client.IsRecap)
		{
			if (data.HasAbility(ImprovementAbility.Type.Consumed) && data.terrainRequirements.Count > 0)
			{
				TerrainRequirements terrainRequirements = data.terrainRequirements[0];
				string value = ((terrainRequirements.terrain != null) ? terrainRequirements.terrain.type.GetName() : terrainRequirements.resource?.type.GetName());
				GameManager.GetAnalyticsManager().SendEvent("resource_extract", new Dictionary<string, object>
				{
					{ "resource_type", value },
					{
						"game_id",
						GameManager.Client.CurrentGameId
					}
				});
			}
			else
			{
				GameManager.GetAnalyticsManager().SendEvent("building_construct", new Dictionary<string, object>
				{
					{ "building_type", action.Type },
					{
						"game_id",
						GameManager.Client.CurrentGameId
					}
				});
			}
		}
		instance.Render();
		instance.SpawnPuff();
		AudioManager.PlaySFXAtTile(SFXTypes.Plop, instance.Coordinates);
		ResourceManager.IncomeChanged(action.PlayerId);
		if (GameManager.IsPlayerViewing(action.PlayerId))
		{
			ResourceManager.RemoveResourceOfType(action.PlayerId, ResourceManager.Type.Currency, data.cost, null, "Add Improvement");
		}
		if (SeasonManager.ShouldShowVengirHalloweenHeadsAfterBuildAction(GameManager.GameState, action))
		{
			(UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).buttonBar.OnStyleUpdated();
			(UIManager.Instance.GetScreen(UIConstants.Screens.TechTree) as TechView).SetDirty();
			List<UnitState> list = new List<UnitState>();
			GameManager.GameState.Map.GetPlayerUnits(action.PlayerId, list);
			foreach (UnitState item in list)
			{
				MapRenderer.Current.GetUnitInstance(item.id).UpdateObject();
			}
		}
		onComplete();
	}

	public override string ToString()
	{
		return $"{GetType()} ()";
	}
}
