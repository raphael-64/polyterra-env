using UnityEngine;

[CreateAssetMenu]
public class UICanvasScaleData : ScriptableObject
{
	public float globalUIScaleMultiplier = 1f;

	public ScreenLimitData[] screenLimits;
}
