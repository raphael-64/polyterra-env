using System.Threading.Tasks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SteamImage : MonoBehaviour
{
	public void LoadTextureFromImage(Image img)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D((int)img.Width, (int)img.Height);
		for (int i = 0; i < img.Width; i++)
		{
			for (int j = 0; j < img.Height; j++)
			{
				Color pixel = ((Image)(ref img)).GetPixel(i, j);
				val.SetPixel(i, (int)img.Height - j, Color32.op_Implicit(new Color32(pixel.r, pixel.g, pixel.b, pixel.a)));
			}
		}
		val.Apply();
		ApplyTexture(val);
	}

	public async Task LoadTextureFromUrl(string url)
	{
		UnityWebRequest request = UnityWebRequestTexture.GetTexture(url, true);
		UnityWebRequestAsyncOperation r = request.SendWebRequest();
		while (!((AsyncOperation)r).isDone)
		{
			await Task.Delay(10);
		}
		if ((int)request.result == 1)
		{
			DownloadHandler downloadHandler = request.downloadHandler;
			DownloadHandlerTexture val = (DownloadHandlerTexture)(object)((downloadHandler is DownloadHandlerTexture) ? downloadHandler : null);
			((Object)val.texture).name = url;
			ApplyTexture(val.texture);
		}
	}

	public virtual void ApplyTexture(Texture2D texture)
	{
		RawImage component = ((Component)this).GetComponent<RawImage>();
		if ((Object)(object)component != (Object)null)
		{
			component.texture = (Texture)(object)texture;
		}
	}
}
