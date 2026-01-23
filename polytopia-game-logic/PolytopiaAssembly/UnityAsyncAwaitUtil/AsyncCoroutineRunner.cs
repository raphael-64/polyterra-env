using UnityEngine;

namespace UnityAsyncAwaitUtil;

public class AsyncCoroutineRunner : MonoBehaviour
{
	private static AsyncCoroutineRunner _instance;

	public static AsyncCoroutineRunner Instance
	{
		get
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_instance == (Object)null)
			{
				_instance = new GameObject("AsyncCoroutineRunner").AddComponent<AsyncCoroutineRunner>();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		((Object)((Component)this).gameObject).hideFlags = (HideFlags)61;
		Object.DontDestroyOnLoad((Object)(object)((Component)this).gameObject);
	}
}
