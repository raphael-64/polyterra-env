using System;
using System.Threading.Tasks;
using UnityEngine.SocialPlatforms;

public static class PlayGamesPlatformExtensions
{
	public static Task<bool> AuthenticateAsync(this ILocalUser localUser)
	{
		TaskCompletionSource<bool> authenticateCompletionSource = new TaskCompletionSource<bool>();
		localUser.Authenticate((Action<bool>)delegate(bool success)
		{
			authenticateCompletionSource.SetResult(success);
		});
		return authenticateCompletionSource.Task;
	}
}
