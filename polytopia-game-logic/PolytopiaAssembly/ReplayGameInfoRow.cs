using System;
using System.Collections.Generic;
using Polytopia.Data;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ReplayGameInfoRow : UIBasicButton, IListCellNavigation
{
	[Header("Replay Game Info row")]
	[SerializeField]
	protected Image coatOfArmIcon;

	[SerializeField]
	protected Image labelIcon;

	[SerializeField]
	protected TextMeshProUGUI nameLabel;

	[SerializeField]
	protected TextMeshProUGUI infoLabel;

	[SerializeField]
	protected Sprite favoriteIconOn;

	[SerializeField]
	protected Sprite favoriteIconOff;

	[SerializeField]
	protected Image favoriteImage;

	[SerializeField]
	protected Button favoriteButton;

	private GameSummaryViewModel summaryViewModel;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	private bool favorite;

	public Action OnFavoritedAction;

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
		favoriteImage.preserveAspect = true;
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetIcon(spriteHandle.sprite, coatOfArmIcon);
		});
	}

	public void SetData(GameSummaryViewModel summaryViewModel, bool isFavorite)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		this.summaryViewModel = summaryViewModel;
		_ = Color.black;
		((Component)labelIcon).gameObject.SetActive(false);
		SerializationHelpers.FromByteArray<GameStateSummary>(summaryViewModel.GameSummaryData, out var result);
		if (result == null)
		{
			((TMP_Text)nameLabel).text = Localization.Get("gameinfo.nodata.title");
			_ = ColorConstants.red;
			LoadFaceIcon("neutral");
			((TMP_Text)infoLabel).text = Localization.Get("gameinfo.nodata");
			return;
		}
		favorite = isFavorite;
		((TMP_Text)nameLabel).text = result.GameName;
		string text = (summaryViewModel.DateEnded.HasValue ? Localization.Get("replay.ended", summaryViewModel.DateEnded.Value.ToString("MMMM dd")) : ((summaryViewModel.State != GameSessionState.Ended) ? Localization.Get("replay.ongoing") : Localization.Get("replay.ended", "")));
		TribeData.Type type = TribeData.Type.None;
		SkinType skinType = SkinType.Default;
		string text2 = "";
		PlayerRankingViewModel playerRankingViewModel = null;
		if (summaryViewModel.Result != null)
		{
			playerRankingViewModel = GetWinner(summaryViewModel.Result.PlayerRankings);
		}
		foreach (GameStateSummary.GamePlayerSummary playerSummary in result.PlayerSummaries)
		{
			if (playerSummary != null)
			{
				if (playerSummary.PolytopiaId == AccountManager.PlayerAccountId)
				{
					type = playerSummary.TribeType;
					skinType = playerSummary.SkinType;
				}
				if (playerRankingViewModel != null && playerSummary.PolytopiaId == playerRankingViewModel.PolytopiaUserId)
				{
					text2 = playerSummary.UserName;
				}
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			text += string.Format("\n{0}", Localization.Get("replay.winner", text2));
		}
		((TMP_Text)infoLabel).text = text;
		if (summaryViewModel.GameContext != null)
		{
			((Component)labelIcon).gameObject.SetActive(summaryViewModel.GameContext.ExternalMatchId.HasValue);
		}
		if (type == TribeData.Type.None)
		{
			LoadFaceIcon("neutral");
		}
		else
		{
			LoadFaceIcon(type, skinType);
		}
		ToggleFavoriteIcon(isFavorite);
		UpdateColors();
	}

	private PlayerRankingViewModel GetWinner(List<PlayerRankingViewModel> playerRankings)
	{
		PlayerRankingViewModel playerRankingViewModel = null;
		if (playerRankings != null && playerRankings.Count > 0)
		{
			for (int i = 0; i < playerRankings.Count; i++)
			{
				PlayerRankingViewModel playerRankingViewModel2 = playerRankings[i];
				if (playerRankingViewModel == null || playerRankingViewModel2.Rank < playerRankingViewModel.Rank)
				{
					playerRankingViewModel = playerRankingViewModel2;
				}
			}
		}
		return playerRankingViewModel;
	}

	private void ToggleFavoriteIcon(bool isFavorite)
	{
		favoriteImage.sprite = (isFavorite ? favoriteIconOn : favoriteIconOff);
	}

	private void LoadFaceIcon(string faceId)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(faceId));
	}

	private void LoadFaceIcon(TribeData.Type type, SkinType skinType)
	{
		if (EnumCache<SkinType>.TryGetName(skinType, out var value))
		{
			iconSpriteHandle.Request(new SpriteAddress[2]
			{
				SpriteData.GetHeadSpriteAddress(value),
				SpriteData.GetHeadSpriteAddress(type)
			});
		}
		else
		{
			Log.Error("Invalid skin type {0}", new object[1] { skinType });
			iconSpriteHandle.Request(new SpriteAddress[1] { SpriteData.GetHeadSpriteAddress(type) });
		}
	}

	private void SetIcon(Sprite sprite, Image icon)
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

	public void OnButtonClicked(int id, BaseEventData eventData)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		GameInfoPopup gameInfoPopup = PopupManager.GetGameInfoPopup();
		gameInfoPopup.SetData(summaryViewModel, isReplay: true);
		gameInfoPopup.Show(InputManager.GetInputPosition());
	}

	public async void OnFavoriteButtonClickedAsync()
	{
		((Behaviour)favoriteButton).enabled = false;
		ToggleFavoriteIcon(!favorite);
		ErrorCode? errorCode = await GameManager.GetReplaysManager().SetCachedFavoriteReplay(summaryViewModel, !favorite);
		if (!((Object)(object)favoriteButton == (Object)null))
		{
			((Behaviour)favoriteButton).enabled = true;
			if (!errorCode.HasValue)
			{
				OnFavoritedAction?.Invoke();
				return;
			}
			NetworkUtils.ShowLoaderError(Localization.GetErrorMessage(errorCode.Value));
			ToggleFavoriteIcon(favorite);
		}
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
