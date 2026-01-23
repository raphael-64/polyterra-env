using System;
using System.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class ClientInteraction : MonoBehaviour
{
	public enum ButtonInputMode
	{
		None,
		Map,
		Menu
	}

	[SerializeField]
	private LayerMask hitMask = LayerMask.op_Implicit(-1);

	[SerializeField]
	private TileMark tileMark;

	[SerializeField]
	private DamageIndicator damageIndicator;

	[SerializeField]
	private DamageIndicator retaliationDamageIndicator;

	private Tile markedTile;

	private Tile currentTile;

	private Tile selectedTile;

	private Unit selectedUnit;

	private Unit hoveredUnit;

	private WorldCoordinates selectedUnitCoordinates = WorldCoordinates.NULL_COORDINATES;

	private Tile lastTileClick;

	private int consecutiveClicks;

	private int lastSelectedUnitIndex;

	private bool wasDownOverUI;

	private Vector2 previousMousePosition;

	private float lastAxisMovementTime;

	private const float AXIS_MOVEMENT_REPEAT_DELAY = 0.3f;

	private List<Touch> touches = new List<Touch>();

	private UINavigationManager.NavigationType currentNavigationType = UINavigationManager.NavigationType.Mouse;

	private ButtonInputMode buttonInputMode = ButtonInputMode.Map;

	public void UpdateAfterMapRefresh()
	{
		if (selectedUnitCoordinates != WorldCoordinates.NULL_COORDINATES)
		{
			selectedUnit = MapRenderer.Current?.GetTileInstance(selectedUnitCoordinates)?.Unit;
			if ((Object)(object)selectedUnit == (Object)null)
			{
				Log.Error("Failed to find selected unit at {0} after map refresh", new object[1] { selectedUnitCoordinates });
				InputEvents.SelectionCleared();
				return;
			}
		}
		GetInteractionBar().UpdateAfterMapRefresh();
	}

	private ButtonInputMode GetSuitableButtonInputMode()
	{
		if ((Object)(object)selectedUnit != (Object)null && GetInteractionBar().HasButtons() && !selectedUnit.IsInteractableByPlayer(GameManager.LocalPlayer.Id))
		{
			return ButtonInputMode.Menu;
		}
		if ((Object)(object)selectedTile != (Object)null && GetInteractionBar().HasButtons())
		{
			return ButtonInputMode.Menu;
		}
		return ButtonInputMode.Map;
	}

	public ButtonInputMode GetCurrentButtonInputMode()
	{
		return buttonInputMode;
	}

	private void ChangeButtonInputMode(ButtonInputMode newInputMode)
	{
		buttonInputMode = newInputMode;
		GetInteractionBar().ShowKeySelection(newInputMode == ButtonInputMode.Menu);
	}

	public void ChangeUINavigationType(UINavigationManager.NavigationType navType)
	{
		currentNavigationType = navType;
		switch (currentNavigationType)
		{
		case UINavigationManager.NavigationType.Buttons:
			ChangeButtonInputMode(GetSuitableButtonInputMode());
			lastAxisMovementTime = Time.time;
			if ((Object)(object)markedTile == (Object)null)
			{
				if ((Object)(object)selectedTile != (Object)null)
				{
					SetMarkedTile(selectedTile);
					break;
				}
				WorldCoordinates startTile = GameManager.LocalPlayer.startTile;
				Tile tileInstance = MapRenderer.Current.GetTileInstance(startTile);
				SetMarkedTile(tileInstance);
			}
			else
			{
				SetMarkedTile(markedTile);
			}
			break;
		case UINavigationManager.NavigationType.Mouse:
			tileMark.Hide();
			break;
		}
	}

	public void SetMarkedTile(Tile tile, bool revealShouldAccountForHUD = true)
	{
		markedTile = tile;
		if ((Object)(object)markedTile == (Object)null)
		{
			tileMark.Hide();
			InputEvents.TileMarkCleared();
		}
		else
		{
			tileMark.Show(tile);
			InputEvents.TileMarked(tile);
		}
		if (!PolytopiaInput.isTrackingOmnicursor)
		{
			CameraController.Instance.RevealTile(tile, revealShouldAccountForHUD, checkEdges: true, 1f, forceChange: true);
		}
	}

	private void OnEnable()
	{
		InputEvents.OnSelectionCleared += OnSelectionCleared;
		InputEvents.OnSelectNextUnit += OnSelectNextUnit;
		GameEvents.OnTurnStarted += OnTurnStarted;
	}

	private void OnDisable()
	{
		InputEvents.OnSelectionCleared -= OnSelectionCleared;
		InputEvents.OnSelectNextUnit -= OnSelectNextUnit;
		GameEvents.OnTurnStarted -= OnTurnStarted;
	}

	private InteractionBar GetInteractionBar()
	{
		return (UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).InteractionBar;
	}

	private void OnTurnStarted()
	{
		if (GameManager.LocalPlayer.Id == GameManager.GameState.CurrentPlayer)
		{
			if ((Object)(object)selectedTile != (Object)null)
			{
				GetInteractionBar().ForceResetMode();
				selectedTile.OnSelected();
			}
			else if ((Object)(object)selectedUnit != (Object)null)
			{
				GetInteractionBar().ForceResetMode();
				selectedUnit.OnSelected();
			}
		}
	}

	private void ToggleScreen(UIConstants.Screens screen)
	{
		if (UIManager.Instance.CurrentScreen == screen)
		{
			UIManager.Instance.OnBack();
			return;
		}
		if (UIManager.Instance.CurrentScreen == UIConstants.Screens.IngameMenu || UIManager.Instance.CurrentScreen == UIConstants.Screens.StatsScreen || UIManager.Instance.CurrentScreen == UIConstants.Screens.TechTree)
		{
			UIManager.Instance.OnBack();
		}
		UIManager.Instance.ShowScreen(screen);
	}

	private void UpdateBottomMenuKeyPresses()
	{
		if (!PopupManager.PopupShowing && !ResultScreen.IsShowing())
		{
			if (InputManager.GetKeyUp((KeyCode)49))
			{
				InputEvents.SelectionCleared();
				ToggleScreen(UIConstants.Screens.IngameMenu);
			}
			else if (InputManager.GetKeyUp((KeyCode)50))
			{
				InputEvents.SelectionCleared();
				ToggleScreen(UIConstants.Screens.StatsScreen);
			}
			else if (InputManager.GetKeyUp((KeyCode)51))
			{
				InputEvents.SelectionCleared();
				ToggleScreen(UIConstants.Screens.TechTree);
			}
			else if (InputManager.GetKeyUp((KeyCode)52) && UIManager.Instance.CurrentScreen == UIConstants.Screens.Hud && !ResultScreen.IsShowing())
			{
				(UIManager.Instance.GetCurrentScreen() as HudScreen).OnNextTurn(forceConfirmation: true);
			}
			else if (InputManager.GetButtonUp("Accept") && (Object)(object)selectedTile == (Object)null && (Object)(object)selectedUnit == (Object)null && UIManager.Instance.CurrentScreen == UIConstants.Screens.Hud && !ResultScreen.IsShowing())
			{
				InputManager.EatButton("Accept");
				(UIManager.Instance.GetCurrentScreen() as HudScreen).OnNextTurn(forceConfirmation: true);
			}
		}
	}

	private void UpdateHover()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector2 inputPosition = InputManager.GetInputPosition();
		if (!InputManager.IsPositionOverUIObject(inputPosition) && !InputManager.OutsideWindow)
		{
			Tile tile = TileAtScreenPosition(inputPosition);
			UpdateHoverOnTile(tile);
		}
	}

	private void UpdateHoverOnTile(Tile tile)
	{
		if (Object.op_Implicit((Object)(object)tile))
		{
			WorldMouseCatcher.StopSunriseTimer();
			if ((Object)(object)tile != (Object)(object)currentTile)
			{
				OnHoverEnd(currentTile);
				currentTile = null;
			}
			OnHoverStart(tile);
		}
		else
		{
			OnHoverEnd(currentTile);
			currentTile = null;
		}
	}

	private Tile TileAtScreenPosition(Vector2 position)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		WorldCoordinates coordinates = Vector2.op_Implicit(Camera.main.ScreenToWorldPoint(Vector2.op_Implicit(position))).ToWorldCoordinates();
		return MapRenderer.Current.GetTileInstance(coordinates);
	}

	private void UpdateMouseInput()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Invalid comparison between Unknown and I4
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Invalid comparison between Unknown and I4
		if (!InputManager.HasTouches())
		{
			WorldMouseCatcher.StopSunriseTimer();
		}
		if (SystemManager.IsMobile && !InputManager.HasTouches())
		{
			return;
		}
		if (InputManager.HasTouches())
		{
			for (int i = 0; i < InputManager.GetTouchCount(); i++)
			{
				Vector2 position = InputManager.CurrentTouches[i].position;
				bool num = InputManager.IsPositionOverUIObject(position);
				if (num)
				{
					TouchPhase phase = InputManager.CurrentTouches[i].phase;
					if ((int)phase == 0 || (int)phase == 1)
					{
						wasDownOverUI = true;
					}
					else
					{
						wasDownOverUI = false;
					}
				}
				if (num || InputManager.OutsideWindow)
				{
					break;
				}
				Tile tile = TileAtScreenPosition(position);
				UpdateHoverOnTile(tile);
				if ((int)InputManager.CurrentTouches[i].phase == 0)
				{
					if ((Object)(object)tile == (Object)null)
					{
						WorldMouseCatcher.StartSunriseTimer();
					}
					OnPress(tile);
					wasDownOverUI = false;
				}
				if ((int)InputManager.CurrentTouches[i].phase == 3)
				{
					if (!wasDownOverUI)
					{
						OnRelease(tile, i);
					}
					else
					{
						wasDownOverUI = false;
					}
				}
				if ((Object)(object)tile != (Object)null && (Object)(object)tile.Unit != (Object)null && (Object)(object)selectedUnit != (Object)null && IsTouchValidLongpress(i))
				{
					StartHoverUnit(tile.Unit);
				}
			}
		}
		else
		{
			UpdateHover();
		}
	}

	private Vector2 NormalizedPrimaryAxisPosition()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(InputManager.GetAxisRaw("PrimaryHorizontal"), InputManager.GetAxisRaw("PrimaryVertical"));
		val.x = ((Mathf.Abs(val.x) < 0.01f) ? (val.x = 0f) : Mathf.Sign(val.x));
		val.y = ((Mathf.Abs(val.y) < 0.01f) ? (val.y = 0f) : Mathf.Sign(val.y));
		return val;
	}

	private void UpdateKeySelectionInput()
	{
		if (!InputManager.GetButtonUp("Jump") && !InputManager.GetButtonDown("Jump"))
		{
			return;
		}
		Tile tile = (((Object)(object)selectedUnit != (Object)null) ? selectedUnit.Tile : selectedTile);
		int num;
		if (!((Object)(object)markedTile != (Object)(object)tile) && !((Object)(object)selectedUnit == (Object)null) && buttonInputMode != ButtonInputMode.Menu)
		{
			num = ((!GetInteractionBar().HasButtons()) ? 1 : 0);
			if (num == 0)
			{
				goto IL_008f;
			}
		}
		else
		{
			num = 1;
		}
		if (InputManager.GetButtonDown("Jump"))
		{
			OnPress(markedTile);
		}
		goto IL_008f;
		IL_008f:
		if (num != 0 && InputManager.GetButtonUp("Jump"))
		{
			ChangeButtonInputMode(GetSuitableButtonInputMode());
		}
		else if (InputManager.GetButtonUp("Jump"))
		{
			ChangeButtonInputMode(ButtonInputMode.Menu);
		}
	}

	private int GetImprovementTotal()
	{
		int num = 0;
		if (!Object.op_Implicit((Object)(object)markedTile))
		{
			return 0;
		}
		PlayerState localPlayer = GameManager.LocalPlayer;
		num += CommandUtils.GetBuildableImprovements(GameManager.GameState, localPlayer, markedTile.Data, includeUnavailable: true).Count;
		foreach (ImprovementData unlockableImprovement in GameManager.GameState.GameLogicData.GetUnlockableImprovements(localPlayer))
		{
			if (GameManager.GameState.GameLogicData.CanBuild(GameManager.GameState, markedTile.Data, localPlayer, unlockableImprovement))
			{
				num++;
			}
		}
		return num;
	}

	private void UpdateKeyMovementInput()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = NormalizedPrimaryAxisPosition();
		if (((Vector2)(ref val)).sqrMagnitude < 1E-07f)
		{
			lastAxisMovementTime = 0f;
		}
		else if (Time.time - lastAxisMovementTime >= 0.3f && buttonInputMode == ButtonInputMode.Map)
		{
			int width = GameManager.GameState.Map.Width;
			int x = (int)Mathf.Clamp((float)markedTile.Coordinates.X + val.y, 0f, (float)(width - 1));
			int y = (int)Mathf.Clamp((float)markedTile.Coordinates.Y - val.x, 0f, (float)(width - 1));
			WorldCoordinates worldCoordinates = new WorldCoordinates(x, y);
			if (worldCoordinates != markedTile.Coordinates)
			{
				Tile tileInstance = MapRenderer.Current.GetTileInstance(worldCoordinates);
				lastAxisMovementTime = Time.time;
				SetMarkedTile(tileInstance);
			}
		}
	}

	private void Update()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		UpdateBottomMenuKeyPresses();
		if (!InputManager.IsEnabled(InputManager.InputType.Map) || (Object)(object)MapRenderer.Current == (Object)null || !MapRenderer.Current.IsRendered)
		{
			return;
		}
		UpdateMouseInput();
		switch (currentNavigationType)
		{
		case UINavigationManager.NavigationType.Buttons:
		{
			if (UIManager.Instance.CurrentScreen == UIConstants.Screens.Hud)
			{
				UpdateKeySelectionInput();
				UpdateKeyMovementInput();
			}
			Vector2 val = PolytopiaInput.mousePosition - previousMousePosition;
			if (((Vector2)(ref val)).sqrMagnitude > 1E-05f)
			{
				ChangeUINavigationType(UINavigationManager.NavigationType.Mouse);
			}
			break;
		}
		case UINavigationManager.NavigationType.Mouse:
			if (InputManager.GetAxisRaw("PrimaryHorizontal") != 0f || InputManager.GetAxisRaw("PrimaryVertical") != 0f || InputManager.GetButton("Jump"))
			{
				ChangeUINavigationType(UINavigationManager.NavigationType.Buttons);
				if (UIManager.Instance.CurrentScreen == UIConstants.Screens.Hud)
				{
					UpdateKeySelectionInput();
				}
			}
			break;
		}
		previousMousePosition = InputManager.GetInputPosition();
	}

	private void ClearSelection()
	{
		InputEvents.SelectionCleared();
	}

	public void SelectTile(Tile tile)
	{
		GameManager.debugTile = tile;
		markedTile = tile;
		if ((Object)(object)tile == (Object)(object)selectedTile)
		{
			ClearSelection();
			return;
		}
		if (Object.op_Implicit((Object)(object)selectedUnit))
		{
			bool flag = selectedUnit.Owner == GameManager.LocalPlayer && GameManager.LocalPlayer.Id == GameManager.GameState.CurrentPlayer;
			if (flag && selectedUnit.CanMoveTo(tile.Coordinates))
			{
				ValidateMove(selectedUnit, tile, delegate
				{
					MoveCommand command2 = new MoveCommand(GameManager.LocalPlayer.Id, selectedUnit.State, tile.Coordinates);
					if (ClientActionManager.CanExecuteCommand(command2, GameManager.GameState))
					{
						GameManager.Client.SendCommand(command2);
						ClearSelection();
					}
				});
			}
			else if (flag && selectedUnit.CanAttack(tile.Coordinates))
			{
				AttackCommand command = new AttackCommand(GameManager.LocalPlayer.Id, selectedUnit.State, tile.Coordinates);
				if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
				{
					GameManager.Client.SendCommand(command);
					ClearSelection();
				}
			}
			else if (tile.IsHidden)
			{
				ClearSelection();
			}
			else if ((Object)(object)tile != (Object)(object)selectedUnit.Tile && Object.op_Implicit((Object)(object)tile.Unit) && tile.UnitVisible)
			{
				SelectUnit(tile.Unit);
			}
			else
			{
				DeselectUnit();
				SelectTileInternal(tile);
			}
		}
		else
		{
			DeselectTile();
			if (!tile.IsHidden && Object.op_Implicit((Object)(object)tile.Unit) && tile.UnitVisible)
			{
				SelectUnit(tile.Unit);
			}
			else if (!tile.IsHidden)
			{
				SelectTileInternal(tile);
			}
			else
			{
				tile.SpawnPuff();
				AudioManager.PlaySFXAtTile(AudioUtils.GetCloudForCoordinate(tile.Coordinates), tile.Coordinates);
				ClearSelection();
			}
		}
		CameraController.Instance.RevealTile(selectedTile, shouldAccountForHUD: true, checkEdges: true, 1f, forceChange: true);
	}

	public void SelectTileInternal(Tile tile)
	{
		if ((Object)(object)tile == (Object)null)
		{
			DeselectTile();
			return;
		}
		selectedTile = tile;
		selectedTile.OnSelected();
	}

	public void DeselectTile()
	{
		if (Object.op_Implicit((Object)(object)selectedTile))
		{
			selectedTile.OnDeselected();
			selectedTile = null;
		}
	}

	public void SelectUnit(Unit unit)
	{
		if ((Object)(object)unit == (Object)null)
		{
			DeselectUnit();
			return;
		}
		DeselectTile();
		DeselectUnit();
		selectedUnit = unit;
		Log.Verbose("Selected unit with id: {0}", new object[1] { selectedUnit.State.id });
		selectedUnitCoordinates = unit.Tile.Coordinates;
		selectedUnit.OnSelected();
		AudioManager.PlaySFXAtTile(SFXTypes.Press, unit.Tile.Coordinates);
		CameraController.Instance.RevealTile(selectedUnit.Tile, shouldAccountForHUD: true, checkEdges: true, 1f, forceChange: true);
	}

	public bool TryUpdateSelection(Unit unit)
	{
		if ((Object)(object)selectedUnit == (Object)(object)unit)
		{
			selectedUnitCoordinates = unit.Tile.Coordinates;
			Log.Verbose("Update Selected unit with id: {0}", new object[1] { selectedUnit.State.id });
			selectedUnit.UpdateSelection();
			return true;
		}
		return false;
	}

	public bool TryDeselectUnit(Unit unit)
	{
		if ((Object)(object)selectedUnit == (Object)(object)unit)
		{
			ClearSelection();
			return true;
		}
		return false;
	}

	public void DeselectUnit()
	{
		if (Object.op_Implicit((Object)(object)selectedUnit))
		{
			selectedUnit.OnDeselected();
			selectedUnitCoordinates = WorldCoordinates.NULL_COORDINATES;
			selectedUnit = null;
			StopHoverUnit();
		}
	}

	public void StartHoverUnit(Unit unit)
	{
		if ((Object)(object)hoveredUnit == (Object)(object)unit || (Object)(object)selectedUnit == (Object)(object)unit || unit.Tile.IsHidden)
		{
			return;
		}
		hoveredUnit = unit;
		if (selectedUnit.HasAttackOption(unit.Tile.Coordinates))
		{
			BattleResults battleResults = BattleHelpers.GetBattleResults(GameManager.GameState, selectedUnit.State, unit.State);
			damageIndicator.Show(hoveredUnit, battleResults.attackDamage, (int)hoveredUnit.Health, 0f);
			if (battleResults.retaliationDamage != -1)
			{
				retaliationDamageIndicator.Show(selectedUnit, battleResults.retaliationDamage, (int)selectedUnit.Health, 0.2f);
			}
		}
	}

	public void StopHoverUnit()
	{
		hoveredUnit = null;
		damageIndicator.Hide();
		retaliationDamageIndicator.Hide();
	}

	private void ValidateMove(Unit unit, Tile tile, Action callback)
	{
		Tile tile2 = unit.Tile;
		Tile tileInstance = MapRenderer.Current.GetTileInstance(tile.Coordinates);
		if (tile2.Data.IsWater && !tileInstance.Data.IsWater && unit.Data.IsType(UnitData.Type.Battleship))
		{
			BasicPopup basicPopup = PopupManager.GetBasicPopup();
			basicPopup.Header = Localization.Get("world.unit.disembark.title", Localization.Get(unit.Data.displayName));
			basicPopup.Description = Localization.Get("world.unit.disembark.message", Localization.Get(unit.Data.displayName));
			basicPopup.buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected, delegate
				{
					callback();
				})
			};
			basicPopup.Show();
		}
		else
		{
			callback();
		}
	}

	private void OnPress(Tile tile)
	{
		if ((Object)(object)tile == (Object)null)
		{
			return;
		}
		tile.OnPress();
		if ((Object)(object)lastTileClick != (Object)null)
		{
			if (tile.Coordinates == lastTileClick.Coordinates && !tile.Data.IsWater && tile.Data.owner == GameManager.LocalPlayer.Id && tile.Data.unit == null && GameManager.IsPlayerViewing(tile.Owner.Id) && GameManager.GameState.Settings.GameType == GameType.SinglePlayer)
			{
				if (consecutiveClicks++ > 15)
				{
					if (GameManager.GameState.TryGetPlayer(byte.MaxValue, out var playerState) && GameManager.GameState.GameLogicData.TryGetData(UnitData.Type.Bunny, out var data))
					{
						UnitState unitState = ActionUtils.TrainUnit(GameManager.GameState, playerState, tile.Data, data);
						AudioManager.PlaySFXAtTile(SFXTypes.Plop, tile.Coordinates);
						unitState.moved = false;
						unitState.attacked = false;
						tile.RenderUnit();
						tile.SpawnPuff();
						if (tile.Data.IsBeingCaptured(GameManager.GameState))
						{
							tile.SpawnFire();
						}
					}
					consecutiveClicks = 0;
				}
			}
			else
			{
				consecutiveClicks = 0;
			}
		}
		lastTileClick = tile;
	}

	private void OnRelease(Tile tile, int touchIndex = 0)
	{
		if (!Object.op_Implicit((Object)(object)tile))
		{
			return;
		}
		if ((Object)(object)currentTile == (Object)(object)tile || currentNavigationType == UINavigationManager.NavigationType.Buttons)
		{
			tile.OnRelease();
			if (IsTouchValidTap(touchIndex) || currentNavigationType == UINavigationManager.NavigationType.Buttons)
			{
				SelectTile(tile);
			}
		}
		if (SystemManager.ShouldUseTouchInterface())
		{
			StopHoverUnit();
		}
	}

	public bool IsTouchValidTap(int index)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		List<PolytopiaTouch> currentTouches = InputManager.CurrentTouches;
		if (currentTouches != null && currentTouches.Count <= index)
		{
			return false;
		}
		PolytopiaTouch polytopiaTouch = InputManager.CurrentTouches[index];
		if ((int)polytopiaTouch.phase != 3)
		{
			return false;
		}
		return polytopiaTouch.IsValidTap();
	}

	public bool IsTouchValidLongpress(int index)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Invalid comparison between Unknown and I4
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		List<PolytopiaTouch> currentTouches = InputManager.CurrentTouches;
		if (currentTouches != null && currentTouches.Count <= index)
		{
			return false;
		}
		PolytopiaTouch polytopiaTouch = InputManager.CurrentTouches[index];
		if ((int)polytopiaTouch.phase == 3 || (int)polytopiaTouch.phase == 4)
		{
			return false;
		}
		return polytopiaTouch.IsValidLongPress();
	}

	private void OnHoverStart(Tile tile)
	{
		if (Object.op_Implicit((Object)(object)tile))
		{
			currentTile = tile;
			currentTile.OnHoverStart();
			if ((Object)(object)selectedUnit != (Object)null && (Object)(object)tile.Unit != (Object)null && Time.time - tile.HoverStartedTime > 1f)
			{
				StartHoverUnit(tile.Unit);
			}
		}
	}

	private void OnHoverEnd(Tile tile)
	{
		if (Object.op_Implicit((Object)(object)tile))
		{
			tile.OnHoverEnd();
			StopHoverUnit();
		}
	}

	private void OnSelectionCleared()
	{
		ChangeButtonInputMode(ButtonInputMode.Map);
		DeselectUnit();
		DeselectTile();
	}

	private void OnSelectNextUnit(byte playerId)
	{
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)MapRenderer.Current == (Object)null || MapRenderer.Current.RenderedUnits == null)
		{
			return;
		}
		int num = lastSelectedUnitIndex;
		for (int num2 = MapRenderer.Current.RenderedUnits.Count; num2 > 0; num2--)
		{
			num++;
			if (num >= MapRenderer.Current.RenderedUnits.Count)
			{
				num = 0;
			}
			Unit unit = MapRenderer.Current.RenderedUnits[num];
			if ((Object)(object)unit != (Object)null && unit.State.owner == playerId && (Object)(object)unit != (Object)(object)selectedUnit && unit.State.CanPerformAnyAction(GameManager.GameState))
			{
				lastSelectedUnitIndex = num;
				CameraController.Instance.CenterOnPosition(unit.Tile.Coordinates.ToPosition(), 1f, delegate
				{
					SelectUnit(unit);
				});
				break;
			}
		}
	}
}
