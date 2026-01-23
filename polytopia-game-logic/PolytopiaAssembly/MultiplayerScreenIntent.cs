using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class MultiplayerScreenIntent : Intent
{
	private TaskCompletionSource<bool> taskCompletion;

	private Guid gameId;

	public MultiplayerScreenIntent(Uri uri)
		: base(uri)
	{
	}

	public override bool CanBeQueuedAfterIntent(Intent intent)
	{
		return intent.Uri != base.Uri;
	}

	public override async Task HandleAsync()
	{
		if (!(await DeepLinkOpenMultiplayerScreen()))
		{
			NotificationManager.Notify(Localization.Get("misc.errorloadinggame"));
		}
		Log.Info("[DeepLinking] processed intent.", Array.Empty<object>());
		base.State = ProcessState.Processed;
	}

	private Task<bool> DeepLinkOpenMultiplayerScreen()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
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
			UIManager.OpenMultiplayerScreen();
			taskCompletion.SetResult(result: true);
		}
		return taskCompletion.Task;
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
			UIManager.OpenMultiplayerScreen();
			taskCompletion.SetResult(result: true);
		}
	}
}
