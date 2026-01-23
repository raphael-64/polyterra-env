using System;

namespace Tesla;

public abstract class NativeWrapper : IDisposable
{
	protected abstract void ReleaseNativeResources();

	~NativeWrapper()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			GC.SuppressFinalize(this);
		}
		ReleaseNativeResources();
	}
}
