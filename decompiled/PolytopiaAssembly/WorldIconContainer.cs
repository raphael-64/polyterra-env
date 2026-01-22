using System.Collections.Generic;
using UnityEngine;

public class WorldIconContainer : UIBasicComponent
{
	public enum ScoreElementTypes
	{
		None,
		Label,
		Icon,
		IconWithLabel
	}

	protected class DelayedElement
	{
		public UIWorldScoreBase element;

		public float delay;
	}

	protected List<DelayedElement> delayedElements = new List<DelayedElement>();

	protected List<UIWorldScoreBase> showingScores = new List<UIWorldScoreBase>();

	protected static WorldIconContainer instance;

	private float nextFreeResourceAnimationTime;

	public override void Init()
	{
		base.Init();
		instance = this;
		ResourceEvents.OnAddResourceToUI += OnAddResourceToUI;
	}

	private void Update()
	{
		for (int i = 0; i < delayedElements.Count; i++)
		{
			DelayedElement delayedElement = delayedElements[i];
			delayedElement.delay -= Time.deltaTime;
			if (delayedElement.delay <= 0f)
			{
				ShowScore(delayedElement.element);
				delayedElements.RemoveAt(i--);
			}
		}
	}

	public static void CancelDelays()
	{
		instance.delayedElements.Clear();
	}

	private void OnDestroy()
	{
		instance = null;
		ResourceEvents.OnAddResourceToUI -= OnAddResourceToUI;
		delayedElements.Clear();
	}

	private void OnAddResourceToUI(ResourceEvents.ResourceEventData data)
	{
		float delay = 0.1f;
		switch (data.resourceType)
		{
		case ResourceManager.Type.Currency:
			delay = 0.04f;
			break;
		case ResourceManager.Type.Population:
			delay = 0.06f;
			break;
		}
		if (data.amount == 1f)
		{
			_ = data.resourceType;
			_ = 1;
		}
		if (data.amount > 1f && (data.resourceType == ResourceManager.Type.Currency || data.resourceType == ResourceManager.Type.Population))
		{
			for (int i = 0; (float)i < data.amount; i++)
			{
				UIWorldScoreBase scoreElement = GetScoreElement(data.scoreElementType);
				((Component)scoreElement).transform.SetParent(((Component)this).transform, false);
				ResourceEvents.ResourceEventData resourceEventData = data.Clone();
				resourceEventData.amount = 1f;
				if ((float)i < data.amount - 1f)
				{
					resourceEventData.result.onComplete = null;
				}
				scoreElement.Data = resourceEventData;
				ShowDelayedScore(scoreElement, GetDelayAndUpdate(delay));
			}
		}
		else
		{
			UIWorldScoreBase scoreElement2 = GetScoreElement(data.scoreElementType);
			((Component)scoreElement2).transform.SetParent(((Component)this).transform, false);
			scoreElement2.Data = data;
			ShowScore(scoreElement2);
		}
	}

	private float GetDelayAndUpdate(float delay)
	{
		nextFreeResourceAnimationTime = Mathf.Max(Time.time, nextFreeResourceAnimationTime) + delay;
		return Mathf.Max(nextFreeResourceAnimationTime - Time.time, 0f);
	}

	private void ShowScore(UIWorldScoreBase scoreElement)
	{
		scoreElement.Show();
		showingScores.Add(scoreElement);
	}

	public static void HideScore(UIWorldScoreBase scoreElement)
	{
		scoreElement.ReturnToPool();
		if ((Object)(object)instance != (Object)null)
		{
			instance.showingScores.Remove(scoreElement);
		}
		if (!HasActiveScoreAnimation())
		{
			ResourceEvents.RefreshWallets(GameManager.LocalPlayer.Id);
		}
	}

	private void ShowDelayedScore(UIWorldScoreBase element, float delay)
	{
		element.IsUsed = true;
		DelayedElement item = new DelayedElement
		{
			element = element,
			delay = delay
		};
		delayedElements.Add(item);
	}

	public static bool HasActiveScoreAnimation()
	{
		if ((Object)(object)instance != (Object)null)
		{
			if (instance.delayedElements.Count <= 0)
			{
				return instance.showingScores.Count > 0;
			}
			return true;
		}
		return false;
	}

	protected UIWorldScoreBase GetScoreElement(ScoreElementTypes type)
	{
		return type switch
		{
			ScoreElementTypes.Label => ObjectPool.GetPooledObject<WorldScore>("WorldScore"), 
			ScoreElementTypes.Icon => ObjectPool.GetPooledObject<WorldIcon>("WorldIcon"), 
			ScoreElementTypes.IconWithLabel => ObjectPool.GetPooledObject<WorldValuedIcon>("WorldValuedIcon"), 
			_ => null, 
		};
	}

	public static void SpawnWorldDamage(WorldCoordinates coordinate, int damage)
	{
		WorldHealth worldHealth = GetWorldHealth(coordinate, "WorldDamage");
		worldHealth.HealthValue = Mathf.Round((float)damage * 0.1f);
		worldHealth.Show();
	}

	public static void SpawnWorldEdgeDamage(WorldCoordinates coordinates)
	{
		UIWorldDamage pooledObject = ObjectPool.GetPooledObject<UIWorldDamage>("WorldEdgeDamage");
		((Component)pooledObject).transform.SetParent(((Component)instance).transform, false);
		pooledObject.Coordinates = coordinates;
		pooledObject.Show();
	}

	public static void SpawnWorldHeal(WorldCoordinates coordinate, int healAmount)
	{
		WorldHealth worldHealth = GetWorldHealth(coordinate, "WorldHeal");
		worldHealth.HealthValue = Mathf.Round((float)healAmount * 0.1f);
		worldHealth.Show();
	}

	public static WorldHealth GetWorldHealth(WorldCoordinates coordinate, string id)
	{
		WorldHealth pooledObject = ObjectPool.GetPooledObject<WorldHealth>(id);
		((Component)pooledObject).transform.SetParent(((Component)instance).transform, false);
		pooledObject.Coordinates = coordinate;
		return pooledObject;
	}

	public static WorldTestIcon GetTestIcon(WorldCoordinates coordinate)
	{
		WorldTestIcon testIcon = GetTestIcon();
		testIcon.Coordinates = coordinate;
		return testIcon;
	}

	public static WorldTestIcon GetTestIcon(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		WorldTestIcon testIcon = GetTestIcon();
		testIcon.Position = position;
		return testIcon;
	}

	protected static WorldTestIcon GetTestIcon()
	{
		WorldTestIcon pooledObject = ObjectPool.GetPooledObject<WorldTestIcon>("WorldTestIcon");
		((Component)pooledObject).transform.SetParent(((Component)instance).transform, false);
		return pooledObject;
	}
}
