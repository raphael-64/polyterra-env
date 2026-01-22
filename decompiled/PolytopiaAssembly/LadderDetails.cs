using System;
using System.Collections.Generic;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Challengermode.Data;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LadderDetails : UIBasicComponent
{
	public const int LADDER_PLACEMENTS_COUNT = 5;

	[SerializeField]
	protected TextMeshProUGUI ladderName;

	[SerializeField]
	protected TextMeshProUGUI ladderDescription;

	[SerializeField]
	protected LadderDetailsTimeline timeline;

	[SerializeField]
	protected UITextButton playButton;

	[SerializeField]
	protected RectTransform placementsHolder;

	[Header("Prefabs")]
	[SerializeField]
	protected LadderPlacementRow placementRowPrefab;

	[SerializeField]
	protected HeaderRow headerRowPrefab;

	private LadderViewModel data;

	private HeaderRow placementHeader;

	private List<LadderPlacementRow> placementRows = new List<LadderPlacementRow>();

	private bool isUpcoming;

	public void SetData(LadderViewModel data, bool isUpcoming = false)
	{
		ClearData();
		this.data = data;
		this.isUpcoming = isUpcoming;
		((TMP_Text)ladderName).text = data.Name;
		((TMP_Text)ladderDescription).text = data.Description;
		timeline.SetData(data.StartDate, data.EndDate);
		UpdateJoinButton();
		LoadLadderPlacements();
	}

	public void ClearData()
	{
		for (int i = 0; i < placementRows.Count; i++)
		{
			Object.Destroy((Object)(object)((Component)placementRows[i]).gameObject);
		}
		if ((Object)(object)placementHeader != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)placementHeader).gameObject);
		}
		placementRows.Clear();
	}

	private async void LoadLadderPlacements()
	{
		List<LadderPlacementViewModel> list = await GameManager.GetLadderManager().GetLadderPlacements(data.Id);
		if (list.Count <= 0)
		{
			return;
		}
		bool flag = false;
		int num = Mathf.Min(list.Count, 5);
		int playersPlaced = 0;
		placementHeader = AddHeader(Localization.Get("onlineview.ladder.placements.header"));
		for (int i = 0; i < num; i++)
		{
			LadderPlacementViewModel ladderPlacementViewModel = list[i];
			if (ladderPlacementViewModel.Placement != -1)
			{
				if (Guid.TryParse(ladderPlacementViewModel.PolytopiaId, out var result) && result == AccountManager.PlayerAccountId)
				{
					AddPlacement(ladderPlacementViewModel.Placement, ladderPlacementViewModel.DisplayName, ladderPlacementViewModel.Score.ToString(), isLocalPlayer: true);
					flag = true;
				}
				else
				{
					AddPlacement(ladderPlacementViewModel.Placement, ladderPlacementViewModel.DisplayName, ladderPlacementViewModel.Score.ToString());
				}
				playersPlaced++;
			}
		}
		if (!flag)
		{
			LadderPlacementViewModel ladderPlacementViewModel2 = await GameManager.GetLadderManager().GetMyLadderPlacement(data.Id);
			if (ladderPlacementViewModel2 != null)
			{
				playersPlaced++;
				AddPlacement(ladderPlacementViewModel2.Placement, ladderPlacementViewModel2.DisplayName, ladderPlacementViewModel2.Score.ToString(), isLocalPlayer: true);
			}
		}
		if (playersPlaced == 0)
		{
			((Component)placementHeader).gameObject.SetActive(false);
		}
	}

	private void AddPlacement(int position, string name, string score, bool isLocalPlayer = false)
	{
		LadderPlacementRow ladderPlacementRow = Object.Instantiate<LadderPlacementRow>(placementRowPrefab, (Transform)(object)placementsHolder);
		if (position == -1)
		{
			ladderPlacementRow.SetData($" - {name}", score, null);
		}
		else
		{
			ladderPlacementRow.SetData($"{position}. {name}", score, null);
		}
		ladderPlacementRow.BgVisible = isLocalPlayer;
		placementRows.Add(ladderPlacementRow);
	}

	private HeaderRow AddHeader(string text)
	{
		HeaderRow headerRow = Object.Instantiate<HeaderRow>(headerRowPrefab, (Transform)(object)placementsHolder);
		headerRow.label.Text = text;
		return headerRow;
	}

	private async void UpdateJoinButton()
	{
		playButton.ButtonEnabled = false;
		bool num = await GameManager.GetLadderManager().IsLadderJoined(data.Id);
		playButton.ButtonEnabled = true;
		if (!num)
		{
			playButton.Key = "onlineview.ladder.join";
			playButton.OnClicked += OnJoinLadder;
			playButton.ButtonEnabled = data.LastJoinDate > DateTime.UtcNow && !isUpcoming;
		}
		else
		{
			playButton.Key = "onlineview.ladder.play";
			playButton.OnClicked += OnPlayLadder;
			playButton.ButtonEnabled = true;
		}
	}

	public async void OnJoinLadder(int id, BaseEventData eventData)
	{
		if (!(await GameManager.GetLadderManager().IsAccountConnected()))
		{
			if (!PopupManager.IsPopupShowing<ChallengermodeConnectPopup>())
			{
				ChallengermodeConnectPopup challengermodeConnectPopup = PopupManager.GetChallengermodeConnectPopup();
				challengermodeConnectPopup.Header = Localization.Get("challengermode.connect.header");
				challengermodeConnectPopup.Description = Localization.Get("challengermode.connect.description");
				challengermodeConnectPopup.buttonData = new PopupBase.PopupButtonData[2]
				{
					new PopupBase.PopupButtonData("buttons.back"),
					new PopupBase.PopupButtonData("buttons.connect", PopupBase.PopupButtonData.States.Selected, delegate
					{
						OnChallengermodeConnect();
					})
				};
				challengermodeConnectPopup.Show();
			}
		}
		else
		{
			await GameManager.GetLadderManager().JoinLadder(data.Id);
			UpdateJoinButton();
		}
	}

	public async void OnLeaveLadder(int id, BaseEventData eventData)
	{
		await GameManager.GetLadderManager().LeaveLadder(data.Id);
		UpdateJoinButton();
	}

	public async void OnPlayLadder(int id, BaseEventData eventData)
	{
		NetworkUtils.ShowLoader();
		ServerResponse<MatchmakingSubmissionViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.SubmitMatchmakingRequest(GameManager.GetLadderManager().GetMatchSettingsPreset(data.Id));
		NetworkUtils.HideLoader();
		if (serverResponse.Success)
		{
			if (serverResponse.Data.IsWaitingForOpponents)
			{
				NotificationManager.Notify(Localization.Get("wcontroller.matchmaking.waitingforplayers", serverResponse.Data.GameName));
			}
			UIManager.OpenMultiplayerScreen();
		}
		else
		{
			PopupManager.ShowBackendErrorPopup(serverResponse.ErrorMessage);
		}
	}

	public async void OnChallengermodeConnect()
	{
		await GameManager.GetLadderManager().ConnectAccount(data.Id);
	}
}
