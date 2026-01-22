using System;
using System.Collections.Generic;
using Polytopia.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechView : UIScreenBase
{
	[Header("Tech View")]
	public TextMeshProUGUI infoText;

	public TechItem techItemPrefab;

	[Header("Containers")]
	public RectTransform techTreeContainer;

	public RectTransform lineContainer;

	public RectTransform nodeContainer;

	public static float NODE_SPACE = 140f;

	public static float NODE_DIAMETER = 120f;

	private Vector2 scrollMargin = new Vector2(240f, 400f);

	private Bounds treeRect;

	private List<TechItem> items = new List<TechItem>();

	private int currTechIdx;

	private byte lastPlayer;

	private bool isDirty;

	public event Action<TechView> OnItemsRefreshed;

	private void OnEnable()
	{
		ResourceEvents.OnResourceChanged += ResourceEvents_OnResourceChanged;
		TechEvents.OnRefreshAllTech += TechEvents_OnRefreshAllTech;
		TechEvents.OnTechCompleted += TechEvents_OnTechCompleted;
		RefreshTechItems();
		GameEvents.OnTurnEnded += OnTurnEnded;
	}

	private void OnDisable()
	{
		ResourceEvents.OnResourceChanged -= ResourceEvents_OnResourceChanged;
		TechEvents.OnRefreshAllTech -= TechEvents_OnRefreshAllTech;
		TechEvents.OnTechCompleted -= TechEvents_OnTechCompleted;
		GameEvents.OnTurnEnded -= OnTurnEnded;
	}

	private void OnTurnEnded()
	{
		SetDirty();
		UpdateTechView();
	}

	public void SetDirty()
	{
		isDirty = true;
	}

	public override void Show(bool instant = false)
	{
		((Component)techTreeContainer).gameObject.SetActive(true);
		UIManager.Instance.BlockHints();
		base.Show(instant);
		UpdateTechView();
		CameraController.Instance.SetTechBoundsState();
		GameManager.GetAnalyticsManager().SendEvent("tech_tree_view", new Dictionary<string, object> { 
		{
			"game_id",
			GameManager.Client.CurrentGameId
		} });
	}

	public override void Hide(bool instant = false)
	{
		if (showState == ShowStates.Showing)
		{
			PopupManager.HideCurrentPopup();
			CameraController.Instance.SetWorldCameraBounds();
		}
		base.Hide(instant);
		UIManager.Instance.UnblockHints();
	}

	private void UpdateTechView()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (lastPlayer != GameManager.LocalPlayer.Id || isDirty)
		{
			isDirty = false;
			lastPlayer = GameManager.LocalPlayer.Id;
			currTechIdx = 0;
			if (GameManager.GameState.GameLogicData.TryGetData(TechData.Type.Basic, out var data))
			{
				data = GameManager.GameState.GameLogicData.GetOverride(data, GameManager.LocalPlayer.GetTribeData(GameManager.GameState));
				TechItem techItem = CreateTechItem(data);
				((Selectable)techItem.button.button).navigation = default(Navigation);
				currTechIdx++;
				CreateNode(data, techItem);
				UIUtils.SetExplicitNavigation(nodeContainer, useCenter: true, 2);
			}
			UpdateTreeSize();
		}
		UpdateInfoText();
	}

	public void RefreshTechItems()
	{
		for (int i = 0; i < items.Count; i++)
		{
			items[i].RefreshState();
		}
		this.OnItemsRefreshed?.Invoke(this);
	}

	public void ForceBlockTechnologies(List<TechData.Type> exceptionsList)
	{
		for (int i = 0; i < items.Count; i++)
		{
			TechItem techItem = items[i];
			if (!exceptionsList.Contains(techItem.TechData.type))
			{
				techItem.RefreshState(forceUnavaliable: true);
			}
		}
	}

	public override void OnBack()
	{
		PopupManager.HideCurrentPopup();
		base.OnBack();
		CameraController.Instance.SetWorldCameraBounds();
	}

	private void UpdateInfoText()
	{
		if (GameManager.LocalPlayer != null && GameManager.GameState != null && GameManager.LocalPlayer.HasAbility(PlayerAbility.Type.Literacy, GameManager.GameState))
		{
			((TMP_Text)infoText).text = string.Format("{0}\n{1}", Localization.Get("techview.info"), string.Format(Localization.Get("techview.info.literacy"), GameConstants.GetLiteracyCostReductionAsString()));
		}
		else
		{
			((TMP_Text)infoText).text = Localization.Get("techview.info");
		}
	}

	private void CreateNode(TechData data, TechItem parentItem = null, float angle = 0f)
	{
		float num = 72f;
		float num2 = 0f;
		if ((Object)(object)parentItem != (Object)null)
		{
			num2 = angle + num * (float)(data.techUnlocks.Count - 1) / 2f;
		}
		foreach (TechData techUnlock in data.techUnlocks)
		{
			if (!GameManager.GameState.GameLogicData.TryGetData(techUnlock.type, out var _))
			{
				continue;
			}
			TechData techData = GameManager.GameState.GameLogicData.GetOverride(techUnlock, GameManager.LocalPlayer.GetTribeData(GameManager.GameState));
			if (techUnlock != null)
			{
				TechItem parentItem2 = CreateTechItem(techData, parentItem, num2);
				currTechIdx++;
				if (techData.techUnlocks != null && techData.techUnlocks.Count > 0)
				{
					CreateNode(techData, parentItem2, num2);
				}
				num2 -= num;
			}
		}
		this.OnItemsRefreshed?.Invoke(this);
	}

	private TechItem CreateTechItem(TechData data, TechItem parentItem = null, float angle = 0f)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		bool foundCachedItem = false;
		TechItem techItem = GetTechItem(currTechIdx, out foundCachedItem);
		if (!foundCachedItem)
		{
			RectTransform component = ((Component)techItem).GetComponent<RectTransform>();
			if ((Object)(object)parentItem != (Object)null)
			{
				component.anchoredPosition = parentItem.rectTransform.anchoredPosition + GetPosFromAngle(angle, NODE_SPACE);
			}
		}
		techItem.SetData(data);
		if (!foundCachedItem)
		{
			techItem.closeTechTreeCallback = OnBack;
			techItem.lineContainer = lineContainer;
			if ((Object)(object)parentItem != (Object)null)
			{
				parentItem.AddChildItem(techItem);
			}
			((Bounds)(ref treeRect)).Encapsulate(techItem.Bounds);
		}
		techItem.SetupComplete();
		techItem.RefreshState();
		return techItem;
	}

	private TechItem GetTechItem(int idx, out bool foundCachedItem)
	{
		if (items.Count > idx)
		{
			foundCachedItem = true;
			return items[idx];
		}
		foundCachedItem = false;
		TechItem techItem = Object.Instantiate<TechItem>(techItemPrefab, (Transform)(object)nodeContainer);
		techItem.RefreshState();
		items.Add(techItem);
		return techItem;
	}

	private void UpdateTreeSize()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		techTreeContainer.sizeDelta = new Vector2(((Bounds)(ref treeRect)).size.x, ((Bounds)(ref treeRect)).size.y) + scrollMargin;
		lineContainer.anchoredPosition = new Vector2(0f - ((Bounds)(ref treeRect)).center.x, 0f - ((Bounds)(ref treeRect)).center.y);
		nodeContainer.anchoredPosition = new Vector2(0f - ((Bounds)(ref treeRect)).center.x, 0f - ((Bounds)(ref treeRect)).center.y);
	}

	private Vector2 GetPosFromAngle(float angle, float distance)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		angle += 305f;
		float num = Mathf.Sin(angle / 57.29578f);
		float num2 = Mathf.Cos(angle / 57.29578f) * -1f;
		return new Vector2(num * distance, num2 * distance);
	}

	private void TechEvents_OnRefreshAllTech()
	{
		UpdateTechView();
	}

	private void TechEvents_OnTechCompleted(TechData tech)
	{
		UpdateInfoText();
	}

	private void ResourceEvents_OnResourceChanged(byte playerId)
	{
		if (playerId == GameManager.LocalPlayer.Id)
		{
			RefreshTechItems();
		}
	}
}
