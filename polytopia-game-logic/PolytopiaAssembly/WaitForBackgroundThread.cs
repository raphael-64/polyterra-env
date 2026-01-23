using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class WaitForBackgroundThread
{
	public ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
	{
		return Task.Run(delegate
		{
		}).ConfigureAwait(continueOnCapturedContext: false).GetAwaiter();
	}
}
