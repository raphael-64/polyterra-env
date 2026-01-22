using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class GameModeButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	private GameMode currentGameMode;

	private GameType currentGameType;

	private GameRules currentGameRules;

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(GameMode summaryGameMode, GameType gameType, int scoreLimit = 10000)
	{
		currentGameMode = summaryGameMode;
		currentGameType = gameType;
		currentGameRules = new GameRules(currentGameMode);
		currentGameRules.ScoreLimit = scoreLimit;
		roundButton.text = Localization.Get(GameModeUtils.GetTitle(summaryGameMode));
		roundButton.sprite = UIManager.IconData.GetSprite($"GM_{summaryGameMode.ToString()}");
	}

	private void OnButtonClicked(int id, BaseEventData eventData = null)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get(GameModeUtils.GetTitle(currentGameMode));
		string text = Localization.Get(GameModeUtils.GetDescription(currentGameMode));
		if (currentGameType != GameType.SinglePlayer)
		{
			text = ((currentGameMode != GameMode.Glory) ? string.Format("{0}\n{1}", Localization.Get("gamestats.gamemode", Localization.Get(GameModeUtils.GetTitle(currentGameMode))), text) : Localization.Get(GameModeUtils.GetDescription(currentGameMode), LocalizationUtils.FormatNumber(currentGameRules.ScoreLimit)));
		}
		basicPopup.Description = text;
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show(InputManager.GetInputPosition());
	}
}
