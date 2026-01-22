using System.Collections.Generic;
using EnumsNET;
using PolytopiaBackendBase.Game;

namespace PolytopiaBackendBase.Api;

public static class PublicGameSettingHelpers
{
	private static Dictionary<PublicGameMode, GameMode> GameModes = new Dictionary<PublicGameMode, GameMode>
	{
		[PublicGameMode.Domination] = GameMode.Domination,
		[PublicGameMode.Glory] = GameMode.Glory,
		[PublicGameMode.Might] = GameMode.Might
	};

	public static GameMode ParseGameMode(string gameMode)
	{
		return GameModes[Enums.Parse<PublicGameMode>(gameMode)];
	}
}
