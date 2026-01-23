using System;
using System.Globalization;
using UnityEngine;

namespace Tesla;

public static class Environment
{
	public class EnvironmentString
	{
		public virtual string Value { get; private set; }

		public EnvironmentString(string name, string defaultValue)
		{
			Value = GetEnvironmentVariable(name, defaultValue);
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class EnvironmentBool
	{
		public virtual bool Value { get; private set; }

		public EnvironmentBool(string name, bool defaultValue)
		{
			Value = GetEnvironmentVariable(name, defaultValue ? 1 : 0) > 0;
		}

		public static implicit operator bool(EnvironmentBool t)
		{
			return t.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class EnvironmentInteger
	{
		public virtual int Value { get; private set; }

		public EnvironmentInteger(string name, int defaultValue)
		{
			Value = GetEnvironmentVariable(name, defaultValue);
		}

		public static implicit operator int(EnvironmentInteger t)
		{
			return t.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class EnvironmentDouble
	{
		public virtual double Value { get; private set; }

		public EnvironmentDouble(string name, double defaultValue)
		{
			Value = GetEnvironmentVariable(name, defaultValue);
		}

		public static implicit operator double(EnvironmentDouble t)
		{
			return t.Value;
		}

		public static implicit operator float(EnvironmentDouble t)
		{
			return (float)t.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class EnvironmentDriverSide
	{
		public virtual DriverSide Value { get; private set; }

		public EnvironmentDriverSide(string name)
		{
			if (GetEnvironmentVariable(name, "Left") == "Right")
			{
				Value = DriverSide.right;
			}
			else
			{
				Value = DriverSide.left;
			}
		}

		public static implicit operator DriverSide(EnvironmentDriverSide t)
		{
			return t.Value;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class EnvironmentColor
	{
		public virtual Color Value { get; private set; }

		private static bool TryParse(string hex, out Color c, bool isHSV)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			c = Color.white;
			if (!hex.StartsWith("0x"))
			{
				return false;
			}
			uint num = uint.Parse(hex.Substring(2), NumberStyles.AllowHexSpecifier);
			uint num2 = num & 0xFF;
			uint num3 = (num >> 8) & 0xFF;
			uint num4 = num >> 16;
			int num5 = (isHSV ? 359 : 255);
			if (num4 > num5)
			{
				return false;
			}
			if (isHSV)
			{
				c = Color.HSVToRGB((float)num4 / (float)num5, (float)num3 / 255f, (float)num2 / 255f, false);
			}
			else
			{
				c = new Color((float)num4 / (float)num5, (float)num3 / 255f, (float)num2 / 255f);
			}
			return true;
		}

		public EnvironmentColor(string name, Color defaultValue, bool isHSV)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			string value = "";
			if (!GetEnvironmentVariable(name, allowEmpty: false, out value) || !TryParse(value, out var c, isHSV))
			{
				Value = defaultValue;
			}
			else
			{
				Value = c;
			}
		}

		public static implicit operator Color(EnvironmentColor t)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return t.Value;
		}

		public override string ToString()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((object)Value/*cast due to .constrained prefix*/).ToString();
		}
	}

	private static EnvironmentBool _developerMode = new EnvironmentBool("TIDK_DEVELOPER_MODE", defaultValue: false);

	public static EnvironmentInteger windowLeft = new EnvironmentInteger("TIDK_WINDOW_LEFT", 0);

	public static EnvironmentInteger windowTop = new EnvironmentInteger("TIDK_WINDOW_TOP", 0);

	public static EnvironmentInteger windowWidth = new EnvironmentInteger("TIDK_WINDOW_WIDTH", 0);

	public static EnvironmentInteger windowHeight = new EnvironmentInteger("TIDK_WINDOW_HEIGHT", 0);

	public static EnvironmentInteger windowRotation = new EnvironmentInteger("TIDK_WINDOW_ROTATION", 0);

	public static EnvironmentString windowName = new EnvironmentString("TIDK_WINDOW_NAME", "Game");

	public static EnvironmentString userDataPath = new EnvironmentString("TIDK_APP_STATE_DIRECTORY", ".");

	public static EnvironmentString vehicleModel = new EnvironmentString("TIDK_VEHICLE_MODEL", "Model3");

	public static EnvironmentColor vehicleColor = new EnvironmentColor("TIDK_VEHICLE_COLOR_RGB", Color.white, isHSV: false);

	public static EnvironmentInteger vehicleColorLighten = new EnvironmentInteger("TIDK_VEHICLE_COLOR_LIGHTEN", 0);

	public static EnvironmentInteger vehicleColorIndex = new EnvironmentInteger("TIDK_VEHICLE_COLOR_INDEX_CODE", 0);

	public static EnvironmentDriverSide driverSide = new EnvironmentDriverSide("TIDK_VEHICLE_DRIVER_SIDE");

	public static EnvironmentString language = new EnvironmentString("TIDK_VEHICLE_LANGUAGE", "English");

	public static EnvironmentBool hasSpoiler = new EnvironmentBool("TIDK_VEHICLE_HAS_SPOILER", defaultValue: false);

	public static EnvironmentString wheelsAsset = new EnvironmentString("TIDK_VEHICLE_WHEELS_ID", "");

	public static EnvironmentInteger displayAreaLeft = new EnvironmentInteger("TIDK_DISPLAY_AREA_LEFT", 0);

	public static EnvironmentInteger displayAreaTop = new EnvironmentInteger("TIDK_DISPLAY_AREA_TOP", 0);

	public static EnvironmentInteger displayAreaWidth = new EnvironmentInteger("TIDK_DISPLAY_AREA_WIDTH", 0);

	public static EnvironmentInteger displayAreaHeight = new EnvironmentInteger("TIDK_DISPLAY_AREA_HEIGHT", 0);

	public static bool developerMode => _developerMode.Value;

	private static bool GetEnvironmentVariable(string name, bool allowEmpty, out string value)
	{
		value = null;
		string environmentVariable = System.Environment.GetEnvironmentVariable(name);
		if (environmentVariable == null)
		{
			return false;
		}
		if (!allowEmpty && StringExtensions.IsNullOrWhiteSpace(environmentVariable))
		{
			return false;
		}
		value = environmentVariable;
		return true;
	}

	public static string GetEnvironmentVariable(string name, string def, bool allowEmpty = false)
	{
		if (!GetEnvironmentVariable(name, allowEmpty, out var value))
		{
			return def;
		}
		Debug.Log((object)(name + " = " + value));
		return value;
	}

	public static int GetEnvironmentVariable(string name, int def)
	{
		if (!GetEnvironmentVariable(name, allowEmpty: false, out var value) || !int.TryParse(value, out var result))
		{
			return def;
		}
		Debug.Log((object)(name + " = " + result));
		return result;
	}

	public static double GetEnvironmentVariable(string name, double def)
	{
		if (!GetEnvironmentVariable(name, allowEmpty: false, out var value) || !double.TryParse(value, out var result))
		{
			return def;
		}
		Debug.Log((object)(name + " = " + result));
		return result;
	}
}
