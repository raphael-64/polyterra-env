using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;
using Polytopia.IO;
using PolytopiaBackendBase.Game;
using SevenZip.Compression.LZMA;
using UnityEngine;

public static class VersionMigration
{
	public class Traits
	{
		public const string BaseTypeAlias = "";

		public string TypeName { get; set; }

		public string[] ClassMembers { get; set; }

		public bool IsExternalizable { get; set; }

		public bool IsDynamic { get; set; }
	}

	public const string LEGACY_SAVE_BACKUP_NAME = "polytopia-bin-backup";

	public static bool MigrateGameVersion(int from, int to)
	{
		if (from == to)
		{
			return true;
		}
		Log.Verbose("[VersionManager] Migrating from GameVersion {0} to {1}", new object[2] { from, to });
		Log.Verbose("[VersionManager] Version migration successful", Array.Empty<object>());
		return true;
	}

	public static bool MigrateSemanticVersion(string from, string to)
	{
		if (from == to)
		{
			return true;
		}
		if (!Version.TryParse(from, out Version _))
		{
			Log.Error("Unable to parse previous version: {0}", new object[1] { from });
			return false;
		}
		if (!Version.TryParse(to, out Version _))
		{
			Log.Error("Unable to parse current version: {0}", new object[1] { to });
			return false;
		}
		if (!Version.TryParse("0.0.0.0", out Version _))
		{
			Log.Error("Default semantic version is wrong: {0}", new object[1] { "0.0.0.0" });
			return false;
		}
		Log.Verbose("[VersionManager] Migrating from SemanticVersion {0} to {1}", new object[2] { from, to });
		CacheManager.ClearVersioningViewModelCache();
		Log.Verbose("[VersionManager] Version migration successful", Array.Empty<object>());
		return true;
	}

	public static async void MigrateTribeRatings()
	{
		bool num = PolytopiaPlayerPrefs.HasKey("polytopia_tribe_scores");
		bool flag = PolytopiaPlayerPrefs.HasKey("polytopia_tribe_ratings");
		if (!num && !flag)
		{
			return;
		}
		Log.Verbose("Migrating scores & ratings...", Array.Empty<object>());
		bool migrationSuccess = true;
		TribeRatingsViewModel tribeRatingsViewModel = CacheManager.GetCachedTribeRatingsViewModel();
		if (tribeRatingsViewModel == null)
		{
			tribeRatingsViewModel = new TribeRatingsViewModel();
		}
		if (tribeRatingsViewModel.Ratings == null)
		{
			tribeRatingsViewModel.Ratings = new Dictionary<int, TribeRatingViewModel>();
		}
		List<TribeData> allTribes = PolytopiaDataManager.GetGameLogicData(VersionManager.GetGameLogicDataVersionFromGameVersion(VersionManager.GameVersion)).GetAllTribes();
		for (int i = 0; i < allTribes.Count; i++)
		{
			TribeData.Type type = allTribes[i].type;
			int tribeScore = PlayerPrefsUtils.GetTribeScore(allTribes[i].type);
			int num2 = (int)PlayerPrefsUtils.GetTribeRating(allTribes[i].type);
			if (tribeScore == -1 && num2 == -1)
			{
				continue;
			}
			if (tribeRatingsViewModel.Ratings.TryGetValue((int)type, out var value))
			{
				if (value.Rating.HasValue && value.Rating.Value < num2)
				{
					tribeRatingsViewModel.Ratings[(int)type].Rating = (uint)num2;
					Log.Verbose("- Migrating rating for {0}, new value: {1}", new object[2] { type, num2 });
				}
				if (value.Score.HasValue && value.Score.Value < tribeScore)
				{
					tribeRatingsViewModel.Ratings[(int)type].Score = (uint)tribeScore;
					Log.Verbose("- Migrating score for {0}, new value: {1}", new object[2] { type, tribeScore });
				}
			}
			else
			{
				value = new TribeRatingViewModel
				{
					TribeType = (int)type,
					Rating = ((num2 == -1) ? ((uint?)null) : new uint?((uint)num2)),
					Score = ((tribeScore == -1) ? ((uint?)null) : new uint?((uint)tribeScore))
				};
				Log.Verbose("- Adding rating & score for {0}, rating: {1}, score: {2}", new object[3] { type, num2, tribeScore });
				tribeRatingsViewModel.Ratings.Add((int)type, value);
			}
		}
		await CacheManager.CacheTribeRatingsViewModel(tribeRatingsViewModel);
		if (migrationSuccess)
		{
			PolytopiaPlayerPrefs.DeleteKey("polytopia_tribe_ratings");
			PolytopiaPlayerPrefs.DeleteKey("polytopia_tribe_scores");
		}
		Log.Verbose("Migration successful", Array.Empty<object>());
	}

	private static string GetLegacySaveFolderPath()
	{
		return Path.Combine(PolytopiaDirectory.PERSISTENT_DATA_PATH, "Legacy");
	}

	private static List<object> ReadArray(SharedObjectReader reader, List<string> stringTable, List<Traits> traitsTable, int fileSize)
	{
		List<object> list = new List<object>();
		int num = reader.ReadCompressedInt32();
		num >>= 1;
		if (ReadString(reader, stringTable) != string.Empty)
		{
			throw new NotImplementedException();
		}
		for (int i = 0; i < num; i++)
		{
			list.Add(ReadValue(reader, stringTable, traitsTable, fileSize));
		}
		return list;
	}

	private static object ReadValue(SharedObjectReader reader, List<string> stringTable, List<Traits> traitsTable, int fileSize)
	{
		switch (reader.ReadByte())
		{
		case 0:
			return null;
		case 1:
			return null;
		case 2:
			return false;
		case 3:
			return true;
		case 4:
			return reader.ReadCompressedInt32();
		case 5:
			return reader.ReadDouble();
		case 6:
			return ReadString(reader, stringTable);
		case 8:
			reader.ReadByte();
			return reader.ReadDouble();
		case 9:
			return ReadArray(reader, stringTable, traitsTable, fileSize);
		case 10:
			return ReadObject(reader, stringTable, traitsTable, fileSize);
		default:
			Log.Verbose("Unkown type", Array.Empty<object>());
			while (reader.BaseStream.Position < fileSize)
			{
				if (reader.ReadByte() == 0)
				{
					reader.BaseStream.Position--;
					return null;
				}
			}
			return null;
		}
	}

	private static string ReadString(SharedObjectReader reader, List<string> stringTable)
	{
		int num = reader.ReadCompressedInt32();
		bool num2 = (num & 1) > 0;
		num >>= 1;
		string text = null;
		if (!num2)
		{
			if (num < stringTable.Count)
			{
				text = stringTable[num];
			}
		}
		else
		{
			text = reader.ReadString(num);
			if (!string.IsNullOrEmpty(text))
			{
				stringTable.Add(text);
			}
		}
		return text;
	}

	private static Traits ReadTraits(SharedObjectReader reader, List<string> stringTable, List<Traits> traitsTable)
	{
		int num = reader.ReadCompressedInt32();
		if ((num & 3) == 1)
		{
			int num2 = num >> 2;
			if (traitsTable.Count <= num2)
			{
				throw new Exception("Invalid reference index: " + num2);
			}
			return traitsTable[num2];
		}
		int num3 = num >> 4;
		bool flag = (num & 4) == 4;
		bool flag2 = (num & 8) == 8;
		string typeName = ReadString(reader, stringTable);
		string[] array = new string[num3];
		for (int i = 0; i < num3; i++)
		{
			array[i] = ReadString(reader, stringTable);
		}
		Traits traits = new Traits
		{
			IsDynamic = flag2,
			IsExternalizable = flag,
			TypeName = typeName,
			ClassMembers = ((flag2 || flag) ? new string[0] : array)
		};
		traitsTable.Add(traits);
		return traits;
	}

	private static Dictionary<string, object> ReadObject(SharedObjectReader reader, List<string> stringTable, List<Traits> traitsTable, int fileSize)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		Traits traits = ReadTraits(reader, stringTable, traitsTable);
		for (int i = 0; i < traits.ClassMembers.Length; i++)
		{
			dictionary[traits.ClassMembers[i]] = ReadValue(reader, stringTable, traitsTable, fileSize);
		}
		if (traits.IsDynamic)
		{
			string text = ReadString(reader, stringTable);
			while (text != string.Empty)
			{
				dictionary[text] = ReadValue(reader, stringTable, traitsTable, fileSize);
				text = ReadString(reader, stringTable);
			}
		}
		return dictionary;
	}

	public static Dictionary<string, object> ReadSOLFile(string path)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		try
		{
			using MemoryStream input = new MemoryStream(PolytopiaFile.ReadAllBytes(path));
			using SharedObjectReader sharedObjectReader = new SharedObjectReader(input);
			Log.Verbose("[VersionManager] Reading legacy date {0}...", new object[1] { path });
			sharedObjectReader.ReadInt16();
			int num = sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadInt16();
			sharedObjectReader.ReadInt32();
			int num2 = num + 6;
			sharedObjectReader.ReadString();
			sharedObjectReader.ReadInt32();
			List<string> stringTable = new List<string>();
			List<Traits> traitsTable = new List<Traits>();
			while (sharedObjectReader.BaseStream.Position < num2)
			{
				string key = ReadString(sharedObjectReader, stringTable);
				if (dictionary.ContainsKey(key))
				{
					return dictionary;
				}
				dictionary[key] = ReadValue(sharedObjectReader, stringTable, traitsTable, num2);
			}
			return dictionary;
		}
		catch (Exception ex)
		{
			Log.Warning("[VersionManager] Failed to migrate legacy save data with error {0}", new object[1] { ex });
			return dictionary;
		}
	}

	public static string GetFloxLegacyID()
	{
		string legacySaveFolderPath = GetLegacySaveFolderPath();
		if (!PolytopiaDirectory.Exists(legacySaveFolderPath))
		{
			Log.Verbose("No shared objects path was found", Array.Empty<object>());
			return null;
		}
		string[] files = PolytopiaDirectory.GetFiles(legacySaveFolderPath, "Flox*.sol");
		if (files.Length == 0)
		{
			Log.Verbose("No flox files found", Array.Empty<object>());
			return null;
		}
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			Dictionary<string, object> dictionary = ReadSOLFile(array[i]);
			if (dictionary != null && dictionary.TryGetValue("currentPlayer", out var value) && value is Dictionary<string, object> dictionary2 && dictionary2.TryGetValue("id", out var value2))
			{
				return value2 as string;
			}
		}
		return null;
	}

	private static string GetLegacySaveFilePath(string file)
	{
		return Path.Combine(GetLegacySaveFolderPath(), file);
	}

	public static void MigrateMiscSettings()
	{
		if (PolytopiaDirectory.Exists(GetLegacySaveFolderPath()))
		{
			if (!PolytopiaPlayerPrefs.HasKey("polytopia_should_ask_for_restore"))
			{
				Log.Verbose("Setting should ask for restore", Array.Empty<object>());
				PolytopiaPlayerPrefs.SetInt("polytopia_should_ask_for_restore", 1);
				PolytopiaPlayerPrefs.Save();
			}
			SettingsUtils.IsFirstTime = false;
		}
	}

	public static bool MigrateLegacySaveData()
	{
		string legacySaveFilePath = GetLegacySaveFilePath("polytopia-bin.sol");
		if (!PolytopiaFile.Exists(legacySaveFilePath))
		{
			Log.Verbose("[VersionManager] No legacy save file found at path {0}", new object[1] { legacySaveFilePath });
			return true;
		}
		try
		{
			using MemoryStream input = new MemoryStream(PolytopiaFile.ReadAllBytes(legacySaveFilePath));
			using SharedObjectReader sharedObjectReader = new SharedObjectReader(input);
			Log.Verbose("[VersionManager] Migrating legacy GameData...", Array.Empty<object>());
			sharedObjectReader.ReadInt16();
			int num = sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadInt16();
			sharedObjectReader.ReadInt32();
			int num2 = num + 6;
			sharedObjectReader.ReadString();
			sharedObjectReader.ReadInt32();
			List<string> list = new List<string>();
			while (sharedObjectReader.BaseStream.Position < num2)
			{
				int num3 = sharedObjectReader.ReadCompressedInt32();
				bool num4 = (num3 & 1) > 0;
				num3 >>= 1;
				string text;
				if (num4)
				{
					text = sharedObjectReader.ReadString(num3);
					list.Add(text);
				}
				else
				{
					text = list[num3];
				}
				switch (sharedObjectReader.ReadByte())
				{
				case 2:
					Log.Verbose("- {0}: false", new object[1] { text });
					break;
				case 3:
					Log.Verbose("- {0}: true", new object[1] { text });
					break;
				case 4:
				{
					int num5 = sharedObjectReader.ReadCompressedInt32();
					Log.Verbose("- {0}: {1} (int)", new object[2] { text, num5 });
					break;
				}
				case 5:
				{
					double num8 = sharedObjectReader.ReadDouble();
					Log.Verbose("- {0}: {1} (double)", new object[2] { text, num8 });
					break;
				}
				case 6:
				{
					int num6 = sharedObjectReader.ReadCompressedInt32();
					bool num7 = (num6 & 1) > 0;
					num6 >>= 1;
					string text2 = null;
					if (!num7)
					{
						if (num3 < list.Count)
						{
							text2 = list[num3];
						}
					}
					else
					{
						text2 = sharedObjectReader.ReadString(num6);
						list.Add(text2);
					}
					if (text == "singlePlayer")
					{
						MigrateSingleplayerGame(text2);
					}
					else if (text.StartsWith("Local:"))
					{
						MigrateHotseatGame(text2);
					}
					break;
				}
				default:
					Log.Verbose("\t{0}: Unkown type", new object[1] { text });
					while (sharedObjectReader.BaseStream.Position < num2)
					{
						if (sharedObjectReader.ReadByte() == 0)
						{
							sharedObjectReader.BaseStream.Position--;
							break;
						}
					}
					break;
				case 1:
					break;
				}
				if (sharedObjectReader.BaseStream.Position < num2)
				{
					sharedObjectReader.ReadByte();
				}
			}
			BackupLegacySaveFile(legacySaveFilePath);
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("[VersionManager] Failed to migrate legacy save data with error {0}", new object[1] { ex });
			BackupLegacySaveFile(legacySaveFilePath);
			return false;
		}
	}

	public static List<string> GetAllBackupLegacySaveFilePaths()
	{
		List<string> list = new List<string>();
		string text = Path.Combine(Paths.GetBackupDirectoryPath(), "polytopia-bin-backup");
		string text2 = ".sol";
		string text3 = text + text2;
		if (PolytopiaFile.Exists(text3))
		{
			list.Add(text3);
			bool flag = true;
			int num = 0;
			while (flag)
			{
				text3 = string.Concat(text + ++num, text2);
				flag = PolytopiaFile.Exists(text3);
				if (flag)
				{
					list.Add(text3);
				}
			}
		}
		return list;
	}

	private static void BackupLegacySaveFile(string legacySaveFile)
	{
		try
		{
			if (!PolytopiaFile.Exists(legacySaveFile))
			{
				return;
			}
			Log.Verbose("[VersionManager] Backing up legacy save file...", Array.Empty<object>());
			string text = Path.Combine(Paths.GetBackupDirectoryPath(), "polytopia-bin-backup");
			string text2 = ".sol";
			if (PolytopiaFile.Exists(text + text2))
			{
				bool flag = true;
				int num = 0;
				while (flag)
				{
					string text3 = text + ++num;
					if (!PolytopiaFile.Exists(text3 + text2))
					{
						text = text3;
						flag = false;
					}
				}
			}
			PolytopiaFile.Move(legacySaveFile, text + text2);
			Log.Verbose("[VersionManager] Legacy save file backup successful", Array.Empty<object>());
		}
		catch (Exception ex)
		{
			Log.Warning("[VersionManager] Failed backup legacy save file with error {0}", new object[1] { ex });
		}
	}

	public static bool MigrateSingleplayerGame(string data)
	{
		string saveDirectoryPath = Paths.GetSaveDirectoryPath("Singleplayer");
		if (PolytopiaDirectory.Exists(saveDirectoryPath))
		{
			string[] files = PolytopiaDirectory.GetFiles(saveDirectoryPath, "*.state");
			if (files != null && files.Length != 0)
			{
				Log.Warning("[VersionManager] Singleplayer game already present, skipping migration...", Array.Empty<object>());
				return false;
			}
		}
		if (MigrateGameData(data, out var gameState))
		{
			LocalGameData serializable = new LocalGameData
			{
				initialGameState = gameState,
				currentGameState = gameState,
				lastSeenCommand = 0
			};
			string text = Guid.NewGuid().ToString();
			string singleplayerFilePath = Paths.GetSingleplayerFilePath(text);
			if (DiskSerializationHelpers.ToDisk(serializable, singleplayerFilePath, gameState.Version, out var _))
			{
				Log.Verbose("[VersionManager] Successfully created Singleplayer game data: {0}", new object[1] { text });
				return true;
			}
		}
		return false;
	}

	public static bool MigrateHotseatGame(string data)
	{
		if (MigrateGameData(data, out var gameState))
		{
			ushort[] array = new ushort[gameState.PlayerStates.Count];
			for (int i = 0; i < gameState.PlayerStates.Count; i++)
			{
				array[i] = gameState.LastProcessedCommand;
			}
			HotseatGameData serializable = new HotseatGameData
			{
				initialGameState = gameState,
				currentGameState = gameState,
				lastTurnGameState = gameState,
				lastSeenCommands = array
			};
			string text = Guid.NewGuid().ToString();
			string hotseatFilePath = Paths.GetHotseatFilePath(text);
			if (DiskSerializationHelpers.ToDisk(serializable, hotseatFilePath, gameState.Version, out var _))
			{
				Log.Verbose("[VersionManager] Successfully created Hotseat game data: {0}", new object[1] { text });
				return true;
			}
		}
		return false;
	}

	public static bool MigrateGameData(string gameData, out GameState gameState)
	{
		Debug.LogFormat("[VersionManager] Migrating GameData: \"{0}\"", new object[1] { gameData });
		byte[] inputBytes = Convert.FromBase64String(gameData);
		if (new AirGameMigration().MigrateGameData(SevenZipHelper.Decompress(inputBytes), VersionManager.GameVersion, null, null, out gameState, out var _))
		{
			return true;
		}
		Log.Warning("[VersionManager] Migration of GameData failed", Array.Empty<object>());
		return false;
	}

	public static bool MigrateLegacyConsentData()
	{
		if (SettingsUtils.HasPrivacyConsentKey)
		{
			return true;
		}
		string legacySaveFilePath = GetLegacySaveFilePath("userConsent.sol");
		if (!PolytopiaFile.Exists(legacySaveFilePath))
		{
			Log.Verbose("[VersionManager] No legacy Consent Data found at path {0}", new object[1] { legacySaveFilePath });
			return true;
		}
		try
		{
			using MemoryStream input = new MemoryStream(PolytopiaFile.ReadAllBytes(legacySaveFilePath));
			using SharedObjectReader sharedObjectReader = new SharedObjectReader(input);
			Log.Verbose("[VersionManager] Migrating legacy consent data...", Array.Empty<object>());
			sharedObjectReader.ReadInt16();
			sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadInt16();
			sharedObjectReader.ReadInt32();
			sharedObjectReader.ReadString();
			sharedObjectReader.ReadInt32();
			int length = (int)(sharedObjectReader.BaseStream.Length - sharedObjectReader.BaseStream.Position);
			string text = sharedObjectReader.ReadString(length);
			if (text.Contains("approved"))
			{
				Log.Info("- Consent approved!", Array.Empty<object>());
				SettingsUtils.PrivacyConsent = true;
			}
			else if (text.Contains("denied"))
			{
				Log.Info("- Consent denied!", Array.Empty<object>());
				SettingsUtils.PrivacyConsent = false;
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("[VersionManager] Failed to migrate legacy consent data with error {0}", new object[1] { ex });
			return false;
		}
	}

	private static void ClearDataFolder()
	{
		string[] files = PolytopiaDirectory.GetFiles(PolytopiaDirectory.PERSISTENT_DATA_PATH);
		for (int i = 0; i < files.Length; i++)
		{
			try
			{
				Log.Verbose("Delete file: {0}", new object[1] { files[i] });
				PolytopiaFile.Delete(files[i]);
			}
			catch (IOException ex)
			{
				Log.Error("{0}", new object[1] { ex });
			}
		}
		string[] directories = PolytopiaDirectory.GetDirectories(PolytopiaDirectory.PERSISTENT_DATA_PATH);
		for (int j = 0; j < directories.Length; j++)
		{
			try
			{
				Log.Verbose("Delete directory: {0}", new object[1] { directories[j] });
				PolytopiaDirectory.Delete(directories[j], recursively: true);
			}
			catch (IOException ex2)
			{
				Log.Error("{0}", new object[1] { ex2 });
			}
		}
	}
}
