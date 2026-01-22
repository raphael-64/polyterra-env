using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSVReader
{
	private static string SPLIT_RE = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";

	private static string LINE_SPLIT_RE = "\\r\\n|\\n\\r|\\n|\\r";

	private static char[] TRIM_CHARS = new char[1] { '"' };

	private static void SetCell(List<List<string>> table, int row, int column, string value)
	{
		for (int i = 0; i <= row - table.Count; i++)
		{
			table.Add(new List<string>());
		}
		List<string> list = table[row];
		for (int j = 0; j <= column - list.Count; j++)
		{
			list.Add("");
		}
		list[column] = value.Replace("\"\"", "\"");
	}

	private static bool HandleDelimiter(List<List<string>> list, string data, ref int rows, ref int columns, ref int i, ref int cellStart, int cellEnd)
	{
		if (data[i] == ',')
		{
			SetCell(list, rows, columns++, data.Substring(cellStart, cellEnd - cellStart));
			cellStart = -1;
			if (i == data.Length - 1)
			{
				SetCell(list, rows, columns++, "");
			}
			return true;
		}
		if (data[i] == '\n')
		{
			if (i < data.Length - 1 && data[i + 1] == '\r')
			{
				i++;
			}
			SetCell(list, rows++, columns++, data.Substring(cellStart, cellEnd - cellStart));
			cellStart = -1;
			columns = 0;
			return true;
		}
		if (data[i] == '\r')
		{
			if (i < data.Length - 1 && data[i + 1] == '\n')
			{
				i++;
			}
			SetCell(list, rows++, columns++, data.Substring(cellStart, cellEnd - cellStart));
			cellStart = -1;
			columns = 0;
			return true;
		}
		if (i == data.Length - 1)
		{
			SetCell(list, rows, columns++, data.Substring(cellStart));
			cellStart = -1;
			return true;
		}
		return false;
	}

	public static List<Dictionary<string, object>> Read(string data)
	{
		List<List<string>> list = new List<List<string>>();
		int rows = 0;
		int columns = 0;
		int num = -1;
		int cellStart = -1;
		for (int i = 0; i < data.Length; i++)
		{
			if (cellStart == -1)
			{
				if (data[i] == ' ')
				{
					continue;
				}
				if (data[i] == '"')
				{
					cellStart = i + 1;
					num = i;
					continue;
				}
				int cellStart2 = i;
				if (!HandleDelimiter(list, data, ref rows, ref columns, ref i, ref cellStart2, i))
				{
					cellStart = i;
				}
			}
			else if (num > -1)
			{
				if (data[i] != '"')
				{
					continue;
				}
				if (i < data.Length - 1 && data[i + 1] == '"')
				{
					i++;
					continue;
				}
				int cellEnd = i++;
				for (; i < data.Length; i++)
				{
					if (data[i] != ' ')
					{
						if (HandleDelimiter(list, data, ref rows, ref columns, ref i, ref cellStart, cellEnd))
						{
							break;
						}
						throw new Exception($"String continues after quotation end row {rows} column {columns} index {i}");
					}
				}
				num = -1;
			}
			else
			{
				HandleDelimiter(list, data, ref rows, ref columns, ref i, ref cellStart, i);
			}
		}
		if (list.Count <= 1)
		{
			return new List<Dictionary<string, object>>();
		}
		List<string> list2 = list[0];
		List<Dictionary<string, object>> list3 = new List<Dictionary<string, object>>();
		for (int j = 1; j < list.Count; j++)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			for (int k = 0; k < list2.Count; k++)
			{
				dictionary[list2[k]] = list[j][k];
			}
			list3.Add(dictionary);
		}
		return list3;
	}

	public static List<Dictionary<string, object>> ReadOriginal(string data)
	{
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		string[] array = Regex.Split(data, LINE_SPLIT_RE);
		if (array.Length <= 1)
		{
			return list;
		}
		string[] array2 = Regex.Split(array[0], SPLIT_RE);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = array2[i].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
		}
		for (int j = 1; j < array.Length; j++)
		{
			string[] array3 = Regex.Split(array[j], SPLIT_RE);
			if (array3.Length == 0 || array3[0] == "")
			{
				continue;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			for (int k = 0; k < array2.Length && k < array3.Length; k++)
			{
				string text = array3[k];
				text = text.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
				object value = text;
				float result2;
				if (int.TryParse(text, out var result))
				{
					value = result;
				}
				else if (float.TryParse(text, out result2))
				{
					value = result2;
				}
				dictionary[array2[k]] = value;
			}
			list.Add(dictionary);
		}
		return list;
	}
}
