using IchiGamepad;
using UnityEngine;
using UnityEngine.Serialization;

public class ButtonGlyph : MonoBehaviour
{
	[FormerlySerializedAs("enabled")]
	[SerializeField]
	private bool isEnabled;

	private void UpdateButtonGlyph()
	{
		if (Backend.IsInitialized)
		{
			((Component)this).gameObject.SetActive(isEnabled && !Backend.GetInstance().IsKeyboard);
		}
	}

	private void Awake()
	{
		if (Backend.IsInitialized)
		{
			Backend.GetInstance().OnControllerMappingChanged += UpdateButtonGlyph;
		}
		UpdateButtonGlyph();
	}

	private void OnDestroy()
	{
		if (Backend.IsInitialized)
		{
			Backend.GetInstance().OnControllerMappingChanged -= UpdateButtonGlyph;
		}
	}
}
