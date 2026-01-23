using UnityEngine;

[ExecuteInEditMode]
public class SimpleBlit : MonoBehaviour
{
	[SerializeField]
	private Texture2D _baseTexture;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit((Texture)(object)_baseTexture, destination);
	}
}
