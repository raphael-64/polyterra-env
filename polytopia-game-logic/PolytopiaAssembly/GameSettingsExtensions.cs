using System;
using System.IO;
using Polytopia.IO;
using PolytopiaBackendBase.Game;

public static class GameSettingsExtensions
{
	public static void SaveToDisk(this GameSettings settings)
	{
		string gameSettingsPath = Paths.GetGameSettingsPath(GetSettingsNameFromModes(settings.GameType, settings.BaseGameMode));
		try
		{
			using BinaryWriter binaryWriter = new BinaryWriter(PolytopiaFile.Open(gameSettingsPath, FileMode.Create));
			binaryWriter.Write(VersionManager.GameVersion);
			settings.Serialize(binaryWriter, VersionManager.GameVersion);
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save settings with error {0}", new object[1] { ex });
		}
	}

	public static string GetSettingsNameFromModes(GameType gameType, GameMode gameMode)
	{
		return gameType switch
		{
			GameType.Matchmaking => GameSettingSaves.Matchmaking.ToString(), 
			GameType.PassAndPlay => GameSettingSaves.Hotseat.ToString(), 
			GameType.Multiplayer => GameSettingSaves.Multiplayer.ToString(), 
			_ => gameMode switch
			{
				GameMode.Custom => GameSettingSaves.Custom.ToString(), 
				GameMode.Domination => GameSettingSaves.Domination.ToString(), 
				_ => GameSettingSaves.Perfection.ToString(), 
			}, 
		};
	}

	public static bool TryLoadFromDisk(out GameSettings settings, string settingsName)
	{
		int version;
		bool num = DiskSerializationHelpers.FromDisk<GameSettings>(Paths.GetGameSettingsPath(settingsName), out settings, out version);
		if (num)
		{
			settings.SoftReset();
		}
		else
		{
			settings = new GameSettings();
		}
		settings.SetUnlockedTribes(GameManager.GetPurchaseManager().GetUnlockedTribes());
		return num;
	}
}
