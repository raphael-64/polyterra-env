using System.Collections.Generic;
using PolytopiaBackendBase.Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UIRoundButton))]
public class MapSizeButtonWrapper : MonoBehaviour
{
	[SerializeField]
	private UIRoundButton roundButton;

	[SerializeField]
	private TextMeshProUGUI roundButtonTextContent;

	private MapPreset currentMapPreset;

	private int currentMapSize;

	private int seed = -1;

	private void Awake()
	{
		roundButton.OnClicked += OnButtonClicked;
	}

	private void OnDestroy()
	{
		roundButton.OnClicked -= OnButtonClicked;
	}

	public void SetData(int MapSize, MapPreset mapPreset, int seed)
	{
		this.seed = seed;
		SetData(MapSize, mapPreset);
	}

	public void SetData(int MapSize, MapPreset mapPreset)
	{
		currentMapPreset = mapPreset;
		currentMapSize = MapSize;
		roundButton.text = Localization.Get(currentMapPreset.GetLocalizationName());
		if (currentMapSize == 0)
		{
			((TMP_Text)roundButtonTextContent).text = string.Empty;
			roundButton.sprite = UIManager.IconData.GetSprite($"GM_{GameMode.None.ToString()}");
		}
		else
		{
			((TMP_Text)roundButtonTextContent).text = (currentMapSize * currentMapSize).ToString();
			roundButton.sprite = null;
		}
	}

	private void OnButtonClicked(int id, BaseEventData eventdata)
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		BasicPopup moreGameInfoPopup = PopupManager.GetMoreGameInfoPopup();
		moreGameInfoPopup.Header = Localization.Get("gamesettings.map");
		List<string> list = new List<string>();
		list.Add(Localization.Get(currentMapPreset.GetLocalizationName()));
		list.Add(string.Format(Localization.Get("gamesettings.info.matchmaking.mapsize"), currentMapSize * currentMapSize));
		if (seed != -1)
		{
			list.Add(Localization.Get("gamesettings.info.seed") + " " + seed);
		}
		string text = string.Empty;
		foreach (string item in list)
		{
			text = text + item + "\n";
		}
		moreGameInfoPopup.Description = text;
		moreGameInfoPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.back", PopupBase.PopupButtonData.States.Selected)
		};
		moreGameInfoPopup.Show(InputManager.GetInputPosition());
	}
}
