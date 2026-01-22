public class ScoreDetails
{
	public int territoryScore;

	public object[] territoryInfoParams;

	public int cultureScore;

	public object[] cultureInfoParams;

	public int cityScore;

	public object[] cityInfoParams;

	public int techScore;

	public object[] techInfoParams;

	public int totalScore;

	public object[] totalInfoParams;

	public float difficultyBonusMultiplier;

	public int difficultyBonus;

	public int TotalScoreIncludingBonus => totalScore + difficultyBonus;
}
