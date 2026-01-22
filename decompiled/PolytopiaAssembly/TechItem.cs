using System;
using System.Collections;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TechItem : MonoBehaviour
{
	public enum State
	{
		None,
		Unavailable,
		Expensive,
		Available,
		Complete
	}

	public GameObject bgContainer;

	public Image bg;

	public Image shine;

	public Image emphase;

	public Image outline;

	public RectTransform iconContainer;

	public TMPLocalizer label;

	public ResourceWidget resourceWidget;

	public UIButtonBase button;

	[Header("Prefabs")]
	public GameObject linePrefab;

	[HideInInspector]
	public RectTransform lineContainer;

	[HideInInspector]
	public Action closeTechTreeCallback;

	private TechData data;

	private State state = State.Unavailable;

	private RectTransform RT;

	private TechItem m_parentItem;

	private List<TechItem> childItems = new List<TechItem>();

	private Image line;

	private TechPopup activePopup;

	private Image headImage;

	private List<GameObject> iconHolders = new List<GameObject>();

	[HideInInspector]
	public bool isBasicNode { get; private set; }

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)RT == (Object)null)
			{
				RT = ((Component)this).GetComponent<RectTransform>();
			}
			return RT;
		}
	}

	public TechItem Parent
	{
		get
		{
			return m_parentItem;
		}
		set
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			m_parentItem = value;
			GameObject obj = Object.Instantiate<GameObject>(linePrefab, (Transform)(object)lineContainer);
			Transform transform = obj.transform;
			obj.GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
			transform.localEulerAngles = new Vector3(0f, 0f, GetAngle(rectTransform.anchoredPosition, m_parentItem.rectTransform.anchoredPosition));
			obj.gameObject.SetActive(true);
			line = ((Component)transform.GetChild(0)).GetComponent<Image>();
			Vector2 sizeDelta = ((Graphic)line).rectTransform.sizeDelta;
			float num = (Parent.isBasicNode ? 50 : 0);
			sizeDelta.x = TechView.NODE_SPACE - TechView.NODE_DIAMETER + num + 4f;
			((Graphic)line).rectTransform.sizeDelta = sizeDelta;
		}
	}

	public Bounds Bounds => new Bounds(Vector2.op_Implicit(rectTransform.anchoredPosition), Vector2.op_Implicit(rectTransform.sizeDelta));

	public bool Complete
	{
		get
		{
			if (data != null)
			{
				return GameManager.GameState.GameLogicData.IsUnlocked(data.type, GameManager.LocalPlayer);
			}
			return false;
		}
	}

	public TechData TechData => data;

	private void Awake()
	{
		((Component)emphase).gameObject.SetActive(false);
		button.OnClicked += OnClicked;
	}

	public void SetData(TechData data)
	{
		this.data = data;
		label.Key = data.displayName;
		((Object)this).name = data.type.GetName();
		resourceWidget.Amount = GameManager.GameState.GameLogicData.GetTechPrice(this.data, GameManager.LocalPlayer, GameManager.GameState);
	}

	public void AddChildItem(TechItem item)
	{
		childItems.Add(item);
		item.Parent = this;
	}

	public void SetupComplete()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Parent == (Object)null)
		{
			isBasicNode = true;
			bgContainer.SetActive(false);
			((Component)label).gameObject.SetActive(false);
			if ((Object)(object)headImage == (Object)null)
			{
				headImage = UIUtils.GetImage(null);
				((Object)headImage).name = ((Object)this).name + " head image";
				Vector2 val = default(Vector2);
				((Vector2)(ref val))._002Ector(180f, 180f);
				headImage.preserveAspect = true;
				((Graphic)headImage).rectTransform.sizeDelta = val;
				((Transform)((Graphic)headImage).rectTransform).SetParent(((Component)this).transform, false);
				((Transform)((Graphic)headImage).rectTransform).localPosition = new Vector3(val.x, 0f - val.y) * 0.05f;
				((Graphic)headImage).raycastTarget = false;
			}
			button.hoverObject = ((Graphic)headImage).rectTransform;
		}
		else
		{
			if (data == null)
			{
				return;
			}
			ClearIconHolders();
			RectTransform[] unlockItems = GetUnlockItems(data, GameManager.LocalPlayer);
			if (unlockItems.Length != 0)
			{
				float num = Mathf.Min((float)((100 - (5 * unlockItems.Length - 1)) / unlockItems.Length), iconContainer.sizeDelta.y);
				RectTransform[] array = unlockItems;
				foreach (RectTransform obj in array)
				{
					GameObject val2 = new GameObject();
					iconHolders.Add(val2);
					((Object)val2).name = "iconHolder";
					RectTransform val3 = val2.AddComponent<RectTransform>();
					((Transform)val3).SetParent((Transform)(object)iconContainer, false);
					val3.sizeDelta = new Vector2(num, num);
					((Transform)obj).SetParent((Transform)(object)val3, false);
					UIUtils.FitImageContentInParent(obj);
				}
			}
		}
	}

	public static RectTransform[] GetUnlockItems(TechData data, PlayerState playerState, bool onlyPickFirstItem = false)
	{
		TribeData tribeData = playerState.GetTribeData(GameManager.GameState);
		List<RectTransform> list = new List<RectTransform>();
		if (data.unitUnlocks != null && data.unitUnlocks.Count > 0)
		{
			foreach (UnitData unitUnlock in data.unitUnlocks)
			{
				if (GameManager.GameState.GameLogicData.TryGetData(unitUnlock.type, out var _))
				{
					list.Add(UIUtils.GetUIUnitRenderer(GameManager.GameState.GameLogicData.GetOverride(unitUnlock, tribeData), GameManager.LocalPlayer).rectTransform);
					if (onlyPickFirstItem)
					{
						return list.ToArray();
					}
				}
			}
		}
		if (data.improvementUnlocks != null && data.improvementUnlocks.Count > 0)
		{
			foreach (ImprovementData improvementUnlock in data.improvementUnlocks)
			{
				if (GameManager.GameState.GameLogicData.TryGetData(improvementUnlock.type, out var _) && !GameManager.GameState.GameLogicData.GetOverride(improvementUnlock, tribeData).hidden)
				{
					Image image = UIUtils.GetImage();
					RectTransform rect = ((Graphic)image).rectTransform;
					list.Add(rect);
					SpriteData.GetBuildingSprite(GameManager.GameState.GameLogicData.GetOverride(improvementUnlock, tribeData), playerState.skinType, tribeData.climate, delegate(string atlasName, string spriteName, Sprite sprite)
					{
						image.sprite = sprite;
						((Graphic)image).SetNativeSize();
						((Object)image).name = ((Object)sprite).name;
						UIUtils.FitImageContentInParent(rect);
					});
					if (onlyPickFirstItem)
					{
						return list.ToArray();
					}
				}
			}
		}
		if (data.movementUnlocks != null && data.movementUnlocks.Count > 0)
		{
			foreach (KeyValuePair<TerrainData.Type, int> movementUnlock in data.movementUnlocks)
			{
				RectTransform tile = UIUtils.GetTile(movementUnlock.Key, tribeData.climate);
				list.Add(tile);
				if (onlyPickFirstItem)
				{
					return list.ToArray();
				}
			}
		}
		if (data.defenceBonusUnlocks != null && data.defenceBonusUnlocks.Count > 0)
		{
			foreach (KeyValuePair<TerrainData.Type, int> defenceBonusUnlock in data.defenceBonusUnlocks)
			{
				RectTransform item = ((Graphic)UIManager.IconData.GetImage("defenceBonus_" + defenceBonusUnlock.Key.GetName())).rectTransform;
				list.Add(item);
				if (onlyPickFirstItem)
				{
					return list.ToArray();
				}
			}
		}
		if (data.abilityUnlocks != null && data.abilityUnlocks.Count > 0)
		{
			foreach (PlayerAbility.Type abilityUnlock in data.abilityUnlocks)
			{
				RectTransform item2 = ((Graphic)UIManager.IconData.GetImage(abilityUnlock.GetName())).rectTransform;
				list.Add(item2);
				if (onlyPickFirstItem)
				{
					return list.ToArray();
				}
			}
		}
		if (data.taskUnlocks != null && data.taskUnlocks.Count > 0)
		{
			foreach (TaskData taskUnlock in data.taskUnlocks)
			{
				_ = taskUnlock;
				RectTransform item3 = ((Graphic)UIManager.IconData.GetImage("AchievementIcon")).rectTransform;
				list.Add(item3);
				if (onlyPickFirstItem)
				{
					return list.ToArray();
				}
			}
		}
		return list.ToArray();
	}

	private void ClearIconHolders()
	{
		if (iconHolders == null || iconHolders.Count == 0)
		{
			return;
		}
		foreach (GameObject iconHolder in iconHolders)
		{
			Object.Destroy((Object)(object)iconHolder);
		}
		iconHolders.Clear();
	}

	public void RefreshState(bool forceUnavaliable = false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		bool active = false;
		bool active2 = false;
		bool flag = false;
		bool active3 = false;
		Color sourceColor = Color.black;
		Color color = Color.white;
		Color color2 = Color.white;
		if (Complete)
		{
			state = State.Complete;
		}
		else if (Object.op_Implicit((Object)(object)Parent) && Parent.Complete)
		{
			if (data != null && !ResourceManager.HaveEnoughResources(GameManager.LocalPlayer.Id, ResourceManager.Type.Currency, GameManager.GameState.GameLogicData.GetTechPrice(data, GameManager.LocalPlayer, GameManager.GameState)))
			{
				state = State.Expensive;
			}
			else
			{
				state = State.Available;
			}
			if (forceUnavaliable)
			{
				state = State.Unavailable;
			}
		}
		else
		{
			state = State.Unavailable;
		}
		switch (state)
		{
		case State.Unavailable:
			active = false;
			active2 = true;
			sourceColor = Color.black;
			color = ColorConstants.gray;
			color2 = ColorConstants.gray;
			button.CanRegisterHover = false;
			break;
		case State.Expensive:
			active = true;
			active2 = true;
			sourceColor = ColorConstants.blue;
			color = ColorConstants.red;
			flag = true;
			active3 = true;
			button.CanRegisterHover = false;
			break;
		case State.Available:
			active = true;
			active2 = true;
			sourceColor = ColorConstants.blue;
			color = Color.white;
			flag = true;
			active3 = true;
			button.CanRegisterHover = true;
			break;
		case State.Complete:
			active = true;
			active2 = false;
			sourceColor = ColorConstants.green;
			color = ColorConstants.green;
			flag = true;
			button.CanRegisterHover = true;
			break;
		}
		((Component)shine).gameObject.SetActive(active);
		((Component)outline).gameObject.SetActive(active2);
		((Component)resourceWidget).gameObject.SetActive(active3);
		((Component)iconContainer).gameObject.SetActive(flag);
		((Graphic)label.TextComponent).color = color2;
		label.rectTransform.anchoredPosition = new Vector2(0f, (float)(flag ? (-30) : 0));
		((Graphic)bg).color = ColorUtil.SetAlphaOnColor(sourceColor, 1f);
		((Graphic)outline).color = color;
		if ((Object)(object)line != (Object)null)
		{
			((Graphic)line).color = color;
		}
		if (data != null)
		{
			resourceWidget.Amount = GameManager.GameState.GameLogicData.GetTechPrice(data, GameManager.LocalPlayer, GameManager.GameState);
		}
		if ((Object)(object)headImage != (Object)null)
		{
			SpriteData.GetHeadSprite(GameManager.GameState, GameManager.LocalPlayer, delegate(Sprite sprite)
			{
				headImage.sprite = sprite;
			});
		}
	}

	private void OnClicked(int id, BaseEventData eventData)
	{
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		if (PopupManager.IsPopupShowing<TechPopup>())
		{
			PopupManager.HideCurrentPopup();
		}
		if ((Object)(object)Parent == (Object)null)
		{
			((MonoBehaviour)this).StartCoroutine(DelayCloseTechTree());
			Log.Verbose("Is Root item :: Need to close the thech view", Array.Empty<object>());
			return;
		}
		bool flag = GameManager.LocalPlayer.Id == GameManager.GameState.CurrentPlayer;
		PopupBase.PopupButtonData[] buttonData = null;
		string format = "{0}";
		string format2 = "{0}";
		switch (state)
		{
		case State.Unavailable:
			format = "{0} " + Localization.Get("techview.locked");
			format2 = Localization.Get("techview.locked.info", Localization.Get(Parent.data.displayName), Localization.Get(data.displayName)) + "\n\n{0}";
			buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
			};
			break;
		case State.Expensive:
			format2 = Localization.Get("techview.expensive.info") + "\n\n{0}";
			buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected),
				new PopupBase.PopupButtonData("techview.research", PopupBase.PopupButtonData.States.Disabled)
			};
			break;
		case State.Available:
			buttonData = new PopupBase.PopupButtonData[2]
			{
				new PopupBase.PopupButtonData("buttons.back"),
				new PopupBase.PopupButtonData((GameManager.Client.ActionManager.IsProcessing && flag) ? "tribepicker.waiting" : "techview.research", (flag && !GameManager.Client.ActionManager.IsProcessing) ? PopupBase.PopupButtonData.States.Selected : PopupBase.PopupButtonData.States.Disabled, OnResearchTech)
			};
			break;
		case State.Complete:
			format = "{0} " + Localization.Get("techview.completed");
			format2 = Localization.Get("techview.completed.info") + "\n\n{0}";
			buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
			};
			break;
		}
		if (GameManager.Client.IsSpectating)
		{
			buttonData = new PopupBase.PopupButtonData[1]
			{
				new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
			};
		}
		activePopup = PopupManager.GetTechPopup();
		activePopup.Header = string.Format(format, Localization.Get(data.displayName));
		activePopup.Description = string.Format(format2, Localization.Get("technology.intro"));
		activePopup.SetTechData(data);
		activePopup.Cost = resourceWidget.Amount;
		activePopup.buttonData = buttonData;
		TechPopup techPopup = activePopup;
		techPopup.HideCallback = (Action)Delegate.Combine(techPopup.HideCallback, new Action(PopupHideCallback));
		ClientActionManager actionManager = GameManager.Client.ActionManager;
		actionManager.OnStartedProcessing = (Action)Delegate.Combine(actionManager.OnStartedProcessing, new Action(UpdatePopupData));
		ClientActionManager actionManager2 = GameManager.Client.ActionManager;
		actionManager2.OnFinishedProcessing = (Action)Delegate.Combine(actionManager2.OnFinishedProcessing, new Action(UpdatePopupData));
		activePopup.Show(InputManager.GetInputPosition());
		PopupManager.LastSelection = null;
		GameManager.GetAnalyticsManager().SendEvent("tech_click", new Dictionary<string, object>
		{
			{
				"game_id",
				GameManager.Client.CurrentGameId
			},
			{ "technology", data.type }
		});
	}

	private void PopupHideCallback()
	{
		ClientActionManager actionManager = GameManager.Client.ActionManager;
		actionManager.OnStartedProcessing = (Action)Delegate.Remove(actionManager.OnStartedProcessing, new Action(UpdatePopupData));
		ClientActionManager actionManager2 = GameManager.Client.ActionManager;
		actionManager2.OnFinishedProcessing = (Action)Delegate.Remove(actionManager2.OnFinishedProcessing, new Action(UpdatePopupData));
		TechPopup techPopup = activePopup;
		techPopup.HideCallback = (Action)Delegate.Remove(techPopup.HideCallback, new Action(PopupHideCallback));
		activePopup = null;
	}

	private void UpdatePopupData()
	{
		if (state == State.Available)
		{
			bool flag = GameManager.LocalPlayer.Id == GameManager.GameState.CurrentPlayer;
			PopupBase.PopupButtonData popupButtonData = activePopup.buttonData[1];
			UITextButton obj = activePopup.Buttons[1];
			popupButtonData.text = ((GameManager.Client.ActionManager.IsProcessing && flag) ? "tribepicker.waiting" : "techview.research");
			popupButtonData.state = ((flag && !GameManager.Client.ActionManager.IsProcessing) ? PopupBase.PopupButtonData.States.Selected : PopupBase.PopupButtonData.States.Disabled);
			obj.Key = activePopup.buttonData[1].text;
			((Object)obj).name = $"PopupButton_{popupButtonData.text}";
			activePopup.RefreshButtonState();
		}
	}

	private IEnumerator DelayCloseTechTree()
	{
		yield return (object)new WaitForSeconds(0.1f);
		closeTechTreeCallback?.Invoke();
	}

	private async void OnResearchTech(int id, BaseEventData eventData)
	{
		GameManager.GetAnalyticsManager().SendEvent("tech_research", new Dictionary<string, object>
		{
			{
				"game_id",
				GameManager.Client.CurrentGameId
			},
			{ "technology", data.type }
		});
		await GameManager.Client.SendCommand(new ResearchCommand(GameManager.LocalPlayer.Id, data.type));
	}

	private float GetAngle(Vector2 from, Vector2 to)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		float num = from.x - to.x;
		float num2 = Mathf.Atan2(from.y - to.y, num) * 57.29578f;
		if (num2 < 0f)
		{
			num2 += 360f;
		}
		return num2;
	}
}
