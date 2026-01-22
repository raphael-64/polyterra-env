using System.Collections.Generic;

public class ProbabilityTable
{
	private List<ValueProbability> _valueProbabilities = new List<ValueProbability>();

	public int Count => _valueProbabilities.Count;

	public void Normalize()
	{
		float num = 0f;
		for (int i = 0; i < _valueProbabilities.Count; i++)
		{
			num += _valueProbabilities[i].probability;
		}
		if (num != 0f)
		{
			for (int j = 0; j < _valueProbabilities.Count; j++)
			{
				ValueProbability value = _valueProbabilities[j];
				value.probability /= num;
				_valueProbabilities[j] = value;
			}
		}
	}

	public void Add(int value, float probability)
	{
		_valueProbabilities.Add(new ValueProbability
		{
			value = value,
			probability = probability
		});
	}

	public int GetValueForRandomValue(float randomValue)
	{
		float num = 0f;
		for (int i = 0; i < _valueProbabilities.Count; i++)
		{
			ValueProbability valueProbability = _valueProbabilities[i];
			num += valueProbability.probability;
			if (randomValue <= num)
			{
				return valueProbability.value;
			}
		}
		return -1;
	}
}
