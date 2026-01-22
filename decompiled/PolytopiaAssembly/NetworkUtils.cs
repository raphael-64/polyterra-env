using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public static class NetworkUtils
{
	public const int DEFAULT_DELAY = 400;

	private static Task delayTask;

	private static CancellationTokenSource cancellationTokenSource;

	public static void ShowLoader(int millisecondDelay = 400, float duration = 0f)
	{
		ShowLoader(Localization.Get("onlineview.loading.title"), millisecondDelay, duration);
	}

	public static void ShowLoader(string message, int millisecondDelay = 400, float duration = 0f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ShowLoader(message, NotificationManager.NETWORK_ALERT_COLOR, millisecondDelay, duration);
	}

	public static void ShowLoaderError(string message, float duration = 2f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		ShowLoader(message, NotificationManager.NETWORK_ERROR_COLOR, 0, duration);
	}

	public static async void ShowLoader(string message, Color color, int millisecondDelay = 400, float duration = 0f)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (millisecondDelay > 0)
		{
			cancellationTokenSource = new CancellationTokenSource();
			delayTask = Task.Delay(millisecondDelay, cancellationTokenSource.Token);
			try
			{
				await delayTask;
				if (!cancellationTokenSource.Token.IsCancellationRequested)
				{
					NotificationManager.NetworkAlert(message, color, instant: false, duration);
				}
				return;
			}
			catch (TaskCanceledException) when (cancellationTokenSource.Token.IsCancellationRequested)
			{
				return;
			}
		}
		CancelLoader();
		NotificationManager.NetworkAlert(message, color, instant: false, duration);
	}

	public static void HideLoader()
	{
		CancelLoader();
		NotificationManager.HideNetworkAlert();
	}

	public static void CancelLoader()
	{
		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
		}
	}
}
