using System;
using System.Collections.Generic;
using System.Globalization;

public class LocalizationUtils
{
	public static string GetPrettyList(List<string> values)
	{
		return GetPrettyList(values.ToArray());
	}

	public static string GetPrettyList(string[] values)
	{
		string text = string.Empty;
		int num = values.Length;
		for (int i = 0; i < num; i++)
		{
			text += values[i];
			if (i < values.Length - 1)
			{
				text = ((i != values.Length - 2) ? (text + ", ") : (text + string.Format(" {0} ", Localization.Get("stringtools.typelist.and"))));
			}
		}
		return text;
	}

	public static string GetTimeString(DateTime time)
	{
		return GetTimeString(DateTime.UtcNow.Subtract(time));
	}

	public static string GetTimeStringHours(TimeSpan time)
	{
		int num = ((time.Minutes > 30) ? ((int)time.TotalHours + 1) : ((int)time.TotalHours));
		if (num == 1)
		{
			return string.Format("{0} {1}", num, Localization.Get("date.hour"));
		}
		return string.Format("{0} {1}", num, Localization.Get("date.hours"));
	}

	public static string GetTimeString(TimeSpan time, out string suffix)
	{
		if (time.TotalDays > 1.0)
		{
			suffix = ((Math.Floor(time.TotalDays) > 1.0) ? Localization.Get("date.days") : Localization.Get("date.day"));
			return time.Days.ToString();
		}
		if (time.TotalHours > 1.0)
		{
			double num = Math.Floor(time.TotalHours);
			suffix = ((num > 1.0) ? Localization.Get("date.hours") : Localization.Get("date.hour"));
			return num.ToString();
		}
		if (time.TotalMinutes > 1.0)
		{
			double num2 = Math.Floor(time.TotalMinutes);
			suffix = ((num2 > 1.0) ? Localization.Get("date.minutes") : Localization.Get("date.minute"));
			return num2.ToString();
		}
		if (time.Seconds > 0)
		{
			suffix = ((time.Seconds > 1) ? Localization.Get("date.seconds") : Localization.Get("date.second"));
			return time.Seconds.ToString();
		}
		suffix = Localization.Get("date.seconds");
		return "0";
	}

	public static string GetTimeString(TimeSpan time)
	{
		string suffix;
		return $"{GetTimeString(time, out suffix)} {suffix}";
	}

	public static string GetDateString(DateTime utcDate)
	{
		DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
		CultureInfo cultureInfo = Localization.GetCultureInfo();
		TimeSpan timeSpan = utcDate - DateTime.UtcNow;
		DateTime dateTime = utcDate.ToLocalTime();
		if (utcDate > DateTime.UtcNow)
		{
			if (utcDate.Date == DateTime.UtcNow.Date)
			{
				return string.Format("{0} - {1}", CapitalizeString(Localization.Get("date.today")), dateTime.ToString("t", cultureInfo));
			}
			if (utcDate.Date == DateTime.UtcNow.Date.AddDays(1.0))
			{
				return string.Format("{0} - {1}", CapitalizeString(Localization.Get("date.tomorrow")), dateTime.ToString("t", cultureInfo));
			}
			if (timeSpan.TotalDays < 7.0)
			{
				return string.Format("{0} - {1}", utcDate.ToString("dddd", cultureInfo), dateTime.ToString("t", cultureInfo));
			}
		}
		return string.Format("{0} - {1}", utcDate.ToString("M", cultureInfo), dateTime.ToString("t", cultureInfo));
	}

	public static string GetCountdownString(DateTime utcDate)
	{
		DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);
		TimeSpan time = utcDate - DateTime.UtcNow;
		return Localization.Get("onlineview.tournament.info.time.left", GetTimeString(time));
	}

	public static string CapitalizeAndEndSentence(string sentence)
	{
		return $"{CapitalizeString(sentence)}.";
	}

	public static string CapitalizeString(string value)
	{
		if (value.Length == 0)
		{
			return value;
		}
		return char.ToUpper(value[0]) + value.Substring(1);
	}

	public static string FormatNumber(int value)
	{
		return value.ToString("N0", CultureInfo.InvariantCulture);
	}

	public static string FormatNumber(uint value)
	{
		return value.ToString("N0", CultureInfo.InvariantCulture);
	}

	public static string FormatNumber(float value)
	{
		return value.ToString("N0", CultureInfo.InvariantCulture);
	}

	public static string AddOrdinal(int value)
	{
		if (value <= 0)
		{
			return value.ToString();
		}
		int num = value % 100;
		if ((uint)(num - 11) <= 2u)
		{
			return Localization.Get("onlineview.placement.th", value);
		}
		return (value % 10) switch
		{
			1 => Localization.Get("onlineview.placement.st", value), 
			2 => Localization.Get("onlineview.placement.nd", value), 
			3 => Localization.Get("onlineview.placement.rd", value), 
			_ => Localization.Get("onlineview.placement.th", value), 
		};
	}
}
