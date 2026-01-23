using UnityEngine;

[ExecuteInEditMode]
public class FilterTest : MonoBehaviour
{
	private enum DownSampleMode
	{
		Off,
		Half,
		Quarter
	}

	[SerializeField]
	[HideInInspector]
	private Shader _shader;

	[SerializeField]
	private DownSampleMode _downSampleMode = DownSampleMode.Quarter;

	[SerializeField]
	[Range(0f, 8f)]
	private int _iteration = 4;

	private Material _material;

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		if ((Object)(object)_material == (Object)null)
		{
			_material = new Material(_shader);
			((Object)_material).hideFlags = (HideFlags)61;
		}
		RenderTexture temporary;
		RenderTexture temporary2;
		if (_downSampleMode == DownSampleMode.Half)
		{
			temporary = RenderTexture.GetTemporary(((Texture)source).width / 2, ((Texture)source).height / 2);
			temporary2 = RenderTexture.GetTemporary(((Texture)source).width / 2, ((Texture)source).height / 2);
			Graphics.Blit((Texture)(object)source, temporary);
		}
		else if (_downSampleMode == DownSampleMode.Quarter)
		{
			temporary = RenderTexture.GetTemporary(((Texture)source).width / 4, ((Texture)source).height / 4);
			temporary2 = RenderTexture.GetTemporary(((Texture)source).width / 4, ((Texture)source).height / 4);
			Graphics.Blit((Texture)(object)source, temporary, _material, 0);
		}
		else
		{
			temporary = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height);
			temporary2 = RenderTexture.GetTemporary(((Texture)source).width, ((Texture)source).height);
			Graphics.Blit((Texture)(object)source, temporary);
		}
		for (int i = 0; i < _iteration; i++)
		{
			Graphics.Blit((Texture)(object)temporary, temporary2, _material, 1);
			Graphics.Blit((Texture)(object)temporary2, temporary, _material, 2);
		}
		Graphics.Blit((Texture)(object)temporary, destination);
		RenderTexture.ReleaseTemporary(temporary);
		RenderTexture.ReleaseTemporary(temporary2);
	}
}
