using System;
using System.Collections.Generic;
using System.Diagnostics;
using Polytopia.Data;

public static class PolytopiaDataManager
{
	public static IPolytopiaDataProvider provider;

	public static GameLogicData currentVersion;

	private static readonly Dictionary<int, GameLogicData> gameLogicDatas = new Dictionary<int, GameLogicData>();

	private static readonly Dictionary<int, AvatarData> avatarDatas = new Dictionary<int, AvatarData>();

	public static AvatarData GetAvatarData(int version)
	{
		if (!avatarDatas.TryGetValue(version, out var value))
		{
			value = LoadAvatarData(version);
		}
		if (value != null)
		{
			return value;
		}
		Log.Error("Could not load avatar data version {0}", new object[1] { version });
		return null;
	}

	public static AvatarData LoadAvatarData(int version)
	{
		if (provider == null)
		{
			throw new Exception("Missing data provider");
		}
		if (avatarDatas.TryGetValue(version, out var value))
		{
			return value;
		}
		string text = provider.LoadAvatarData(version);
		if (string.IsNullOrEmpty(text))
		{
			Log.Error("Failed to load version {0}", new object[1] { version });
			return null;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		Log.Verbose("Loading avatar data version {0}...", new object[1] { version });
		value = new AvatarData();
		value.Parse(text);
		avatarDatas.Add(version, value);
		Log.Info("Avatar data version {0} loaded in {0} ms", new object[2]
		{
			version,
			stopwatch.Elapsed.TotalMilliseconds
		});
		return value;
	}

	public static GameLogicData GetGameLogicData(int version)
	{
		if (!gameLogicDatas.TryGetValue(version, out var value))
		{
			value = LoadGameLogicData(version);
		}
		if (value != null)
		{
			return value;
		}
		Log.Error("Could not load gameLogicData version {0}", new object[1] { version });
		return null;
	}

	public static GameLogicData LoadGameLogicData(int version)
	{
		if (provider == null)
		{
			throw new Exception("Missing data provider");
		}
		if (gameLogicDatas.TryGetValue(version, out var value))
		{
			return value;
		}
		string text = provider.LoadGameLogicData(version);
		if (string.IsNullOrEmpty(text))
		{
			Log.Error("Failed to load version {0}", new object[1] { version });
			return null;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		Log.Verbose("Loading game logic data version {0}...", new object[1] { version });
		value = new GameLogicData();
		value.Parse(text);
		gameLogicDatas.Add(version, value);
		Log.Info("Game logic data version {0} loaded in {0} ms", new object[2]
		{
			version,
			stopwatch.Elapsed.TotalMilliseconds
		});
		return value;
	}
}
