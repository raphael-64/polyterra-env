using UnityEngine;
using UnityEngine.UI;

public class PopupScroller : UIBasicComponent
{
	public PopupScrollerContent content;

	public Image bg;

	[HideInInspector]
	public bool bgEnabled = true;

	private void OnEnable()
	{
		((Component)bg).gameObject.SetActive(bgEnabled);
	}

	private void OnDisable()
	{
		bgEnabled = true;
		((Component)bg).gameObject.SetActive(bgEnabled);
	}
}
