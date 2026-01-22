using System;
using UnityEngine;

public class ScreenKeyboardManager : MonoBehaviour
{
	private int lastCachedHeight;

	public event Action<float> OnTouchScreenHeightChanged;

	protected void Awake()
	{
		lastCachedHeight = 0;
	}

	private void Update()
	{
		int keyboardSize = NativeHelpers.GetKeyboardSize();
		if (lastCachedHeight != keyboardSize && SystemManager.IsTouchScreenKeyboardSupported())
		{
			lastCachedHeight = keyboardSize;
			this.OnTouchScreenHeightChanged?.Invoke(keyboardSize);
		}
	}
}
