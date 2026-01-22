using UnityEngine.UI;

public interface ISelectableContainer
{
	Selectable DefaultSelectable { get; set; }

	Selectable CurrentSelectable { get; set; }

	Selectable GetDefaultSelectableOrFallback();

	Selectable GetCurrentSelectableOrFallback();
}
