using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

public class DeepLinkManager
{
	public class IntentFilter
	{
		public string Host { get; set; }

		public string Version { get; set; }

		public string Path { get; set; }

		public string Name
		{
			get
			{
				if (Version != null)
				{
					return (Host + "/" + Version + "/" + Path).Trim('/');
				}
				return (Host + "/" + Path).Trim('/');
			}
		}

		public List<string> DefinedParameterKeys { get; set; }
	}

	public static Dictionary<string, IntentFilter> IntentSignatures = new List<IntentFilter>
	{
		new IntentFilter
		{
			Host = "newgame",
			DefinedParameterKeys = new List<string> { "gamemode", "tribe", "difficulty", "opponents" }
		},
		new IntentFilter
		{
			Host = "cm",
			Version = "v1",
			Path = "verify_account",
			DefinedParameterKeys = new List<string> { "ott" }
		},
		new IntentFilter
		{
			Host = "cm",
			Version = "v1",
			Path = "launch_game",
			DefinedParameterKeys = new List<string> { "id" }
		},
		new IntentFilter
		{
			Host = "cm_oauth_callback",
			DefinedParameterKeys = new List<string> { "code", "state" }
		},
		new IntentFilter
		{
			Host = "opengame",
			DefinedParameterKeys = new List<string> { "id" }
		},
		new IntentFilter
		{
			Host = "openstartedgame",
			DefinedParameterKeys = new List<string> { "id" }
		},
		new IntentFilter
		{
			Host = "multiplayerscreen",
			DefinedParameterKeys = new List<string>()
		},
		new IntentFilter
		{
			Host = "joinlobby",
			DefinedParameterKeys = new List<string> { "id" }
		},
		new IntentFilter
		{
			Host = "opentournament",
			DefinedParameterKeys = new List<string> { "id" }
		}
	}.ToDictionary((IntentFilter signature) => signature.Name, (IntentFilter signature) => signature);

	private readonly List<Intent> queuedIntents = new List<Intent>();

	private object queueLock = new object();

	private Intent processingIntent;

	private bool shouldAllowProcessing = true;

	public async void Initialize()
	{
		await InitializeAsync();
	}

	private async Task InitializeAsync()
	{
		Application.deepLinkActivated += async delegate(string url)
		{
			if (Uri.TryCreate(url, UriKind.Absolute, out Uri result2))
			{
				await ProcessAsync(result2);
			}
		};
		if (!string.IsNullOrEmpty(Application.absoluteURL) && Uri.TryCreate(Application.absoluteURL, UriKind.Absolute, out Uri result))
		{
			await ProcessAsync(result);
		}
		SteamApps.OnNewLaunchParameters += async delegate
		{
			if (!string.IsNullOrEmpty(SteamApps.CommandLine))
			{
				await ProcessFromSteamAsync();
			}
		};
		try
		{
			FacepunchHelpers.TryInit((uint)Config.steamAppId.IntValue);
			if (!string.IsNullOrEmpty(SteamApps.CommandLine))
			{
				await ProcessFromSteamAsync();
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString(), Array.Empty<object>());
		}
	}

	public bool IsProcessing()
	{
		if (processingIntent != null)
		{
			return processingIntent.State == Intent.ProcessState.Processing;
		}
		return false;
	}

	public void SetShouldAllowProcessing(bool shouldAllowProcessing)
	{
		this.shouldAllowProcessing = shouldAllowProcessing;
	}

	public async void ProcessQueued()
	{
		await ProcessQueuedAsync();
	}

	public bool ShouldProcessQueued()
	{
		if (IsProcessing() || !shouldAllowProcessing)
		{
			return false;
		}
		return queuedIntents.Count > 0;
	}

	private async Task ProcessQueuedAsync()
	{
		Task task = null;
		lock (queueLock)
		{
			if (queuedIntents.Count > 0)
			{
				processingIntent = queuedIntents[0];
				queuedIntents.RemoveAt(0);
				task = processingIntent.ProcessAsync();
			}
		}
		if (task != null)
		{
			await task;
		}
		if (queuedIntents.Count > 0)
		{
			await ProcessQueuedAsync();
		}
	}

	public async void Process(Uri uri)
	{
		await ProcessAsync(uri);
	}

	private async Task ProcessAsync(Uri uri)
	{
		if (uri == null || string.IsNullOrEmpty(uri.Host))
		{
			Log.Info("[DeepLinking] received empty intent.", Array.Empty<object>());
			return;
		}
		if (!IsKnownIntent(uri, out var intentSignature))
		{
			Log.Info("[DeepLinking] unknown intent: {0}", new object[1] { uri });
			return;
		}
		Log.Info("[DeepLinking] processing: {0}", new object[1] { uri });
		Intent intent;
		switch (intentSignature.Name)
		{
		case "cm/v1/verify_account":
			intent = new LinkCmAccountIntent(uri);
			break;
		case "cm/v1/launch_game":
			intent = new OpenGameIntent(uri);
			break;
		case "cm_oauth_callback":
			intent = new CmOAuthCallbackIntent(uri);
			break;
		case "opengame":
			intent = new OpenGameIntent(uri);
			break;
		case "openstartedgame":
			intent = new OpenStartedGameIntent(uri);
			break;
		case "multiplayerscreen":
			intent = new MultiplayerScreenIntent(uri);
			break;
		case "joinlobby":
			intent = new JoinLobbyIntent(uri);
			break;
		case "opentournament":
			intent = new OpenTournamentIntent(uri);
			break;
		default:
			Log.Info("[DeepLinking] could not process intent: {0}", new object[1] { uri });
			return;
		}
		lock (queueLock)
		{
			if (AllowsQueuingOfIntent(intent))
			{
				queuedIntents.Add(intent);
			}
		}
		if (ShouldProcessQueued())
		{
			await ProcessQueuedAsync();
		}
	}

	private async Task ProcessFromSteamAsync()
	{
		Log.Info("[DeepLinking] received from steam: {0}", new object[1] { SteamApps.CommandLine });
		string text = FormatSteamCommandLine(SteamApps.CommandLine);
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		UriBuilder uriBuilder = new UriBuilder(text)
		{
			Scheme = "steam"
		};
		if (!IsKnownIntent(uriBuilder.Uri, out var intentSignature))
		{
			Log.Info("[DeepLinking] unknown Steam intent: {0}", new object[1] { uriBuilder.Uri });
			return;
		}
		if (intentSignature.DefinedParameterKeys != null)
		{
			Dictionary<string, string> dictionary = (from key in intentSignature.DefinedParameterKeys
				select Tuple.Create(key, SteamApps.GetLaunchParam(key)) into kvp
				where kvp.Item2 != null
				select kvp).ToDictionary((Tuple<string, string> kvp) => kvp.Item1, (Tuple<string, string> kvp) => kvp.Item2);
			if (dictionary != null)
			{
				foreach (KeyValuePair<string, string> item in dictionary)
				{
					Log.Info("[DeepLinking] Steam intent parameter: {0}={1}", new object[2] { item.Key, item.Value });
				}
				string query = "?" + string.Join("&", dictionary.Select((KeyValuePair<string, string> kvp) => kvp.Key + "=" + kvp.Value));
				uriBuilder.Query = query;
			}
		}
		await ProcessAsync(uriBuilder.Uri);
	}

	private string FormatSteamCommandLine(string commandLine)
	{
		return commandLine.Replace("\\", "/");
	}

	private bool IsKnownIntent(Uri uri, out IntentFilter intentSignature)
	{
		string key = (uri.Host + uri.AbsolutePath).Trim('/');
		return IntentSignatures.TryGetValue(key, out intentSignature);
	}

	private bool AllowsQueuingOfIntent(Intent intent)
	{
		if (IsProcessing() && !intent.CanBeQueuedAfterIntent(processingIntent))
		{
			Log.Verbose("[DeepLinking] Intent {0} was blocked from queue by {1}", new object[2] { intent.Uri, processingIntent.Uri });
			return false;
		}
		for (int i = 0; i < queuedIntents.Count; i++)
		{
			Intent intent2 = queuedIntents[i];
			if (!intent.CanBeQueuedAfterIntent(intent2))
			{
				Log.Verbose("[DeepLinking] Intent {0} was blocked from queue by {1}", new object[2] { intent.Uri, intent2.Uri });
				return false;
			}
		}
		return true;
	}

	public void ProcessIntents()
	{
		if (PolytopiaPlayerPrefs.HasKey("cm_verification_uri"))
		{
			string uriString = PolytopiaPlayerPrefs.GetString("cm_verification_uri");
			Process(new Uri(uriString));
		}
		else if (ShouldProcessQueued())
		{
			ProcessQueued();
		}
	}
}
