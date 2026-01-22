using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public static class AwaitExtensions
{
	public static TaskAwaiter<int> GetAwaiter(this Process process)
	{
		TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
		process.EnableRaisingEvents = true;
		process.Exited += delegate
		{
			tcs.TrySetResult(process.ExitCode);
		};
		if (process.HasExited)
		{
			tcs.TrySetResult(process.ExitCode);
		}
		return tcs.Task.GetAwaiter();
	}

	public static async void WrapErrors(this Task task)
	{
		await task;
	}
}
