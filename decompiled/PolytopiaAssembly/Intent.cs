using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Intent
{
	public enum ProcessState
	{
		Unknown,
		None,
		Queued,
		Processing,
		Processed,
		Failed,
		Cancelled
	}

	public Uri Uri { get; private set; }

	public ProcessState State { get; protected set; }

	public Dictionary<string, string> QueryMap => (from pair in Uri.Query.TrimStart('?').Split('&')
		select pair.Split('=') into pair
		select Tuple.Create(pair.ElementAtOrDefault(0), pair.ElementAtOrDefault(1)) into kvp
		where kvp.Item1 != null && kvp.Item2 != null
		select kvp).ToDictionary((Tuple<string, string> kvp) => kvp.Item1, (Tuple<string, string> kvp) => kvp.Item2);

	public Intent(Uri uri)
	{
		Uri = uri;
		State = ProcessState.None;
	}

	public async Task ProcessAsync()
	{
		if (State != ProcessState.Processing)
		{
			Log.Info("[DeepLinking] processing intent: {0}", new object[1] { Uri });
			State = ProcessState.Processing;
			await HandleAsync();
		}
	}

	public virtual Task HandleAsync()
	{
		return Task.CompletedTask;
	}

	public void Resolve(bool resolved)
	{
		State = (resolved ? ProcessState.Processed : ProcessState.Failed);
	}

	public virtual bool CanBeQueuedAfterIntent(Intent intent)
	{
		return true;
	}

	internal bool IsDone()
	{
		if (State != ProcessState.None && State != ProcessState.Processed && State != ProcessState.Failed)
		{
			return State == ProcessState.Cancelled;
		}
		return true;
	}
}
