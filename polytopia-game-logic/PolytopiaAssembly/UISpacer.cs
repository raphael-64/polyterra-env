using UnityEngine;
using UnityEngine.UI;

public class UISpacer : UIBasicComponent
{
	[SerializeField]
	private float width = 10f;

	[SerializeField]
	private float height = 10f;

	[SerializeField]
	protected LayoutElement layoutElement;

	protected bool widthModified;

	protected bool heightModified;

	public float Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
			layoutElement.minWidth = width;
			base.rectTransform.SetWidth(width);
			widthModified = true;
		}
	}

	public float Height
	{
		get
		{
			return height;
		}
		set
		{
			height = value;
			layoutElement.minHeight = height;
			base.rectTransform.SetHeight(height);
			heightModified = true;
		}
	}

	private void Start()
	{
		if (!widthModified)
		{
			Width = width;
		}
		if (!heightModified)
		{
			Height = height;
		}
	}
}
