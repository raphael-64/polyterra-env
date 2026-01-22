using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[CreateAssetMenu(fileName = "UIIconData", menuName = "UIIconData", order = 9002)]
public class UIIconData : ScriptableObject
{
	[Serializable]
	public class SpriteData
	{
		public string id;

		public Sprite sprite;
	}

	[Serializable]
	public class CursorData
	{
		public enum CursorStyles
		{
			Default,
			Hover
		}

		public CursorStyles style;

		public Texture2D texture;
	}

	public string defaultIcon;

	public List<SpriteData> iconData;

	public List<CursorData> cursorData;

	protected Dictionary<string, int> iconMap = new Dictionary<string, int>();

	protected Dictionary<CursorData.CursorStyles, int> cursorMap = new Dictionary<CursorData.CursorStyles, int>();

	public void Init()
	{
		iconMap.Clear();
		cursorMap.Clear();
		for (int i = 0; i < iconData.Count; i++)
		{
			iconMap.Add(iconData[i].id, i);
		}
		for (int j = 0; j < cursorData.Count; j++)
		{
			cursorMap.Add(cursorData[j].style, j);
		}
	}

	public Sprite GetSprite(string id)
	{
		int value = 0;
		if (iconMap.TryGetValue(id, out value))
		{
			return iconData[value].sprite;
		}
		if (iconMap.TryGetValue(defaultIcon, out value))
		{
			return iconData[value].sprite;
		}
		Log.Warning("Couldn't find sprite with id : {0}", new object[1] { id });
		return null;
	}

	public Image GetImage(string id)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		int value = 0;
		Sprite val = null;
		if (iconMap.TryGetValue(id, out value))
		{
			val = iconData[value].sprite;
		}
		else
		{
			if (iconMap.TryGetValue(defaultIcon, out value))
			{
				val = iconData[value].sprite;
			}
			Log.Warning("Couldn't find sprite with id : {0}", new object[1] { id });
		}
		if ((Object)(object)val != (Object)null)
		{
			Image obj = new GameObject
			{
				name = $"{id}_Icon"
			}.AddComponent<Image>();
			obj.sprite = val;
			obj.useSpriteMesh = true;
			((Graphic)obj).SetNativeSize();
			return obj;
		}
		return null;
	}

	public Texture2D GetCursor(CursorData.CursorStyles style)
	{
		if (cursorMap.TryGetValue(style, out var value))
		{
			return cursorData[value].texture;
		}
		Log.Error("Couldn't find cursor with style : {0}", new object[1] { style });
		return null;
	}
}
