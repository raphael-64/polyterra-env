using System.Collections.Generic;

public struct AIState
{
	public uint missionsForTurn;

	public List<AI.Mission> missions;

	public List<AI.UnitTarget> hitList;

	public Dictionary<byte, MilitaryStats> playerMilitaryStats;

	public int enemyCities;

	public int enemyPopulation;

	public MilitaryStats militaryNeeds;

	public float roughTerrain;

	public void Clear()
	{
		if (missions == null)
		{
			missions = new List<AI.Mission>();
		}
		missions.Clear();
		if (hitList == null)
		{
			hitList = new List<AI.UnitTarget>();
		}
		hitList.Clear();
		if (playerMilitaryStats == null)
		{
			playerMilitaryStats = new Dictionary<byte, MilitaryStats>();
		}
		if (militaryNeeds == null)
		{
			militaryNeeds = new MilitaryStats();
		}
		militaryNeeds.Defense = 0f;
		militaryNeeds.Attack = 0f;
		militaryNeeds.Movement = 0f;
		militaryNeeds.Range = 0f;
		playerMilitaryStats.Clear();
		enemyCities = 0;
		enemyPopulation = 0;
		roughTerrain = 0f;
	}
}
