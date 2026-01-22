using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CMAccountInfoRow : UIButtonBase, IListCellNavigation
{
	public Image icon;

	public TextMeshProUGUI header;

	private string link;

	private Sprite profileSprite;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	public override void Awake()
	{
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetProfileImage(spriteHandle.sprite);
		});
		base.Awake();
	}

	public void LoadProfileImage(string url, bool forceUpdate = false)
	{
		if ((Object)(object)profileSprite == (Object)null || forceUpdate)
		{
			((MonoBehaviour)this).StartCoroutine(DownloadImage(url));
		}
		else
		{
			SetProfileImage(profileSprite);
		}
	}

	private IEnumerator DownloadImage(string url)
	{
		((Graphic)icon).color = new Color(1f, 1f, 1f, 0f);
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();
		if ((int)www.result == 2)
		{
			Log.Warning(www.error, Array.Empty<object>());
			((Component)icon).gameObject.SetActive(false);
			yield break;
		}
		Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
		((Texture)texture).wrapMode = (TextureWrapMode)1;
		profileSprite = Sprite.Create(texture, new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), new Vector2((float)((Texture)texture).width * 0.5f, (float)((Texture)texture).height * 0.5f));
		SetProfileImage(profileSprite);
	}

	private void SetProfileImage(Sprite sprite)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		((Component)icon).gameObject.SetActive(true);
		icon.sprite = sprite;
		icon.useSpriteMesh = true;
		((Graphic)icon).color = new Color(1f, 1f, 1f, 1f);
	}

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)base.button;
	}

	public Selectable GetAccessorySelectable()
	{
		return (Selectable)(object)base.button;
	}
}
