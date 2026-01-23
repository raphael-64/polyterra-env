using UnityEngine;

public class OutlineCreatorTest : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer outlineTarget;

	[SerializeField]
	protected SpriteRenderer[] renderers;

	[SerializeField]
	protected float borderSize;

	protected Texture2D outlineTexture;

	private void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector((float)Mathf.CeilToInt(((Bounds)(ref bounds)).size.x * 1056f + borderSize * 2f), (float)Mathf.CeilToInt(((Bounds)(ref bounds)).size.y * 1056f + borderSize * 2f));
		Log.Verbose("pixelSize: {0}", new object[1] { val });
		outlineTexture = new Texture2D(Mathf.CeilToInt(val.x), Mathf.CeilToInt(val.y));
		Color[] pixels = outlineTexture.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = Color.clear;
		}
		outlineTexture.SetPixels(pixels);
		DrawSprite(renderers[0]);
		DrawSprite(renderers[1]);
		DrawSprite(renderers[2]);
		outlineTexture.Apply();
		((Object)outlineTexture).name = "OutlineTexture";
		Sprite sprite = Sprite.Create(outlineTexture, new Rect(0f, 0f, (float)Mathf.CeilToInt(val.x), (float)Mathf.CeilToInt(val.y)), new Vector2(0.5f, 0.5f), 1056f);
		outlineTarget.sprite = sprite;
		((Component)outlineTarget).transform.localPosition = ((Bounds)(ref bounds)).center;
	}

	protected void DrawSprite(SpriteRenderer spriteRenderer)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		Bounds bounds2 = ((Renderer)spriteRenderer).bounds;
		Vector2 val = Vector2.op_Implicit(-(((Bounds)(ref bounds)).min - ((Bounds)(ref bounds2)).min) * 1056f + new Vector3(borderSize, borderSize));
		Log.Verbose("spriteRenderer.sprite: {0}", new object[1] { spriteRenderer.sprite });
		Texture2D texture = spriteRenderer.sprite.texture;
		Rect val2 = spriteRenderer.sprite.textureRect;
		int num = Mathf.RoundToInt(((Rect)(ref val2)).x);
		val2 = spriteRenderer.sprite.textureRect;
		int num2 = Mathf.RoundToInt(((Rect)(ref val2)).y);
		val2 = spriteRenderer.sprite.rect;
		int num3 = Mathf.RoundToInt(((Rect)(ref val2)).width);
		val2 = spriteRenderer.sprite.rect;
		Color[] pixels = texture.GetPixels(num, num2, num3, Mathf.RoundToInt(((Rect)(ref val2)).height));
		Texture2D obj = outlineTexture;
		int num4 = Mathf.RoundToInt(val.x);
		int num5 = Mathf.RoundToInt(val.y);
		val2 = spriteRenderer.sprite.rect;
		int num6 = Mathf.RoundToInt(((Rect)(ref val2)).width);
		val2 = spriteRenderer.sprite.rect;
		Color[] pixels2 = obj.GetPixels(num4, num5, num6, Mathf.RoundToInt(((Rect)(ref val2)).height));
		_ = pixels.Length;
		int num7 = 0;
		int num8 = 0;
		while (true)
		{
			float num9 = num8;
			val2 = spriteRenderer.sprite.rect;
			if (!(num9 < ((Rect)(ref val2)).width))
			{
				break;
			}
			int num10 = 0;
			while (true)
			{
				float num11 = num10;
				val2 = spriteRenderer.sprite.rect;
				if (!(num11 < ((Rect)(ref val2)).height))
				{
					break;
				}
				pixels[num7].r = 1f;
				pixels[num7].g = 1f;
				pixels[num7].b = 1f;
				pixels[num7].a = Mathf.Clamp01(pixels2[num7].a + pixels[num7].a);
				num7++;
				num10++;
			}
			num8++;
		}
		Texture2D obj2 = outlineTexture;
		int num12 = Mathf.RoundToInt(val.x);
		int num13 = Mathf.RoundToInt(val.y);
		val2 = spriteRenderer.sprite.rect;
		int num14 = Mathf.RoundToInt(((Rect)(ref val2)).width);
		val2 = spriteRenderer.sprite.rect;
		obj2.SetPixels(num12, num13, num14, Mathf.RoundToInt(((Rect)(ref val2)).height), pixels);
		Log.Verbose("spriteRenderer.name: {0}", new object[1] { ((Object)spriteRenderer).name });
		Log.Verbose("pos : {0}", new object[1] { val });
		Log.Verbose("testObjectBounds: {0}", new object[1] { bounds2 });
		Log.Verbose("spriteRenderer.sprite.textureRect: {0}", new object[1] { spriteRenderer.sprite.pivot });
		Log.Verbose("spriteRenderer.sprite.rect: {0}", new object[1] { spriteRenderer.sprite.rect });
		Log.Verbose("Mathf.CeilToInt(spriteRenderer.sprite.textureRect.width): {0}", new object[1] { ((Texture)spriteRenderer.sprite.texture).width });
	}

	protected Bounds GetBounds()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Bounds result = default(Bounds);
		SpriteRenderer[] array = renderers;
		foreach (SpriteRenderer val in array)
		{
			((Bounds)(ref result)).Encapsulate(((Renderer)val).bounds);
		}
		return result;
	}
}
