using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class TextMeshProUGUIExtension
{
	public const string TRIBE_LINK_NAME = "Tribe_";

	public const string OPINION_LINK_NAME = "Opinion_";

	public static string TRIBE_ICON_SPACE_TEXT = $"<space={25}>";

	public const int TRIBE_BUTTON_ICON_SIZE = 25;

	public const int TRIBE_BUTTON_OFFSET_SIZE = 14;

	public const float FONT_SIZE = 18f;

	public static byte GetTribeID(this string text)
	{
		return Convert.ToByte(text.Replace("Tribe_", ""));
	}

	public static string AddInvisibleTag(this string text)
	{
		return "<alpha=#00>" + text + "<alpha=#FF>";
	}

	public static string AddNoBreakingTag(this string text)
	{
		return "<nobr>" + text + "</nobr>";
	}

	public static string AddFontSizeTag(this string text, float fontSize)
	{
		return $"<size={fontSize}>{text}<size=100%>";
	}

	public static string AddSpaceTag(this string text, int space)
	{
		return $"<space={space}>{text}";
	}

	public static string AddLinkTag(this string text)
	{
		return "<link>" + text + "</link>";
	}

	public static string AddLinkTag(this string text, string tag)
	{
		return "<link=\"" + tag + "\">" + text + "</link>";
	}

	public static string GetLinkedText(this string text)
	{
		return text.AddSpaceTag(14).AddLinkTag().AddInvisibleTag()
			.AddNoBreakingTag()
			.AddFontSizeTag(18f);
	}

	public static string GetLinkedText(this string text, string tag)
	{
		return text.AddSpaceTag(14).AddLinkTag(tag).AddInvisibleTag()
			.AddNoBreakingTag()
			.AddFontSizeTag(18f);
	}

	public static string GetLinkedTribeNameWithSpace(this PlayerState playerState, GameState gameState)
	{
		string localizedTribeName = playerState.GetLocalizedTribeName(gameState);
		return (TRIBE_ICON_SPACE_TEXT + localizedTribeName).AddSpaceTag(14).AddLinkTag("Tribe_" + playerState.Id).AddInvisibleTag()
			.AddNoBreakingTag()
			.AddFontSizeTag(18f);
	}

	public static TMP_WordInfo? GetWordInfo(this TextMeshProUGUI textUGUI, string word)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		TMP_WordInfo? result = null;
		float num = ((TMP_Text)textUGUI).text.Length;
		TMP_WordInfo[] wordInfo = ((TMP_Text)textUGUI).textInfo.wordInfo;
		for (int i = 0; i < wordInfo.Length; i++)
		{
			TMP_WordInfo val = wordInfo[i];
			if (val.characterCount > 0 && ((TMP_WordInfo)(ref val)).GetWord().Equals(word) && !((float)val.lastCharacterIndex >= num))
			{
				result = val;
				break;
			}
		}
		return result;
	}

	public static List<TMP_WordInfo?> GetWordsInfo(this TextMeshProUGUI textUGUI, List<string> words)
	{
		List<TMP_WordInfo?> list = new List<TMP_WordInfo?>(words.Count);
		for (int i = 0; i < words.Count; i++)
		{
			TMP_WordInfo? wordInfo = textUGUI.GetWordInfo(words[i]);
			if (wordInfo.HasValue)
			{
				list.Add(wordInfo);
			}
		}
		return list;
	}

	public static Bounds GetLinkBounds(this TextMeshProUGUI textUGUI, TMP_LinkInfo linkInfo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return textUGUI.GetBounds(linkInfo.textComponent, linkInfo.linkTextfirstCharacterIndex, linkInfo.linkTextLength);
	}

	private static Bounds GetBounds(this TextMeshProUGUI textUGUI, TMP_Text textComponent, int firstCharacterIndex, int textLength)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Invalid comparison between Unknown and I4
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((TMP_Text)textUGUI).transform;
		TMP_TextInfo textInfo = ((TMP_Text)textUGUI).textInfo;
		bool flag = false;
		Vector3 val = Vector3.zero;
		Vector3 val2 = Vector3.zero;
		_ = Vector3.zero;
		Vector3 val3 = Vector3.zero;
		float num = float.NegativeInfinity;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < textLength; i++)
		{
			int num3 = firstCharacterIndex + i;
			TMP_CharacterInfo val4 = textInfo.characterInfo[num3];
			int lineNumber = val4.lineNumber;
			_ = ref textInfo.lineInfo[lineNumber];
			bool flag2 = num3 <= textComponent.maxVisibleCharacters && val4.lineNumber <= textComponent.maxVisibleLines && ((int)textComponent.overflowMode != 5 || val4.pageNumber + 1 == textComponent.pageToDisplay);
			num = Mathf.Round(Mathf.Max(num, val4.ascender));
			num2 = Mathf.Round(Mathf.Min(num2, val4.descender));
			if (!flag && flag2)
			{
				flag = true;
				((Vector3)(ref val))._002Ector(val4.bottomLeft.x, val4.descender, 0f);
				((Vector3)(ref val2))._002Ector(val4.bottomLeft.x, val4.ascender, 0f);
				if (textLength == 1)
				{
					flag = false;
					val2 = transform.TransformPoint(new Vector3(val2.x, num, 0f));
					val = transform.TransformPoint(new Vector3(val.x, num2, 0f));
					transform.TransformPoint(new Vector3(val4.topRight.x, num2, 0f));
					val3 = transform.TransformPoint(new Vector3(val4.topRight.x, num, 0f));
				}
			}
			if (flag && i == textLength - 1)
			{
				flag = false;
				val2 = transform.TransformPoint(new Vector3(val2.x, num, 0f));
				val = transform.TransformPoint(new Vector3(val.x, num2, 0f));
				transform.TransformPoint(new Vector3(val4.topRight.x, num2, 0f));
				val3 = transform.TransformPoint(new Vector3(val4.topRight.x, num, 0f));
			}
			else if (flag && lineNumber != textInfo.characterInfo[num3 + 1].lineNumber)
			{
				flag = false;
				val2 = transform.TransformPoint(new Vector3(val2.x, num, 0f));
				val = transform.TransformPoint(new Vector3(val.x, num2, 0f));
				transform.TransformPoint(new Vector3(val4.topRight.x, num2, 0f));
				val3 = transform.TransformPoint(new Vector3(val4.topRight.x, num, 0f));
				num = float.NegativeInfinity;
				num2 = float.PositiveInfinity;
			}
		}
		Vector3 val5 = val3 - val;
		return new Bounds(val + val5 * 0.5f, val5);
	}
}
