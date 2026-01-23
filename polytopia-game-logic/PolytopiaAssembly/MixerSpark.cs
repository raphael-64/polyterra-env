using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class MixerSpark : UIBasicComponent, IPooledObject
{
	[SerializeField]
	protected Image image;

	[SerializeField]
	protected Color lightColor;

	[SerializeField]
	protected Color darkColor;

	protected bool m_isUsed;

	protected Vector2 speed = Vector2.one;

	protected Vector2 destination = Vector2.zero;

	protected float offset;

	protected int age;

	protected int lifeTime;

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
		IsUsed = false;
	}

	public void Setup(RectTransform parent, RectTransform source, RectTransform target)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		((Transform)base.rectTransform).SetParent((Transform)(object)parent, false);
		RectTransform obj = base.rectTransform;
		Vector3 position = ((Transform)source).position;
		Rect rect = source.rect;
		float num = ((Rect)(ref rect)).width * (Random.value - 0.5f) * 0.5f;
		rect = source.rect;
		((Transform)obj).position = position + new Vector3(num, ((Rect)(ref rect)).height * (Random.value - 0.5f) * 0.5f);
		((Transform)base.rectTransform).localScale = Vector3.one + Vector3.one * Random.value;
		speed.x = (Random.value - 0.5f) * 10f;
		speed.y = (Random.value - 0.5f) * 10f;
		destination = Vector2.op_Implicit(((Transform)target).position);
		offset = Random.value * 10000f;
		age = 0;
		lifeTime = Mathf.RoundToInt(50f + Random.value * 50f);
		IsUsed = true;
		((Component)this).gameObject.SetActive(true);
	}

	private void Update()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		if (destination != Vector2.zero)
		{
			speed.x += (destination.x - ((Transform)base.rectTransform).position.x) / 200f;
			speed.y -= (destination.y - ((Transform)base.rectTransform).position.y) / 200f;
			speed.x += (Random.value - 0.5f) * 2f;
			speed.y += (Random.value - 0.5f) * 2f;
			speed.y *= 0.9f;
		}
		((Transform)base.rectTransform).position = new Vector3(((Transform)base.rectTransform).position.x + speed.x, ((Transform)base.rectTransform).position.y - speed.y, 0f);
		speed.x += Random.value - 0.5f;
		speed.x *= 0.9f;
		((Graphic)image).color = ColorUtil.SetAlphaOnColor(Color.white, Random.value * 0.1f);
		age++;
		if (Mathf.Sin((float)(age / 5) + offset) > 0.8f)
		{
			((Graphic)image).color = lightColor;
		}
		else
		{
			((Graphic)image).color = ColorUtil.SetAlphaOnColor(darkColor, ((Graphic)image).color.a);
		}
		if (age > lifeTime)
		{
			Kill();
		}
	}

	public void Kill()
	{
		ReturnToPool();
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
