public class MilitaryStats
{
	public float Defense;

	public float Attack;

	public float Movement;

	public float Range;

	public float GetTotal()
	{
		return Defense + Attack + Movement + Range;
	}

	public float GetWeightedTotal()
	{
		return Defense * 9f + Attack * 10f + Movement * 7f + Range * 7f;
	}

	public override string ToString()
	{
		return string.Format("Unit stats defence {0} attack {1} movement {2} range {3}", Defense.ToString("F2"), Attack.ToString("F2"), Movement.ToString("F2"), Range.ToString("F2"));
	}
}
