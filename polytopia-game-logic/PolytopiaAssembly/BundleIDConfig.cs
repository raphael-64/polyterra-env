using UnityEngine;

[CreateAssetMenu]
public class BundleIDConfig : ScriptableObject
{
	public string iOSBundleID;

	public string AndroidBundleID;

	public string StandaloneBundleID;

	public TextAsset GoogleServicesInfoPlist;

	public TextAsset GooglePlayGameSettings;

	public string KeystoreName;

	public string KeyaliasName;
}
