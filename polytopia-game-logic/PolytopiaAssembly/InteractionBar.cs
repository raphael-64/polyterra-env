using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractionBar : UIBasicComponent
{
	public enum Type
	{
		None,
		MobileBar,
		PcBar
	}

	private enum Mode
	{
		None,
		Tile,
		Unit,
		Building
	}

	private enum VisibilityState
	{
		Hidden,
		AnimateHide,
		Showing,
		AnimatingShow
	}

	public Type type;

	[Header("References")]
	public UIDuplicatedSprites duplicatedSprites;

	public TextMeshProUGUI header;

	public TextMeshProUGUI description;

	public RectTransform topBarContainer;

	public RectTransform bottomBar;

	public RectTransform bottomBarButtonContainer;

	public LayoutGroup buttonContainerLayoutGroup;

	public Image infoIcon;

	public GameObject ButtonGlyphLeft;

	public GameObject ButtonGlyphRight;

	[Header("Prefabs")]
	public UIRoundButton roundButtonPrefab;

	private Tile tile;

	private Unit unit;

	private Building building;

	private WorldCoordinates coordinates = WorldCoordinates.NULL_COORDINATES;

	private Mode mode;

	private Mode showingMode;

	private Tile showingTile;

	private int oldSelectedButton = -1;

	private VisibilityState visibilityState = VisibilityState.Showing;

	private List<UIRoundButton> buttons = new List<UIRoundButton>();

	private float spriteTopOffset;

	private const float TOP_OFFSET_MARGIN = 10f;

	private Tween barPositionTween;

	private PopupManager.UnitPopupData unitPopupData;

	private PopupManager.IconPopupData iconPopupData;

	private QuickActions quickActions;

	private UISpriteDuplicator.SpriteDuplicationData spriteDuplicationData;

	private string headerString;

	private string descriptionString;

	private bool showInfoIcon;

	private bool forceReset;

	private BasicPopup activePopup;

	public RectTransform RectTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public bool IsShowing => ShouldShow();

	private float ButtonBarOffset
	{
		get
		{
			RectTransform obj = ((HudScreen)UIManager.Instance.GetScreen(UIConstants.Screens.Hud)).buttonBar.menuButton.rectTransform;
			Vector3[] array = (Vector3[])(object)new Vector3[4];
			base.rectTransform.GetWorldCorners(array);
			Vector3[] array2 = (Vector3[])(object)new Vector3[4];
			obj.GetWorldCorners(array2);
			return (array[2].x > array2[0].x) ? 100 : 0;
		}
	}

	public override void Init()
	{
		base.Init();
		InputEvents.OnTileSelected += OnTileSelected;
		InputEvents.OnBuildingSelected += OnBuildingSelected;
		InputEvents.OnUnitSelected += OnUnitSelected;
		InputEvents.OnSelectionCleared += OnSelectionCleared;
		GameEvents.OnWaitingForServer += OnWaitingForServer;
		((Component)infoIcon).gameObject.SetActive(false);
		Hide(instant: true);
	}

	public override void DeInit()
	{
		Reset();
	}

	private void OnWaitingForServer(bool isWaiting)
	{
		activePopup?.RefreshButtonState();
	}

	public void Reset()
	{
		InputEvents.OnTileSelected -= OnTileSelected;
		InputEvents.OnBuildingSelected -= OnBuildingSelected;
		InputEvents.OnUnitSelected -= OnUnitSelected;
		InputEvents.OnSelectionCleared -= OnSelectionCleared;
		GameEvents.OnWaitingForServer -= OnWaitingForServer;
		TweenUtils.KillTween(barPositionTween);
		barPositionTween = null;
	}

	private void OnDestroy()
	{
		Reset();
	}

	private bool ShouldShow()
	{
		if ((!((Object)(object)unit == (Object)null) || mode != Mode.Unit) && (!((Object)(object)building == (Object)null) || mode != Mode.Building) && (Object)(object)tile != (Object)null)
		{
			return mode != Mode.None;
		}
		return false;
	}

	private bool ShouldRefresh()
	{
		if (showingMode == mode)
		{
			return (Object)(object)showingTile != (Object)(object)tile;
		}
		return true;
	}

	public void Show(bool instant = false, bool force = false)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Expected O, but got Unknown
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		if (mode != Mode.None && HasQuickActions() && !IsShowingQuickActions())
		{
			quickActions.Show();
		}
		if (visibilityState != VisibilityState.Hidden && !force)
		{
			return;
		}
		((TMP_Text)header).text = headerString;
		((TMP_Text)description).text = descriptionString;
		SetTribeInfoButtons(header, TextType.Header);
		SetTribeInfoButtons(description, TextType.Description);
		UIManager.Instance.SpriteDuplicator.RenderSprites(spriteDuplicationData);
		Rect actualRect = UIUtils.GetActualRect(duplicatedSprites.rectTransform);
		spriteTopOffset = Mathf.Max(0f, ((Rect)(ref actualRect)).position.y - topBarContainer.sizeDelta.y * 0.5f + ((Rect)(ref actualRect)).size.y * 0.5f);
		if (showInfoIcon)
		{
			Rect actualRect2 = UIUtils.GetActualRect(duplicatedSprites.rectTransform, includeRootSize: false);
			Rect rect = ((Graphic)infoIcon).rectTransform.rect;
			((Graphic)infoIcon).rectTransform.anchoredPosition = ((Rect)(ref actualRect2)).position + new Vector2(((Rect)(ref actualRect2)).size.x * 0.5f - ((Rect)(ref rect)).size.x * 0.5f, ((Rect)(ref actualRect2)).size.y * 0.5f + ((Rect)(ref rect)).size.y * 0.5f);
		}
		((Component)infoIcon).gameObject.SetActive(showInfoIcon);
		showingMode = mode;
		showingTile = tile;
		float num = ((buttons.Count <= 0) ? ((type == Type.PcBar) ? (-74f + ButtonBarOffset) : (-110f)) : ((type == Type.PcBar) ? (26f + ButtonBarOffset) : 0f));
		((Component)bottomBar).gameObject.SetActive(buttons.Count > 0);
		if (instant)
		{
			TweenUtils.KillTween(barPositionTween, complete: true);
			visibilityState = VisibilityState.Showing;
			((Component)this).gameObject.SetActive(true);
			RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, num);
			if (ShouldRefresh())
			{
				Refresh();
			}
		}
		else
		{
			TweenUtils.KillTween(barPositionTween);
			visibilityState = VisibilityState.AnimatingShow;
			((Component)this).gameObject.SetActive(true);
			barPositionTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(RectTransform.DOAnchorPosY(num, 0.2f), (Ease)6), (TweenCallback)delegate
			{
				visibilityState = VisibilityState.Showing;
				if (ShouldRefresh())
				{
					Refresh();
				}
			});
		}
		UIEvents.InteractionBarOpen(open: true);
	}

	public bool HasButtons()
	{
		if (buttons.Count <= 0)
		{
			if ((Object)(object)quickActions != (Object)null)
			{
				return quickActions.HasButtons();
			}
			return false;
		}
		return true;
	}

	public void Hide(bool instant = false, bool hideQuickActions = true)
	{
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		showingMode = Mode.None;
		showingTile = null;
		if (hideQuickActions)
		{
			ClearPopup();
		}
		PolytopiaInput.Omnicursor.RestoreTileAffix();
		if (instant)
		{
			TweenUtils.KillTween(barPositionTween, complete: true);
			visibilityState = VisibilityState.Hidden;
			RectTransform.anchoredPosition = new Vector2(RectTransform.anchoredPosition.x, 0f - (RectTransform.sizeDelta.y + spriteTopOffset + 10f));
			if (Object.op_Implicit((Object)(object)InfoButtonManager.Instance))
			{
				InfoButtonManager.Instance.DestroyInfoButtons(this);
			}
			((Component)this).gameObject.SetActive(false);
			if (HasQuickActions() || hideQuickActions)
			{
				ClearButtons();
			}
			if (ShouldRefresh())
			{
				Refresh();
			}
		}
		else
		{
			TweenUtils.KillTween(barPositionTween);
			visibilityState = VisibilityState.AnimateHide;
			barPositionTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(RectTransform.DOAnchorPosY(0f - (RectTransform.sizeDelta.y + spriteTopOffset + 10f), 0.2f), (Ease)5), (TweenCallback)delegate
			{
				InfoButtonManager.Instance.DestroyInfoButtons(this);
				visibilityState = VisibilityState.Hidden;
				((Component)this).gameObject.SetActive(false);
				if (HasQuickActions() || hideQuickActions)
				{
					ClearButtons();
				}
				if (ShouldRefresh() && IsActiveInteractionBar())
				{
					Refresh();
				}
			});
		}
		if (hideQuickActions && HasQuickActions())
		{
			quickActions.Hide(instant);
		}
		UIEvents.InteractionBarOpen(open: false);
	}

	private bool IsShowingQuickActions()
	{
		if ((Object)(object)quickActions != (Object)null && ((Component)quickActions).gameObject.activeSelf)
		{
			return quickActions.IsShowing;
		}
		return false;
	}

	private bool HasQuickActions()
	{
		if ((Object)(object)quickActions != (Object)null)
		{
			return quickActions.HasButtons();
		}
		return false;
	}

	private bool IsActiveInteractionBar()
	{
		if (type != Type.MobileBar || !SettingsUtils.UseCompactUI)
		{
			if (type == Type.PcBar)
			{
				return !SettingsUtils.UseCompactUI;
			}
			return false;
		}
		return true;
	}

	public void ShowKeySelection(bool shouldShow)
	{
		if (!shouldShow)
		{
			UINavigationManager.Select(null);
			return;
		}
		UINavigationManager.ChangeInputMode(UINavigationManager.NavigationType.Buttons);
		if ((Object)(object)quickActions != (Object)null && quickActions.HasButtons())
		{
			quickActions.ShowKeySelection(shouldShow);
		}
		else if (buttons.Count > 0)
		{
			UINavigationManager.Select((Selectable)(object)buttons[0].button);
		}
	}

	private void OnSelectionCleared()
	{
		PopupManager.HideCurrentPopup();
		SetMode(Mode.None);
		SetTile(null);
		unit = null;
		Refresh();
	}

	private void OnTileSelected(Tile tile)
	{
		if (!((Object)(object)tile == (Object)null))
		{
			if ((Object)(object)tile != (Object)null && (Object)(object)tile.Improvement != (Object)null)
			{
				OnBuildingSelected(tile.Improvement);
				return;
			}
			SetMode(Mode.Tile);
			SetTile(tile);
			building = null;
			unit = null;
			Refresh();
		}
	}

	private void OnBuildingSelected(Building building)
	{
		if (!((Object)(object)building == (Object)null))
		{
			this.building = building;
			SetMode(Mode.Building);
			SetTile(building.Tile);
			unit = null;
			Refresh();
		}
	}

	private void OnUnitSelected(Unit unit)
	{
		if (!((Object)(object)unit == (Object)null))
		{
			this.unit = unit;
			SetMode(Mode.Unit);
			SetTile(unit.Tile);
			building = null;
			Refresh();
		}
	}

	public void OnClose()
	{
		PopupManager.HideCurrentPopup();
		InputEvents.SelectionCleared();
	}

	public void ForceResetMode()
	{
		forceReset = true;
		SetMode(Mode.None);
	}

	private void SetTribeInfoButtons(TextMeshProUGUI textMeshPro, TextType textType)
	{
		TribeInfoButtonData tribeInfoButtonData = new TribeInfoButtonData
		{
			adjustPosition = (textType == TextType.Header)
		};
		InfoButtonManager.Instance.SetTribeInfoButtons(this, textMeshPro, tribeInfoButtonData);
	}

	private void SetMode(Mode mode)
	{
		if (this.mode != mode)
		{
			this.mode = mode;
		}
	}

	private void SetTile(Tile tile)
	{
		if (!((Object)(object)this.tile == (Object)(object)tile))
		{
			this.tile = tile;
			coordinates = (((Object)(object)tile == (Object)null) ? WorldCoordinates.NULL_COORDINATES : tile.Coordinates);
		}
	}

	public void UpdateAfterMapRefresh()
	{
		if (mode == Mode.None || (Object)(object)MapRenderer.Current == (Object)null)
		{
			return;
		}
		SetTile(MapRenderer.Current.GetTileInstance(coordinates));
		if ((Object)(object)tile == (Object)null)
		{
			Log.Error("Failed to update interaction bar state after reset. No tile at {0}", new object[1] { coordinates });
			InputEvents.SelectionCleared();
			return;
		}
		switch (mode)
		{
		case Mode.Unit:
			unit = tile.Unit;
			if ((Object)(object)unit == (Object)null)
			{
				Log.Error("Failed to update interaction bar state after reset. No unit at {0}", new object[1] { coordinates });
				InputEvents.SelectionCleared();
				return;
			}
			break;
		case Mode.Building:
			building = tile.Improvement;
			if ((Object)(object)building == (Object)null)
			{
				Log.Error("Failed to update interaction bar state after reset. No building at {0}", new object[1] { coordinates });
				InputEvents.SelectionCleared();
				return;
			}
			break;
		}
		Refresh();
	}

	private void RefreshInternal()
	{
		RefreshUnitOptions();
		RefreshTileOptions();
		RefreshBuildingOptions();
		SortButtons();
		SetupButtonNavigation();
		if ((Object)(object)ButtonGlyphLeft != (Object)null && (Object)(object)ButtonGlyphRight != (Object)null)
		{
			ButtonGlyphLeft.SetActive(buttons.Count >= 1);
			ButtonGlyphRight.SetActive(buttons.Count >= 1);
		}
	}

	public void MoveSelectedButton(int offset)
	{
		if (buttons.Count != 0)
		{
			if (oldSelectedButton < 0)
			{
				SetSelectedButton((offset > 0) ? (buttons.Count - 1) : 0);
			}
			else
			{
				SetSelectedButton((oldSelectedButton + buttons.Count + offset) % buttons.Count);
			}
		}
	}

	public void SetSelectedButton(int index)
	{
		UIRoundButton uIRoundButton = buttons[index];
		uIRoundButton.Highlighted = true;
		PolytopiaInput.Omnicursor.AffixToUIElement(uIRoundButton.rectTransform);
		UINavigationManager.Select(((Component)uIRoundButton).GetComponent<Selectable>());
		oldSelectedButton = index;
	}

	private void RefreshPCBarInternal()
	{
		ClearButtons();
		ClearPopup();
		if (IsShowingQuickActions())
		{
			quickActions.Hide(instant: true);
		}
		RefreshInternal();
	}

	private void Refresh()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		ClearPopup();
		if (type == Type.PcBar)
		{
			RefreshPCBarInternal();
		}
		else
		{
			ClearButtons();
		}
		if (visibilityState == VisibilityState.AnimateHide || visibilityState == VisibilityState.AnimatingShow)
		{
			return;
		}
		if (ShouldRefresh() && showingMode != Mode.None)
		{
			Hide(instant: false, hideQuickActions: false);
			return;
		}
		if (type != Type.PcBar)
		{
			RefreshInternal();
		}
		if (ShouldShow())
		{
			Show(instant: false, forceReset);
		}
		if ((Object)(object)buttonContainerLayoutGroup != (Object)null)
		{
			buttonContainerLayoutGroup.CalculateLayoutInputHorizontal();
			bottomBarButtonContainer.sizeDelta = new Vector2(buttonContainerLayoutGroup.preferredWidth, bottomBarButtonContainer.sizeDelta.y);
		}
		forceReset = false;
	}

	private void SetupButtonNavigation()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < buttons.Count; i++)
		{
			Button button = buttons[i].button;
			((Selectable)button).navigation = default(Navigation);
			if (i > 0)
			{
				UIUtils.SetSelectOnLeft((Selectable)(object)button, (Selectable)(object)buttons[i - 1].button);
			}
			if (i < buttons.Count - 1)
			{
				UIUtils.SetSelectOnRight((Selectable)(object)button, (Selectable)(object)buttons[i + 1].button);
			}
		}
	}

	private void SortButtons()
	{
		if ((Object)(object)quickActions != (Object)null)
		{
			quickActions.SortButtons();
		}
		else if (buttons != null && buttons.Count != 0)
		{
			buttons.Sort(new ButtonSort());
			for (int i = 0; i < buttons.Count; i++)
			{
				((Transform)buttons[i].rectTransform).SetSiblingIndex(i);
			}
		}
	}

	private void ClearPopup()
	{
		unitPopupData = null;
		if (iconPopupData != null && (Object)(object)iconPopupData.iconContent != (Object)null)
		{
			Object.Destroy((Object)(object)iconPopupData.iconContent);
		}
		iconPopupData = null;
	}

	private void ClearButtons()
	{
		int count = buttons.Count;
		for (int i = 0; i < count; i++)
		{
			Object.Destroy((Object)(object)((Component)buttons[i]).gameObject);
		}
		buttons.Clear();
		if ((Object)(object)quickActions != (Object)null)
		{
			quickActions.Clear();
		}
	}

	private void RefreshBuildingOptions()
	{
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)building == (Object)null || mode != Mode.Building)
		{
			return;
		}
		string info = building.Info;
		int num;
		string text;
		if (building.Data.type == ImprovementData.Type.City)
		{
			if (building.Owner != null)
			{
				num = ((building.Owner.Id != 0) ? 1 : 0);
				if (num != 0)
				{
					text = Localization.Get("actionbox.city");
					goto IL_0078;
				}
			}
			else
			{
				num = 0;
			}
			text = Localization.Get("actionbox.village");
			goto IL_0078;
		}
		text = Localization.Get(building.Data.displayName);
		int num2;
		if (building.HasAbility(ImprovementAbility.Type.Patina))
		{
			GameState gameState = GameManager.GameState;
			if (gameState != null && gameState.Version >= 40)
			{
				num2 = 1;
				goto IL_0135;
			}
		}
		num2 = 0;
		goto IL_0135;
		IL_0135:
		int num3 = num2;
		if (building.Data.maxLevel > 0)
		{
			text = string.Format("{0} {1}", text, Localization.Get("actionbox.building.level", building.Level + num3, building.Data.maxLevel + num3));
		}
		if (building.Tile.Owner != null && building.Tile.Owner != GameManager.LocalPlayer)
		{
			bool flag = GameManager.LocalPlayer.KnowsPlayer(tile.Owner.Id);
			text += (flag ? $" {building.Tile.Owner.GetLinkedTribeNameWithSpace(GameManager.GameState)}" : string.Format(" ({0})", Localization.Get("gamestatus.unknown.tribe")));
		}
		headerString = text;
		if (building.State.effects.Count > 0)
		{
			string text2 = "";
			foreach (ImprovementEffect effect in building.State.effects)
			{
				if (text2 != "")
				{
					text2 += ", ";
				}
				text2 += Localization.Get("actionbox.building." + effect);
			}
			text2 = " (" + text2 + ")";
			headerString += text2;
		}
		if (tile.Data.HasRoad)
		{
			headerString += string.Format(", {0}", Localization.Get("actionbox.tile.roads"));
		}
		goto IL_0314;
		IL_0078:
		if (building.Level > 1)
		{
			text = string.Format("{0} {1}", text, Localization.Get("actionbox.city.level", building.Level));
		}
		if (num != 0)
		{
			string linkedTribeNameWithSpace = tile.Owner.GetLinkedTribeNameWithSpace(GameManager.GameState);
			text += $" {linkedTribeNameWithSpace}";
		}
		headerString = text;
		iconPopupData = new PopupManager.IconPopupData(text, info);
		goto IL_0314;
		IL_0314:
		iconPopupData = new PopupManager.IconPopupData(text, info);
		descriptionString = GetTileTip(building.Tile, GameManager.LocalPlayer.Id);
		spriteDuplicationData = new UISpriteDuplicator.SpriteDuplicationData(((Component)building.Tile).transform, duplicatedSprites, 1f, "Units", new Vector2(0f, 15f));
		showInfoIcon = true;
		AddTrainUnitButtons(tile);
		AddAbilityButtons(tile);
		AddImprovementButtons(tile);
	}

	private IEnumerator VisualizeRating()
	{
		MapGenerator.GetCityRating(tile.Data, GameManager.GameState, out var tiles);
		foreach (KeyValuePair<WorldCoordinates, int> item in tiles)
		{
			Tile tileInstance = MapRenderer.Current.GetTileInstance(item.Key);
			if (item.Value > 0)
			{
				tileInstance.ShowRateCount(item.Value);
				yield return (object)new WaitForSeconds(0.1f);
			}
		}
	}

	private void RefreshUnitOptions()
	{
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)unit == (Object)null || mode != Mode.Unit)
		{
			return;
		}
		string arg = ((unit.Owner.Id == byte.MaxValue) ? "" : unit.Owner.GetLinkedTribeNameWithSpace(GameManager.GameState));
		string text;
		if (unit.State.passengerUnit != null)
		{
			GameManager.GameState.GameLogicData.TryGetData(unit.State.passengerUnit.type, out var data);
			text = $"{Localization.Get(unit.Data.displayName)} ({Localization.Get(data.displayName)})";
		}
		else
		{
			text = Localization.Get(unit.Data.displayName);
		}
		if (unit.State.effects.Count > 0)
		{
			string text2 = "";
			foreach (UnitEffect effect in unit.State.effects)
			{
				if (text2 != "")
				{
					text2 += ", ";
				}
				text2 += Localization.Get("actionbox.unit." + effect);
			}
			text2 = " (" + text2 + ")";
			text += text2;
		}
		string arg2 = "";
		if (unit.State.promotionLevel > 0)
		{
			arg2 = string.Format(" ({0})", Localization.Get("actionbox.unit.veteran"));
		}
		else if (unit.Data.promotionLimit > 0 && !unit.Data.IsVehicle() && unit.State.xp > 0)
		{
			arg2 = string.Format(" ({0})", Localization.Get("actionbox.unit.kills", unit.State.xp, unit.Data.promotionLimit));
		}
		headerString = $"{arg} {text}{arg2}";
		spriteDuplicationData = new UISpriteDuplicator.SpriteDuplicationData(((Component)unit).transform, duplicatedSprites, 1.8f, "UnitStatusDisplays", new Vector2(0f, 0f));
		spriteDuplicationData.forceFullAlpha = true;
		showInfoIcon = true;
		unitPopupData = new PopupManager.UnitPopupData(unit);
		PlayerState localPlayer = GameManager.LocalPlayer;
		descriptionString = GetUnitTip(unit, localPlayer.Id);
		AddUnitActionButtons(tile);
	}

	private void RefreshTileOptions()
	{
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tile == (Object)null || mode != Mode.Tile)
		{
			return;
		}
		TerrainData data2;
		if (Object.op_Implicit((Object)(object)tile.Resource) && tile.Resource.IsVisibleToPlayer)
		{
			if (GameManager.GameState.GameLogicData.TryGetData(tile.Data.terrain, out var data))
			{
				headerString = $"{Localization.Get(data.displayName)}{LanguageUtils.CommaWithSpace}{Localization.Get(tile.Resource.Data.displayName)}";
			}
			else
			{
				headerString = Localization.Get(tile.Resource.Data.displayName);
			}
		}
		else if (GameManager.GameState.GameLogicData.TryGetData(tile.Data.terrain, out data2))
		{
			headerString = Localization.Get(data2.displayName);
		}
		if (tile.Data.HasRoad)
		{
			headerString += string.Format("{0}{1}", LanguageUtils.CommaWithSpace, Localization.Get("actionbox.tile.roads"));
		}
		if (tile.Owner != null && !tile.OwnedBy(0) && !tile.OwnedBy(GameManager.LocalPlayer.Id))
		{
			bool flag = GameManager.LocalPlayer.KnowsPlayer(tile.Owner.Id);
			headerString += (flag ? $" {tile.Owner.GetLinkedTribeNameWithSpace(GameManager.GameState)}" : string.Format(" ({0})", Localization.Get("gamestatus.unknown.tribe")));
		}
		descriptionString = GetTileTip(tile, GameManager.LocalPlayer.Id);
		spriteDuplicationData = new UISpriteDuplicator.SpriteDuplicationData(((Component)tile).transform, duplicatedSprites, 1f, "Units", new Vector2(0f, 15f));
		showInfoIcon = iconPopupData != null || unitPopupData != null;
		AddImprovementButtons(tile);
		AddTrainUnitButtons(tile);
	}

	private UIRoundButton CreateRoundBottomBarButton(string text, bool forceToBottom = false)
	{
		RectTransform buttonContainer;
		if (SettingsUtils.UseCompactUI || forceToBottom)
		{
			buttonContainer = bottomBarButtonContainer;
		}
		else
		{
			if ((Object)(object)quickActions == (Object)null)
			{
				HudScreen hudScreen = UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen;
				quickActions = hudScreen.GetQuickActions();
				((Component)quickActions).gameObject.SetActive(false);
				quickActions.KeepWorldPosition = true;
			}
			buttonContainer = quickActions.buttonContainer;
			quickActions.Coordinates = tile.Coordinates;
		}
		UIRoundButton uIRoundButton = Object.Instantiate<UIRoundButton>(roundButtonPrefab, (Transform)(object)buttonContainer);
		uIRoundButton.text = LocalizationUtils.CapitalizeString(text);
		uIRoundButton.buttonActive = true;
		uIRoundButton.Cost = -1f;
		if (SettingsUtils.UseCompactUI || forceToBottom)
		{
			buttons.Add(uIRoundButton);
		}
		else if ((Object)(object)quickActions != (Object)null)
		{
			quickActions.AddButton(uIRoundButton);
		}
		return uIRoundButton;
	}

	private void AddImprovementButtons(Tile tile)
	{
		PlayerState player = GameManager.LocalPlayer;
		if (player.AutoPlay)
		{
			return;
		}
		foreach (BuildCommand buildCommand in CommandUtils.GetBuildableImprovements(GameManager.GameState, player, tile.Data, includeUnavailable: true))
		{
			GameManager.GameState.GameLogicData.TryGetData(buildCommand.Type, out var data);
			UIRoundButton uIRoundButton = CreateRoundBottomBarButton(Localization.Get(data.displayName));
			int climate = (data.type.IsMonument() ? player.GetTribeStyle(GameManager.GameState) : tile.Data.climate);
			SkinType skinType = (data.type.IsMonument() ? player.skinType : tile.Data.skinType);
			uIRoundButton.iconSpriteHandle.Request(SpriteData.GetBuildingSpriteAddresses(data.type, skinType, climate));
			uIRoundButton.buttonActive = buildCommand.IsValid(GameManager.GameState);
			uIRoundButton.buttonExpensive = !uIRoundButton.buttonActive;
			uIRoundButton.Cost = data.cost;
			if (data.cost <= 0)
			{
				uIRoundButton.Cost = -1f;
			}
			uIRoundButton.OnClicked += delegate
			{
				PopupManager.HideCurrentPopup();
				ClickedImprovement(buildCommand);
			};
		}
		foreach (ImprovementData improvement in GetSuggestedUnlockableImprovements(player))
		{
			UIRoundButton uIRoundButton2 = CreateRoundBottomBarButton(Localization.Get(improvement.displayName));
			uIRoundButton2.iconSpriteHandle.Request(SpriteData.GetBuildingSpriteAddresses(improvement.type, tile.SkinType, tile.Climate));
			uIRoundButton2.buttonActive = false;
			uIRoundButton2.OnClicked += delegate
			{
				PopupManager.HideCurrentPopup();
				OnUnlockableClicked(improvement, tile, player);
			};
		}
	}

	private List<ImprovementData> GetSuggestedUnlockableImprovements(PlayerState player)
	{
		List<ImprovementData> list = new List<ImprovementData>();
		List<ImprovementData> unlockableImprovements = GameManager.GameState.GameLogicData.GetUnlockableImprovements(player);
		if (unlockableImprovements != null && unlockableImprovements.Count > 0 && player.Id == GameManager.GameState.CurrentPlayer)
		{
			foreach (ImprovementData item in unlockableImprovements)
			{
				if (item.shouldSuggestUnlock && GameManager.GameState.GameLogicData.CanBuild(GameManager.GameState, tile.Data, player, item))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	private bool ShouldTileShowBottomBar(Tile tile)
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		if ((Object)(object)tile == (Object)null || localPlayer == null)
		{
			return false;
		}
		if (mode == Mode.Unit && CommandUtils.GetUnitActions(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true).Count > 0)
		{
			return true;
		}
		if (!SettingsUtils.UseCompactUI || mode == Mode.Unit)
		{
			return false;
		}
		if (mode == Mode.Tile && (CommandUtils.GetBuildableImprovements(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true).Count > 0 || GetSuggestedUnlockableImprovements(localPlayer).Count > 0))
		{
			return true;
		}
		if (mode == Mode.Building)
		{
			if (CommandUtils.GetTrainableUnits(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true).Count <= 0)
			{
				return CommandUtils.GetImprovementAbilities(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true).Count > 0;
			}
			return true;
		}
		return false;
	}

	private void AddTrainUnitButtons(Tile tile)
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (localPlayer.AutoPlay)
		{
			return;
		}
		foreach (TrainCommand trainCommand in CommandUtils.GetTrainableUnits(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true))
		{
			GameManager.GameState.GameLogicData.TryGetData(trainCommand.Type, out var unit);
			UIRoundButton uIRoundButton = CreateRoundBottomBarButton(Localization.Get(unit.displayName));
			uIRoundButton.iconContent = UIUtils.GetUIUnitRenderer(unit, localPlayer).rectTransform;
			UIButtonBase.ButtonAction validationErrorCallback = GetValidationErrorCallback(trainCommand);
			uIRoundButton.buttonActive = trainCommand.IsValid(GameManager.GameState) && validationErrorCallback == null;
			if (CommandValidation.CanCitySupportUnit(GameManager.GameState, tile.Coordinates) && !localPlayer.CanAfford(unit))
			{
				uIRoundButton.buttonExpensive = true;
			}
			else
			{
				uIRoundButton.buttonExpensive = false;
			}
			uIRoundButton.Cost = unit.cost;
			UIButtonBase.ButtonAction buttonAction = delegate
			{
				PopupManager.HideCurrentPopup();
				ClickedSpawnUnit(trainCommand, unit);
			};
			uIRoundButton.OnClicked += validationErrorCallback ?? buttonAction;
		}
	}

	private UIButtonBase.ButtonAction GetValidationErrorCallback(TrainCommand command)
	{
		if (!command.IsValid(GameManager.GameState, out var validationError) && validationError == CommandBase.VALIDATION_ERROR_WITHIN_TEMPLE_RANGE)
		{
			return delegate
			{
				ShowValidationErrorPopup("actionbox.cantbuildagent.title", "actionbox.cantbuildagent.description");
			};
		}
		return null;
	}

	private void ShowValidationErrorPopup(string titleKey, string descriptionKey)
	{
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get(titleKey);
		basicPopup.Description = Localization.Get(descriptionKey);
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show();
	}

	private void AddUnitActionButtons(Tile tile)
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (localPlayer.AutoPlay)
		{
			return;
		}
		foreach (CommandBase command in CommandUtils.GetUnitActions(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true))
		{
			UIRoundButton uIRoundButton = CreateRoundBottomBarButton(command.GetShortInfo(), forceToBottom: true);
			uIRoundButton.sprite = UIManager.IconData.GetSprite(command.Id);
			if (!HandleButtonSpecialCases(uIRoundButton, command, localPlayer))
			{
				string displayName = unit.Data.displayName;
				if (unit.Data.IsVehicle() && GameManager.GameState.GameLogicData.TryGetData(unit.State.passengerUnit.type, out var data))
				{
					displayName = data.displayName;
				}
				uIRoundButton.text = string.Format(Localization.Get("action.info." + command.Id), Localization.Get(displayName));
			}
			if (command.GetCommandType() == CommandType.Upgrade)
			{
				GameManager.GameState.GameLogicData.TryGetData((command as UpgradeCommand).Type, out var upgrade);
				uIRoundButton.text = string.Format(Localization.Get("action.info." + command.Id), Localization.Get(upgrade.displayName));
				uIRoundButton.iconContent = UIUtils.GetUIUnitRenderer(upgrade, localPlayer).rectTransform;
				uIRoundButton.buttonActive = localPlayer.CanAfford(upgrade);
				uIRoundButton.buttonExpensive = !localPlayer.CanAfford(upgrade);
				uIRoundButton.Cost = upgrade.cost;
				uIRoundButton.OnClicked += delegate
				{
					PopupManager.HideCurrentPopup();
					_ClickedUpgradeUnit(command as UpgradeCommand, upgrade);
				};
			}
			else if (command.IsValid(GameManager.GameState))
			{
				uIRoundButton.OnClicked += delegate
				{
					ClickedAbility(command, canShowOverPopup: false);
				};
				uIRoundButton.buttonActive = true;
			}
			else
			{
				uIRoundButton.OnClicked += delegate
				{
					ShowCantPerformActionPopup(command);
				};
				uIRoundButton.buttonActive = false;
				uIRoundButton.buttonExpensive = false;
			}
		}
	}

	private void AddAbilityButtons(Tile tile)
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (localPlayer.AutoPlay)
		{
			return;
		}
		foreach (CommandBase command in CommandUtils.GetImprovementAbilities(GameManager.GameState, localPlayer, tile.Data, includeUnavailable: true))
		{
			UIRoundButton uIRoundButton = CreateRoundBottomBarButton(Localization.Get("action.info." + command.Id));
			uIRoundButton.sprite = UIManager.IconData.GetSprite(command.Id);
			HandleButtonSpecialCases(uIRoundButton, command, localPlayer);
			if (command.IsValid(GameManager.GameState))
			{
				uIRoundButton.OnClicked += delegate
				{
					ClickedAbility(command, canShowOverPopup: false);
				};
			}
		}
	}

	private static bool HandleButtonSpecialCases(UIRoundButton button, CommandBase command, PlayerState player)
	{
		if (command.GetCommandType() == CommandType.Destroy)
		{
			button.text = string.Format("{0} {1}", Localization.Get(command.Id), Localization.Get("actionbtn.remove.building"));
		}
		else
		{
			if (command.GetCommandType() == CommandType.Upgrade)
			{
				GameManager.GameState.GameLogicData.TryGetData((command as UpgradeCommand).Type, out var upgrade);
				button.text = Localization.Get(upgrade.displayName);
				button.iconContent = UIUtils.GetUIUnitRenderer(upgrade, player).rectTransform;
				button.buttonActive = player.CanAfford(upgrade);
				button.buttonExpensive = !player.CanAfford(upgrade);
				button.Cost = upgrade.cost;
				InteractionBar interactionBar = (UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).InteractionBar;
				button.OnClicked += delegate
				{
					PopupManager.HideCurrentPopup();
					interactionBar.ClickedSpawnUnit(command, upgrade);
				};
				return true;
			}
			if (command.GetCommandType() == CommandType.Clone)
			{
				GameManager.GameState.GameLogicData.TryGetData((command as CloneCommand).Type, out var clone);
				button.text = "Clone " + Localization.Get(clone.displayName);
				button.iconContent = UIUtils.GetUIUnitRenderer(clone, player).rectTransform;
				button.buttonActive = player.CanAfford(clone);
				button.buttonExpensive = !player.CanAfford(clone);
				button.Cost = CloneCommand.CLONE_COST;
				InteractionBar interactionBar2 = (UIManager.Instance.GetScreen(UIConstants.Screens.Hud) as HudScreen).InteractionBar;
				button.OnClicked += delegate
				{
					PopupManager.HideCurrentPopup();
					interactionBar2.ClickedSpawnUnit(command, clone);
				};
				return true;
			}
		}
		return false;
	}

	private UnitPopup GetPopulatedUnitPopup(UnitData unit)
	{
		UnitPopup unitPopup = PopupManager.GetUnitPopup();
		unitPopup.Player = GameManager.LocalPlayer;
		unitPopup.UnitData = unit;
		unitPopup.Description = Localization.Get("actionbox.unit.new", Localization.Get(unitPopup.Player.GetLocalizedTribeName(GameManager.GameState)), Localization.Get(unit.displayName));
		unitPopup.cost = unit.cost;
		return unitPopup;
	}

	private void _ClickedUpgradeUnit(CommandBase command, UnitData unit)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (SettingsUtils.InfoOnBuild)
		{
			if (!PopupManager.PopupShowing)
			{
				UnitPopup populatedUnitPopup = GetPopulatedUnitPopup(unit);
				PopupBase.PopupButtonData okButton = new PopupBase.PopupButtonData("actionbox.unit.upgrade", PopupBase.PopupButtonData.States.Selected, delegate
				{
					OnPopupAccepted(command);
				}, 0);
				okButton.stateCheck = delegate
				{
					okButton.state = (ClientActionManager.CanExecuteCommand(command, GameManager.GameState) ? PopupBase.PopupButtonData.States.Selected : PopupBase.PopupButtonData.States.Disabled);
				};
				populatedUnitPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					okButton
				};
				populatedUnitPopup.RefreshButtonState();
				activePopup = populatedUnitPopup;
				populatedUnitPopup.Show(InputManager.GetInputPosition());
			}
		}
		else if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
		{
			OnPopupAccepted(command);
		}
	}

	private void ClickedSpawnUnit(CommandBase command, UnitData unit)
	{
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		GameState gameState = GameManager.GameState;
		if (SettingsUtils.InfoOnBuild)
		{
			if (!PopupManager.PopupShowing)
			{
				UnitPopup populatedUnitPopup = GetPopulatedUnitPopup(unit);
				if (!CommandValidation.CanCitySupportUnit(gameState, tile.Coordinates))
				{
					populatedUnitPopup.Description = populatedUnitPopup.Description + "\n" + Localization.Get("tooltip.tile.limit");
				}
				PopupBase.PopupButtonData okButton = new PopupBase.PopupButtonData("actionbox.unit.train", PopupBase.PopupButtonData.States.Selected, delegate
				{
					OnPopupAccepted(command);
				}, 0);
				okButton.stateCheck = delegate
				{
					okButton.state = (ClientActionManager.CanExecuteCommand(command, gameState) ? PopupBase.PopupButtonData.States.Selected : PopupBase.PopupButtonData.States.Disabled);
				};
				populatedUnitPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					okButton
				};
				populatedUnitPopup.RefreshButtonState();
				activePopup = populatedUnitPopup;
				populatedUnitPopup.Show(InputManager.GetInputPosition());
			}
		}
		else if (ClientActionManager.CanExecuteCommand(command, gameState))
		{
			OnPopupAccepted(command);
		}
	}

	private void ClickedImprovement(BuildCommand command)
	{
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		GameManager.GameState.GameLogicData.TryGetData(command.Type, out var data);
		if (SettingsUtils.InfoOnBuild)
		{
			if (!PopupManager.PopupShowing)
			{
				Tile tileInstance = MapRenderer.Current.GetTileInstance(coordinates);
				ImprovementData improvementData = data;
				IconPopup iconPopup = PopupManager.GetIconPopup();
				iconPopup.Header = Localization.Get(improvementData.displayName);
				iconPopup.Description = BuildingUtils.GetInfo(improvementData, tileInstance.Improvement, tileInstance.Owner, tileInstance);
				iconPopup.cost = improvementData.cost;
				PopupBase.PopupButtonData okButton = new PopupBase.PopupButtonData("actionbox.building.doit", PopupBase.PopupButtonData.States.Selected, delegate
				{
					OnPopupAccepted(command);
				}, 0);
				okButton.stateCheck = delegate
				{
					okButton.state = (ClientActionManager.CanExecuteCommand(command, GameManager.GameState) ? PopupBase.PopupButtonData.States.Selected : PopupBase.PopupButtonData.States.Disabled);
				};
				iconPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					okButton
				};
				UIBuildingRenderer instance = UIBuildingRenderer.GetInstance();
				int climate = (data.type.IsMonument() ? GameManager.LocalPlayer.GetTribeStyle(GameManager.GameState) : tileInstance.Data.climate);
				SkinType skinType = (data.type.IsMonument() ? GameManager.LocalPlayer.skinType : tileInstance.Data.skinType);
				instance.SetBuildingData(improvementData, skinType, climate);
				((Component)instance).transform.SetParent((Transform)(object)iconPopup.iconContainer, false);
				UIUtils.FitImageContentInParent(instance.rectTransform);
				iconPopup.RefreshButtonState();
				activePopup = iconPopup;
				iconPopup.Show(InputManager.GetInputPosition());
			}
		}
		else if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
		{
			OnPopupAccepted(command);
		}
	}

	private void ClickedAbility(CommandBase command, bool canShowOverPopup)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if ((SettingsUtils.InfoOnBuild && command.ShouldAskForConfirmation()) || command.ShouldAlwaysAskForConfirmation())
		{
			if (!PopupManager.PopupShowing || canShowOverPopup)
			{
				BasicPopup basicPopup = PopupManager.GetBasicPopup();
				basicPopup.Header = Localization.Get("actionbox.confirm", command.GetShortInfo());
				basicPopup.Description = command.GetConfirmInfo();
				basicPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData("actionbox.building.doit", PopupBase.PopupButtonData.States.Selected, delegate
					{
						ExecuteCommand(command);
					}, 0)
				};
				basicPopup.Show(InputManager.GetInputPosition());
			}
		}
		else
		{
			ExecuteCommand(command);
		}
		void ExecuteCommand(CommandBase command2)
		{
			if (ClientActionManager.CanExecuteCommand(command2, GameManager.GameState))
			{
				InputEvents.SelectionCleared();
				GameManager.Client.SendCommand(command2);
			}
			else
			{
				ShowCantPerformActionPopup(command2);
			}
		}
	}

	private void ShowCantPerformActionPopup(CommandBase command, string customDescriptionKey = "")
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get(command.GetShortInfo());
		string key = (string.IsNullOrEmpty(customDescriptionKey) ? "actionbox.cantperformaction.description" : customDescriptionKey);
		basicPopup.Description = Localization.Get(key);
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	private void OnUnlockableClicked(ImprovementData improvement, Tile tile, PlayerState player)
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		GameState gameState = GameManager.GameState;
		string text = "";
		if (!gameState.GameLogicData.IsUnlocked(improvement.type, player))
		{
			text = " " + Localization.Get("tooltip.tile.extract.research", Localization.Get(gameState.GameLogicData.GetRequiredTech(player.GetTribeData(gameState), improvement.type).displayName));
		}
		IconPopup iconPopup = PopupManager.GetIconPopup();
		iconPopup.Header = Localization.Get(improvement.displayName);
		iconPopup.Description = BuildingUtils.GetInfo(improvement) + text;
		iconPopup.cost = improvement.cost;
		iconPopup.buttonData = new PopupBase.PopupButtonData[2]
		{
			new PopupBase.PopupButtonData("buttons.back"),
			new PopupBase.PopupButtonData("actionbox.building.techtree", PopupBase.PopupButtonData.States.Selected, OnUnlockablePopupAccepted, 0)
		};
		UIBuildingRenderer instance = UIBuildingRenderer.GetInstance();
		instance.SetBuildingData(improvement, tile.SkinType, tile.Climate);
		((Component)instance).transform.SetParent((Transform)(object)iconPopup.iconContainer, false);
		UIUtils.FitImageContentInParent(instance.rectTransform);
		iconPopup.Show(InputManager.GetInputPosition());
		void OnUnlockablePopupAccepted(int id, BaseEventData eventData)
		{
			InputEvents.SelectionCleared();
			UIManager.Instance.ShowScreen(UIConstants.Screens.TechTree);
			Hide();
		}
	}

	private void OnPopupAccepted(CommandBase command)
	{
		InputEvents.SelectionCleared();
		if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
		{
			GameManager.Client.SendCommand(command);
		}
		Hide();
	}

	private void OnPopupAccepted(BuildCommand command)
	{
		InputEvents.SelectionCleared();
		if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
		{
			GameManager.Client.SendCommand(command);
		}
		Hide();
	}

	public void OnInfoButtonClicked()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)infoIcon).gameObject.activeSelf)
		{
			return;
		}
		PopupManager.HideCurrentPopup();
		if (unitPopupData != null)
		{
			unitPopupData.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			PopupManager.GetUnitPopup(unitPopupData).Show(InputManager.GetInputPosition());
			AudioManager.PlaySFX(SFXTypes.Press, 1f, 1f, AudioManager.GetPanFromPosition(Vector2.op_Implicit(((Component)this).transform.position)));
		}
		else if (iconPopupData != null)
		{
			if ((Object)(object)building != (Object)null)
			{
				UIBuildingRenderer instance = UIBuildingRenderer.GetInstance();
				instance.SetBuilding(building);
				((Transform)instance.rectTransform).SetParent((Transform)(object)RectTransform, false);
				((Component)instance).gameObject.SetActive(false);
				iconPopupData.iconContent = ((Component)instance).gameObject;
			}
			iconPopupData.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			PopupManager.GetIconPopup(iconPopupData).Show(InputManager.GetInputPosition());
			AudioManager.PlaySFX(SFXTypes.Press, 1f, 1f, AudioManager.GetPanFromPosition(Vector2.op_Implicit(((Component)this).transform.position)));
		}
	}

	private string GetTileTip(Tile tile, byte playerId)
	{
		string text = string.Empty;
		if ((Object)(object)tile == (Object)null)
		{
			return text;
		}
		GameManager.GameState.TryGetPlayer(playerId, out var playerState);
		if (tile.Data.GetExplored(playerId))
		{
			if ((Object)(object)tile.Improvement != (Object)null)
			{
				if (tile.Improvement.Data.type == ImprovementData.Type.City)
				{
					if (tile.Data.improvement.HasEffect(ImprovementEffect.robbed))
					{
						text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.riot"));
					}
					else if (!tile.OwnedBy(playerId))
					{
						text = ((!((Object)(object)tile.Unit != (Object)null) || (tile.Owner != null && (tile.Owner == null || tile.Owner.Id == tile.Unit.Owner.Id)) || tile.Unit.Data.HasAbility(UnitAbility.Type.Fly)) ? string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.capture.tip")) : string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.capture")));
					}
					else
					{
						bool flag = CommandValidation.CanCitySupportUnit(GameManager.GameState, tile.Coordinates);
						if (!flag)
						{
							text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.limit"));
						}
						else if (!tile.Data.IsConnected)
						{
							text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.road"));
						}
						else if ((Object)(object)tile.Unit == (Object)null && !playerState.blockTrainUnits && flag)
						{
							text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.choose_unit"));
						}
						if ((Object)(object)tile.Unit != (Object)null && tile.Owner.Id != playerId && !tile.Unit.Data.HasAbility(UnitAbility.Type.Fly))
						{
							text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.capture.enemy"));
						}
					}
				}
				else if (tile.Improvement.Level != 0 || !BuildingUtils.IsStarProductionType(tile.Improvement.Data.type))
				{
					int amount = tile.Data.CalculateWork(GameManager.GameState);
					if (tile.Improvement.Data.work > 0)
					{
						string resourceEntryText = ResourceUtils.GetResourceEntryText(ResourceManager.Type.Currency, amount);
						text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.produces", resourceEntryText));
					}
				}
				bool flag2 = false;
				if (tile.Improvement.Data.adjacencyImprovements != null && tile.Improvement.Data.adjacencyImprovements.Count > 0)
				{
					foreach (AdjacencyImprovements adjacencyImprovement in tile.Improvement.Data.adjacencyImprovements)
					{
						if (adjacencyImprovement.improvement != null && adjacencyImprovement.improvement.type == ImprovementData.Type.PolarisClimate)
						{
							flag2 = true;
							break;
						}
					}
				}
				if (flag2)
				{
					if (tile.Improvement.Level == 0)
					{
						int num = GameManager.GameState.CountIceTiles();
						int num2 = BuildingUtils.ICEBANK_TILES - (num - tile.Improvement.Level * BuildingUtils.ICEBANK_TILES);
						text = string.Format("{0} ", Localization.Get("tooltip.tile.level.polaris", num2));
					}
					else if (tile.Improvement.Level < tile.Improvement.MaxLevel)
					{
						int num3 = GameManager.GameState.CountIceTiles();
						int num4 = BuildingUtils.ICEBANK_TILES - (num3 - tile.Improvement.Level * BuildingUtils.ICEBANK_TILES);
						text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.level.polaris", num4));
					}
					else
					{
						text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.level.max"));
					}
				}
				if (tile.Improvement.Data.HasAbility(ImprovementAbility.Type.Patina))
				{
					if (tile.Improvement.Level < tile.Improvement.MaxLevel)
					{
						int num5 = tile.Improvement.Data.growthRate - tile.Improvement.State.GetAge(GameManager.GameState) % tile.Improvement.Data.growthRate;
						text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.level.next", num5));
					}
					else
					{
						text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.level.max"));
					}
				}
				if (tile.Improvement.Data.IsType(ImprovementData.Type.Port) && tile.Owner.Id == playerId)
				{
					text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.sailing"));
				}
				if (tile.Improvement.State.IsMonument() && tile.Owner.Id == playerId)
				{
					text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.monuments"));
				}
				if (tile.Improvement.Data.IsType(ImprovementData.Type.Ruin))
				{
					text = string.Format("{0}{1} ", text, Localization.Get("tooltip.tile.ruin"));
				}
				if (building.State.HasEffect(ImprovementEffect.decomposing))
				{
					text = Localization.Get("actionbox.building.decomposing.info");
				}
			}
			else
			{
				ResourceState resource = tile.Data.GetResource(GameManager.GameState, playerId);
				if (resource != null && tile.Resource.IsVisibleToPlayer)
				{
					ImprovementData improvementForResource = GameManager.GameState.GameLogicData.GetImprovementForResource(resource.type);
					if (tile.OwnedBy(playerId))
					{
						UnitState unitState = tile.Data.GetUnit(GameManager.GameState, playerId);
						text = ((unitState != null && unitState.owner != playerId && !playerState.HasPeaceWith(unitState.owner)) ? Localization.Get("tooltip.tile.blocked") : ((!GameManager.GameState.GameLogicData.IsUnlocked(improvementForResource.type, playerState)) ? Localization.Get("tooltip.tile.extract.research", Localization.Get(GameManager.GameState.GameLogicData.GetRequiredTech(playerState.GetTribeData(GameManager.GameState), improvementForResource.type).displayName)) : ((improvementForResource.GetPopulationReward() != 0) ? Localization.Get("tooltip.tile.extract.upgrade") : ((!improvementForResource.CanCreateUnit()) ? Localization.Get("tooltip.tile.extract.stars") : Localization.Get("tooltip.tile.extract.convert")))));
					}
					else
					{
						text = Localization.Get("tooltip.tile.outside");
					}
				}
				else
				{
					TechData requiredTech = GameManager.GameState.GameLogicData.GetRequiredTech(playerState.GetTribeData(GameManager.GameState), tile.Terrain);
					if (!GameManager.GameState.GameLogicData.GetUnlockedMovements(playerState).Contains(tile.Terrain) && requiredTech != null)
					{
						text = Localization.Get("tooltip.tile.research", Localization.Get(requiredTech.displayName));
					}
				}
			}
		}
		else
		{
			text = Localization.Get("tooltip.tile.explore");
		}
		return text;
	}

	private string GetUnitTip(Unit unit, byte playerId)
	{
		string text = string.Empty;
		if ((Object)(object)unit == (Object)null)
		{
			return text;
		}
		GameManager.GameState.TryGetPlayer(playerId, out var playerState);
		if (unit.Owner.Id == playerId)
		{
			if (unit.State.CanCapture(GameManager.GameState, unit.Tile.Data, includeNextTurn: true))
			{
				text = ((unit.State.CanMove() && unit.State.CanAttack() && GameManager.IsPlayerViewing(playerId)) ? Localization.Get("tooltip.unit.city.capture") : Localization.Get("tooltip.unit.city.capture.next"));
			}
			else if (unit.State.IsDetectingHiddenUnits(GameManager.GameState))
			{
				text = Localization.Get("tooltip.unit.detect");
			}
			else if (!unit.State.CanMove() && !unit.CanAttackAnything())
			{
				text = Localization.Get("tooltip.unit.actions.none");
			}
			else if (GameManager.IsPlayerViewing(playerId) && GameManager.GameState.CurrentPlayer == playerId)
			{
				if (unit.CanMoveAnywhere())
				{
					text = Localization.Get("tooltip.unit.actions.move");
				}
				if (unit.CanAttackAnything())
				{
					text = Localization.Get("tooltip.unit.actions.attack");
				}
			}
		}
		else if (playerState.HasPeaceWith(unit.Owner.Id))
		{
			text = Localization.Get("tooltip.unit.ally");
		}
		else
		{
			text = Localization.Get("tooltip.unit.enemy");
			if (unit.Tile.OwnedBy(playerId))
			{
				text = Localization.Get("tooltip.unit.enemy.territory");
			}
			if (playerState.HasBrokenPeaceWith(unit.Owner.Id))
			{
				text = text + " " + Localization.Get("tooltip.unit.enemy.brokenpeace");
			}
		}
		if (unit.Owner.Id != playerId && unit.Tile.OwnedBy(playerId) && (Object)(object)unit.Tile.Improvement != (Object)null && unit.Tile.Improvement.Data.type == ImprovementData.Type.City)
		{
			text = Localization.Get("tooltip.unit.enemy.city");
		}
		int age = (int)unit.State.GetAge(GameManager.GameState);
		if (unit.Data.HasAbility(UnitAbility.Type.Grow))
		{
			List<UnitData> unlockedUpgradesForUnit = GameManager.GameState.GameLogicData.GetUnlockedUpgradesForUnit(playerState, GameManager.GameState, unit.Data);
			if (unlockedUpgradesForUnit.Count > 0)
			{
				text = ((age != unit.Data.growthRate) ? string.Format("{0} {1}", Localization.Get("tooltip.unit.grow.later", Localization.Get(unlockedUpgradesForUnit[0].displayName), unit.Data.growthRate - age), text) : string.Format("{0} {1}", Localization.Get("tooltip.unit.grow.now", Localization.Get(unlockedUpgradesForUnit[0].displayName)), text));
			}
		}
		return text;
	}

	public float GetTop()
	{
		return ((!ShouldTileShowBottomBar(tile)) ? ((type == Type.PcBar) ? (-74f + ButtonBarOffset) : (-110f)) : ((type == Type.PcBar) ? (26f + ButtonBarOffset) : 0f)) + base.rectTransform.GetHeight();
	}
}
