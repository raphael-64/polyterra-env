using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SweatDrops : MonoBehaviour, IPooledObject
{
	[Serializable]
	public struct DropData
	{
		public Transform transform;

		public float zRotation;

		public float gravity;

		public float distance;

		public float offset;

		public float speed;
	}

	[SerializeField]
	private DropData[] drops;

	[SerializeField]
	private AnimationCurve translationCurve;

	[SerializeField]
	private AnimationCurve scaleCurve;

	private float globalOffset;

	public bool IsUsed { get; set; }

	private void Awake()
	{
		globalOffset = Random.Range(0f, 0.39f);
	}

	private void Update()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < drops.Length; i++)
		{
			DropData dropData = drops[i];
			float num = (Time.time + dropData.offset + globalOffset) % dropData.speed * (1f / dropData.speed);
			Vector3 localPosition = dropData.transform.localPosition;
			Vector3 val = Quaternion.Euler(0f, 0f, dropData.zRotation) * (Vector3.down * translationCurve.Evaluate(num) * dropData.distance) + Vector3.down * dropData.gravity * num;
			dropData.transform.localPosition = val;
			Vector3 val2 = localPosition - val;
			Quaternion localRotation = Quaternion.LookRotation(Vector3.forward, val2);
			dropData.transform.localRotation = localRotation;
			float num2 = scaleCurve.Evaluate(num);
			dropData.transform.localScale = new Vector3(num2, num2, 1f);
		}
	}

	public void ReturnToPool()
	{
		ObjectPool.ReturnObject((IPooledObject)this);
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
