using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UINavigationManager : MonoBehaviour
{
	public delegate void OnUINavigationTypeChangedEvent(NavigationType navType);

	public enum NavigationType
	{
		None,
		Buttons,
		Mouse
	}

	protected Vector2 lastMousePos;

	public static NavigationType navigationType { get; protected set; }

	public static event OnUINavigationTypeChangedEvent OnUINavigationTypeChanged;

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		lastMousePos = PolytopiaInput.mousePosition;
		ChangeInputMode(NavigationType.Mouse);
	}

	private void Update()
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		if (SystemManager.ShouldUseTouchInterface())
		{
			return;
		}
		switch (navigationType)
		{
		case NavigationType.Mouse:
		{
			if (ShouldDeselectCurrentSelectedGameObject())
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
			}
			bool num = Mathf.Abs(InputManager.GetAxis("Horizontal")) > 0.01f;
			bool flag = Mathf.Abs(InputManager.GetAxis("Vertical")) > 0.01f;
			Selectable defaultSelection;
			if (num || (flag && !PolytopiaInput.GetMouseButton(0)))
			{
				ChangeInputMode(NavigationType.Buttons);
			}
			else if (InputManager.GetButtonUp("Accept", isUINavigation: true) && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null && GetDefaultSelectable(out defaultSelection))
			{
				ISubmitHandler[] components2 = ((Component)defaultSelection).GetComponents<ISubmitHandler>();
				ISubmitHandler[] array = components2;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].OnSubmit((BaseEventData)null);
				}
				if (components2.Length != 0)
				{
					InputManager.EatButton("Accept");
				}
			}
			break;
		}
		case NavigationType.Buttons:
		{
			if (PolytopiaInput.mousePosition != lastMousePos || PolytopiaInput.GetMouseButton(0))
			{
				ChangeInputMode(NavigationType.Mouse);
				break;
			}
			if ((double)Mathf.Abs(InputManager.GetAxisRaw("Horizontal")) > 0.001 || (double)Mathf.Abs(InputManager.GetAxisRaw("Vertical")) > 0.001)
			{
				GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
				if (((Object)(object)currentSelectedGameObject == (Object)null || !IsValidSelectable(currentSelectedGameObject.GetComponent<Selectable>())) && GetSuggestedSelectable(out var suggestedSelection))
				{
					Select(suggestedSelection);
				}
			}
			if (!InputManager.GetButtonUp("Accept", isUINavigation: true))
			{
				break;
			}
			GameObject currentSelectedGameObject2 = EventSystem.current.currentSelectedGameObject;
			TMP_InputField val = (((Object)(object)currentSelectedGameObject2 != (Object)null) ? currentSelectedGameObject2.GetComponent<TMP_InputField>() : null);
			if ((Object)(object)val != (Object)null)
			{
				InputManager.EatButton("Accept");
				((UnityEvent<string>)(object)val.onSubmit)?.Invoke((string)null);
			}
			else if ((Object)(object)currentSelectedGameObject2 != (Object)null)
			{
				ISubmitHandler[] components = currentSelectedGameObject2.GetComponents<ISubmitHandler>();
				ISubmitHandler[] array = components;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].OnSubmit((BaseEventData)null);
				}
				if (components.Length != 0)
				{
					InputManager.EatButton("Accept");
				}
			}
			break;
		}
		}
		lastMousePos = PolytopiaInput.mousePosition;
	}

	private static bool ShouldDeselectCurrentSelectedGameObject()
	{
		EventSystem current = EventSystem.current;
		if ((Object)(object)((current != null) ? current.currentSelectedGameObject : null) != (Object)null)
		{
			return (Object)(object)EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != (Object)null;
		}
		return false;
	}

	public static void ChangeInputMode(NavigationType navigationType)
	{
		UINavigationManager.navigationType = navigationType;
		switch (navigationType)
		{
		case NavigationType.Mouse:
			UINavigationManager.OnUINavigationTypeChanged?.Invoke(navigationType);
			if (ShouldDeselectCurrentSelectedGameObject())
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
			}
			break;
		case NavigationType.Buttons:
			if (Object.op_Implicit((Object)(object)EventSystem.current))
			{
				if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null && GetSuggestedSelectable(out var suggestedSelection))
				{
					EventSystem.current.SetSelectedGameObject(((Component)suggestedSelection).gameObject);
				}
				UINavigationManager.OnUINavigationTypeChanged?.Invoke(navigationType);
			}
			break;
		}
		if (!SystemManager.IsMobile)
		{
			Cursor.visible = navigationType == NavigationType.Mouse;
		}
	}

	public static bool GetDefaultSelectable(out Selectable defaultSelection)
	{
		defaultSelection = null;
		if (UIBlackFader.IsShowing())
		{
			defaultSelection = UIBlackFader.instance.DefaultSelectable;
		}
		else if (PopupManager.PopupShowing)
		{
			defaultSelection = PopupManager.GetCurrentPopup().DefaultSelectable;
		}
		else if (ResultScreen.IsShowing())
		{
			defaultSelection = ResultScreen.DefaultSelectable;
		}
		else if ((Object)(object)UIManager.Instance.GetCurrentScreen() != (Object)null)
		{
			defaultSelection = UIManager.Instance.GetCurrentScreen().DefaultSelectable;
		}
		return (Object)(object)defaultSelection != (Object)null;
	}

	public static bool GetSuggestedSelectable(out Selectable suggestedSelection)
	{
		suggestedSelection = null;
		if (UIBlackFader.IsShowing())
		{
			suggestedSelection = UIBlackFader.instance.GetCurrentSelectableOrFallback();
		}
		else if (PopupManager.PopupShowing)
		{
			suggestedSelection = PopupManager.GetCurrentPopup().GetCurrentSelectableOrFallback();
		}
		else if ((Object)(object)UIManager.Instance.GetCurrentScreen() != (Object)null)
		{
			suggestedSelection = UIManager.Instance.GetCurrentScreen().GetCurrentSelectableOrFallback();
		}
		return (Object)(object)suggestedSelection != (Object)null;
	}

	private static void SendDeselectMessages(Selectable selectable)
	{
		IDeselectHandler[] components = ((Component)selectable).GetComponents<IDeselectHandler>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].OnDeselect((BaseEventData)null);
		}
	}

	public static void Select(Selectable selectable)
	{
		if (!((Object)(object)EventSystem.current == (Object)null) && (!((Object)(object)selectable != (Object)null) || !((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)selectable).gameObject)) && (!((Object)(object)selectable == (Object)null) || !((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null)))
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			if ((Object)(object)selectable != (Object)null)
			{
				selectable.Select();
			}
		}
	}

	public static bool IsValidSelectable(Selectable selectable)
	{
		if ((Object)(object)selectable != (Object)null && ((Behaviour)selectable).enabled)
		{
			return ((Component)selectable).gameObject.activeInHierarchy;
		}
		return false;
	}

	public static Selectable FindFirstNavigableSelectable(Transform transform)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Selectable[] componentsInChildren = ((Component)transform).GetComponentsInChildren<Selectable>();
		foreach (Selectable val in componentsInChildren)
		{
			if (((Behaviour)val).enabled)
			{
				Navigation navigation = val.navigation;
				if ((int)((Navigation)(ref navigation)).mode != 0)
				{
					return val;
				}
			}
		}
		return null;
	}
}
