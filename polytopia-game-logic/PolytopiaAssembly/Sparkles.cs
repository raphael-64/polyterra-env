using System.Runtime.CompilerServices;
using UnityEngine;

public class Sparkles : MonoBehaviour, IPooledObject
{
	[SerializeField]
	protected ParticleSystem particles;

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

	public void StartAnimation(Tile targetTile, float animTime = 1f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = ((Component)targetTile).transform.TransformPoint(targetTile.VisualCenterObject.localPosition);
		MainModule main = particles.main;
		((MainModule)(ref main)).duration = animTime;
		IsUsed = true;
		((Component)this).gameObject.SetActive(true);
	}

	private void Update()
	{
		if (!particles.IsAlive(true))
		{
			ReturnToPool();
		}
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
