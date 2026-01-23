using UnityEngine.UI;

public class ButtonRow : UIBasicComponent, IListCellNavigation
{
	public UITextButton buttonComp;

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)buttonComp.button;
	}

	public Selectable GetAccessorySelectable()
	{
		return (Selectable)(object)buttonComp.button;
	}
}
