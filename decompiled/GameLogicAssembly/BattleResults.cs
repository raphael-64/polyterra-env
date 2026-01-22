using System;

[Serializable]
public struct BattleResults
{
	public int attackDamage;

	public int retaliationDamage;

	public bool shouldMoveToDefeatedEnemyTile;

	public override string ToString()
	{
		return $"BattleResults: attackDamage {attackDamage}, retaliationDamage {retaliationDamage}, shouldMoveToDefeatedEnemyTile {shouldMoveToDefeatedEnemyTile}";
	}
}
