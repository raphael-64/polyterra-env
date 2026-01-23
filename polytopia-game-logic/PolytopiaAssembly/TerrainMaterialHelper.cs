using UnityEngine;

public class TerrainMaterialHelper
{
	public static void SetSpriteSaturated(PolytopiaSpriteRenderer spriteRenderer, bool desaturated = false)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		spriteRenderer.Color = Color32.op_Implicit(desaturated ? new Color32((byte)243, (byte)243, (byte)243, (byte)127) : new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
	}

	public static void SetSpriteTint(PolytopiaSpriteRenderer spriteRenderer, Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		spriteRenderer.Color = color;
	}
}
