using System;
using NaughtyAttributes;
using Polytopia.Data;
using UnityEngine;

public class PopupTester : MonoBehaviour
{
	[Button("Test Basic Popup")]
	public void OnTestBasicPopup()
	{
		Log.Verbose("Test Basic popup", Array.Empty<object>());
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = "Test Header";
		basicPopup.Description = DebugUtils.GetLorem() + DebugUtils.GetLorem();
		basicPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show();
	}

	[Button("Test Icon Popup")]
	public void OnTestIconPopup()
	{
		Log.Verbose("Test Icon popup", Array.Empty<object>());
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.Header = "Test Header";
		iconPopup.Description = DebugUtils.GetLorem();
		iconPopup.sprite = UIManager.IconData.GetSprite("Unknown");
		iconPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		iconPopup.Show();
	}

	[Button("Test Tech Popup")]
	public void OnTestTechPopup()
	{
		TechPopup techPopup = PopupManager.GetTechPopup();
		techPopup.Header = "Test Header";
		techPopup.Description = DebugUtils.GetLorem();
		TechData data = null;
		GameManager.GameState.GameLogicData.TryGetData(TechData.Type.Aquatism, out data);
		techPopup.SetTechData(data);
		techPopup.Cost = Random.Range(1, 200);
		techPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		techPopup.Show();
	}

	[Button("Test Unit Popup")]
	public void OnTestUnitPopup()
	{
		UnitData data = null;
		GameManager.GameState.GameLogicData.TryGetData(UnitData.Type.Knight, out data);
		UnitPopup unitPopup = PopupManager.GetUnitPopup();
		unitPopup.Player = GameManager.LocalPlayer;
		unitPopup.UnitData = data;
		unitPopup.Description = Localization.Get("actionbox.unit.new", unitPopup.Player.GetLocalizedTribeName(GameManager.GameState), Localization.Get(data.displayName));
		unitPopup.cost = data.cost;
		unitPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		unitPopup.Show();
	}

	[Button("Test Select Tribe Popup")]
	public void OnTestSelectTribePopup()
	{
		SelectTribePopup selectTribePopup = PopupManager.GetSelectTribePopup();
		if (PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).TryGetData(GameStateUtils.GetRandomPickableTribe(VersionManager.GameVersion, GameManager.PreliminaryGameSettings, null), out var data))
		{
			selectTribePopup.SetData(data);
			selectTribePopup.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
			};
			selectTribePopup.Show();
		}
	}

	[Button("Test Reward Popup")]
	public void OnTestRewardPopup()
	{
		CityReward[] array = new CityReward[4]
		{
			CityReward.Workshop,
			CityReward.CityWall,
			CityReward.PopulationGrowth,
			CityReward.Park
		};
		CityReward[] array2 = new CityReward[4]
		{
			CityReward.Explorer,
			CityReward.Resources,
			CityReward.BorderGrowth,
			CityReward.SuperUnit
		};
		int num = Random.Range(2, array.Length + 2);
		int num2 = Mathf.Min(num - 2, array.Length - 1);
		CityReward[] rewards = new CityReward[2]
		{
			array[num2],
			array2[num2]
		};
		RewardPopup rewardPopup = PopupManager.GetRewardPopup();
		rewardPopup.Header = Localization.Get("world.reward.levelup", "City Name");
		rewardPopup.Description = Localization.Get("wcontroller.building.upgrade.reward", "City Name", num.ToString());
		GameManager.GameState.TryGetPlayer(GameManager.GameState.CurrentPlayer, out var playerState);
		rewardPopup.SetRewards(playerState, rewards);
		rewardPopup.RewardChoosenCallback = null;
		rewardPopup.Show();
	}

	[Button("Check if Basic popup is active")]
	public void OnTestBasicPopupShowing()
	{
		Log.Verbose($"Basic popup showing: {PopupManager.IsPopupShowing<BasicPopup>()}", Array.Empty<object>());
	}

	[Button("Check if Icon popup is active")]
	public void OnTestIconPopupShowing()
	{
		Log.Verbose($"Icon popup showing: {PopupManager.IsPopupShowing<IconPopup>()}", Array.Empty<object>());
	}

	[Button("Test 2 Basic Popup")]
	public void OnTestTwoPopups()
	{
		OnTestBasicPopup();
		OnTestBasicPopup();
	}
}
