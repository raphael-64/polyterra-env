using System;
using System.Threading.Tasks;

public class OpenStartedGameIntent : OpenGameIntent
{
	public OpenStartedGameIntent(Uri uri)
		: base(uri)
	{
	}

	protected override async Task<bool> OpenGame(Guid gameId)
	{
		if (!(await EnsureLoggedIn()))
		{
			return false;
		}
		shouldFadeOut = false;
		if (!(await GameManager.Instance.OpenMultiplayerGame(gameId)))
		{
			shouldFadeOut = true;
			Log.Error("Failed to open game: OpenMultiplayerGame failed", Array.Empty<object>());
			return false;
		}
		return true;
	}
}
