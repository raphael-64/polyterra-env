using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class TechPopup : BasicPopup
{
	[Header("Tech Popup")]
	[SerializeField]
	protected RectTransform techContainer;

	[SerializeField]
	protected ResourceWidget resourceWidget;

	[SerializeField]
	protected TechUnlockButton techUnlockButtonPrefab;

	protected List<TechUnlockButton> buttons = new List<TechUnlockButton>();

	public float Cost
	{
		set
		{
			resourceWidget.Amount = value;
			((Component)resourceWidget).gameObject.SetActive(value >= 0f);
		}
	}

	public void SetTechData(TechData data)
	{
		if (data.unitUnlocks != null && data.unitUnlocks.Count > 0)
		{
			foreach (UnitData unitUnlock in data.unitUnlocks)
			{
				if (GameManager.GameState.GameLogicData.TryGetData(unitUnlock.type, out var _))
				{
					GetNewButton().SetUnitData(GameManager.GameState.GameLogicData.GetOverride(unitUnlock, GameManager.LocalPlayer.GetTribeData(GameManager.GameState)));
				}
			}
		}
		if (data.improvementUnlocks != null && data.improvementUnlocks.Count > 0)
		{
			foreach (ImprovementData improvementUnlock in data.improvementUnlocks)
			{
				if (GameManager.GameState.GameLogicData.TryGetData(improvementUnlock.type, out var _) && !GameManager.GameState.GameLogicData.GetOverride(improvementUnlock, GameManager.LocalPlayer.GetTribeData(GameManager.GameState)).hidden)
				{
					GetNewButton().SetBuildingData(GameManager.GameState.GameLogicData.GetOverride(improvementUnlock, GameManager.LocalPlayer.GetTribeData(GameManager.GameState)));
				}
			}
		}
		if (data.movementUnlocks != null && data.movementUnlocks.Count > 0)
		{
			foreach (KeyValuePair<TerrainData.Type, int> movementUnlock in data.movementUnlocks)
			{
				GetNewButton().SetMovementData(movementUnlock.Key);
			}
		}
		if (data.defenceBonusUnlocks != null && data.defenceBonusUnlocks.Count > 0)
		{
			foreach (KeyValuePair<TerrainData.Type, int> defenceBonusUnlock in data.defenceBonusUnlocks)
			{
				GetNewButton().SetDefenceBonusData(defenceBonusUnlock.Key);
			}
		}
		if (data.abilityUnlocks != null && data.abilityUnlocks.Count > 0)
		{
			foreach (PlayerAbility.Type abilityUnlock in data.abilityUnlocks)
			{
				GetNewButton().SetAbilityData(abilityUnlock);
			}
		}
		if (data.taskUnlocks != null && data.taskUnlocks.Count > 0)
		{
			foreach (TaskData taskUnlock in data.taskUnlocks)
			{
				GetNewButton().SetAchiementData(taskUnlock);
			}
		}
		((Component)techContainer).gameObject.SetActive(((Transform)techContainer).childCount > 0);
	}

	private TechUnlockButton GetNewButton()
	{
		TechUnlockButton techUnlockButton = Object.Instantiate<TechUnlockButton>(techUnlockButtonPrefab, (Transform)(object)techContainer);
		buttons.Add(techUnlockButton);
		return techUnlockButton;
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		foreach (TechUnlockButton button in buttons)
		{
			Object.Destroy((Object)(object)((Component)button).gameObject);
		}
		buttons.Clear();
		((Component)resourceWidget).gameObject.SetActive(false);
	}
}
