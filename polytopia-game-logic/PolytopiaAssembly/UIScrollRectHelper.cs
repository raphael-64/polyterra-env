using UnityEngine;
using UnityEngine.UI;

public class UIScrollRectHelper : MonoBehaviour
{
	private void Awake()
	{
		ScrollRect val = default(ScrollRect);
		if (SystemManager.ShouldUseTouchInterface() && ((Component)this).TryGetComponent<ScrollRect>(ref val))
		{
			if ((Object)(object)val.verticalScrollbar != (Object)null)
			{
				((Component)val.verticalScrollbar).gameObject.SetActive(false);
				val.verticalScrollbar = null;
			}
			if ((Object)(object)val.horizontalScrollbar != (Object)null)
			{
				((Component)val.horizontalScrollbar).gameObject.SetActive(false);
				val.horizontalScrollbar = null;
			}
		}
	}
}
