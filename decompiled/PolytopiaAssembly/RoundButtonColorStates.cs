using System;
using UnityEngine;

[Serializable]
public class RoundButtonColorStates
{
	public Color activeColorHighlighted = ColorConstants.blue;

	public Color deactiveColorHighlighted = Color.black;

	public Color activeColor = Color.white;

	public Color deactiveColor = Color.gray;

	public Color GetColorForState(bool active = false, bool highlighted = false)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (highlighted)
		{
			if (active)
			{
				return activeColorHighlighted;
			}
			return deactiveColorHighlighted;
		}
		if (active)
		{
			return activeColor;
		}
		return deactiveColor;
	}
}
