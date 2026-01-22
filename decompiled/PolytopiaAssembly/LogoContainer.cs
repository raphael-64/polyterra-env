using UnityEngine;
using UnityEngine.UI;

public class LogoContainer : UIBasicComponent
{
	[SerializeField]
	protected Image bigLogo;

	[SerializeField]
	protected Image smallLogo;

	private void Awake()
	{
		Refresh();
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		Refresh();
	}

	protected void Refresh()
	{
		bool flag = true;
		if (((Graphic)bigLogo).rectTransform.GetHeight() + 20f > UIManager.GetUIHeight() * 0.3333f)
		{
			flag = false;
		}
		((Component)bigLogo).gameObject.SetActive(flag);
		((Component)smallLogo).gameObject.SetActive(!flag);
	}
}
