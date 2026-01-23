using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityAsyncAwaitUtil;

public class AsyncUtilTests : MonoBehaviour
{
	private const string AssetBundleSampleUrl = "http://www.stevevermeulen.com/wp-content/uploads/2017/09/teapot.unity3d";

	private const string AssetBundleSampleAssetName = "Teapot";

	[SerializeField]
	private TestButtonHandler.Settings _buttonSettings;

	private TestButtonHandler _buttonHandler;

	public void Awake()
	{
		_buttonHandler = new TestButtonHandler(_buttonSettings);
	}

	public void OnGUI()
	{
		_buttonHandler.Restart();
		if (_buttonHandler.Display("Test await seconds"))
		{
			RunAwaitSecondsTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test return value"))
		{
			RunReturnValueTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test try-catch exception"))
		{
			RunTryCatchExceptionTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test unhandled exception"))
		{
			RunUnhandledExceptionTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test IEnumerator"))
		{
			RunIEnumeratorTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test IEnumerator with return value (untyped)"))
		{
			RunIEnumeratorUntypedStringTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test IEnumerator with return value (typed)"))
		{
			RunIEnumeratorStringTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test IEnumerator unhandled exception"))
		{
			RunIEnumeratorUnhandledExceptionAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test IEnumerator try-catch exception"))
		{
			RunIEnumeratorTryCatchExceptionAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Load assetbundle"))
		{
			RunAsyncOperationAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test opening notepad"))
		{
			RunOpenNotepadTestAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test www download"))
		{
			RunWwwAsync().WrapErrors();
		}
		if (_buttonHandler.Display("Test Call Async from coroutine"))
		{
			((MonoBehaviour)this).StartCoroutine(RunAsyncFromCoroutineTest());
		}
		if (_buttonHandler.Display("Test multiple threads"))
		{
			RunMultipleThreadsTestAsync().WrapErrors();
		}
	}

	private IEnumerator RunAsyncFromCoroutineTest()
	{
		Debug.Log((object)"Waiting 1 second...");
		yield return (object)new WaitForSeconds(1f);
		Debug.Log((object)"Waiting 1 second again...");
		yield return RunAsyncFromCoroutineTest2().AsIEnumerator();
		Debug.Log((object)"Done");
	}

	private async Task RunMultipleThreadsTestAsync()
	{
		PrintCurrentThreadContext("Start");
		await Task.Delay(TimeSpan.FromSeconds(1.0));
		PrintCurrentThreadContext("After delay");
		await new WaitForBackgroundThread();
		PrintCurrentThreadContext("After WaitForBackgroundThread");
		Debug.Log((object)"Waiting 1 second...");
		await Task.Delay(TimeSpan.FromSeconds(1.0));
		PrintCurrentThreadContext("After Waiting");
		await new WaitForUpdate();
		PrintCurrentThreadContext("After WaitForUpdate");
	}

	private async Task RunMultipleThreadsTestAsyncWait()
	{
		PrintCurrentThreadContext("RunMultipleThreadsTestAsyncWait1");
		await new WaitForSeconds(1f);
		PrintCurrentThreadContext("RunMultipleThreadsTestAsyncWait2");
	}

	private void PrintCurrentThreadContext(string prefix = null)
	{
		Debug.Log((object)string.Format("{0}Current Thread: {1}, Scheduler: {2}", (prefix == null) ? "" : (prefix + ": "), Thread.CurrentThread.ManagedThreadId, (SynchronizationContext.Current == null) ? "null" : SynchronizationContext.Current.GetType().Name));
	}

	private async Task RunAsyncFromCoroutineTest2()
	{
		await new WaitForSeconds(1f);
	}

	private async Task RunWwwAsync()
	{
		Debug.Log((object)"Downloading asset bundle using WWW");
		byte[] bytes = (await new WWW("http://www.stevevermeulen.com/wp-content/uploads/2017/09/teapot.unity3d")).bytes;
		Debug.Log((object)("Downloaded " + bytes.Length / 1024 + " kb"));
	}

	private async Task RunOpenNotepadTestAsync()
	{
		Debug.Log((object)"Waiting for user to close notepad...");
		await Process.Start("notepad.exe");
		Debug.Log((object)"Closed notepad");
	}

	private async Task RunUnhandledExceptionTestAsync()
	{
		await WaitThenThrowException();
	}

	private async Task RunTryCatchExceptionTestAsync()
	{
		try
		{
			await NestedRunAsync();
		}
		catch (Exception ex)
		{
			Debug.Log((object)("Caught exception! " + ex.Message));
		}
	}

	private async Task NestedRunAsync()
	{
		await new WaitForSeconds(1f);
		throw new Exception("foo");
	}

	private async Task WaitThenThrowException()
	{
		await new WaitForSeconds(1.5f);
		throw new Exception("asdf");
	}

	private async Task RunAsyncOperationAsync()
	{
		await InstantiateAssetBundleAsync("http://www.stevevermeulen.com/wp-content/uploads/2017/09/teapot.unity3d", "Teapot");
	}

	private async Task InstantiateAssetBundleAsync(string abUrl, string assetName)
	{
		Debug.Log((object)"Downloading asset bundle data...");
		AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(await DownloadRawDataAsync(abUrl));
		Object.Instantiate<GameObject>((GameObject)(await assetBundle.LoadAssetAsync<GameObject>(assetName)));
		assetBundle.Unload(false);
		Debug.Log((object)"Asset bundle instantiated");
	}

	private async Task<byte[]> DownloadRawDataAsync(string url)
	{
		UnityWebRequest request = UnityWebRequest.Get(url);
		await request.SendWebRequest();
		return request.downloadHandler.data;
	}

	private async Task RunIEnumeratorTryCatchExceptionAsync()
	{
		try
		{
			await WaitThenThrow();
		}
		catch (Exception ex)
		{
			Debug.Log((object)("Caught exception! " + ex.Message));
		}
	}

	private async Task RunIEnumeratorUnhandledExceptionAsync()
	{
		await WaitThenThrow();
	}

	private IEnumerator WaitThenThrow()
	{
		yield return WaitThenThrowNested();
	}

	private IEnumerator WaitThenThrowNested()
	{
		Debug.Log((object)"Waiting 1 second...");
		yield return (object)new WaitForSeconds(1f);
		throw new Exception("zxcv");
	}

	private async Task RunIEnumeratorStringTestAsync()
	{
		Debug.Log((object)"Waiting for ienumerator...");
		Debug.Log((object)("Done! Result: " + await WaitForString()));
	}

	private async Task RunIEnumeratorUntypedStringTestAsync()
	{
		Debug.Log((object)"Waiting for ienumerator...");
		string text = (string)(await WaitForStringUntyped());
		Debug.Log((object)("Done! Result: " + text));
	}

	private async Task RunIEnumeratorTestAsync()
	{
		Debug.Log((object)"Waiting for ienumerator...");
		await WaitABit();
		Debug.Log((object)"Done!");
	}

	private IEnumerator<string> WaitForString()
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < 2f)
		{
			yield return null;
		}
		yield return "bsdfgas";
	}

	private IEnumerator WaitForStringUntyped()
	{
		yield return WaitABit();
		yield return "qwer";
	}

	private IEnumerator WaitABit()
	{
		yield return (object)new WaitForSeconds(1.5f);
	}

	private async Task RunReturnValueTestAsync()
	{
		Debug.Log((object)"Waiting to get value...");
		Debug.Log((object)("Got value: " + await GetValueExampleAsync()));
	}

	private async Task<string> GetValueExampleAsync()
	{
		await new WaitForSeconds(1f);
		return "asdf";
	}

	private async Task RunAwaitSecondsTestAsync()
	{
		Debug.Log((object)"Waiting 1 second...");
		await new WaitForSeconds(1f);
		Debug.Log((object)"Done!");
	}
}
