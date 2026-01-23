using UnityEngine;

public class ArrayElementTitleAttribute : PropertyAttribute
{
	protected string titleFormat = "{0}";

	protected string[] varNames;

	public string TitleFormat => titleFormat;

	public string[] VarNames => varNames;

	public ArrayElementTitleAttribute(params string[] args)
	{
		varNames = args;
	}

	public ArrayElementTitleAttribute(string titleFormat, params string[] args)
	{
		this.titleFormat = titleFormat;
		varNames = args;
	}
}
