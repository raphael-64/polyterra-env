using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class GenericTester : MonoBehaviour
{
	[ToggleButton]
	public bool testCityNames;

	public Random random = new Random();

	private void Update()
	{
		if (testCityNames)
		{
			TestCityNames();
		}
	}

	private void TestCityNames()
	{
		foreach (KeyValuePair<TribeData.Type, TribeData> allTribeDatum in GameManager.GameState.GameLogicData.AllTribeData)
		{
			string text = $"Testing city names for : {allTribeDatum.Value.type.ToString()} :: Language: {allTribeDatum.Value.language}";
			for (int i = 0; i < 50; i++)
			{
			}
			Log.Verbose("{0}", new object[1] { text });
		}
	}
}
