using System;
using UnityEngine;

[CreateAssetMenu]
public class BuildConfig : ScriptableObject
{
	[Header("Applied -  Makes changes to the repo when applied. Automatically applied in pre-export.")]
	public BundleIDConfig bundleIDConfig;

	public string semanticVersion = "1.0.0";

	public string iosDefineSymbols;

	public string androidDefineSymbols;

	public string standaloneDefineSymbols;

	public BuildIconSet iconSet = BuildIconSet.Release;

	public BuildBackendType buildBackendType = BuildBackendType.Release;

	[Tooltip("Set 0 to never expire")]
	public int daysUntilBuildExpires;

	[Header("Pre-export - These values are used during pre-export")]
	public int buildNumberOffset;

	[Tooltip("If this is a non-negative value then it will be used instead of the git number that is normally assigned in pre-export. Specifying this also ignores the build number offset")]
	public int buildNumberOverride = -1;

	[Header("Build - These values are used on build. Ignored if built from unity build dialog")]
	public bool developmentBuild;

	[Tooltip("If you're not logged in to Unity Services then analytics and crash reporting won't work. Because of this the build will throw an exception if you're not logged in. If you want to make internal test builds without having to be logged in then you can check this box. This box should never be checked for release builds")]
	public bool allowMissingUnityServices;

	[Tooltip("Will fall back on mono if building IL2CPP is not possible")]
	public PolytopiaScriptingBackend scriptingBackend;

	public AndroidBuildType androidBuildType;

	[Header("Immediate - Takes effect immediately when you select this build config")]
	public BuildServerURL buildServerURL = BuildServerURL.Production;

	public string customServerURL;

	[SerializeField]
	[HideInInspector]
	private long expirationDateTicks;

	public string GetServerURL()
	{
		return buildServerURL switch
		{
			BuildServerURL.None => "", 
			BuildServerURL.Custom => customServerURL, 
			BuildServerURL.Localhost => "http://localhost:8080", 
			BuildServerURL.Production => "https://polytopia-prod.net/", 
			BuildServerURL.Slot1 => "https://polytopia-backend-prod-linux-slot1.azurewebsites.net/", 
			BuildServerURL.Slot2 => "https://polytopia-backend-prod-linux-slot2.azurewebsites.net/", 
			BuildServerURL.Staging => "https://polytopia-backend-dev-linux-esport.azurewebsites.net/", 
			BuildServerURL.Testing => "https://polytopia-backend-dev-linux.azurewebsites.net/", 
			_ => throw new Exception("Not implemented"), 
		};
	}

	public DateTime? GetExpirationDate()
	{
		if (expirationDateTicks <= 0)
		{
			return null;
		}
		return new DateTime(expirationDateTicks);
	}

	public void ApplyExpirationDate()
	{
		expirationDateTicks = ((daysUntilBuildExpires == 0) ? 0 : DateTime.Now.AddDays(daysUntilBuildExpires).Ticks);
	}
}
