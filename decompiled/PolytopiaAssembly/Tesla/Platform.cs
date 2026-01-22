using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

namespace Tesla;

public class Platform : NativeWrapper
{
	public enum Status
	{
		Uninitialized,
		Unavailable,
		OK
	}

	public delegate void AudioStateHandler(AudioState value);

	public delegate void DeepLinkHandler(string value);

	public delegate void AppStateHandler(AppState value);

	private class VirtualKeyboardImpl : VirtualKeyboard
	{
		private static Dictionary<IntPtr, object> senderList = new Dictionary<IntPtr, object>();

		private static Dictionary<IntPtr, VirtualKeyboardTextEntryHandler> handlerList = new Dictionary<IntPtr, VirtualKeyboardTextEntryHandler>();

		public override bool isVisible => API.Tesla_VirtualKeyboard_IsVisible(instance.clientPtr);

		public static VirtualKeyboardTextEntryHandler LookupHandler(IntPtr hashCode)
		{
			if (handlerList.ContainsKey(hashCode))
			{
				return handlerList[hashCode];
			}
			return null;
		}

		public static object LookupOwner(IntPtr hashCode)
		{
			if (senderList.ContainsKey(hashCode))
			{
				return senderList[hashCode];
			}
			return null;
		}

		public static IntPtr RegisterSession(VirtualKeyboardTextEntryHandler handler, object owner)
		{
			if (owner == null)
			{
				return IntPtr.Zero;
			}
			IntPtr intPtr = (IntPtr)handler.GetHashCode();
			if (!senderList.ContainsKey(intPtr))
			{
				senderList.Add(intPtr, owner);
			}
			if (!handlerList.ContainsKey(intPtr))
			{
				handlerList.Add(intPtr, handler);
			}
			return intPtr;
		}

		public static void RemoveSession(IntPtr hashCode)
		{
			if (senderList.ContainsKey(hashCode))
			{
				senderList.Remove(hashCode);
			}
			if (handlerList.ContainsKey(hashCode))
			{
				handlerList.Remove(hashCode);
			}
		}

		[MonoPInvokeCallback(typeof(API.VirtualKeyboardVisibilityDelegate))]
		public static void VisibilityChangedFunc(IntPtr clientPtr, bool visible, IntPtr userData)
		{
			virtualKeyboard.Internal_VirtualKeyboardVisibilityChange(visible);
		}

		[MonoPInvokeCallback(typeof(API.VirtualKeyboardTextStartedDelegate))]
		public static void SimpleTextStartedFunc(IntPtr clientPtr, IntPtr sessionID, IntPtr text, int cursorPos, IntPtr userData)
		{
			VirtualKeyboardTextEntryHandler handler = LookupHandler(sessionID);
			if (handler == null)
			{
				Debug.Log((object)"Tesla API Error SimpleTextStartedFunc: failed to find handler");
				return;
			}
			Task.Run(delegate
			{
				handler.OnTextEntryStarted(LookupOwner(sessionID), NativeString.PointerToString(text), cursorPos);
			});
		}

		[MonoPInvokeCallback(typeof(API.VirtualKeyboardTextUpdatedDelegate))]
		public static void SimpleTextUpdatedFunc(IntPtr clientPtr, IntPtr sessionID, IntPtr text, int cursorPos, IntPtr userData)
		{
			VirtualKeyboardTextEntryHandler handler = LookupHandler(sessionID);
			if (handler == null)
			{
				Debug.Log((object)"Tesla API Error SimpleTextUpdatedFunc: failed to find handler");
				return;
			}
			Task.Run(delegate
			{
				handler.OnTextEntryUpdated(LookupOwner(sessionID), NativeString.PointerToString(text), cursorPos);
			});
		}

		[MonoPInvokeCallback(typeof(API.VirtualKeyboardTextSubmittedDelegate))]
		public static void SimpleTextSubmittedFunc(IntPtr clientPtr, IntPtr sessionID, IntPtr text, IntPtr userData)
		{
			VirtualKeyboardTextEntryHandler handler = LookupHandler(sessionID);
			if (handler == null)
			{
				Debug.Log((object)"Tesla API Error SimpleTextSubmittedFunc: failed to find handler");
			}
			else
			{
				Task.Run(delegate
				{
					handler.OnTextEntrySubmitted(LookupOwner(sessionID), NativeString.PointerToString(text));
				});
			}
			RemoveSession(sessionID);
		}

		[MonoPInvokeCallback(typeof(API.VirtualKeyboardTextCanceledDelegate))]
		public static void SimpleTextCanceledFunc(IntPtr clientPtr, IntPtr sessionID, IntPtr userData)
		{
			VirtualKeyboardTextEntryHandler handler = LookupHandler(sessionID);
			if (handler == null)
			{
				Debug.Log((object)"Tesla API Error SimpleTextCanceledFunc: failed to find handler");
			}
			else
			{
				Task.Run(delegate
				{
					handler.OnTextEntryCanceled(LookupOwner(sessionID));
				});
			}
			RemoveSession(sessionID);
		}

		public override void BeginSimpleTextEntrySession(VirtualKeyboardTextEntryHandler handler, object owner, string text, int caretPos)
		{
			IntPtr sessionID = RegisterSession(handler, owner);
			Check(API.Tesla_VirtualKeyboard_BeginSimpleTextEntry(instance.clientPtr, text, caretPos, sessionID));
		}

		public override void DismissSession(VirtualKeyboardTextEntryHandler handler)
		{
			IntPtr intPtr = (IntPtr)handler.GetHashCode();
			if (LookupHandler(intPtr) == handler)
			{
				Check(API.Tesla_VirtualKeyboard_Dismiss(instance.clientPtr, intPtr));
			}
		}

		public void InitializeCallBacks()
		{
			Check(API.Tesla_VirtualKeyboard_SetVisibilityHandler(instance.clientPtr, VisibilityChangedFunc, IntPtr.Zero));
			Check(API.Tesla_VirtualKeyboard_SetSimpleTextHandlers(instance.clientPtr, SimpleTextStartedFunc, SimpleTextUpdatedFunc, SimpleTextSubmittedFunc, SimpleTextCanceledFunc, IntPtr.Zero));
		}
	}

	private static Platform instance = new Platform();

	private IntPtr _clientPtr = IntPtr.Zero;

	private VirtualKeyboardImpl _virtualKeyboard = new VirtualKeyboardImpl();

	protected IntPtr clientPtr
	{
		get
		{
			return _clientPtr;
		}
		private set
		{
			if (_clientPtr != IntPtr.Zero)
			{
				throw new InvalidOperationException("Tesla Apps is alreadyinitialized.");
			}
			_clientPtr = value;
		}
	}

	public static Status status
	{
		get
		{
			if (instance.clientPtr == IntPtr.Zero)
			{
				return Status.Uninitialized;
			}
			if (API.TeslaAppClient_GetStatus(instance.clientPtr) != 0)
			{
				return Status.Unavailable;
			}
			return Status.OK;
		}
	}

	public static AudioState audioState => API.Tesla_GetAudioState(instance.clientPtr);

	public static string deepLink => NativeString.PointerToString(API.Tesla_GetDeepLink(instance.clientPtr));

	public static AppState appState => API.Tesla_GetAppState(instance.clientPtr);

	public static VirtualKeyboard virtualKeyboard => instance._virtualKeyboard;

	public static event AudioStateHandler audioStateChanged;

	public static event DeepLinkHandler deepLinkChanged;

	public static event AppStateHandler appStateChanged;

	protected override void ReleaseNativeResources()
	{
		Debug.Log((object)"Platform.ReleaseNativeResources");
		if (_clientPtr != IntPtr.Zero)
		{
			API.TeslaAppClient_Free(_clientPtr);
			_clientPtr = IntPtr.Zero;
		}
	}

	protected static void Check(int result)
	{
		if (result != 0)
		{
			string text = NativeString.PointerToString(API.Tesla_AppsStatusToString(result));
			throw new Exception("A Tesla Apps call failed: " + text + "(" + result + ")");
		}
	}

	private void InitializeCallBacks()
	{
		Check(API.Tesla_SetAudioStateHandler(clientPtr, AudioStateHandlerFunc, IntPtr.Zero));
		Check(API.Tesla_SetDeepLinkHandler(clientPtr, DeepLinkHandlerFunc, IntPtr.Zero));
		Check(API.Tesla_SetAppStateHandler(clientPtr, AppStateHandlerFunc, IntPtr.Zero));
	}

	public static void Initialize(string appName)
	{
		if (!(instance.clientPtr != IntPtr.Zero))
		{
			PointerOutputValue<IntPtr> pointerOutputValue = new PointerOutputValue<IntPtr>();
			Check(API.TeslaAppClient_Create(appName, pointerOutputValue.address));
			instance.clientPtr = pointerOutputValue.value;
			instance.InitializeCallBacks();
			instance._virtualKeyboard.InitializeCallBacks();
		}
	}

	public static void Shutdown()
	{
		if (!(instance.clientPtr == IntPtr.Zero))
		{
			instance.ReleaseNativeResources();
		}
	}

	[MonoPInvokeCallback(typeof(API.AudioStateDelegate))]
	public static void AudioStateHandlerFunc(IntPtr clientPtr, AudioState value, IntPtr userData)
	{
		Task.Run(delegate
		{
			Platform.audioStateChanged?.Invoke(value);
		});
	}

	[MonoPInvokeCallback(typeof(API.DeepLinkDelegate))]
	public static void DeepLinkHandlerFunc(IntPtr clientPtr, IntPtr value, IntPtr userData)
	{
		string s = NativeString.PointerToString(value);
		Task.Run(delegate
		{
			Platform.deepLinkChanged?.Invoke(s);
		});
	}

	[MonoPInvokeCallback(typeof(API.AppStateDelegate))]
	public static void AppStateHandlerFunc(IntPtr clientPtr, AppState value, IntPtr userData)
	{
		Task.Run(delegate
		{
			Platform.appStateChanged?.Invoke(value);
		});
	}

	public static void ShowWebBrowserAndNavigateTo(string url)
	{
		Check(API.Tesla_WebBrowser_ShowAndNavigateTo(instance.clientPtr, url));
	}
}
