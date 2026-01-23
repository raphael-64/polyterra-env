using System.Collections;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class OutlinesManager : MonoBehaviour
{
	public Dictionary<Texture2D, RenderTexture> outlines = new Dictionary<Texture2D, RenderTexture>();

	public List<StylePicker> testStylePickers = new List<StylePicker>();

	public Texture2D outlineTexture;

	public RenderTexture renderTexture;

	public List<Vector2> points = new List<Vector2>();

	public Rect rect;

	public Bounds bounds;

	private IEnumerator Start()
	{
		yield return (object)new WaitForSeconds(0.3f);
		foreach (StylePicker testStylePicker in testStylePickers)
		{
			testStylePicker.SetStyleAndSkin("1", SkinType.Ninja);
		}
		yield return (object)new WaitForSeconds(0.3f);
		GenerateOutlineTexture();
	}

	private void GenerateOutlineTexture()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		StylePicker stylePicker = testStylePickers[0];
		_ = stylePicker.SpriteRenderer;
		Sprite sprite = stylePicker.SpriteRenderer.sprite;
		Texture2D texture = sprite.texture;
		rect = sprite.rect;
		bounds = sprite.bounds;
		sprite.GetPhysicsShape(0, points);
		Vector2Int size = default(Vector2Int);
		((Vector2Int)(ref size))._002Ector(((Texture)texture).width, ((Texture)texture).height);
		Debug.LogWarning((object)((Texture)sprite.texture).width);
		outlineTexture = new Texture2D(((Vector2Int)(ref size)).x, ((Vector2Int)(ref size)).y);
		FillTexture(ref outlineTexture);
		DrawTexture(texture, ref outlineTexture, size, new Vector2Int(0, 0));
		outlineTexture.Apply();
	}

	private void DrawTexture(Texture2D inputTexture, ref Texture2D outlineTexture, Vector2Int size, Vector2Int pos)
	{
		int x = ((Vector2Int)(ref size)).x;
		int y = ((Vector2Int)(ref size)).y;
		Color[] pixels = inputTexture.GetPixels(0, 0, x, y);
		Color[] pixels2 = outlineTexture.GetPixels(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, x, y);
		int num = pixels.Length;
		for (int i = 0; i < num; i++)
		{
			pixels[i].r = 1f;
			pixels[i].g = 1f;
			pixels[i].b = 1f;
			pixels[i].a = Mathf.Clamp01(pixels2[i].a + ((pixels[i].a > 0.5f) ? pixels[i].a : 0f));
		}
		outlineTexture.SetPixels(((Vector2Int)(ref pos)).x, ((Vector2Int)(ref pos)).y, x, y, pixels);
	}

	private void FillTexture(ref Texture2D outlineTexture)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		Color[] pixels = outlineTexture.GetPixels();
		for (int i = 0; i < pixels.Length; i++)
		{
			pixels[i] = Color.clear;
		}
		outlineTexture.SetPixels(pixels);
	}
}
