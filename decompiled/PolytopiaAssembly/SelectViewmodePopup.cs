using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectViewmodePopup : BasicPopup
{
	[Header("SelectViewmodePopup")]
	[SerializeField]
	protected GridLayoutGroup gridLayout;

	[SerializeField]
	protected LayoutElement gridBottomSpacer;

	[SerializeField]
	protected UIRoundButton buttonPrefab;

	private List<UIRoundButton> buttons;

	public UIButtonBase.ButtonAction onAutoClicked;

	public UIButtonBase.ButtonAction onPlayerClicked;

	private void OnEnable()
	{
		UIEvents.OnForceRefreshHud += OnForceRefreshHud;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UIEvents.OnForceRefreshHud -= OnForceRefreshHud;
	}

	public void SetData(GameState gameState)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		ClearButtons();
		buttons = new List<UIRoundButton>();
		float num = 0f;
		UIRoundButton autoButton = Object.Instantiate<UIRoundButton>(buttonPrefab, ((Component)gridLayout).transform);
		if (gameState.TryGetPlayer(gameState.CurrentPlayer, out var playerState) && gameState.GameLogicData.TryGetData(playerState.tribe, out var _))
		{
			autoButton.id = playerState.Id;
			autoButton.rectTransform.sizeDelta = new Vector2(56f, 56f);
			((Component)autoButton.Outline).gameObject.SetActive(false);
			((Graphic)autoButton.BG).color = ColorConstants.gray;
			autoButton.Key = "replay.viewmode.auto";
			autoButton.OnClicked += OnAutoButtonClicked;
			autoButton.iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandleCallback)
			{
				autoButton.SetFaceIcon(spriteHandleCallback.sprite);
			});
			autoButton.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress("neutral"));
			buttons.Add(autoButton);
		}
		for (int num2 = 0; num2 < gameState.PlayerStates.Count; num2++)
		{
			PlayerState playerState2 = gameState.PlayerStates[num2];
			if (playerState2.Id != byte.MaxValue && gameState.GameLogicData.TryGetData(playerState2.tribe, out var _))
			{
				bool flag = playerState2.IsAlive(gameState);
				UIRoundButton playerButton = Object.Instantiate<UIRoundButton>(buttonPrefab, ((Component)gridLayout).transform);
				playerButton.id = playerState2.Id;
				playerButton.rectTransform.sizeDelta = new Vector2(56f, 56f);
				((Component)playerButton.Outline).gameObject.SetActive(false);
				((Graphic)playerButton.BG).color = ColorUtil.SetAlphaOnColor(playerState2.GetPlayerColor(gameState), flag ? 1f : 0.5f);
				playerButton.text = playerState2.UserName;
				playerButton.SetIconColor((Color)(flag ? Color.white : new Color(1f, 1f, 1f, 0.5f)));
				playerButton.ButtonEnabled = flag;
				playerButton.OnClicked += OnPlayerButtonClicked;
				playerButton.iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandleCallback)
				{
					playerButton.SetFaceIcon(spriteHandleCallback.sprite);
				});
				playerButton.iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddresses(gameState, playerState2));
				if (playerButton.Label.PreferedValues.y > num)
				{
					num = playerButton.Label.PreferedValues.y;
				}
				buttons.Add(playerButton);
			}
		}
		gridLayout.spacing = new Vector2(gridLayout.spacing.x, num + 10f);
		((LayoutGroup)gridLayout).padding.bottom = Mathf.RoundToInt(num + 10f);
		gridBottomSpacer.minHeight = num + 10f;
		UpdateTopButton();
		if (GameManager.Client != null && GameManager.Client is ReplayClient replayClient)
		{
			SetSelectedButton((!replayClient.doAutoSwitchPlayers) ? replayClient.currentViewingPlayer : 0);
		}
	}

	private void OnForceRefreshHud()
	{
		SetData(GameManager.GameState);
	}

	private void UpdateTopButton()
	{
		if (GameManager.Client != null && GameManager.Client is ReplayClient replayClient)
		{
			((Component)topButton).gameObject.SetActive(true);
			string text = (replayClient.doShowAllPlayers ? "replay.viewmode.fog.on" : "replay.viewmode.fog.off");
			topButton.Key = text;
			if (topButtonData == null)
			{
				TopButtonData = new PopupButtonData(text, PopupButtonData.States.None, OnTopButtonClicked, -1, closesPopup: false);
			}
		}
	}

	private void OnTopButtonClicked(int id, BaseEventData eventData)
	{
		if (GameManager.Client != null && GameManager.Client is ReplayClient replayClient)
		{
			replayClient.doShowAllPlayers = !replayClient.doShowAllPlayers;
			if ((Object)(object)MapRenderer.Current != (Object)null)
			{
				MapRenderer.Current.Refresh();
			}
			UpdateTopButton();
		}
	}

	private void ClearButtons()
	{
		if (buttons == null)
		{
			return;
		}
		for (int i = 0; i < buttons.Count; i++)
		{
			if (!((Object)(object)buttons[i] == (Object)null))
			{
				Object.Destroy((Object)(object)((Component)buttons[i]).gameObject);
			}
		}
		buttons.Clear();
	}

	private void SetSelectedButton(int selectedButtonIndex)
	{
		if (buttons != null && buttons.Count > selectedButtonIndex)
		{
			for (int i = 0; i < buttons.Count; i++)
			{
				((Component)buttons[i].Outline).gameObject.SetActive(selectedButtonIndex == i);
			}
		}
	}

	private void OnAutoButtonClicked(int id, BaseEventData eventData = null)
	{
		SetSelectedButton(0);
		onAutoClicked?.Invoke(id, eventData);
	}

	private void OnPlayerButtonClicked(int id, BaseEventData eventData = null)
	{
		SetSelectedButton(id);
		onPlayerClicked?.Invoke(id, eventData);
	}
}
