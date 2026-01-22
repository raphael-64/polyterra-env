using System;
using Polytopia.Data;
using UnityEngine;

namespace Polytopia.Tutorial;

[Serializable]
public class TutorialTask
{
	public enum TaskType
	{
		Default,
		IncreaseCapitalLevel,
		TrainUnit,
		TurnNumber,
		NumberOfCities,
		HaveTechnology,
		MeetTribe
	}

	public string descriptionKey;

	public Sprite sprite;

	public TaskType taskType;

	public int targetValue;

	public UnitData.Type unitType;

	public TechData.Type techType;

	public bool isCompleted;
}
