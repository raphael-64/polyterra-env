using System.Collections.Generic;

public class OpinionState
{
	public Dictionary<OpinionManager.Type, float> reasons = new Dictionary<OpinionManager.Type, float>();

	public float total;

	public void AddOpinion(float ammount, OpinionManager.Type type)
	{
		if (!reasons.ContainsKey(type))
		{
			reasons[type] = 0f;
		}
		reasons[type] += ammount;
		total += ammount;
	}

	public float GetOpinion(OpinionManager.Type type)
	{
		return reasons.GetValueOrDefault(type, 0f);
	}

	public void Clear()
	{
		total = 0f;
		reasons = new Dictionary<OpinionManager.Type, float>();
	}
}
