using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitPopup : IconPopup
{
	[Header("Unit Popup")]
	[SerializeField]
	protected RectTransform abilityButtonContainer;

	[SerializeField]
	protected RectTransform statsList;

	[Header("Prefabs")]
	[SerializeField]
	protected UITextButton smallTextButtonPrefab;

	[SerializeField]
	protected UnitStatsRow statsRowPrefab;

	[Header("Ability Button Styles")]
	[SerializeField]
	protected UIButtonBase.ColorStates bgColors;

	[SerializeField]
	protected UIButtonBase.ColorStates labelColors;

	protected UnitData data;

	protected Unit unit;

	protected PlayerState player;

	protected UIUnitRenderer unitRenderer;

	protected List<UITextButton> abilityButtons = new List<UITextButton>();

	protected List<UnitStatsRow> statsRows = new List<UnitStatsRow>();

	protected float defenceBonus = 1f;

	protected float attackBonus;

	protected float moveBonus;

	public PlayerState Player
	{
		get
		{
			if (player == null)
			{
				if (!((Object)(object)Unit != (Object)null))
				{
					return GameManager.LocalPlayer;
				}
				return Unit.Owner;
			}
			return player;
		}
		set
		{
			player = value;
		}
	}

	public Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			if ((Object)(object)unit == (Object)null)
			{
				attackBonus = 0f;
				defenceBonus = 0f;
				moveBonus = 0f;
			}
			else
			{
				defenceBonus = (float)unit.State.GetDefenceBonus(GameManager.GameState) * 0.1f;
				attackBonus = (float)(unit.State.GetAttack(GameManager.GameState) - unit.Data.GetAttack()) * 0.1f;
				moveBonus = unit.State.GetMovement(GameManager.GameState) - unit.Data.GetMovement();
				UnitData = unit.Data;
			}
		}
	}

	public UnitData UnitData
	{
		set
		{
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			data = value;
			if (data == null)
			{
				return;
			}
			Header = Localization.Get(data.displayName);
			base.cost = (data.hidden ? (-1) : data.cost);
			if ((Object)(object)unit != (Object)null)
			{
				unitRenderer = UIUtils.GetUIUnitRenderer(unit, Player);
			}
			else
			{
				unitRenderer = UIUtils.GetUIUnitRenderer(data, Player);
			}
			((Transform)unitRenderer.rectTransform).SetParent((Transform)(object)base.iconContainer, false);
			UIUtils.FitImageContentInParent(unitRenderer.rectTransform);
			Vector2 anchoredPosition = base.iconContainer.anchoredPosition;
			string text = string.Empty;
			if ((Object)(object)Unit != (Object)null && GameManager.GameState != null && GameManager.GameState.Map != null)
			{
				TileData tile = GameManager.GameState.Map.GetTile(Unit.State.home);
				if (tile != null && tile.HasImprovement(ImprovementData.Type.City))
				{
					text = tile.improvement.name;
				}
			}
			string arg = (string.IsNullOrEmpty(text) ? string.Empty : string.Format("{0}\n", Localization.Get("world.unit.info.from", text)));
			if (UIManager.Instance.CurrentScreen != UIConstants.Screens.TechTree && data.promotionLimit > 0)
			{
				int num = (Object.op_Implicit((Object)(object)Unit) ? Unit.State.xp : 0);
				string arg2 = ((num < data.promotionLimit) ? Localization.Get("world.unit.veteran.progress", num.ToString(), data.promotionLimit.ToString()) : Localization.Get("world.unit.veteran"));
				Description = $"{arg}{arg2}";
				anchoredPosition.x = 48f;
			}
			else
			{
				Description = arg;
				anchoredPosition.x = base.rectTransform.sizeDelta.x * 0.5f;
			}
			((Component)description).gameObject.SetActive(!string.IsNullOrEmpty(Description));
			base.iconContainer.anchoredPosition = anchoredPosition;
			AddAbilityButtons(data.unitAbilities);
			if ((Object)(object)Unit != (Object)null || data.health > 0)
			{
				int num2 = (Object.op_Implicit((Object)(object)Unit) ? Unit.Health : data.health);
				int num3 = ((Object.op_Implicit((Object)(object)Unit) && GameManager.GameState != null) ? unit.State.GetMaxHealth(GameManager.GameState) : data.health);
				AddStatsRow("world.unit.health", $"{Mathf.Round((float)num2 * 0.1f)}/{Mathf.Round((float)num3 * 0.1f)}");
			}
			string value2 = ((attackBonus > 0f) ? $"{(float)data.attack * 0.1f} (+{attackBonus * 0.1f})" : ((float)data.attack * 0.1f).ToString());
			AddStatsRow("world.unit.attack", value2);
			string value3 = ((defenceBonus != 1f) ? $"{(float)data.defence * 0.1f} (x{defenceBonus})" : ((float)data.defence * 0.1f).ToString());
			Log.Verbose($"UnitPopup :: defenceBonus: {defenceBonus}", Array.Empty<object>());
			AddStatsRow("world.unit.defence", value3);
			string value4 = ((moveBonus > 0f) ? $"{data.movement} (+{moveBonus})" : data.movement.ToString());
			AddStatsRow("world.unit.movement", value4);
			AddStatsRow("world.unit.range", data.range);
		}
	}

	protected void AddAbilityButtons(List<UnitAbility.Type> abilities)
	{
		if (abilities == null)
		{
			((Component)abilityButtonContainer).gameObject.SetActive(false);
		}
		int count = abilities.Count;
		for (int i = 0; i < count; i++)
		{
			UITextButton abilityButton = GetAbilityButton();
			abilityButton.Key = "unit.abilities." + abilities[i].GetName();
			abilityButton.id = i;
		}
		((Component)abilityButtonContainer).gameObject.SetActive(count > 0);
	}

	protected UITextButton GetAbilityButton()
	{
		if (abilityButtons.Count > 0)
		{
			foreach (UITextButton abilityButton in abilityButtons)
			{
				if (!((Component)abilityButton).gameObject.activeSelf)
				{
					((Component)abilityButton).gameObject.SetActive(true);
					return abilityButton;
				}
			}
		}
		UITextButton uITextButton = Object.Instantiate<UITextButton>(smallTextButtonPrefab, (Transform)(object)abilityButtonContainer);
		uITextButton.BgColorStates.CopyFrom(bgColors);
		uITextButton.LabelColorStates.CopyFrom(labelColors);
		uITextButton.OnClicked += OnClickedAbility;
		abilityButtons.Add(uITextButton);
		return uITextButton;
	}

	private void OnClickedAbility(int id, BaseEventData eventData)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("unit.abilities." + data.unitAbilities[id].GetName());
		basicPopup.Description = Localization.Get("tooltip.ability." + data.unitAbilities[id].GetName());
		basicPopup.buttonData = new PopupButtonData[1]
		{
			new PopupButtonData("buttons.back", PopupButtonData.States.Selected)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}

	protected void AddStatsRow(string name, float value)
	{
		AddStatsRow(name, value.ToString());
	}

	protected void AddStatsRow(string name, string value)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		UnitStatsRow statsRow = GetStatsRow();
		statsRow.SetData(name, value);
		statsRow.UpdateSize();
		statsList.sizeDelta = new Vector2(statsList.sizeDelta.x, statsList.sizeDelta.y + statsRow.rectTransform.sizeDelta.y);
	}

	protected UnitStatsRow GetStatsRow()
	{
		if (statsRows.Count > 0)
		{
			foreach (UnitStatsRow statsRow in statsRows)
			{
				if (!((Component)statsRow).gameObject.activeSelf)
				{
					((Component)statsRow).gameObject.SetActive(true);
					return statsRow;
				}
			}
		}
		UnitStatsRow unitStatsRow = Object.Instantiate<UnitStatsRow>(statsRowPrefab, (Transform)(object)statsList);
		statsRows.Add(unitStatsRow);
		return unitStatsRow;
	}

	public override void ResetPopup()
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		base.ResetPopup();
		if ((Object)(object)unitRenderer != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)unitRenderer).gameObject);
		}
		foreach (UITextButton abilityButton in abilityButtons)
		{
			((Component)abilityButton).gameObject.SetActive(false);
		}
		foreach (UnitStatsRow statsRow in statsRows)
		{
			((Component)statsRow).gameObject.SetActive(false);
		}
		statsList.sizeDelta = new Vector2(statsList.sizeDelta.x, 0f);
		Player = null;
		Unit = null;
		UnitData = null;
		defenceBonus = 1f;
	}
}
