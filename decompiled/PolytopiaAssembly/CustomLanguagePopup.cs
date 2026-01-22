using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Polytopia.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomLanguagePopup : BasicPopup
{
	[Header("Custom Language Popup")]
	[SerializeField]
	protected TMP_InputField inputfield;

	[HideInInspector]
	public Action<bool> customFileAddedCallback;

	[HideInInspector]
	public Action closeCallback;

	protected bool isLoadingFile;

	public override void Init()
	{
		base.Init();
		((UnityEvent<string>)(object)inputfield.onSubmit).AddListener((UnityAction<string>)OnInputDone);
	}

	public override void Show()
	{
		buttonData = new PopupButtonData[2]
		{
			new PopupButtonData("buttons.back", PopupButtonData.States.None, OnClose),
			new PopupButtonData("settings.language.load.button", PopupButtonData.States.Disabled, OnLoad, -1, closesPopup: false)
		};
		Header = Localization.Get("settings.language.load.title");
		Description = Localization.Get("settings.language.load.info");
		base.Show();
		UINavigationManager.Select((Selectable)(object)inputfield);
		CurrentSelectable = (Selectable)(object)inputfield;
	}

	protected void OnClose(int id, BaseEventData eventData)
	{
		closeCallback?.Invoke();
	}

	public void OnOpenInfo()
	{
		NativeHelpers.OpenURL("https://www.reddit.com/r/Polytopia/comments/em8y7u/make_your_own_polytopia_translation/");
	}

	public override void ResetPopup()
	{
		base.ResetPopup();
		inputfield.text = string.Empty;
	}

	public void OnInputChanged(string value)
	{
		bool buttonEnabled = Buttons[1].ButtonEnabled;
		Buttons[1].ButtonEnabled = !string.IsNullOrEmpty(inputfield.text);
		if (buttonEnabled != Buttons[1].ButtonEnabled)
		{
			Transform transform = ((Component)this).transform;
			UIUtils.SetExplicitNavigation((RectTransform)(object)((transform is RectTransform) ? transform : null));
		}
	}

	public void OnInputDone(string value)
	{
		if (!isLoadingFile)
		{
			((MonoBehaviour)this).StartCoroutine(LoadFile());
		}
	}

	private void OnLoad(int id, BaseEventData eventData)
	{
		if (!isLoadingFile)
		{
			((MonoBehaviour)this).StartCoroutine(LoadFile());
		}
	}

	private IEnumerator LoadFile()
	{
		if (string.IsNullOrEmpty(inputfield.text))
		{
			yield break;
		}
		isLoadingFile = true;
		string text = inputfield.text;
		bool success = false;
		string localizationCacheDirectoryPath = Paths.GetLocalizationCacheDirectoryPath();
		string path = "customLanguage.json";
		string filePath = Path.Combine(localizationCacheDirectoryPath, path);
		PolytopiaPlayerPrefs.SetString("customlanguageurl", text);
		PolytopiaPlayerPrefs.Save();
		try
		{
			if (!PolytopiaDirectory.Exists(localizationCacheDirectoryPath))
			{
				PolytopiaDirectory.CreateDirectory(localizationCacheDirectoryPath);
			}
		}
		catch (IOException ex)
		{
			isLoadingFile = false;
			Console.WriteLine(ex.Message);
		}
		UnityWebRequest www = UnityWebRequest.Get(text);
		yield return www.SendWebRequest();
		if ((int)www.result != 1)
		{
			Debug.Log((object)www.error);
			isLoadingFile = false;
		}
		else if (www.downloadHandler.text.IsValidJson<Dictionary<string, string>>())
		{
			byte[] data = www.downloadHandler.data;
			PolytopiaFile.WriteAllBytes(filePath, data);
			Log.Verbose("Saved file to: " + filePath, Array.Empty<object>());
			success = true;
		}
		else
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("settings.language.load.error.title");
			basicPopup.Description = Localization.Get("settings.language.load.error.info");
			basicPopup.buttonData = new PopupButtonData[1]
			{
				new PopupButtonData("buttons.ok", PopupButtonData.States.Selected)
			};
			basicPopup.Show();
			isLoadingFile = false;
		}
		customFileAddedCallback?.Invoke(success);
		if (success)
		{
			OnHide(-1, null);
		}
		isLoadingFile = false;
	}
}
