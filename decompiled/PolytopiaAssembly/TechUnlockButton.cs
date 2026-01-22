using Polytopia.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TechUnlockButton : UIButtonBase
{
	[Header("Tech Unlock Button")]
	public RectTransform iconContainer;

	public TMPLocalizer label;

	protected PopupManager.BasicPopupData basicPopupData;

	protected PopupManager.IconPopupData iconPopupData;

	protected PopupManager.UnitPopupData unitPopupData;

	public void SetUnitData(UnitData data)
	{
		UIUnitRenderer uIUnitRenderer = UIUtils.GetUIUnitRenderer(data, GameManager.LocalPlayer);
		((Transform)uIUnitRenderer.rectTransform).SetParent((Transform)(object)iconContainer, false);
		UIUtils.FitImageContentInParent(uIUnitRenderer.rectTransform);
		unitPopupData = new PopupManager.UnitPopupData(data);
		label.Key = data.displayName;
	}

	public void SetBuildingData(ImprovementData data)
	{
		PlayerState localPlayer = GameManager.LocalPlayer;
		SpriteData.GetBuildingSprite(data, localPlayer.skinType, localPlayer.GetTribeClimate(GameManager.GameState), delegate(string atlasName, string spriteName, Sprite sprite)
		{
			Image image = UIUtils.GetImage(sprite);
			((Transform)((Graphic)image).rectTransform).SetParent((Transform)(object)iconContainer, false);
			UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
			int cost = -1;
			if (data.cost > 0)
			{
				cost = Mathf.RoundToInt((float)data.cost);
			}
			iconPopupData = new PopupManager.IconPopupData(Localization.Get(data.displayName), BuildingUtils.GetInfo(data), image.sprite, cost);
		});
		label.Key = data.displayName;
	}

	public void SetMovementData(TerrainData.Type terrain)
	{
		RectTransform tile = UIUtils.GetTile(terrain, GameManager.LocalPlayer.GetTribeData(GameManager.GameState).climate);
		((Transform)tile).SetParent((Transform)(object)iconContainer, false);
		UIUtils.FitImageContentInParent(tile);
		basicPopupData = new PopupManager.BasicPopupData(Localization.Get("technology.movement"), LocalizationUtils.CapitalizeString(Localization.Get("technology.movement.info", Localization.Get(terrain.GetDisplayName()))));
		if (GameManager.GameState.GameLogicData.TryGetData(terrain, out var _))
		{
			label.Key = "technology.movement";
		}
	}

	public void SetDefenceBonusData(TerrainData.Type data)
	{
		Image image = UIManager.IconData.GetImage("defenceBonus_" + data.GetName());
		((Transform)((Graphic)image).rectTransform).SetParent((Transform)(object)iconContainer, false);
		UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
		basicPopupData = new PopupManager.BasicPopupData(Localization.Get("technology.defence"), LocalizationUtils.CapitalizeString(Localization.Get("technology.defence.info", Localization.Get(data.GetDisplayName()))));
		label.Key = "technology.defence";
	}

	public void SetAbilityData(PlayerAbility.Type ability)
	{
		Image image = UIManager.IconData.GetImage(ability.GetName());
		((Transform)((Graphic)image).rectTransform).SetParent((Transform)(object)iconContainer, false);
		UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
		string text = Localization.Get(ability.GetDescription());
		if (ability == PlayerAbility.Type.Literacy)
		{
			text = string.Format(text, GameConstants.GetLiteracyCostReductionAsString());
		}
		basicPopupData = new PopupManager.BasicPopupData(Localization.Get(ability.GetDisplayName()), text);
		label.Key = ability.GetDisplayName();
	}

	public void SetAchiementData(TaskData taskData)
	{
		Image image = UIManager.IconData.GetImage("AchievementIcon");
		((Transform)((Graphic)image).rectTransform).SetParent((Transform)(object)iconContainer, false);
		UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
		ImprovementData improvementData = null;
		if (taskData.improvementUnlocks != null && taskData.improvementUnlocks.Count > 0)
		{
			improvementData = taskData.improvementUnlocks[0];
		}
		basicPopupData = new PopupManager.BasicPopupData(Localization.Get(taskData.displayName), Localization.Get("task.info", Localization.Get(taskData.description), (improvementData != null) ? Localization.Get(improvementData.displayName) : null));
		label.Key = taskData.displayName;
	}

	public override void PointerClick(PointerEventData eventData)
	{
		base.PointerClick(eventData);
		if (!m_blockClick && ButtonEnabled)
		{
			ShowInfoPopup();
		}
	}

	public override void OnSubmit(BaseEventData data)
	{
		base.OnSubmit(data);
		ShowInfoPopup();
	}

	protected void ShowInfoPopup()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (basicPopupData != null)
		{
			basicPopupData.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			PopupManager.GetBasicPopup(basicPopupData).Show(Vector2.op_Implicit(((Transform)base.rectTransform).position));
		}
		else if (iconPopupData != null)
		{
			iconPopupData.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			PopupManager.GetIconPopup(iconPopupData).Show(Vector2.op_Implicit(((Transform)base.rectTransform).position));
		}
		else if (unitPopupData != null)
		{
			unitPopupData.buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
			};
			PopupManager.GetUnitPopup(unitPopupData).Show(Vector2.op_Implicit(((Transform)base.rectTransform).position));
		}
	}
}
