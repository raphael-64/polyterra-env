using System.Threading;
using System.Threading.Tasks;

namespace Tesla;

internal class AsyncResult<T>
{
	private SemaphoreSlim ev = new SemaphoreSlim(0, 1);

	public T result { get; private set; }

	public async Task WaitAsync()
	{
		await ev.WaitAsync();
	}

	public void Set(T r)
	{
		result = r;
		ev.Release();
	}
}
