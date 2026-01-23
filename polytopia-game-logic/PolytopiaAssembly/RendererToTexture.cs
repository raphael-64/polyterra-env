using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RendererToTexture : MonoBehaviour
{
	public SpriteRenderer SpriteRenderer;

	public Sprite Sprite;

	public Texture2D texture2D;

	public RenderTexture renderTexture;

	public SpriteRenderer AdditionalSpriteRenderer;

	private void Awake()
	{
		SpriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		Sprite = SpriteRenderer.sprite;
	}

	private void OnDestroy()
	{
		Object.Destroy((Object)(object)renderTexture);
	}

	public void SetRenderTexture()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Debug.LogWarning((object)"here");
		texture2D = ToTexture(renderTexture);
		Sprite = Sprite.Create(texture2D, Sprite.rect, Sprite.pivot, Sprite.pixelsPerUnit);
		AdditionalSpriteRenderer.sprite = Sprite;
	}

	public void RenderToTexture()
	{
		RendererToTextureFeature.Instance.Add(this);
	}

	public Texture2D ToTexture(RenderTexture renderTexture)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		Texture2D val = new Texture2D(((Texture)renderTexture).width, ((Texture)renderTexture).height, (TextureFormat)5, false);
		RenderTexture.active = renderTexture;
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)renderTexture).width, (float)((Texture)renderTexture).height), 0, 0);
		val.Apply();
		return val;
	}
}
