using UnityEngine;

public static class AudioUtils
{
	public static SFXTypes RandomCoin => Random.Range(1, 5) switch
	{
		1 => SFXTypes.Coin1, 
		2 => SFXTypes.Coin2, 
		3 => SFXTypes.Coin3, 
		_ => SFXTypes.Coin4, 
	};

	public static SFXTypes RandomCloud => Random.Range(1, 5) switch
	{
		1 => SFXTypes.Cloud1, 
		2 => SFXTypes.Cloud2, 
		3 => SFXTypes.Cloud3, 
		_ => SFXTypes.Cloud4, 
	};

	public static SFXTypes GetCloudForCoordinate(WorldCoordinates coordinate)
	{
		return Mathf.CeilToInt(Mathf.Abs(Mathf.Sin((float)(coordinate.X + coordinate.Y))) * 4f) switch
		{
			1 => SFXTypes.Cloud1, 
			2 => SFXTypes.Cloud2, 
			3 => SFXTypes.Cloud3, 
			_ => SFXTypes.Cloud4, 
		};
	}
}
