using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIValueBoxWidget : UIBasicComponent
{
	[SerializeField]
	protected Image background;

	[SerializeField]
	protected TextMeshProUGUI valueField;

	[SerializeField]
	protected TextMeshProUGUI labelField;

	public void SetValue(string value)
	{
		((TMP_Text)valueField).text = value;
	}

	public string GetValue()
	{
		return ((TMP_Text)valueField).text;
	}

	public void SetLabel(string label)
	{
		((TMP_Text)labelField).text = label;
	}

	public string GetLabel()
	{
		return ((TMP_Text)labelField).text;
	}

	public void SetValueColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)valueField).color = color;
	}

	public Color GetValueColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Graphic)valueField).color;
	}

	public void SetLabelColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)labelField).color = color;
	}

	public Color GetLabelColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Graphic)labelField).color;
	}

	public void SetBackgroundColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)background).color = color;
	}

	public Color GetBackgroundColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Graphic)background).color;
	}
}
