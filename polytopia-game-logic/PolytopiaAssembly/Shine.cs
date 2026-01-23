using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class Shine : MonoBehaviour, IPooledObject
{
	[SerializeField]
	protected SpriteRenderer spriteRenderer;

	private bool m_isUsed;

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
		ObjectPool.ReturnObject(((Component)this).gameObject);
	}

	public void StartAnimation(Tile targetTile, float animTime)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		((Component)this).transform.position = ((Component)targetTile).transform.TransformPoint(targetTile.VisualCenterObject.localPosition);
		IsUsed = true;
		((Component)this).gameObject.SetActive(true);
		spriteRenderer.color = Color.white;
		TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(spriteRenderer.DOFade(0f, animTime), TweenUtils.GetRoughEase()), new TweenCallback(OnAnimComplete));
	}

	private void OnAnimComplete()
	{
		ReturnToPool();
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
