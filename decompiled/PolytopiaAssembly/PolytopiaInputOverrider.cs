using UnityEngine;
using UnityEngine.EventSystems;

public class PolytopiaInputOverrider : MonoBehaviour
{
	[SerializeField]
	private PolytopiaUIInput polytopiaInput;

	private bool isDone;

	public void Update()
	{
		if (!isDone)
		{
			EventSystem current = EventSystem.current;
			if ((Object)(object)((current != null) ? current.currentInputModule : null) != (Object)null)
			{
				EventSystem.current.currentInputModule.inputOverride = (BaseInput)(object)polytopiaInput;
				isDone = true;
			}
		}
	}
}
