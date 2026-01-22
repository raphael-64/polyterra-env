using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSliderContainer : UIBasicComponent
{
	[SerializeField]
	protected TMPLocalizer header;

	[SerializeField]
	protected Slider slider;

	[HideInInspector]
	public Action<float> SliderValueChangedCallback;

	public string HeaderKey
	{
		get
		{
			return header.Key;
		}
		set
		{
			header.Key = value;
		}
	}

	public string[] Arguments
	{
		get
		{
			return header.Arguments;
		}
		set
		{
			header.Arguments = value;
		}
	}

	public float SliderValue
	{
		get
		{
			return slider.value;
		}
		set
		{
			slider.value = value;
		}
	}

	public bool Highlighted
	{
		set
		{
			if (value)
			{
				UINavigationManager.Select((Selectable)(object)slider);
			}
		}
	}

	public void OnSliderValueChanged(float value)
	{
		SliderValueChangedCallback?.Invoke(value);
	}
}
