using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LadderPlacementRow : UIButtonBase
{
	public const string STATS_ROW_PADDING = ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ";

	public const string DIPLOMACY_INCOME_SPACE = "<space=75m>";

	public const int BG_HEIGHT_BIG = 50;

	public const int BG_HEIGHT_SMALL = 30;

	[SerializeField]
	protected Image bg;

	[SerializeField]
	protected TextMeshProUGUI statsNameLabel;

	[SerializeField]
	protected TextMeshProUGUI statsValueLabel;

	[SerializeField]
	protected TextMeshProUGUI subLabel;

	[SerializeField]
	protected RectTransform dottedLine;

	[SerializeField]
	protected RectTransform iconContainer;

	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected Button buttonComp;

	[SerializeField]
	protected Image iconBackgroundCircle;

	[Header("Embassy")]
	[SerializeField]
	protected RectTransform embassyIncomeContainer;

	[SerializeField]
	protected TextMeshProUGUI embassyIncomeText;

	[Header("Special content")]
	[SerializeField]
	protected AvatarView avatarView;

	[SerializeField]
	protected PlayerInfoIcon playerInfoIcon;

	[Space(10f)]
	public bool shouldFitIconToParent = true;

	[SerializeField]
	private float iconSizeMultiplier = 1.8f;

	[SerializeField]
	private float iconPadding = 5f;

	protected bool haveValidIcon;

	protected int statsValueInteger;

	protected string valueFormat = string.Empty;

	protected bool playSfx;

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
			string arg = (EmbassyIncomeContainerVisible ? "<space=75m>" : "");
			((TMP_Text)statsNameLabel).text = string.Format("<link=\"Name\">{0}</link> {1}{2}", value, arg, ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ");
			RefreshLayout();
		}
	}

	public string StatsNameKey
	{
		get
		{
			return ((TMP_Text)statsNameLabel).text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				((TMP_Text)statsNameLabel).text = string.Format("{0} {1}", Localization.Get(value), ". . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . . ");
				RefreshLayout();
			}
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

	public string EmbassyIncomeText
	{
		get
		{
			return ((TMP_Text)embassyIncomeText).text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				((TMP_Text)embassyIncomeText).text = value;
			}
		}
	}

	public bool EmbassyIncomeContainerVisible
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)embassyIncomeContainer))
			{
				return false;
			}
			return ((Component)embassyIncomeContainer).gameObject.activeInHierarchy;
		}
		set
		{
			if (Object.op_Implicit((Object)(object)embassyIncomeContainer))
			{
				((Component)embassyIncomeContainer).gameObject.SetActive(value);
			}
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
			bool num = value != statsValueInteger;
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
			if (num && playSfx)
			{
				AudioManager.PlaySFX(SFXTypes.Count, 0.2f);
			}
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

	public void SetData(string statsNameKey, string statsValue, string description)
	{
		StatsNameKey = statsNameKey;
		StatsValue = statsValue;
		Description = description;
		RefreshLayout();
	}

	public void SetTextColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)statsNameLabel).color = color;
		((Graphic)statsValueLabel).color = color;
	}

	public void RefreshLayout()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		float x = (haveValidIcon ? (iconPadding + iconContainer.sizeDelta.x + iconContainer.anchoredPosition.x * 2f) : 0f);
		((TMP_Text)statsNameLabel).rectTransform.SetAnchoredX(x);
		Vector2 offsetMin = ((TMP_Text)subLabel).rectTransform.offsetMin;
		offsetMin.x = x;
		((TMP_Text)subLabel).rectTransform.offsetMin = offsetMin;
		((TMP_Text)statsValueLabel).ForceMeshUpdate(false, false);
		float width = base.rectTransform.GetWidth() - ((TMP_Text)statsNameLabel).rectTransform.anchoredPosition.x - (((TMP_Text)statsValueLabel).GetRenderedValues(true).x + 5f);
		((TMP_Text)statsNameLabel).rectTransform.SetWidth(width);
		base.rectTransform.SetHeight(string.IsNullOrEmpty(Description) ? 30 : 50);
		if (((TMP_Text)statsNameLabel).textInfo != null)
		{
			((TMP_Text)statsNameLabel).textInfo.Clear();
		}
		((MonoBehaviour)UIManager.Instance).StartCoroutine(SetTextCoroutine());
		IEnumerator SetTextCoroutine()
		{
			while (((TMP_Text)statsNameLabel).textInfo == null || (((TMP_Text)statsNameLabel).textInfo.characterCount == 0 && ((TMP_Text)statsNameLabel).text.Length > 0))
			{
				yield return null;
			}
			if (((TMP_Text)statsNameLabel).textInfo.linkCount > 0)
			{
				float scaleFactor = UIManager.CanvasScaler.scaleFactor;
				TMP_LinkInfo linkInfo = ((TMP_Text)statsNameLabel).textInfo.linkInfo[0];
				Bounds linkBounds = statsNameLabel.GetLinkBounds(linkInfo);
				if (Object.op_Implicit((Object)(object)embassyIncomeContainer))
				{
					Vector2 anchoredPosition = embassyIncomeContainer.anchoredPosition;
					anchoredPosition.x = ((Bounds)(ref linkBounds)).size.x / scaleFactor;
					embassyIncomeContainer.anchoredPosition = anchoredPosition;
				}
			}
		}
	}

	public void AnimateValue(int from, int to, float time, float delay = 0f)
	{
		playSfx = true;
		StatsValueInteger = from;
		TweenSettingsExtensions.SetDelay<TweenerCore<int, int, NoOptions>>(DOTween.To((DOGetter<int>)(() => StatsValueInteger), (DOSetter<int>)delegate(int x)
		{
			StatsValueInteger = x;
		}, to, time), delay);
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
