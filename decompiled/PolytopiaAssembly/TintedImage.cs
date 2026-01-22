using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TintedImage
{
	public Image main;

	public Image tint;

	[NonSerialized]
	public SpriteHandle mainSpriteHandle;

	[NonSerialized]
	public SpriteHandle tintSpriteHandle;

	[NonSerialized]
	public Color tintColor;

	public void Init()
	{
		mainSpriteHandle = new SpriteHandle();
		mainSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			SetSprite(main, spriteHandle.sprite, Color.white);
		});
		tintSpriteHandle = new SpriteHandle();
		tintSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			SetSprite(tint, spriteHandle.sprite, tintColor);
		});
	}

	private void SetSprite(Image image, Sprite sprite, Color color)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)image == (Object)null))
		{
			if ((Object)(object)sprite == (Object)null)
			{
				((Behaviour)image).enabled = false;
				return;
			}
			((Behaviour)image).enabled = true;
			image.sprite = sprite;
			image.useSpriteMesh = true;
			((Graphic)image).color = color;
		}
	}
}
