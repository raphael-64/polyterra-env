using System;
using UnityEngine;

public static class InputEvents
{
	public delegate void OnTileSelectedEvent(Tile tile);

	public delegate void OnTileMarkedEvent(Tile tile);

	public delegate void OnBuildingSelectedEvent(Building building);

	public delegate void OnUnitSelectedEvent(Unit unit);

	public delegate void OnSelectionClearedEvent();

	public delegate void OnEnabledInputChangedEvent(InputManager.InputType enabledInput);

	public delegate void OnButtonEvent(InputManager.Buttons button);

	public delegate void OnSelectNextUnitEvent(byte playerId);

	public delegate void OnOmnicursorSnapToTileEvent(Tile tile);

	public delegate void OnOmnicursorSnapToUIElementEvent(RectTransform transform);

	public delegate void OnOmnicursorStartMovingEvent();

	public static event OnTileSelectedEvent OnTileSelected;

	public static event OnTileSelectedEvent OnTileMarked;

	public static event Action OnTileMarkCleared;

	public static event OnBuildingSelectedEvent OnBuildingSelected;

	public static event OnUnitSelectedEvent OnUnitSelected;

	public static event OnSelectionClearedEvent OnSelectionCleared;

	public static event OnEnabledInputChangedEvent OnEnabledInputChanged;

	public static event OnButtonEvent OnButtonDown;

	public static event OnButtonEvent OnButtonUp;

	public static event OnSelectNextUnitEvent OnSelectNextUnit;

	public static event OnOmnicursorSnapToTileEvent OnOmnicursorSnapToTile;

	public static event OnOmnicursorSnapToUIElementEvent OnOmnicursorSnapToUIElement;

	public static event OnOmnicursorStartMovingEvent OnOmnicursorStartMoving;

	public static void TileSelected(Tile tile)
	{
		InputEvents.OnTileSelected?.Invoke(tile);
	}

	public static void TileMarked(Tile tile)
	{
		InputEvents.OnTileMarked?.Invoke(tile);
	}

	public static void TileMarkCleared()
	{
		InputEvents.OnTileMarkCleared?.Invoke();
	}

	public static void BuildingSelected(Building building)
	{
		InputEvents.OnBuildingSelected?.Invoke(building);
	}

	public static void UnitSelected(Unit unit)
	{
		InputEvents.OnUnitSelected?.Invoke(unit);
	}

	public static void SelectionCleared()
	{
		InputEvents.OnSelectionCleared?.Invoke();
	}

	public static void EnabledInputChanged(InputManager.InputType enabledInput)
	{
		InputEvents.OnEnabledInputChanged?.Invoke(enabledInput);
	}

	public static void ButtonDown(InputManager.Buttons button)
	{
		InputEvents.OnButtonDown?.Invoke(button);
	}

	public static void ButtonUp(InputManager.Buttons button)
	{
		InputEvents.OnButtonUp?.Invoke(button);
	}

	public static void SelectNextUnit(byte playerId)
	{
		InputEvents.OnSelectNextUnit?.Invoke(playerId);
	}

	public static void OmnicursorSnapToTile(Tile tile)
	{
		InputEvents.OnOmnicursorSnapToTile?.Invoke(tile);
	}

	public static void OmnicursorSnapToUIElement(RectTransform transform)
	{
		InputEvents.OnOmnicursorSnapToUIElement?.Invoke(transform);
	}

	public static void OmnicursorStartMoving()
	{
		InputEvents.OnOmnicursorStartMoving?.Invoke();
	}
}
