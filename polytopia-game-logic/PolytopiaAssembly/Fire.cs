using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;

public class Fire : MonoBehaviour, IPooledObject
{
	[SerializeField]
	protected ParticleSystem[] particles;

	[SerializeField]
	protected SpriteRenderer shadow;

	private bool isStopping;

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
		isStopping = false;
		ObjectPool.ReturnObject(((Component)this).gameObject);
	}

	private void OnDisable()
	{
		if ((Object)(object)shadow != (Object)null)
		{
			ShortcutExtensions.DOKill((Component)(object)shadow, false);
		}
	}

	public void StartAnimation(Tile targetTile, float animTime = -1f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = ((Component)targetTile).transform.TransformPoint(targetTile.VisualCenterObject.localPosition);
		for (int i = 0; i < particles.Length; i++)
		{
			MainModule main = particles[i].main;
			if (animTime > 0f)
			{
				((MainModule)(ref main)).duration = animTime;
				((MainModule)(ref main)).loop = false;
			}
			else
			{
				((MainModule)(ref main)).loop = true;
			}
			IsUsed = true;
			((Component)this).gameObject.SetActive(true);
			particles[i].Play();
		}
		if ((Object)(object)shadow != (Object)null)
		{
			shadow.color = Color.clear;
			shadow.DOFade(1f, 0.6f);
		}
	}

	public void StopAnimation()
	{
		if (!isStopping)
		{
			isStopping = true;
			for (int i = 0; i < particles.Length; i++)
			{
				particles[i].Stop();
			}
			if ((Object)(object)shadow != (Object)null)
			{
				shadow.DOFade(0f, 0.4f);
			}
		}
	}

	private void Update()
	{
		bool flag = false;
		for (int i = 0; i < particles.Length; i++)
		{
			if (particles[i].IsAlive(true))
			{
				flag = true;
			}
		}
		if (isStopping && !flag)
		{
			Log.Verbose("Returning fire to pool", Array.Empty<object>());
			ReturnToPool();
		}
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
