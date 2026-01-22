using System;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Scripting;

namespace DG.Tweening;

public static class DOTweenModuleUtils
{
	public static class Physics
	{
		public static void SetOrientationOnPath(PathOptions options, Tween t, Quaternion newRot, Transform trans)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (options.isRigidbody)
			{
				((Rigidbody)t.target).rotation = newRot;
			}
			else
			{
				trans.rotation = newRot;
			}
		}

		public static bool HasRigidbody2D(Component target)
		{
			return (Object)(object)target.GetComponent<Rigidbody2D>() != (Object)null;
		}

		[Preserve]
		public static bool HasRigidbody(Component target)
		{
			return (Object)(object)target.GetComponent<Rigidbody>() != (Object)null;
		}

		[Preserve]
		public static TweenerCore<Vector3, Path, PathOptions> CreateDOTweenPathTween(MonoBehaviour target, bool tweenRigidbody, bool isLocal, Path path, float duration, PathMode pathMode)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			Rigidbody val = (tweenRigidbody ? ((Component)target).GetComponent<Rigidbody>() : null);
			if (tweenRigidbody && (Object)(object)val != (Object)null)
			{
				return isLocal ? val.DOLocalPath(path, duration, pathMode) : val.DOPath(path, duration, pathMode);
			}
			return isLocal ? ShortcutExtensions.DOLocalPath(((Component)target).transform, path, duration, pathMode) : ShortcutExtensions.DOPath(((Component)target).transform, path, duration, pathMode);
		}
	}

	private static bool _initialized;

	[Preserve]
	public static void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			DOTweenExternalCommand.SetOrientationOnPath += Physics.SetOrientationOnPath;
		}
	}

	[Preserve]
	private static void Preserver()
	{
		AppDomain.CurrentDomain.GetAssemblies();
		typeof(MonoBehaviour).GetMethod("Stub");
	}
}
