using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Puff : MonoBehaviour, IPooledObject
{
	public SpriteRenderer spriteRenderer;

	protected bool m_isUsed;

	[SerializeField]
	private float startScale;

	[SerializeField]
	private float targetScale = 1f;

	[SerializeField]
	private float scaleDuration = 0.3f;

	[SerializeField]
	private float yTranslation = 0.3f;

	[SerializeField]
	private float fadeDuration = 0.3f;

	private Sequence puffSequence;

	public bool IsUsed
	{
		get
		{
			return m_isUsed;
		}
		set
		{
			m_isUsed = value;
		}
	}

	public void ReturnToPool()
	{
		KillTweens();
		ObjectPool.ReturnObject(((Component)this).gameObject);
	}

	private void OnDisable()
	{
		KillTweens();
	}

	public void KillTweens()
	{
		if (puffSequence != null)
		{
			TweenExtensions.Kill((Tween)(object)puffSequence, false);
			ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
			ShortcutExtensions.DOKill((Component)(object)spriteRenderer, false);
		}
	}

	public void StartAnimation(Transform parentTransform, Vector3 localPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = parentTransform.TransformPoint(localPosition);
		StartAnimation();
	}

	public void StartAnimation()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		IsUsed = true;
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.localScale = Vector3.one * startScale;
		puffSequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(puffSequence, (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, targetScale, scaleDuration), (Ease)27));
		TweenSettingsExtensions.Append(puffSequence, (Tween)(object)ShortcutExtensions.DOMoveY(((Component)this).transform, ((Component)this).transform.position.y + yTranslation, fadeDuration, false));
		TweenSettingsExtensions.Join(puffSequence, (Tween)(object)spriteRenderer.DOFade(0f, fadeDuration));
		TweenSettingsExtensions.AppendCallback(puffSequence, new TweenCallback(AnimComplete));
	}

	private void AnimComplete()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		spriteRenderer.color = Color.white;
		ReturnToPool();
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
