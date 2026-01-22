using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolytopiaBackendBase.Challengermode;
using UnityEngine.SceneManagement;

public class OpenTournamentIntent : Intent
{
	private TaskCompletionSource<bool> taskCompletion;

	private Guid tournamentId;

	public OpenTournamentIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return intent.Uri != base.Uri;
	}

	public override async Task HandleAsync()
	{
		await DeepLinkOpenTournamentScreen();
		Log.Info("[DeepLinking] processed intent.", Array.Empty<object>());
		base.State = ProcessState.Processed;
	}

	private Task<bool> DeepLinkOpenTournamentScreen()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		tournamentId = ParseTournamentId(base.QueryMap);
		Scene activeScene = SceneManager.GetActiveScene();
		bool num = ((Scene)(ref activeScene)).name == "Level";
		taskCompletion = new TaskCompletionSource<bool>();
		if (num)
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
			GameManager.ReturnToMenu();
		}
		else
		{
			if (GameManager.IsMultiplayerEnabled)
			{
				UIManager.OpenTournamentsScreen();
				OpenTournamentPopupAsync();
			}
			else
			{
				UIManager.OpenMultiplayerScreen();
			}
			taskCompletion.SetResult(result: true);
		}
		return taskCompletion.Task;
	}

	private async void OpenTournamentPopupAsync()
	{
		if (!(tournamentId == Guid.Empty))
		{
			TournamentViewModel data = await GameManager.GetTournamentManager().GetTournament(tournamentId);
			TournamentInfoPopup tournamentInfoPopup = PopupManager.GetTournamentInfoPopup();
			tournamentInfoPopup.SetData(data);
			tournamentInfoPopup.Show();
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (((Scene)(ref scene)).name != "StartScene")
		{
			Log.Error("Failed to open game: Failed to return to start scene", Array.Empty<object>());
			taskCompletion.SetResult(result: false);
		}
		else
		{
			UIManager.OpenTournamentsScreen();
			taskCompletion.SetResult(result: true);
		}
	}

	private Guid ParseTournamentId(Dictionary<string, string> queryVariables)
	{
		if (queryVariables.TryGetValue("id", out var value) && Guid.TryParse(value, out var result))
		{
			return result;
		}
		return Guid.Empty;
	}
}
