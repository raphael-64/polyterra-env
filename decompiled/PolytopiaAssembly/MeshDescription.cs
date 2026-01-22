using System;
using UnityEngine;

[Serializable]
public struct MeshDescription : IEquatable<MeshDescription>
{
	public Sprite sprite;

	public Color color;

	public int flip;

	public MeshDescription(SpriteRenderer spriteRenderer)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		sprite = spriteRenderer.sprite;
		color = spriteRenderer.color;
		flip = (spriteRenderer.flipX ? 2 : 0) + (spriteRenderer.flipY ? 1 : 0);
	}

	public MeshDescription(PolytopiaSpriteRenderer spriteRenderer)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		sprite = spriteRenderer.Sprite;
		color = spriteRenderer.Color;
		flip = 0;
	}

	public MeshDescription(Sprite sprite, Color color, int flip)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		this.sprite = sprite;
		this.color = color;
		this.flip = flip;
	}

	public bool Equals(MeshDescription otherDescription)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sprite == (Object)(object)otherDescription.sprite && color == otherDescription.color)
		{
			return flip == otherDescription.flip;
		}
		return false;
	}

	public override int GetHashCode()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return ((13 * 7 + ((!((Object)(object)sprite == (Object)null)) ? ((object)sprite).GetHashCode() : 0)) * 7 + (int)ColorUtil.ColorToInt(color)) * 7 + flip;
	}
}
