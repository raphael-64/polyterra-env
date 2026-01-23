using System;
using System.Collections;
using System.IO;
using Polytopia.IO;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NewsListItem : UIButtonBase
{
	[Header("News Item")]
	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected TextMeshProUGUI bodylabel;

	[SerializeField]
	protected TextMeshProUGUI timeLabel;

	[SerializeField]
	protected RawImage image;

	[SerializeField]
	protected LayoutElement imageLayoutElement;

	[Space]
	[SerializeField]
	protected ColorStates bgColorStates;

	[SerializeField]
	protected ColorStates labelColorStates;

	protected string linkUrl;

	protected string imageUrl;

	public Color BgColor
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			switch (buttonState)
			{
			case ButtonStates.None:
				if (!ButtonEnabled)
				{
					return bgColorStates.disabledColor;
				}
				if (Highlighted)
				{
					return bgColorStates.highlightedColor;
				}
				return bgColorStates.defaultColor;
			case ButtonStates.Over:
			case ButtonStates.Down:
				if (!ButtonEnabled)
				{
					return bgColorStates.disabledColor;
				}
				if (Highlighted)
				{
					return bgColorStates.highlightedHoverColor;
				}
				return bgColorStates.hoverColor;
			default:
				return bgColorStates.defaultColor;
			}
		}
	}

	public Color LabelColor
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			switch (buttonState)
			{
			case ButtonStates.None:
				if (!ButtonEnabled)
				{
					return labelColorStates.disabledColor;
				}
				if (Highlighted)
				{
					return labelColorStates.highlightedColor;
				}
				return labelColorStates.defaultColor;
			case ButtonStates.Over:
			case ButtonStates.Down:
				if (!ButtonEnabled)
				{
					return labelColorStates.disabledColor;
				}
				if (Highlighted)
				{
					return labelColorStates.highlightedHoverColor;
				}
				return labelColorStates.hoverColor;
			default:
				return labelColorStates.defaultColor;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		((MonoBehaviour)this).StopAllCoroutines();
	}

	public void SetData(NewsItem data)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		((Component)image).gameObject.SetActive(false);
		((TMP_Text)bodylabel).text = data.Body;
		((Component)timeLabel).gameObject.SetActive(false);
		linkUrl = data.Link;
		if (string.IsNullOrEmpty(linkUrl))
		{
			((Behaviour)base.button).enabled = false;
			ButtonEnabled = false;
			bg.sprite = null;
			((Graphic)bg).color = ColorUtil.SetAlphaOnColor(Color.black, 0.8f);
		}
		else
		{
			ButtonEnabled = true;
			((Behaviour)base.button).enabled = true;
		}
		imageUrl = data.Image;
		if (!string.IsNullOrEmpty(imageUrl) && ((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(GetTexture(imageUrl));
		}
		if (data.Date.HasValue)
		{
			UpdateTime(data.Date.Value);
		}
		UpdateHeight();
	}

	public void ReloadImage()
	{
		if (!string.IsNullOrEmpty(imageUrl))
		{
			((MonoBehaviour)this).StartCoroutine(GetTexture(imageUrl));
		}
	}

	private IEnumerator GetTexture(string url)
	{
		((Component)image).gameObject.SetActive(false);
		string imageCacheDirectoryPath = Paths.GetImageCacheDirectoryPath();
		int num = url.LastIndexOf("/");
		string fileName = url.Substring(num + 1);
		string filePath = Path.Combine(imageCacheDirectoryPath, fileName);
		filePath = Path.ChangeExtension(filePath, ".png");
		Texture2D val = null;
		bool loadingCached = false;
		if (PolytopiaFile.Exists(filePath))
		{
			url = ((!filePath.StartsWith("/")) ? $"file:///{filePath}" : $"file://{filePath}");
			loadingCached = true;
		}
		if ((Object)(object)val == (Object)null)
		{
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, loadingCached);
			yield return www.SendWebRequest();
			if ((int)www.result != 1)
			{
				Log.Warning("Error loading: \"{0}\" ({1})", new object[2] { url, www.error });
				yield break;
			}
			val = ((DownloadHandlerTexture)www.downloadHandler).texture;
		}
		((Texture)val).wrapMode = (TextureWrapMode)1;
		if (!loadingCached)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			PolytopiaFile.WriteAllBytes(filePath, ImageConversion.EncodeToPNG(val));
			Log.Spam($"NewsListItem :: GetTexture :: Save image {fileName}, save time {Time.realtimeSinceStartup - realtimeSinceStartup} seconds", Array.Empty<object>());
		}
		image.texture = (Texture)(object)val;
		((Component)image).gameObject.SetActive(true);
		UpdateHeight();
	}

	private Texture2D SwitchLoadTextureFromCache(string path)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_0017: Expected O, but got Unknown
		byte[] array = PolytopiaFile.ReadAllBytes(path);
		Texture2D val = new Texture2D(1, 1);
		ImageConversion.LoadImage(val, array);
		return val;
	}

	protected void UpdateTime(long articleUnixTime)
	{
		DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(articleUnixTime);
		((TMP_Text)timeLabel).text = Localization.Get("news.timeago", LocalizationUtils.GetTimeString(time));
		((Component)timeLabel).gameObject.SetActive(true);
	}

	protected void UpdateHeight()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		imageLayoutElement.preferredWidth = base.rectTransform.sizeDelta.x;
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		UpdateColors();
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		UpdateColors();
	}

	public override void PointerClick(PointerEventData eventData)
	{
		base.PointerClick(eventData);
		if (ButtonEnabled && !m_blockClick)
		{
			UpdateColors();
			if (!string.IsNullOrEmpty(linkUrl))
			{
				NativeHelpers.OpenURL(linkUrl);
			}
		}
	}

	public override void OnSubmit(BaseEventData data)
	{
		if (ButtonEnabled)
		{
			OnPointerClick(null);
		}
	}

	public override void OnDeselect(BaseEventData data)
	{
		base.OnDeselect(data);
		if (ButtonEnabled)
		{
			UpdateColors();
		}
	}

	protected void UpdateColors()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)bg).color = BgColor;
		((Graphic)bodylabel).color = LabelColor;
		((Graphic)timeLabel).color = LabelColor;
	}

	public void Clear()
	{
		Object.Destroy((Object)(object)image.texture);
	}
}
