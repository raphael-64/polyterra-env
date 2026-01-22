using Polytopia.Data;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MatchmakingGameInfoRow : UIBasicButton, IListCellNavigation
{
	[Header("Game Info row")]
	[SerializeField]
	protected Image icon;

	[SerializeField]
	protected TextMeshProUGUI nameLabel;

	[SerializeField]
	protected TextMeshProUGUI infoLabel;

	[SerializeField]
	protected GameInfoTimer timer;

	private MatchmakingGameSummaryViewModel summary;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	[SerializeField]
	protected ColorStates labelColorStates = new ColorStates
	{
		defaultColor = new Color(1f, 1f, 1f, 1f),
		hoverColor = new Color(0f, 0f, 0f, 1f),
		highlightedColor = new Color(0f, 0f, 0f, 1f),
		highlightedHoverColor = new Color(0f, 0f, 0f, 1f),
		disabledColor = new Color(1f, 1f, 1f, 0.502f)
	};

	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += OnButtonClicked;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.OnClicked -= OnButtonClicked;
	}

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetIcon(spriteHandle.sprite);
		});
	}

	public void SetData(MatchmakingGameSummaryViewModel summary)
	{
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		this.summary = summary;
		((TMP_Text)nameLabel).text = summary.Name;
		int num = Mathf.Max(1, (int)summary.OpponentCount) - summary.Participators.Count + 1;
		string arg = ((num < 2) ? Localization.Get("misc.player") : Localization.Get("misc.players"));
		((TMP_Text)infoLabel).text = Localization.Get("matchmakinggameinfo.waitingforplayers", num, arg);
		if (VersionManager.GameVersion >= 90 && summary.WithPickedTribe)
		{
			if (GameManager.GetRemoteGameDataManager().TryGetLocalParticipator(summary.Id, out var participatorViewModel))
			{
				iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress((TribeData.Type)participatorViewModel.SelectedTribe));
			}
		}
		else
		{
			iconSpriteHandle.Request(SpriteData.GetBuildingSpriteAddress(ImprovementData.Type.GrowForest));
		}
		timer.RefreshCountUp(summary.DateCreated);
		bgColorStates.defaultColor = new Color(0f, 0f, 0f, 0.8f);
		UpdateColors();
	}

	private void SetIcon(Sprite sprite)
	{
		icon.sprite = sprite;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
	}

	public override void UpdateColors()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		TextMeshProUGUI obj = nameLabel;
		Color color = (((Graphic)infoLabel).color = GetColorForState(labelColorStates));
		((Graphic)obj).color = color;
	}

	private void OnButtonClicked(int id, BaseEventData eventData)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
		gameInfoPopup.SetData(summary);
		gameInfoPopup.Show(InputManager.GetInputPosition());
	}

	public Selectable GetMainSelectable()
	{
		return (Selectable)(object)base.button;
	}

	public Selectable GetAccessorySelectable()
	{
		return (Selectable)(object)base.button;
	}
}
