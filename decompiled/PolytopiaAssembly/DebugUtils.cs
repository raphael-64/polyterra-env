using System;
using System.Collections.Generic;
using K4os.Compression.LZ4;
using Polytopia.IO;
using UnityEngine;

public static class DebugUtils
{
	public static void CopyFilesToClipBoard(List<string> paths)
	{
		string text = "";
		foreach (string path in paths)
		{
			if (PolytopiaFile.Exists(path))
			{
				text = text + path + "\n\n" + Convert.ToBase64String(LZ4Pickler.Pickle(PolytopiaFile.ReadAllBytes(path), (LZ4Level)12)) + "\n\n";
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			GUIUtility.systemCopyBuffer = text;
			NotificationManager.Notify("Copied game data to clipboard!");
		}
	}

	public static string GetLorem(int characterLimit = int.MaxValue)
	{
		string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
		return text.Substring(0, Mathf.Min(characterLimit, text.Length));
	}

	public static void LogState(GameState state, ClientBase client, int level)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		if (state == null)
		{
			return;
		}
		int num = 1;
		DebugOverlay.DrawText(1f, (float)num++, Color.red, "GameState ({0}):", new object[1] { state.CurrentState });
		DebugOverlay.DrawText(2f, (float)num++, "Game Version: {0}", new object[1] { state.Version });
		DebugOverlay.DrawText(2f, (float)num++, "Turn: {0}", new object[1] { state.CurrentTurn });
		if (client != null)
		{
			DebugOverlay.DrawText(2f, (float)num++, "Player: {0}{1}", new object[2]
			{
				state.CurrentPlayer,
				client.ActionManager.IsRecap ? " (REPLAY)" : ""
			});
		}
		else
		{
			DebugOverlay.DrawText(2f, (float)num++, "Player: {0} (REPLAY)", new object[1] { state.CurrentPlayer });
		}
		num++;
		if (client != null)
		{
			DebugOverlay.DrawText(2f, (float)num++, "Replay: {0}", new object[1] { client.ActionManager.IsRecap });
		}
		DebugOverlay.DrawText(2f, (float)num++, "Total commands: {0}", new object[1] { (state.CommandStack != null) ? state.CommandStack.Count : 0 });
		if (client != null)
		{
			DebugOverlay.DrawText(2f, (float)num++, "Last command: {0}/{1}", new object[2]
			{
				client.ActionManager.LastSeenCommand,
				state.LastProcessedCommand
			});
		}
		DebugOverlay.DrawText(3f, (float)num++, "CommandTriggers: {0}", new object[1] { (state.pendingCommandTriggers != null) ? state.pendingCommandTriggers.Count : 0 });
		DebugOverlay.DrawText(3f, (float)num++, "Actions: {0}", new object[1] { (state.ActionStack != null) ? state.ActionStack.Count : 0 });
		num++;
		DebugOverlay.DrawText(1f, (float)num++, Color.red, "GameType: {0}", new object[1] { state.Settings?.GameType });
		DebugOverlay.DrawText(1f, (float)num++, Color.red, "GameMode: {0}", new object[1] { state.Settings?.BaseGameMode });
		DebugOverlay.DrawText(2f, (float)num++, "TurnLimit: {0}", new object[1] { state.Settings?.rules.TurnLimit });
		DebugOverlay.DrawText(2f, (float)num++, "ScoreLimit: {0}", new object[1] { state.Settings?.rules.ScoreLimit });
		DebugOverlay.DrawText(2f, (float)num++, "WinByCapital: {0}", new object[1] { state.Settings?.rules.WinByCapital });
		DebugOverlay.DrawText(2f, (float)num++, "Tech Sharing: {0}", new object[1] { state.Settings?.rules.AllowTechSharing });
		DebugOverlay.DrawText(2f, (float)num++, "Mirror Picking: {0}", new object[1] { state.Settings?.rules.AllowMirrorPick });
		DebugOverlay.DrawText(2f, (float)num++, "Special tribes: {0}", new object[1] { state.Settings?.rules.AllowSpecialTribes });
		DebugOverlay.DrawText(2f, (float)num++, "Death condition: {0}", new object[1] { state.Settings?.rules.PlayerDeathCondition });
		num++;
		if (level < 2)
		{
			return;
		}
		DebugOverlay.DrawText(1f, (float)num++, "Players:", Array.Empty<object>());
		if (state.PlayerStates == null)
		{
			return;
		}
		foreach (PlayerState playerState in state.PlayerStates)
		{
			DebugOverlay.DrawText(2f, (float)num++, Color.red, "Player {0}: {1} ({2})", new object[3] { playerState.Id, playerState.tribe, playerState.UserName });
			DebugOverlay.DrawText(3f, (float)num++, "UserId: {0}", new object[1] { playerState.AccountId?.ToString() ?? "none" });
			if (level < 3)
			{
				continue;
			}
			if (client != null && client is HotseatClient hotseatClient)
			{
				DebugOverlay.DrawText(3f, (float)num++, "LastSeenCommand: {0}", new object[1] { hotseatClient.GetLastSeenCommand() });
			}
			DebugOverlay.DrawText(3f, (float)num++, "AutoPlay: {0}", new object[1] { playerState.AutoPlay });
			DebugOverlay.DrawText(3f, (float)num++, "Start tile: {0}", new object[1] { playerState.startTile });
			DebugOverlay.DrawText(3f, (float)num++, "Score: {0}", new object[1] { playerState.score });
			DebugOverlay.DrawText(3f, (float)num++, "Population: {0}", new object[1] { playerState.CountPopulation(state) });
			DebugOverlay.DrawText(3f, (float)num++, "Currency: {0}", new object[1] { playerState.Currency });
			DebugOverlay.DrawText(3f, (float)num++, "Cities: {0}", new object[1] { playerState.cities });
			if (playerState.availableTech != null && playerState.availableTech.Count > 0)
			{
				DebugOverlay.DrawText(3f, (float)num++, "Available tech: ", Array.Empty<object>());
				for (int i = 0; i < playerState.availableTech.Count; i++)
				{
					DebugOverlay.DrawText(4f, (float)num++, "- {0}", new object[1] { playerState.availableTech[i] });
				}
			}
			num++;
		}
	}

	public static void LogTile(Tile tile, GameState state)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)tile == (Object)null || state == null)
		{
			return;
		}
		int num = 1;
		int num2 = 1;
		DebugOverlay.DrawText((float)num, (float)num2++, Color.red, "TileInstance", Array.Empty<object>());
		if ((Object)(object)tile.Unit != (Object)null)
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, "Unit: {0}", new object[1] { ((Object)tile.Unit).name });
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, "Unit.Tile: {0}", new object[1] { tile.Unit.Tile.Coordinates });
		}
		else
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, "No Unit", Array.Empty<object>());
		}
		num2++;
		if (tile.Data == null)
		{
			return;
		}
		DebugOverlay.DrawText((float)num, (float)num2++, Color.red, "TileData", Array.Empty<object>());
		DebugOverlay.DrawText((float)(num + 1), (float)num2++, "Coordinates: {0}", new object[1] { tile.Data.coordinates });
		DebugOverlay.DrawText((float)(num + 1), (float)num2++, "Terrain: {0}", new object[1] { tile.Data.terrain });
		DebugOverlay.DrawText((float)(num + 1), (float)num2++, "Owner: {0}", new object[1] { tile.Data.owner });
		if (tile.Data.improvement != null)
		{
			state.GameLogicData.TryGetData(tile.Data.improvement.type, out var data);
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, Color.red, "Improvement: {0}", new object[1] { tile.Data.improvement.type });
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Level: {0}/{1}", new object[2]
			{
				tile.Data.improvement.level,
				data.maxLevel
			});
		}
		else
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, Color.red, "Improvement: None", Array.Empty<object>());
		}
		if (tile.Data.unit != null)
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, Color.red, "Unit: {0} (ID: {1})", new object[2]
			{
				tile.Data.unit.type,
				tile.Data.unit.id
			});
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Owner: {0}", new object[1] { tile.Data.unit.owner });
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Home: {0}", new object[1] { tile.Data.unit.home });
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Moved: {0}", new object[1] { tile.Data.unit.moved });
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Attacked {0}", new object[1] { tile.Data.unit.attacked });
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Follower {0}", new object[1] { tile.Data.unit.follower });
			DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Following {0}", new object[1] { tile.Data.unit.leader });
			if (tile.Data.unit.passengerUnit != null)
			{
				DebugOverlay.DrawText((float)(num + 2), (float)num2++, Color.red, "Passenger Unit: {0} (ID: {1})", new object[2]
				{
					tile.Data.unit.passengerUnit.type,
					tile.Data.unit.passengerUnit.id
				});
				DebugOverlay.DrawText((float)(num + 3), (float)num2++, "Owner: {0}", new object[1] { tile.Data.unit.passengerUnit.owner });
				DebugOverlay.DrawText((float)(num + 3), (float)num2++, "Home: {0}", new object[1] { tile.Data.unit.passengerUnit.home });
				DebugOverlay.DrawText((float)(num + 3), (float)num2++, "Moved: {0}", new object[1] { tile.Data.unit.passengerUnit.moved });
				DebugOverlay.DrawText((float)(num + 3), (float)num2++, "Attacked {0}", new object[1] { tile.Data.unit.passengerUnit.attacked });
				DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Follower {0}", new object[1] { tile.Data.unit.passengerUnit.follower });
				DebugOverlay.DrawText((float)(num + 2), (float)num2++, "Following {0}", new object[1] { tile.Data.unit.passengerUnit.leader });
			}
		}
		else
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, Color.red, "Unit: None", Array.Empty<object>());
		}
		if (tile.Data.resource != null)
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, Color.red, "Resource: {0}", new object[1] { tile.Data.resource.type });
		}
		else
		{
			DebugOverlay.DrawText((float)(num + 1), (float)num2++, Color.red, "Resource: None", Array.Empty<object>());
		}
	}

	public static void DebugCheckUnits(GameState gameState)
	{
		Dictionary<uint, WorldCoordinates> dictionary = new Dictionary<uint, WorldCoordinates>();
		TileData[] tiles = gameState.Map.Tiles;
		foreach (TileData tileData in tiles)
		{
			if (tileData.unit == null)
			{
				continue;
			}
			if (dictionary.TryGetValue(tileData.unit.id, out var value))
			{
				Log.Error("Found duplicate unit id {0} at {1} and {2}", new object[3]
				{
					tileData.unit.id,
					tileData.coordinates,
					value
				});
			}
			else
			{
				dictionary[tileData.unit.id] = tileData.coordinates;
			}
			if (tileData.unit.passengerUnit != null)
			{
				if (dictionary.TryGetValue(tileData.unit.passengerUnit.id, out var value2))
				{
					Log.Error("Found duplicate passnger unit id {0} at {1} and {2}", new object[3]
					{
						tileData.unit.passengerUnit.id,
						tileData.coordinates,
						value2
					});
				}
				else
				{
					dictionary[tileData.unit.passengerUnit.id] = tileData.coordinates;
				}
			}
			if (tileData.unit.coordinates != tileData.coordinates)
			{
				Log.Error("Found position mismatch for unit id {0} at unit coordinates {1} and tile coordinates {2}", new object[3]
				{
					tileData.unit.id,
					tileData.unit.coordinates,
					tileData.coordinates
				});
			}
		}
	}
}
