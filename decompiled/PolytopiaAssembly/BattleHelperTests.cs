using System;
using System.Collections.Generic;
using NaughtyAttributes;
using Polytopia.Data;
using UnityEngine;

public class BattleHelperTests : MonoBehaviour
{
	public enum DefenceBonusType
	{
		None,
		Water,
		Forest,
		City,
		Mountain
	}

	[Serializable]
	public struct UnitSetup
	{
		public UnitData.Type type;

		public int hp;

		public bool isVeteran;

		public bool requireWaterTerrain;

		public DefenceBonusType defenceBonusType;

		public override string ToString()
		{
			return string.Format("{0}Unit {1}, hp {2}", isVeteran ? "Veteran " : "", type, hp);
		}
	}

	[Serializable]
	public struct BattleHelperTest
	{
		public UnitSetup attackingUnitSetup;

		public UnitSetup defendingUnitSetup;

		public bool shouldCompareAndFail;

		public BattleResults expectedResults;

		public override string ToString()
		{
			return string.Format("attacker: {0},\ndefender: {1}\nexpected:{2}", attackingUnitSetup.ToString(), defendingUnitSetup.ToString(), shouldCompareAndFail ? expectedResults.ToString() : "none");
		}
	}

	public List<BattleHelperTest> tests;

	public BattleHelperTest quickTest;

	[Button(null)]
	public void RunList()
	{
		int num = 0;
		foreach (BattleHelperTest test in tests)
		{
			if (RunTest(test))
			{
				num++;
			}
		}
		if (num < tests.Count)
		{
			Log.Error("Failed {0} of {1} tests", new object[2]
			{
				tests.Count - num,
				tests.Count
			});
		}
		else
		{
			Log.Verbose("Passed all {0} tests", new object[1] { num });
		}
	}

	[Button(null)]
	public void RunQuick()
	{
		RunTest(quickTest);
	}

	private UnitState CreateUnit(GameState gameState, UnitSetup unitSetup, TileData tile, PlayerState playerState)
	{
		gameState.GameLogicData.TryGetData(unitSetup.type, out var data);
		UnitState unitState = ActionUtils.TrainUnit(gameState, playerState, tile, data);
		if (unitSetup.isVeteran)
		{
			unitState.promotionLevel++;
		}
		if (unitSetup.hp <= 0)
		{
			unitState.health = (ushort)tile.unit.GetMaxHealth(gameState);
		}
		else
		{
			unitState.health = (ushort)unitSetup.hp;
		}
		if (unitSetup.requireWaterTerrain && unitSetup.defenceBonusType != DefenceBonusType.Water && unitSetup.defenceBonusType != DefenceBonusType.None)
		{
			Log.Error("Incorrect input, can't have water in combination with defence bonus {0}", new object[1] { unitSetup.defenceBonusType });
			return null;
		}
		if (unitSetup.requireWaterTerrain)
		{
			tile.terrain = TerrainData.Type.Water;
			tile.altitude = -1;
		}
		else
		{
			tile.terrain = TerrainData.Type.Field;
			tile.altitude = 1;
		}
		switch (unitSetup.defenceBonusType)
		{
		case DefenceBonusType.City:
			tile.improvement = new ImprovementState
			{
				type = ImprovementData.Type.City,
				founded = 0,
				name = "Test",
				level = 1,
				borderSize = 1,
				production = 1
			};
			break;
		case DefenceBonusType.Forest:
			tile.terrain = TerrainData.Type.Forest;
			tile.altitude = 1;
			if (!gameState.GameLogicData.IsUnlocked(TechData.Type.Archery, playerState))
			{
				ActionUtils.LearnTech(gameState, playerState, TechData.Type.Archery, 0, shouldUseActions: false);
			}
			break;
		case DefenceBonusType.Mountain:
			tile.terrain = TerrainData.Type.Mountain;
			tile.altitude = 2;
			if (!gameState.GameLogicData.IsUnlocked(TechData.Type.Meditation, playerState))
			{
				ActionUtils.LearnTech(gameState, playerState, TechData.Type.Meditation, 0, shouldUseActions: false);
			}
			break;
		case DefenceBonusType.Water:
			tile.terrain = TerrainData.Type.Water;
			tile.altitude = -1;
			if (!gameState.GameLogicData.IsUnlocked(TechData.Type.Aquatism, playerState))
			{
				ActionUtils.LearnTech(gameState, playerState, TechData.Type.Aquatism, 0, shouldUseActions: false);
			}
			break;
		}
		return unitState;
	}

	public bool RunTest(BattleHelperTest test)
	{
		bool result = false;
		GameState gameState = GameManager.GameState;
		if (gameState == null)
		{
			Log.Error("Cannot run tests because no gamestate exists", Array.Empty<object>());
			return result;
		}
		PlayerState playerState = gameState.PlayerStates[0];
		PlayerState playerState2 = gameState.PlayerStates[1];
		TileData tile = gameState.Map.GetTile(new WorldCoordinates(0, 0));
		TileData tile2 = gameState.Map.GetTile(new WorldCoordinates(1, 0));
		tile.SetExplored(playerState.Id, explored: true);
		tile.SetExplored(playerState2.Id, explored: true);
		tile2.SetExplored(playerState.Id, explored: true);
		tile2.SetExplored(playerState2.Id, explored: true);
		tile.improvement = null;
		tile2.improvement = null;
		UnitState unitState = CreateUnit(gameState, test.attackingUnitSetup, tile, playerState);
		UnitState unitState2 = CreateUnit(gameState, test.defendingUnitSetup, tile2, playerState2);
		BattleResults battleResults = BattleHelpers.GetBattleResults(gameState, unitState, unitState2);
		if (test.shouldCompareAndFail)
		{
			if (test.expectedResults.Equals(battleResults))
			{
				result = true;
				Log.Verbose("Passed test {0}", new object[1] { test.ToString() });
			}
			else
			{
				Log.Error("Failed test {0}\nActual:{1}", new object[2]
				{
					test.ToString(),
					battleResults.ToString()
				});
			}
		}
		else
		{
			result = true;
			Log.Verbose("Ran test {0}\nResult:{1}", new object[2]
			{
				test.ToString(),
				battleResults.ToString()
			});
		}
		ActionUtils.KillUnit(gameState, gameState.Map.GetTile(unitState.coordinates));
		ActionUtils.KillUnit(gameState, gameState.Map.GetTile(unitState2.coordinates));
		return result;
	}
}
