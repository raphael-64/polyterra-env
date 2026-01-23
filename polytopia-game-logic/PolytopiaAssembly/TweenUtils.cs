using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class TweenUtils
{
	private static int roughCashCount = 10;

	public static void CacheRoughTweens()
	{
		for (int i = 0; i < roughCashCount; i++)
		{
			RoughEase.Create(1.5f, 100u, restrictMinAndMax: true, null, RoughEase.Taper.None, randomize: true, $"rough_{i}");
		}
	}

	public static EaseFunction GetRoughEase()
	{
		int num = Random.Range(0, roughCashCount);
		return RoughEase.ByName($"rough_{num}");
	}

	public static void KillTween(Tween tween, bool complete = false)
	{
		if (tween != null)
		{
			TweenExtensions.Kill(tween, complete);
		}
	}

	public static void KillTweens(List<Tween> tweens, bool complete = false)
	{
		for (int i = 0; i < tweens.Count; i++)
		{
			KillTween(tweens[i], complete);
		}
	}

	public static void KillTweens(Tween[] tweens, bool complete = false)
	{
		for (int i = 0; i < tweens.Length; i++)
		{
			KillTween(tweens[i], complete);
		}
	}

	public static void KillSequence(Sequence sequence, bool complete = false)
	{
		if (sequence != null)
		{
			TweenExtensions.Kill((Tween)(object)sequence, complete);
		}
	}

	public static void KillSequences(List<Sequence> sequences, bool complete = false)
	{
		for (int i = 0; i < sequences.Count; i++)
		{
			KillSequence(sequences[i], complete);
		}
	}

	public static void KillSequences(Sequence[] sequences, bool complete = false)
	{
		for (int i = 0; i < sequences.Length; i++)
		{
			KillSequence(sequences[i], complete);
		}
	}

	public static Tween Sway(Transform target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = target.localPosition;
		target.localPosition -= new Vector3(0f, 0.05f, 0f);
		return (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(target, localPosition.y, 0.5f, false), (Ease)24);
	}

	public static Tween AnimateGrow(Tile tile, Action OnLevelUp, Action OnComplete)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		return (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)tile).transform, ((Component)tile).transform.localPosition.y - 0.1f, 0.5f, false), 0.3f), GetRoughEase()), (TweenCallback)delegate
		{
			OnLevelUp?.Invoke();
			GrowAnimComplete(tile, OnComplete);
		});
	}

	protected static void GrowAnimComplete(Tile tile, Action OnComplete)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)tile).transform, Vector2.op_Implicit(tile.Coordinates.ToPosition()), 0.2f, false), (Ease)24), (TweenCallback)delegate
		{
			OnComplete?.Invoke();
		});
		tile.SpawnSparkles();
	}
}
