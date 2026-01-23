using System;
using PolytopiaBackendBase.Game;
using UnityEngine;
using UnityEngine.UI;

public class GamemodeButton : UITextButton
{
	[Serializable]
	public class GamemodeButtonData
	{
		public string id;

		public string headerKey;

		public string descriptionKey;

		public Sprite icon;

		public GameMode gameMode;
	}

	[Header("Gamemode Button")]
	[SerializeField]
	protected TMPLocalizer description;

	[SerializeField]
	protected int selectedGamemode;

	[SerializeField]
	protected GamemodeButtonData[] gamemodeData;

	protected bool gamemodeSet;

	public override void Start()
	{
		base.Start();
		if (!gamemodeSet)
		{
			SetGamemode(selectedGamemode);
		}
	}

	public void SetGamemode(int gamemode)
	{
		if (!m_blockClick)
		{
			gamemodeSet = true;
			GamemodeButtonData gamemodeButtonData = gamemodeData[gamemode];
			base.Key = gamemodeButtonData.headerKey;
			description.Key = gamemodeButtonData.descriptionKey;
			icon.sprite = gamemodeButtonData.icon;
			icon.useSpriteMesh = true;
			icon.preserveAspect = true;
		}
	}

	public GameMode GetGameMode()
	{
		return gamemodeData[selectedGamemode].gameMode;
	}

	public override void UpdateColors()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateColors();
		((Graphic)description.TextComponent).color = GetColorForState(labelColorStates);
	}
}
