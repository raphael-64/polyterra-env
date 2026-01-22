using System;
using Tesla;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

internal class TeslaTextFieldComponent : MonoBehaviour, VirtualKeyboardTextEntryHandler
{
	private InputField inputField;

	private bool isRegistered;

	public SubmitEvent onSubmit = new SubmitEvent();

	private void Start()
	{
		inputField = ((Component)this).GetComponent<InputField>();
	}

	public void OnTextEntryStarted(object sender, string text, int caretPosition)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.text = text;
				inputField.caretPosition = caretPosition;
			}
		});
	}

	public void OnTextEntryUpdated(object sender, string text, int caretPosition)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.text = text;
				inputField.caretPosition = caretPosition;
			}
		});
	}

	public void OnTextEntrySubmitted(object sender, string text)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			if ((Object)(object)inputField != (Object)null)
			{
				((UnityEvent<string>)(object)onSubmit)?.Invoke(text);
			}
		});
	}

	public void OnTextEntryCanceled(object sender)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		});
	}

	private void RegisterTeslaInput()
	{
		if (!isRegistered)
		{
			Debug.Log((object)"Showing Tesla virtual keyboard");
			try
			{
				Platform.virtualKeyboard.BeginSimpleTextEntrySession(this, inputField, inputField.text, inputField.caretPosition);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex.Message);
			}
			isRegistered = true;
		}
	}

	private void UnregisterTeslaInput()
	{
		if (isRegistered)
		{
			Debug.Log((object)"Hiding Tesla virtual keyboard");
			try
			{
				Platform.virtualKeyboard.DismissSession(this);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex.Message);
			}
			isRegistered = false;
		}
	}

	private void Update()
	{
		if (inputField.isFocused && !isRegistered)
		{
			RegisterTeslaInput();
		}
		else if (!inputField.isFocused && isRegistered)
		{
			UnregisterTeslaInput();
		}
	}
}
