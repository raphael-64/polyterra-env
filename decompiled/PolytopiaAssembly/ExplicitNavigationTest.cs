using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExplicitNavigationTest : UIBasicComponent
{
	[SerializeField]
	protected Selectable startSelection;

	[SerializeField]
	protected UITextButton bottomLeftButton;

	[SerializeField]
	protected UITextButton bottomRightButton;

	[SerializeField]
	protected ButtonRow buttonRow;

	private void Awake()
	{
		((MonoBehaviour)this).StartCoroutine(DelayNavigationFix());
	}

	private IEnumerator DelayNavigationFix()
	{
		yield return (object)new WaitForEndOfFrame();
		UIUtils.SetExplicitNavigation(base.rectTransform);
		UIUtils.SetSelectOnDown((Selectable)(object)bottomLeftButton.button, (Selectable)(object)buttonRow.buttonComp.button);
		UIUtils.SetSelectOnDown((Selectable)(object)bottomRightButton.button, (Selectable)(object)buttonRow.buttonComp.button);
		UIUtils.SetSelectOnUp((Selectable)(object)buttonRow.buttonComp.button, (Selectable)(object)bottomRightButton.button);
		UINavigationManager.Select(startSelection);
	}

	public void OnBack()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
