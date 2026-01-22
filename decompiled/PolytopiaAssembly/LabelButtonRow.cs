using TMPro;
using UnityEngine.UI;

public class LabelButtonRow : UIBasicComponent, IListCellNavigation
{
	public TextMeshProUGUI header;

	public UILabelButton button;

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)button.button;
	}

	public Selectable GetAccessorySelectable()
	{
		return (Selectable)(object)button.button;
	}
}
