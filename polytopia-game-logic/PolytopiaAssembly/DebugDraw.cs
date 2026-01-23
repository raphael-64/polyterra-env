using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DebugDraw
{
	private class DebugLog : MonoBehaviour
	{
		private struct LogItem
		{
			public string key;

			public string text;

			public float timeToLive;

			public int order;

			public bool rightAligned;

			public LogItem(string text, float timeToLive, int order, string key, bool rightAligned = false)
			{
				this.key = key;
				this.text = text;
				this.timeToLive = timeToLive;
				this.order = order;
				this.rightAligned = rightAligned;
			}
		}

		[Range(10f, 20f)]
		public int fontSize = 18;

		public Color textColor = Color.white;

		private Dictionary<string, LogItem> log = new Dictionary<string, LogItem>();

		private bool stylesReady;

		private GUIStyle textStyle;

		private GUIStyle shadowStyle;

		private static Rect[] characterCoordinates;

		private int drawOrder;

		public static Rect[] CharacterCoordinates
		{
			get
			{
				//IL_0097: Unknown result type (might be due to invalid IL or missing references)
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				if (characterCoordinates == null || characterCoordinates.Length == 0)
				{
					characterCoordinates = (Rect[])(object)new Rect[193];
					float num = 1f / (float)bitmapFont.width;
					float num2 = 1f / (float)bitmapFont.height;
					float num3 = 8f * num;
					float num4 = 16f * num2;
					Rect val = default(Rect);
					for (int i = 32; i < 161; i++)
					{
						((Rect)(ref val))._002Ector(num + (float)((i - 32) % 16) * (16f * num), 1f - num4 - (float)((i - 32) / 16) * (16f * num2), num3, num4);
						characterCoordinates[i] = val;
					}
				}
				return characterCoordinates;
			}
		}

		public void SetItem(string text, float timeToLive, string key = null, bool rightAligned = false)
		{
			if (string.IsNullOrEmpty(key))
			{
				key = Random.Range(int.MinValue, 0).ToString();
			}
			if (log.TryGetValue(key, out var value))
			{
				value.text = text;
				value.timeToLive = timeToLive;
				log[key] = value;
			}
			else
			{
				log.Add(key, new LogItem(text, timeToLive, drawOrder++, key, rightAligned));
			}
		}

		public void RemoveItem(string key)
		{
			try
			{
				log.Remove(key);
			}
			catch (KeyNotFoundException)
			{
			}
		}

		public void Clear()
		{
			log.Clear();
		}

		private void OnGUI()
		{
			if (!stylesReady)
			{
				SetupStyles();
			}
			DrawLog();
		}

		private void SetupStyles()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Expected O, but got Unknown
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			textStyle = new GUIStyle();
			textStyle.normal.textColor = textColor;
			textStyle.richText = true;
			shadowStyle = new GUIStyle();
			shadowStyle.normal.textColor = Color32.op_Implicit(new Color32((byte)40, (byte)40, (byte)40, byte.MaxValue));
			shadowStyle.richText = false;
			stylesReady = true;
		}

		private void DrawLog()
		{
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			float num = 5f;
			float num2 = 5f;
			float num3 = 5f;
			float num4 = 5f;
			float num5 = 20f;
			List<LogItem> list = new List<LogItem>(log.Values);
			list.Sort((LogItem x, LogItem y) => x.order.CompareTo(y.order));
			for (int num6 = 0; num6 < list.Count; num6++)
			{
				LogItem value = list[num6];
				if (string.IsNullOrEmpty(value.text))
				{
					RemoveItem(value.key);
					continue;
				}
				if (value.rightAligned)
				{
					textStyle.alignment = (TextAnchor)5;
					shadowStyle.alignment = (TextAnchor)5;
					DrawLine(new Rect(0f, num4, (float)Screen.width - num3, num5), value.text);
					num4 += num5;
				}
				else
				{
					textStyle.alignment = (TextAnchor)3;
					shadowStyle.alignment = (TextAnchor)3;
					if ((Object)(object)bitmapFont != (Object)null)
					{
						DrawLine(new Rect(num, num2, (float)Screen.width, num5), value.text, bitmapFont);
					}
					else
					{
						DrawLine(new Rect(num, num2, (float)Screen.width, num5), value.text);
					}
					num2 += num5;
				}
				if (value.timeToLive > 0f)
				{
					value.timeToLive -= Time.unscaledDeltaTime;
					if (list[num6].timeToLive <= 0f)
					{
						RemoveItem(value.key);
					}
					else
					{
						log[value.key] = value;
					}
				}
			}
		}

		private void DrawLine(Rect rect, string text)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			textStyle.normal.textColor = textColor;
			textStyle.fontSize = fontSize;
			shadowStyle.fontSize = fontSize;
			string text2 = Regex.Replace(text, "<.*?>", "");
			GUI.Label(new Rect(((Rect)(ref rect)).x + 1f, ((Rect)(ref rect)).y + 1f, ((Rect)(ref rect)).width, ((Rect)(ref rect)).height), text2, shadowStyle);
			GUI.Label(new Rect(((Rect)(ref rect)).x + 1f, ((Rect)(ref rect)).y - 1f, ((Rect)(ref rect)).width, ((Rect)(ref rect)).height), text2, shadowStyle);
			GUI.Label(new Rect(((Rect)(ref rect)).x - 1f, ((Rect)(ref rect)).y + 1f, ((Rect)(ref rect)).width, ((Rect)(ref rect)).height), text2, shadowStyle);
			GUI.Label(new Rect(((Rect)(ref rect)).x - 1f, ((Rect)(ref rect)).y - 1f, ((Rect)(ref rect)).width, ((Rect)(ref rect)).height), text2, shadowStyle);
			GUI.Label(rect, text, textStyle);
		}

		private void DrawLine(Rect rect, string text, Texture bitmapText)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			char[] array = Regex.Replace(text, "<.*?>", "").ToCharArray();
			((Rect)(ref rect)).width = 8f;
			((Rect)(ref rect)).height = 16f;
			for (int i = 0; i < array.Length; i++)
			{
				int num = array[i];
				if (num > 160 || num < 32)
				{
					num = Convert.ToChar("?");
				}
				Rect val = CharacterCoordinates[num];
				GUI.DrawTextureWithTexCoords(rect, bitmapText, val);
				((Rect)(ref rect)).x = ((Rect)(ref rect)).x + ((Rect)(ref rect)).width;
			}
		}
	}

	private class FPSCounter : MonoBehaviour
	{
		public bool showLowest;

		public bool showAverage;

		public float updateInterval = 0.3f;

		public int precision = 1;

		private int frameCount;

		private int frameCountAverage;

		private float frameAccumulation;

		private float frameAccumulationAverage;

		private float lastFPS;

		private float lowestFPS = float.PositiveInfinity;

		private float averageFPS;

		private bool initialUpdate = true;

		private Coroutine FPSRoutine;

		private StringBuilder fpsLabel = new StringBuilder(64);

		public void ShowFPS(bool enabled)
		{
			ShowFPS(enabled, showLowest: false, showAverage: false);
		}

		public void ShowFPS(bool enabled, bool showLowest, bool showAverage)
		{
			if (enabled)
			{
				this.showLowest = showLowest;
				this.showAverage = showAverage;
				TextLog.SetItem("Calculating FPS", -1f, "DebugDrawFPSCounter", rightAligned: true);
				FPSRoutine = ((MonoBehaviour)this).StartCoroutine(UpdateFPS());
			}
			else
			{
				((MonoBehaviour)this).StopCoroutine(FPSRoutine);
				Reset();
			}
		}

		private void Reset()
		{
			frameCount = 0;
			frameCountAverage = 0;
			lastFPS = 0f;
			averageFPS = 0f;
			lowestFPS = float.PositiveInfinity;
			frameAccumulation = 0f;
			frameAccumulationAverage = 0f;
			initialUpdate = true;
		}

		private void Update()
		{
			frameAccumulation += 1f / Time.unscaledDeltaTime;
			frameCount++;
		}

		private IEnumerator UpdateFPS()
		{
			frameAccumulation = 0f;
			while (Application.isPlaying)
			{
				fpsLabel.Length = 0;
				float num = frameAccumulation / (float)((frameCount <= 0) ? 1 : frameCount);
				if (num < lastFPS)
				{
					fpsLabel.Append("<color=#FF0000>▼</color>");
				}
				else
				{
					fpsLabel.Append("<color=#00FF00>▲</color>");
				}
				if (num < (float)Application.targetFrameRate)
				{
					fpsLabel.Append("<color=#FF0000>" + num.ToString("f" + precision) + "</color>");
				}
				else
				{
					fpsLabel.Append(num.ToString("f" + precision));
				}
				lastFPS = num;
				frameAccumulation = 0f;
				frameCount = 0;
				if (showLowest)
				{
					fpsLabel.Append(" / LOW: ");
					if (num < lowestFPS && num > 0f)
					{
						lowestFPS = num;
						fpsLabel.Append("<color=#FF0000>" + lowestFPS.ToString("f" + precision) + "</color>");
					}
					else
					{
						fpsLabel.Append(lowestFPS.ToString("f" + precision));
					}
					if (initialUpdate)
					{
						initialUpdate = false;
						lowestFPS = float.PositiveInfinity;
					}
				}
				if (showAverage)
				{
					fpsLabel.Append(" / AVG: ");
					if (!float.IsNaN(num))
					{
						frameCountAverage++;
						frameAccumulationAverage += num;
						averageFPS = frameAccumulationAverage / (float)frameCountAverage;
						fpsLabel.Append(averageFPS.ToString("f" + precision));
					}
				}
				TextLog.SetItem(fpsLabel.ToString(), -1f, "DebugDrawFPSCounter", rightAligned: true);
				float start = Time.realtimeSinceStartup;
				while (Time.realtimeSinceStartup < start + updateInterval)
				{
					yield return null;
				}
			}
		}
	}

	private const float DEFAULT_LOG_TIME = 10f;

	public static bool enableTextLog = true;

	public static Texture bitmapFont;

	private static GameObject m_debugObject;

	private static FPSCounter m_fpsLog;

	private static DebugLog m_textLog;

	private static GameObject DebugObject
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			if ((Object)(object)m_debugObject == (Object)null)
			{
				m_debugObject = new GameObject("DebugDraw.Log");
			}
			return m_debugObject;
		}
	}

	private static FPSCounter FPSLog
	{
		get
		{
			if ((Object)(object)m_fpsLog == (Object)null)
			{
				m_fpsLog = DebugObject.AddComponent<FPSCounter>();
			}
			return m_fpsLog;
		}
	}

	private static DebugLog TextLog
	{
		get
		{
			if ((Object)(object)m_textLog == (Object)null)
			{
				m_textLog = DebugObject.AddComponent<DebugLog>();
			}
			return m_textLog;
		}
	}

	public static void Point(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Point(position, Color.white, 1f, 0f, depthTest: false);
	}

	public static void Point(Vector3 position, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Point(position, color, 1f, 0f, depthTest: false);
	}

	public static void Point(Vector3 position, Color color, float size)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Point(position, color, size, 0f, depthTest: false);
	}

	public static void Point(Vector3 position, Color color, float size, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Point(position, color, size, duration, depthTest: false);
	}

	public static void Point(Vector3 position, Color color, float size, float duration, bool depthTest)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		float num = size * 0.5f;
		Debug.DrawRay(position + Vector3.up * num, -Vector3.up * size, color, duration, depthTest);
		Debug.DrawRay(position + Vector3.right * num, -Vector3.right * size, color, duration, depthTest);
		Debug.DrawRay(position + Vector3.forward * num, -Vector3.forward * size, color, duration, depthTest);
	}

	public static void PointGizmo(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.DrawRay(position + Vector3.up * 0.5f, -Vector3.up);
		Gizmos.DrawRay(position + Vector3.right * 0.5f, -Vector3.right);
		Gizmos.DrawRay(position + Vector3.forward * 0.5f, -Vector3.forward);
	}

	public static void Cube(Vector3 center, Vector3 size)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Cube(center, size, Color.white, 0f, depthTest: false);
	}

	public static void Cube(Vector3 center, Vector3 size, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Cube(center, size, color, 0f, depthTest: false);
	}

	public static void Cube(Vector3 center, Vector3 size, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Cube(center, size, color, duration, depthTest: false);
	}

	public static void Cube(Vector3 center, Vector3 size, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		float x = size.x;
		float y = size.y;
		float z = size.z;
		Vector3 val = center + new Vector3(0f - x, y, 0f - z);
		Vector3 val2 = center + new Vector3(x, y, 0f - z);
		Vector3 val3 = center + new Vector3(0f - x, 0f - y, 0f - z);
		Vector3 val4 = center + new Vector3(x, 0f - y, 0f - z);
		Vector3 val5 = center + new Vector3(0f - x, y, z);
		Vector3 val6 = center + new Vector3(x, y, z);
		Vector3 val7 = center + new Vector3(0f - x, 0f - y, z);
		Vector3 val8 = center + new Vector3(x, 0f - y, z);
		Debug.DrawLine(val, val2, color, duration, depthTest);
		Debug.DrawLine(val2, val4, color, duration, depthTest);
		Debug.DrawLine(val4, val3, color, duration, depthTest);
		Debug.DrawLine(val3, val, color, duration, depthTest);
		Debug.DrawLine(val5, val6, color, duration, depthTest);
		Debug.DrawLine(val6, val8, color, duration, depthTest);
		Debug.DrawLine(val8, val7, color, duration, depthTest);
		Debug.DrawLine(val7, val5, color, duration, depthTest);
		Debug.DrawLine(val, val5, color, duration, depthTest);
		Debug.DrawLine(val2, val6, color, duration, depthTest);
		Debug.DrawLine(val4, val8, color, duration, depthTest);
		Debug.DrawLine(val3, val7, color, duration, depthTest);
	}

	public static void Bounds(Bounds bounds)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Bounds(bounds, Color.white, 0f, depthTest: false);
	}

	public static void Bounds(Bounds bounds, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Bounds(bounds, color, 0f, depthTest: false);
	}

	public static void Bounds(Bounds bounds, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		Bounds(bounds, color, duration, depthTest: false);
	}

	public static void Bounds(Bounds bounds, Color color, float duration, bool depthTest)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Cube(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).extents, color, duration, depthTest);
	}

	public static void BoundsGizmo(Bounds bounds)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.DrawWireCube(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).size);
	}

	public static void Sphere(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Sphere(position, 1f, Color.white, 0f, depthTest: false);
	}

	public static void Sphere(Vector3 position, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Sphere(position, radius, Color.white, 0f, depthTest: false);
	}

	public static void Sphere(Vector3 position, float radius, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Sphere(position, radius, color, 0f, depthTest: false);
	}

	public static void Sphere(Vector3 position, float radius, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Sphere(position, radius, color, duration, depthTest: false);
	}

	public static void Sphere(Vector3 position, float radius, Color color, float duration, bool depthTest)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		float num = 10f;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(position.x, position.y + radius * Mathf.Sin(0f), position.z + radius * Mathf.Cos(0f));
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(position.x + radius * Mathf.Cos(0f), position.y, position.z + radius * Mathf.Sin(0f));
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(position.x + radius * Mathf.Cos(0f), position.y + radius * Mathf.Sin(0f), position.z);
		Vector3 val4 = default(Vector3);
		Vector3 val5 = default(Vector3);
		Vector3 val6 = default(Vector3);
		for (int i = 0; i < 37; i++)
		{
			((Vector3)(ref val4))._002Ector(position.x, position.y + radius * Mathf.Sin(num * (float)i * ((float)Math.PI / 180f)), position.z + radius * Mathf.Cos(num * (float)i * ((float)Math.PI / 180f)));
			((Vector3)(ref val5))._002Ector(position.x + radius * Mathf.Cos(num * (float)i * ((float)Math.PI / 180f)), position.y, position.z + radius * Mathf.Sin(num * (float)i * ((float)Math.PI / 180f)));
			((Vector3)(ref val6))._002Ector(position.x + radius * Mathf.Cos(num * (float)i * ((float)Math.PI / 180f)), position.y + radius * Mathf.Sin(num * (float)i * ((float)Math.PI / 180f)), position.z);
			Debug.DrawLine(val, val4, color, duration, depthTest);
			Debug.DrawLine(val2, val5, color, duration, depthTest);
			Debug.DrawLine(val3, val6, color, duration, depthTest);
			val = val4;
			val2 = val5;
			val3 = val6;
		}
	}

	public static void Capsule(Vector3 start, Vector3 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Capsule(start, end, 1f, Color.white, 0f, depthTest: false);
	}

	public static void Capsule(Vector3 start, Vector3 end, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Capsule(start, end, radius, Color.white, 0f, depthTest: false);
	}

	public static void Capsule(Vector3 start, Vector3 end, float radius, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Capsule(start, end, radius, color, 0f, depthTest: false);
	}

	public static void Capsule(Vector3 start, Vector3 end, float radius, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Capsule(start, end, radius, color, duration, depthTest: false);
	}

	public static void Capsule(Vector3 start, Vector3 end, float radius, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		Vector3 val2 = ((Vector3)(ref val)).normalized * radius;
		Vector3 val3 = Vector3.Slerp(val2, -val2, 0.5f);
		val = Vector3.Cross(val2, val3);
		Vector3 val4 = ((Vector3)(ref val)).normalized * radius;
		val = start - end;
		float num = Mathf.Max(0f, ((Vector3)(ref val)).magnitude * 0.5f - radius);
		Vector3 val5 = (end + start) * 0.5f;
		val = start - val5;
		start = val5 + ((Vector3)(ref val)).normalized * num;
		val = end - val5;
		end = val5 + ((Vector3)(ref val)).normalized * num;
		Cylinder(start, end, radius, color, duration, depthTest);
		for (int i = 1; i < 26; i++)
		{
			float num2 = (float)i / 25f;
			float num3 = (float)(i - 1) / 25f;
			Debug.DrawLine(Vector3.Slerp(val4, -val2, num2) + start, Vector3.Slerp(val4, -val2, num3) + start, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(-val4, -val2, num2) + start, Vector3.Slerp(-val4, -val2, num3) + start, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(val3, -val2, num2) + start, Vector3.Slerp(val3, -val2, num3) + start, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(-val3, -val2, num2) + start, Vector3.Slerp(-val3, -val2, num3) + start, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(val4, val2, num2) + end, Vector3.Slerp(val4, val2, num3) + end, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(-val4, val2, num2) + end, Vector3.Slerp(-val4, val2, num3) + end, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(val3, val2, num2) + end, Vector3.Slerp(val3, val2, num3) + end, color, duration, depthTest);
			Debug.DrawLine(Vector3.Slerp(-val3, val2, num2) + end, Vector3.Slerp(-val3, val2, num3) + end, color, duration, depthTest);
		}
	}

	public static void CapsuleGizmo(Vector3 start, Vector3 end, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		Vector3 val2 = ((Vector3)(ref val)).normalized * radius;
		Vector3 val3 = Vector3.Slerp(val2, -val2, 0.5f);
		val = Vector3.Cross(val2, val3);
		Vector3 val4 = ((Vector3)(ref val)).normalized * radius;
		val = start - end;
		float num = Mathf.Max(0f, ((Vector3)(ref val)).magnitude * 0.5f - radius);
		Vector3 val5 = (end + start) * 0.5f;
		val = start - val5;
		start = val5 + ((Vector3)(ref val)).normalized * num;
		val = end - val5;
		end = val5 + ((Vector3)(ref val)).normalized * num;
		CylinderGizmo(start, end, radius);
		for (int i = 1; i < 26; i++)
		{
			float num2 = (float)i / 25f;
			float num3 = (float)(i - 1) / 25f;
			Gizmos.DrawLine(Vector3.Slerp(val4, -val2, num2) + start, Vector3.Slerp(val4, -val2, num3) + start);
			Gizmos.DrawLine(Vector3.Slerp(-val4, -val2, num2) + start, Vector3.Slerp(-val4, -val2, num3) + start);
			Gizmos.DrawLine(Vector3.Slerp(val3, -val2, num2) + start, Vector3.Slerp(val3, -val2, num3) + start);
			Gizmos.DrawLine(Vector3.Slerp(-val3, -val2, num2) + start, Vector3.Slerp(-val3, -val2, num3) + start);
			Gizmos.DrawLine(Vector3.Slerp(val4, val2, num2) + end, Vector3.Slerp(val4, val2, num3) + end);
			Gizmos.DrawLine(Vector3.Slerp(-val4, val2, num2) + end, Vector3.Slerp(-val4, val2, num3) + end);
			Gizmos.DrawLine(Vector3.Slerp(val3, val2, num2) + end, Vector3.Slerp(val3, val2, num3) + end);
			Gizmos.DrawLine(Vector3.Slerp(-val3, val2, num2) + end, Vector3.Slerp(-val3, val2, num3) + end);
		}
	}

	public static void Cylinder(Vector3 start, Vector3 end)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Cylinder(start, end, 1f, Color.white, 0f, depthTest: false);
	}

	public static void Cylinder(Vector3 start, Vector3 end, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Cylinder(start, end, radius, Color.white, 0f, depthTest: false);
	}

	public static void Cylinder(Vector3 start, Vector3 end, float radius, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Cylinder(start, end, radius, color, 0f, depthTest: false);
	}

	public static void Cylinder(Vector3 start, Vector3 end, float radius, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Cylinder(start, end, radius, color, duration, depthTest: false);
	}

	public static void Cylinder(Vector3 start, Vector3 end, float radius, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		Vector3 val2 = ((Vector3)(ref val)).normalized * radius;
		Vector3 val3 = Vector3.Slerp(val2, -val2, 0.5f);
		val = Vector3.Cross(val2, val3);
		Vector3 val4 = ((Vector3)(ref val)).normalized * radius;
		Circle(start, val2, radius, color, duration, depthTest);
		Circle(end, -val2, radius, color, duration, depthTest);
		Debug.DrawLine(start + val4, end + val4, color, duration, depthTest);
		Debug.DrawLine(start - val4, end - val4, color, duration, depthTest);
		Debug.DrawLine(start + val3, end + val3, color, duration, depthTest);
		Debug.DrawLine(start - val3, end - val3, color, duration, depthTest);
	}

	public static void CylinderGizmo(Vector3 start, Vector3 end, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = end - start;
		Vector3 val2 = ((Vector3)(ref val)).normalized * radius;
		Vector3 val3 = Vector3.Slerp(val2, -val2, 0.5f);
		val = Vector3.Cross(val2, val3);
		Vector3 val4 = ((Vector3)(ref val)).normalized * radius;
		CircleGizmo(start, val2, radius);
		CircleGizmo((start + end) * 0.5f, val2, radius);
		CircleGizmo(end, -val2, radius);
		Gizmos.DrawLine(start + val4, end + val4);
		Gizmos.DrawLine(start - val4, end - val4);
		Gizmos.DrawLine(start + val3, end + val3);
		Gizmos.DrawLine(start - val3, end - val3);
	}

	public static void Circle(Vector3 position, Vector3 up)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Circle(position, up, 1f, Color.white, 0f, depthTest: false);
	}

	public static void Circle(Vector3 position, Vector3 up, float radius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Circle(position, up, radius, Color.white, 0f, depthTest: false);
	}

	public static void Circle(Vector3 position, Vector3 up, float radius, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Circle(position, up, radius, color, 0f, depthTest: false);
	}

	public static void Circle(Vector3 position, Vector3 up, float radius, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Circle(position, up, radius, color, duration, depthTest: false);
	}

	public static void Circle(Vector3 position, Vector3 up, float radius, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((up == Vector3.zero) ? Vector3.up : ((Vector3)(ref up)).normalized) * radius;
		Vector3 val2 = Vector3.Slerp(val, -val, 0.5f);
		Vector3 val3 = Vector3.Cross(val, val2);
		Vector3 val4 = ((Vector3)(ref val3)).normalized * radius;
		Matrix4x4 val5 = default(Matrix4x4);
		((Matrix4x4)(ref val5))[0] = val4.x;
		((Matrix4x4)(ref val5))[1] = val4.y;
		((Matrix4x4)(ref val5))[2] = val4.z;
		((Matrix4x4)(ref val5))[4] = val.x;
		((Matrix4x4)(ref val5))[5] = val.y;
		((Matrix4x4)(ref val5))[6] = val.z;
		((Matrix4x4)(ref val5))[8] = val2.x;
		((Matrix4x4)(ref val5))[9] = val2.y;
		((Matrix4x4)(ref val5))[10] = val2.z;
		Vector3 val6 = position + ((Matrix4x4)(ref val5)).MultiplyPoint3x4(new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)));
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 91; i++)
		{
			zero = position + ((Matrix4x4)(ref val5)).MultiplyPoint3x4(new Vector3(Mathf.Cos((float)(i * 4) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((float)(i * 4) * ((float)Math.PI / 180f))));
			Debug.DrawLine(val6, zero, color, duration, depthTest);
			val6 = zero;
		}
	}

	public static void CircleGizmo(Vector3 position, Vector3 up, float radius)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Vector3)(ref up)).normalized * radius;
		Vector3 val2 = Vector3.Slerp(val, -val, 0.5f);
		Vector3 val3 = Vector3.Cross(val, val2);
		Vector3 val4 = ((Vector3)(ref val3)).normalized * radius;
		Matrix4x4 identity = Matrix4x4.identity;
		((Matrix4x4)(ref identity))[0] = val4.x;
		((Matrix4x4)(ref identity))[1] = val4.y;
		((Matrix4x4)(ref identity))[2] = val4.z;
		((Matrix4x4)(ref identity))[4] = val.x;
		((Matrix4x4)(ref identity))[5] = val.y;
		((Matrix4x4)(ref identity))[6] = val.z;
		((Matrix4x4)(ref identity))[8] = val2.x;
		((Matrix4x4)(ref identity))[9] = val2.y;
		((Matrix4x4)(ref identity))[10] = val2.z;
		Vector3 val5 = position + ((Matrix4x4)(ref identity)).MultiplyPoint3x4(new Vector3(Mathf.Cos(0f), 0f, Mathf.Sin(0f)));
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < 91; i++)
		{
			zero = position + ((Matrix4x4)(ref identity)).MultiplyPoint3x4(new Vector3(Mathf.Cos((float)(i * 4) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((float)(i * 4) * ((float)Math.PI / 180f))));
			Gizmos.DrawLine(val5, zero);
			val5 = zero;
		}
	}

	public static void Arrow(Vector3 position, Vector3 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = direction * 0.25f;
		Arrow(position, direction, ((Vector3)(ref val)).magnitude, Color.white, 0f, depthTest: false);
	}

	public static void Arrow(Vector3 position, Vector3 direction, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = direction * 0.25f;
		Arrow(position, direction, ((Vector3)(ref val)).magnitude, color, 0f, depthTest: false);
	}

	public static void Arrow(Vector3 position, Vector3 direction, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = direction * 0.25f;
		Arrow(position, direction, ((Vector3)(ref val)).magnitude, color, duration, depthTest: false);
	}

	public static void Arrow(Vector3 position, Vector3 direction, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = direction * 0.25f;
		Arrow(position, direction, ((Vector3)(ref val)).magnitude, color, duration, depthTest);
	}

	public static void Arrow(Vector3 position, Vector3 direction, float headLength)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Arrow(position, direction, headLength, Color.white, 0f, depthTest: false);
	}

	public static void Arrow(Vector3 position, Vector3 direction, float headLength, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Arrow(position, direction, headLength, color, 0f, depthTest: false);
	}

	public static void Arrow(Vector3 position, Vector3 direction, float headLength, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		Arrow(position, direction, headLength, color, duration, depthTest: false);
	}

	public static void Arrow(Vector3 position, Vector3 direction, float headLength, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Debug.DrawRay(position, direction, color, duration, depthTest);
		Cone(position + direction, -direction, headLength, 15f, color, duration, depthTest);
	}

	public static void ArrowGizmo(Vector3 position, Vector3 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = direction * 0.25f;
		ArrowGizmo(position, direction, ((Vector3)(ref val)).magnitude);
	}

	public static void ArrowGizmo(Vector3 position, Vector3 direction, float headLength)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.DrawRay(position, direction);
		ConeGizmo(position + direction, -direction, headLength, 15f);
	}

	public static void Cone(Vector3 position, Vector3 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, ((Vector3)(ref direction)).magnitude, 45f, Color.white, 0f, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, 45f, Color.white, 0f, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, 45f, color, 0f, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, 45f, color, duration, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, Color color, float duration, bool depthTest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, 45f, color, duration, depthTest);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, float angle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, angle, Color.white, 0f, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, float angle, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, angle, color, 0f, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, float angle, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		Cone(position, direction, length, angle, color, duration, depthTest: false);
	}

	public static void Cone(Vector3 position, Vector3 direction, float length, float angle, Color color, float duration, bool depthTest)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		float num = angle / 90f;
		((Vector3)(ref direction)).Normalize();
		Vector3 val = direction * length;
		Vector3 val2 = Vector3.Slerp(val, -val, 0.5f);
		Vector3 val3 = Vector3.Cross(val, val2);
		Vector3 val4 = ((Vector3)(ref val3)).normalized * length;
		val3 = Vector3.Slerp(val, val2, num);
		Vector3 normalized = ((Vector3)(ref val3)).normalized;
		Plane val5 = default(Plane);
		((Plane)(ref val5))._002Ector(-direction, position + val);
		float num2 = default(float);
		((Plane)(ref val5)).Raycast(new Ray(position, normalized), ref num2);
		Debug.DrawRay(position, normalized * num2, color, duration, depthTest);
		val3 = Vector3.Slerp(val, -val2, num);
		Debug.DrawRay(position, ((Vector3)(ref val3)).normalized * num2, color, duration, depthTest);
		val3 = Vector3.Slerp(val, val4, num);
		Debug.DrawRay(position, ((Vector3)(ref val3)).normalized * num2, color, duration, depthTest);
		val3 = Vector3.Slerp(val, -val4, num);
		Debug.DrawRay(position, ((Vector3)(ref val3)).normalized * num2, color, duration, depthTest);
		Vector3 position2 = position + val;
		Vector3 up = direction;
		val3 = val - normalized * num2;
		Circle(position2, up, ((Vector3)(ref val3)).magnitude, color, duration, depthTest);
		Vector3 position3 = position + val * 0.5f;
		Vector3 up2 = direction;
		val3 = val * 0.5f - normalized * (num2 * 0.5f);
		Circle(position3, up2, ((Vector3)(ref val3)).magnitude, color, duration, depthTest);
	}

	public static void ConeGizmo(Vector3 position, Vector3 direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ConeGizmo(position, direction, ((Vector3)(ref direction)).magnitude, 45f);
	}

	public static void ConeGizmo(Vector3 position, Vector3 direction, float length)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ConeGizmo(position, direction, length, 45f);
	}

	public static void ConeGizmo(Vector3 position, Vector3 direction, float length, float angle)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ConeGizmo(position, direction, length, angle, 0f);
	}

	public static void ConeGizmo(Vector3 position, Vector3 direction, float length, float angle, float baseWidth)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		float num = angle / 90f;
		((Vector3)(ref direction)).Normalize();
		Vector3 val = direction * length;
		Vector3 val2 = Vector3.Slerp(val, -val, 0.5f);
		Vector3 val3 = Vector3.Cross(val, val2);
		Vector3 val4 = ((Vector3)(ref val3)).normalized * length;
		val3 = Vector3.Slerp(val, val2, num);
		Vector3 normalized = ((Vector3)(ref val3)).normalized;
		Plane val5 = default(Plane);
		((Plane)(ref val5))._002Ector(-direction, position + val);
		float num2 = default(float);
		((Plane)(ref val5)).Raycast(new Ray(position, normalized), ref num2);
		if (baseWidth > 0f)
		{
			CircleGizmo(position, direction, baseWidth);
			Vector3 normalized2 = ((Vector3)(ref val2)).normalized;
			Vector3 normalized3 = ((Vector3)(ref val4)).normalized;
			Gizmos.DrawLine(position + normalized2 * baseWidth, position + normalized * num2);
			Vector3 val6 = position - normalized2 * baseWidth;
			val3 = Vector3.Slerp(val, -val2, num);
			Gizmos.DrawLine(val6, position + ((Vector3)(ref val3)).normalized * num2);
			Vector3 val7 = position + normalized3 * baseWidth;
			val3 = Vector3.Slerp(val, val4, num);
			Gizmos.DrawLine(val7, position + ((Vector3)(ref val3)).normalized * num2);
			Vector3 val8 = position - normalized3 * baseWidth;
			val3 = Vector3.Slerp(val, -val4, num);
			Gizmos.DrawLine(val8, position + ((Vector3)(ref val3)).normalized * num2);
		}
		else
		{
			Gizmos.DrawRay(position, normalized * num2);
			val3 = Vector3.Slerp(val, -val2, num);
			Gizmos.DrawRay(position, ((Vector3)(ref val3)).normalized * num2);
			val3 = Vector3.Slerp(val, val4, num);
			Gizmos.DrawRay(position, ((Vector3)(ref val3)).normalized * num2);
			val3 = Vector3.Slerp(val, -val4, num);
			Gizmos.DrawRay(position, ((Vector3)(ref val3)).normalized * num2);
		}
		Vector3 position2 = position + val;
		Vector3 up = direction;
		val3 = val - normalized * num2;
		CircleGizmo(position2, up, ((Vector3)(ref val3)).magnitude);
		Vector3 position3 = position + val * 0.5f;
		Vector3 up2 = direction;
		val3 = val * 0.5f - normalized * (num2 * 0.5f);
		CircleGizmo(position3, up2, ((Vector3)(ref val3)).magnitude);
	}

	public static void FPS(bool enabled)
	{
		FPS(enabled, showLowest: false, showAverage: false);
	}

	public static void FPS(bool enabled, bool showLowest, bool showAverage)
	{
		if (enableTextLog)
		{
			FPSLog.ShowFPS(enabled, showLowest, showAverage);
		}
	}

	public static void Log(string content)
	{
		Log(content, 10f, null, rightAligned: false);
	}

	public static void Log(string content, bool rightAligned)
	{
		Log(content, 10f, null, rightAligned);
	}

	public static void Log(string content, string key)
	{
		Log(content, -1f, key, rightAligned: false);
	}

	public static void Log(string content, string key, bool rightAligned)
	{
		Log(content, -1f, key, rightAligned);
	}

	public static void Log(string content, float timeToLive)
	{
		Log(content, timeToLive, null, rightAligned: false);
	}

	public static void Log(string content, float timeToLive, bool rightAligned)
	{
		Log(content, timeToLive, null, rightAligned);
	}

	public static void Log(string content, float timeToLive, string key)
	{
		Log(content, timeToLive, key, rightAligned: false);
	}

	public static void Log(string content, float timeToLive, string key, bool rightAligned)
	{
		if (enableTextLog)
		{
			TextLog.SetItem(content, timeToLive, key, rightAligned);
		}
	}
}
