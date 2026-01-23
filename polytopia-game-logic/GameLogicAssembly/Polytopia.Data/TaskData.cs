using System.Collections.Generic;
using Newtonsoft.Json;

namespace Polytopia.Data;

public class TaskData
{
	public enum Type
	{
		None,
		Pacifist,
		Genius,
		Network,
		Wealth,
		Killer,
		Metropolis,
		Explorer
	}

	public int idx;

	[JsonConverter(typeof(StringIDsToObjectsConverter<ImprovementData, ImprovementData.Type>))]
	public List<ImprovementData> improvementUnlocks = new List<ImprovementData>();

	public string displayName => $"polyplayer.task.{type.GetName()}.title";

	public string description => $"polyplayer.task.{type.GetName()}.description";

	public Type type => (Type)idx;
}
