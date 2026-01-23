using UnityEngine.UI;

public interface IListCellNavigation
{
	Selectable GetMainSelectable();

	Selectable GetAccessorySelectable();
}
