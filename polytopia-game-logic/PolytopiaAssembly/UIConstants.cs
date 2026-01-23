using System.Collections.Generic;

public class UIConstants
{
	public enum Screens
	{
		None,
		StartScreen,
		Settings,
		Highscore,
		ThroneRoom,
		About,
		News,
		GameMode,
		TribeSelector,
		GameSetup,
		Hud,
		TechTree,
		IngameMenu,
		StatsScreen,
		LoadingScreen,
		MultiplayerScreen,
		PlayerPicker,
		FriendsList,
		BetaInfo,
		ProfileScreen,
		HotSeatOverlay,
		PlayerSelectionScreen,
		ReplaysScreen,
		LadderScreen,
		UpsellScreen,
		TournamentsScreen
	}

	public static float UI_PIXELS_PER_UNIT = 100f;

	public static Dictionary<Screens, Screens[]> deepLinks = new Dictionary<Screens, Screens[]>
	{
		{
			Screens.StatsScreen,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.Highscore,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.ThroneRoom,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.About,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.News,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.MultiplayerScreen,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.ReplaysScreen,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.LadderScreen,
			new Screens[1] { Screens.StartScreen }
		},
		{
			Screens.UpsellScreen,
			new Screens[1] { Screens.StartScreen }
		}
	};
}
