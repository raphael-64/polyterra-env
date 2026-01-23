using UnityEngine;

public interface IPooledObject
{
	GameObject gameObject { get; }

	bool IsUsed { get; set; }

	void ReturnToPool();
}
