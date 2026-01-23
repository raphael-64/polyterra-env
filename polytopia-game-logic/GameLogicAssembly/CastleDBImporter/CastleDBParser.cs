using System.Collections.Generic;
using SimpleJSON;

namespace CastleDBImporter;

public class CastleDBParser
{
	public class RootNode
	{
		private JSONNode value;

		public List<SheetNode> Sheets { get; protected set; }

		public RootNode(JSONNode root)
		{
			value = root;
			Sheets = new List<SheetNode>();
			JSONNode.Enumerator enumerator = value["sheets"].GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JSONNode> current = enumerator.Current;
				Sheets.Add(new SheetNode(current.Value));
			}
		}

		public SheetNode GetSheetWithName(string name)
		{
			foreach (SheetNode sheet in Sheets)
			{
				if (sheet.Name == name)
				{
					return sheet;
				}
			}
			return null;
		}
	}

	public class SheetNode
	{
		private JSONNode value;

		public bool NestedType { get; protected set; }

		public string Name { get; protected set; }

		public List<ColumnNode> Columns { get; protected set; }

		public List<JSONNode> Rows { get; protected set; }

		public SheetNode(JSONNode sheetValue)
		{
			value = sheetValue;
			string text = value["name"];
			char separator = '@';
			string[] array = text.Split(separator);
			if (array.Length <= 1)
			{
				Name = value["name"];
				NestedType = false;
			}
			else
			{
				Name = array[^1];
				NestedType = true;
			}
			Columns = new List<ColumnNode>();
			Rows = new List<JSONNode>();
			JSONNode.Enumerator enumerator = value["columns"].GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JSONNode> current = enumerator.Current;
				Columns.Add(new ColumnNode(current.Value));
			}
			enumerator = value["lines"].GetEnumerator();
			while (enumerator.MoveNext())
			{
				KeyValuePair<string, JSONNode> current2 = enumerator.Current;
				Rows.Add(current2.Value);
			}
		}

		public int ConvertIdxToIndex(int idx)
		{
			for (int i = 0; i < Rows.Count; i++)
			{
				if (Rows[i]["idx"] == idx)
				{
					return i;
				}
			}
			return -1;
		}
	}

	public class ColumnNode
	{
		private JSONNode value;

		public string TypeStr { get; protected set; }

		public string Name { get; protected set; }

		public string Display { get; protected set; }

		public ColumnNode(JSONNode sheetValue)
		{
			value = sheetValue;
			Name = value["name"];
			Display = value["display"];
			TypeStr = value["typeStr"];
		}
	}

	private string DBContent;

	public RootNode Root { get; private set; }

	public CastleDBParser(string db)
	{
		DBContent = db;
		Root = new RootNode(JSON.Parse(DBContent));
	}

	public void RegenerateDB()
	{
		Root = new RootNode(JSON.Parse(DBContent));
	}
}
