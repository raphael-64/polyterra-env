using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UITestRangeManager : UIManager
{
	[SerializeField]
	protected Selectable startSelection;

	[SerializeField]
	protected GameObject explicitNavTester;

	[SerializeField]
	protected GameObject cubicBezierTester;

	protected override void Awake()
	{
		base.Awake();
		UINavigationManager.Select(startSelection);
		explicitNavTester.SetActive(false);
		cubicBezierTester.SetActive(false);
		canvas = ((Component)this).GetComponent<Canvas>();
		canvasScaler = ((Component)this).GetComponent<CanvasScaler>();
		if ((Object)(object)canvasScaler != (Object)null)
		{
			UIConstants.UI_PIXELS_PER_UNIT = canvasScaler.referencePixelsPerUnit;
		}
		DebugConsole.Hide();
		List<TribeData> allTribes = PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).GetAllTribes();
		TribeData tribeData = allTribes[Random.Range(0, allTribes.Count)];
		new PlayerState
		{
			tribe = tribeData.type,
			startTile = default(WorldCoordinates)
		};
	}

	public void TestExplicitNavigation()
	{
		explicitNavTester.SetActive(true);
	}

	public void TestCubicBezier()
	{
		cubicBezierTester.SetActive(!cubicBezierTester.activeSelf);
	}
}
