using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyPlayerRow : UIButtonBase
{
	public const string STATS_ROW_PADDING = ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ";

	public const string DIPLOMACY_INCOME_SPACE = "<space=75m>";

	public const int BG_HEIGHT = 40;

	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected TextMeshProUGUI statsNameLabel;

	[SerializeField]
	protected TextMeshProUGUI statsValueLabel;

	[SerializeField]
	protected TextMeshProUGUI smallStatsValueLabel;

	[SerializeField]
	protected TextMeshProUGUI subLabel;

	[SerializeField]
	protected RectTransform iconContainer;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected Button buttonComp;

	[SerializeField]
	protected Image iconBackgroundCircle;

	[Header("Special content")]
	[SerializeField]
	protected AvatarView avatarView;

	[SerializeField]
	protected PlayerInfoIcon playerInfoIcon;

	[SerializeField]
	protected Button increaseValueButton;

	[SerializeField]
	protected Button decreaseValueButton;

	[SerializeField]
	protected Button inviteButton;

	[SerializeField]
	protected Image inviteButtonTick;

	[Space(10f)]
	public bool shouldFitIconToParent = true;

	[SerializeField]
	private float iconSizeMultiplier = 1.8f;

	[SerializeField]
	private float iconPadding = 5f;

	protected bool haveValidIcon;

	protected int statsValueInteger;

	protected string valueFormat = string.Empty;

	protected bool isValueAdjustable;

	public ButtonAction OnIncreaseValueClick;

	public ButtonAction OnDecreaseValueClick;

	public ButtonAction OnInviteClick;

	[NonSerialized]
	public SpriteHandle iconSpriteHandle = new SpriteHandle();

	private RectTransform iconRectTransform;

	public string StatsName
	{
		get
		{
			return ((TMP_Text)statsNameLabel).text;
		}
		set
		{
			((TMP_Text)statsNameLabel).text = value;
			RefreshLayout();
		}
	}

	public string StatsValue
	{
		get
		{
			return ((TMP_Text)statsValueLabel).text;
		}
		set
		{
			((TMP_Text)statsValueLabel).text = value;
			RefreshLayout();
		}
	}

	public string Description
	{
		get
		{
			return ((TMP_Text)subLabel).text;
		}
		set
		{
			((TMP_Text)subLabel).text = value;
		}
	}

	public bool BgVisible
	{
		get
		{
			return ((Component)bg).gameObject.activeSelf;
		}
		set
		{
			((Component)bg).gameObject.SetActive(value);
		}
	}

	public Color BgColor
	{
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Graphic)bg).color = value;
		}
	}

	public bool ValueVisible
	{
		get
		{
			return ((Component)statsValueLabel).gameObject.activeSelf;
		}
		set
		{
			((Component)statsValueLabel).gameObject.SetActive(value);
		}
	}

	public RectTransform IconRectTransform
	{
		get
		{
			if ((Object)(object)icon == (Object)null)
			{
				return null;
			}
			if ((Object)(object)iconRectTransform == (Object)null)
			{
				iconRectTransform = ((Component)icon).GetComponent<RectTransform>();
			}
			return iconRectTransform;
		}
	}

	public Sprite IconSprite
	{
		get
		{
			return icon.sprite;
		}
		set
		{
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			((Component)icon).gameObject.SetActive(true);
			((Component)avatarView).gameObject.SetActive(false);
			if ((Object)(object)playerInfoIcon != (Object)null)
			{
				((Component)playerInfoIcon).gameObject.SetActive(false);
			}
			icon.sprite = value;
			haveValidIcon = (Object)(object)icon.sprite != (Object)null;
			((Component)iconContainer).gameObject.SetActive(haveValidIcon);
			if (haveValidIcon)
			{
				icon.useSpriteMesh = true;
				((Graphic)icon).SetNativeSize();
				if (shouldFitIconToParent)
				{
					UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
				}
				else
				{
					Vector2 sizeDelta = ((Graphic)icon).rectTransform.sizeDelta;
					((Graphic)icon).rectTransform.sizeDelta = sizeDelta * iconSizeMultiplier;
				}
			}
			RefreshLayout();
		}
	}

	public RectTransform IconContainer => iconContainer;

	public int StatsValueInteger
	{
		get
		{
			return statsValueInteger;
		}
		set
		{
			statsValueInteger = value;
			if (string.IsNullOrEmpty(valueFormat))
			{
				((TMP_Text)statsValueLabel).text = LocalizationUtils.FormatNumber(statsValueInteger);
			}
			else
			{
				((TMP_Text)statsValueLabel).text = string.Format(ValueFormat, LocalizationUtils.FormatNumber(statsValueInteger));
			}
			RefreshLayout();
		}
	}

	public string ValueFormat
	{
		get
		{
			return valueFormat;
		}
		set
		{
			valueFormat = value;
		}
	}

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			IconSprite = spriteHandle.sprite;
		});
		RefreshLayout();
	}

	public void SetData(string name, string value, string description, bool isValueAdjustable = false)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		this.isValueAdjustable = isValueAdjustable;
		StatsName = name;
		StatsValue = value;
		Description = description;
		((UnityEvent)increaseValueButton.onClick).AddListener((UnityAction)delegate
		{
			OnIncreaseValueClick?.Invoke(id);
		});
		((UnityEvent)decreaseValueButton.onClick).AddListener((UnityAction)delegate
		{
			OnDecreaseValueClick?.Invoke(id);
		});
		((UnityEvent)inviteButton.onClick).AddListener((UnityAction)delegate
		{
			OnInviteClick?.Invoke(id);
		});
		RefreshLayout();
	}

	public void SetTextColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)statsNameLabel).color = color;
		((Graphic)statsValueLabel).color = color;
		((Graphic)subLabel).color = color;
	}

	public void RefreshLayout()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		((Component)increaseValueButton).gameObject.SetActive(isValueAdjustable);
		((Component)decreaseValueButton).gameObject.SetActive(isValueAdjustable);
		((Component)inviteButton).gameObject.SetActive(!isValueAdjustable);
		float x = (haveValidIcon ? (iconPadding + iconContainer.sizeDelta.x + iconContainer.anchoredPosition.x * 2f) : 0f);
		((TMP_Text)statsNameLabel).rectTransform.SetAnchoredX(x);
		Vector2 offsetMin = ((TMP_Text)subLabel).rectTransform.offsetMin;
		offsetMin.x = x;
		((TMP_Text)subLabel).rectTransform.offsetMin = offsetMin;
		((TMP_Text)statsValueLabel).ForceMeshUpdate(false, false);
		float width = base.rectTransform.GetWidth() - ((TMP_Text)statsNameLabel).rectTransform.anchoredPosition.x - (((TMP_Text)statsValueLabel).GetRenderedValues(true).x + 5f);
		((TMP_Text)statsNameLabel).rectTransform.SetWidth(width);
		base.rectTransform.SetHeight(40f);
		if (((TMP_Text)statsNameLabel).textInfo != null)
		{
			((TMP_Text)statsNameLabel).textInfo.Clear();
		}
	}

	public void SetInvited(bool isInvited)
	{
		((Component)inviteButtonTick).gameObject.SetActive(isInvited);
	}

	public bool GetInvited()
	{
		return ((Component)inviteButtonTick).gameObject.activeSelf;
	}

	public Vector2 DescriptionGetPreferredValues(string s)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((TMP_Text)subLabel).GetPreferredValues(s);
	}

	public Rect DescriptionRect()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((TMP_Text)subLabel).rectTransform.rect;
	}

	public void SetSmallStatsValue(string value, Color color)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Component)smallStatsValueLabel).gameObject.SetActive(true);
		((TMP_Text)smallStatsValueLabel).text = value;
		((Graphic)smallStatsValueLabel).color = color;
	}

	public void SetAvatarState(AvatarState avatarState)
	{
		haveValidIcon = true;
		((Component)iconContainer).gameObject.SetActive(true);
		((Component)icon).gameObject.SetActive(false);
		((Component)avatarView).gameObject.SetActive(true);
		if ((Object)(object)playerInfoIcon != (Object)null)
		{
			((Component)playerInfoIcon).gameObject.SetActive(false);
		}
		avatarView.SetState(avatarState);
	}

	public void SetPlayerInfo(PlayerState player, PlayerState otherPlayer)
	{
		haveValidIcon = true;
		((Component)iconContainer).gameObject.SetActive(true);
		((Component)icon).gameObject.SetActive(false);
		((Component)avatarView).gameObject.SetActive(false);
		((Component)playerInfoIcon).gameObject.SetActive(true);
		playerInfoIcon.SetData(player, otherPlayer);
	}

	public void ShowIconBackground(bool show, Color color)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		((Component)iconBackgroundCircle).gameObject.SetActive(show);
		((Graphic)iconBackgroundCircle).color = color;
	}
}
