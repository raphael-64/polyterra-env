using UnityEngine;

public static class BuildConfigHelper
{
	public static BuildConfig GetSelectedBuildConfig()
	{
		BuildConfigHolder selectedBuildConfigHolder = GetSelectedBuildConfigHolder();
		if ((Object)(object)selectedBuildConfigHolder == (Object)null)
		{
			Debug.LogError((object)"Could not find build config holder");
			return null;
		}
		return selectedBuildConfigHolder.BuildConfig;
	}

	public static BuildConfigHolder GetSelectedBuildConfigHolder()
	{
		return Resources.Load<BuildConfigHolder>(Paths.GetSelectedBuildConfigResourcePath());
	}
}
