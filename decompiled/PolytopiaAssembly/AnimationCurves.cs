using UnityEngine;

[CreateAssetMenu]
public class AnimationCurves : ScriptableObject
{
	public AnimationCurve moveUnitAnimationCurve;

	public AnimationCurve unitAttackAndMoveAnimationCurve;

	public AnimationCurve unitAttackAndReturnAnimationCurve;

	public AnimationCurve stickSensitivityCurve;
}
