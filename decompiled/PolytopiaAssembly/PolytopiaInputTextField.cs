using System;
using TMPro;
using Tesla;
using UnityEngine;
using UnityEngine.EventSystems;

internal class PolytopiaInputTextField : MonoBehaviour, VirtualKeyboardTextEntryHandler
{
	public TMP_InputField inputField;

	private bool isRegistered;

	private void Start()
	{
		((Behaviour)this).enabled = false;
	}

	private void OnDisable()
	{
		UnregisterTeslaInput();
	}

	public void OnTextEntryStarted(object sender, string text, int caretPosition)
	{
		Log.Verbose("OnTextEntryStarted {0}, {1} {2}", new object[3] { sender, text, caretPosition });
	}

	public void OnTextEntryUpdated(object sender, string text, int caretPosition)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			Log.Verbose("OnTextEntryUpdated {0}, {1} {2}", new object[3] { sender, text, caretPosition });
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.text = text;
				inputField.caretPosition = caretPosition;
				inputField.ForceLabelUpdate();
			}
		});
	}

	public void OnTextEntrySubmitted(object sender, string text)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Expected O, but got Unknown
			Log.Verbose("OnTextEntrySubmitted {0}, {1}", new object[2] { sender, text });
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.OnSubmit(new BaseEventData(EventSystem.current));
				inputField.DeactivateInputField(true);
			}
		});
	}

	public void OnTextEntryCanceled(object sender)
	{
		TeslaDispatchComponent.Enqueue(delegate
		{
			Log.Verbose("OnTextEntryCanceled {0}", new object[1] { sender });
			if ((Object)(object)inputField != (Object)null)
			{
				inputField.DeactivateInputField(true);
			}
		});
	}

	private void RegisterTeslaInput()
	{
		if (!isRegistered)
		{
			Log.Verbose("RegisterTeslaInput Showing Tesla virtual keyboard {0} {1}", new object[2]
			{
				((object)this).GetHashCode(),
				Time.frameCount
			});
			try
			{
				inputField.caretPosition = inputField.text.Length;
				inputField.ForceLabelUpdate();
				Platform.virtualKeyboard.BeginSimpleTextEntrySession(this, inputField, inputField.text, inputField.caretPosition);
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, Array.Empty<object>());
			}
			isRegistered = true;
		}
	}

	private void UnregisterTeslaInput()
	{
		if (isRegistered)
		{
			Log.Verbose("UnregisterTeslaInput Hiding Tesla virtual keyboard {0} {1}", new object[2]
			{
				((object)this).GetHashCode(),
				Time.frameCount
			});
			try
			{
				Platform.virtualKeyboard.DismissSession(this);
			}
			catch (Exception ex)
			{
				Log.Error(ex.Message, Array.Empty<object>());
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
		inputField.selectionAnchorPosition = inputField.caretPosition;
		inputField.selectionFocusPosition = inputField.caretPosition;
	}
}
