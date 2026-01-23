using System;
using UnityEngine;

public static class ResourceEvents
{
	public delegate void OnResourceAddedEvent(byte playerId, ResourceManager.Type type, float amount, float total);

	public delegate void OnResourceRemovedEvent(byte playerId, ResourceManager.Type type, float amount, float total);

	public delegate void OnResourceChangedEvent(byte playerId);

	public delegate void OnAddResourceToUIEvent(ResourceEventData data);

	public delegate void OnIncomeChangedEvent(byte playerId);

	public delegate void OnRefreshWalletsEvent(byte playerId);

	public class ResourceEventData
	{
		public byte playerId;

		public ResourceManager.Type resourceType;

		public WorldIconContainer.ScoreElementTypes scoreElementType;

		public float amount;

		public float total;

		public ResourceEventPositionData from;

		public ResourceEventPositionData to;

		public ResourceEventResultData result;

		public string reason = string.Empty;

		public ResourceEventData()
		{
		}

		public ResourceEventData(byte playerId, ResourceManager.Type resourceType, WorldIconContainer.ScoreElementTypes scoreElementType, float amount, ResourceEventPositionData from = null, ResourceEventPositionData to = null, ResourceEventResultData result = null, string reason = "")
		{
			this.playerId = playerId;
			this.resourceType = resourceType;
			this.scoreElementType = scoreElementType;
			this.amount = amount;
			total = ResourceManager.GetResourceOfType(this.playerId, this.resourceType) + this.amount;
			this.from = from;
			this.to = to;
			if (result == null)
			{
				result = new ResourceEventResultData(ResourceEventResultData.ResultType.None);
			}
			this.result = result;
			this.reason = reason;
		}

		public ResourceEventData Clone()
		{
			return new ResourceEventData(playerId, resourceType, scoreElementType, amount, (from != null) ? from.Clone() : null, (to != null) ? to.Clone() : null, (result != null) ? result.Clone() : null, reason);
		}
	}

	public class ResourceEventPositionData
	{
		public enum PositionType
		{
			None,
			WorldCoordinate,
			UIElement
		}

		public PositionType type;

		public WorldCoordinates worldCoordinate;

		public Vector3 worldPosition;

		public RectTransform UIElement;

		public ResourceEventPositionData()
		{
		}

		public ResourceEventPositionData(WorldCoordinates worldCoordinate)
		{
			type = PositionType.WorldCoordinate;
			this.worldCoordinate = worldCoordinate;
		}

		public ResourceEventPositionData(RectTransform UIElement)
		{
			type = PositionType.UIElement;
			this.UIElement = UIElement;
		}

		public ResourceEventPositionData Clone()
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			return new ResourceEventPositionData
			{
				type = type,
				worldCoordinate = worldCoordinate,
				worldPosition = worldPosition,
				UIElement = UIElement
			};
		}
	}

	public class ResourceEventResultData
	{
		public enum ResultType
		{
			None,
			ReturnToResourceManager,
			AddToBuilding,
			UpdateIncome
		}

		public ResultType type;

		public Building buildingTarget;

		public Action onComplete;

		public Action onResourceAdded;

		public string reason = string.Empty;

		public ResourceEventResultData()
		{
		}

		public ResourceEventResultData(ResultType type, Action onComplete = null, string reason = "")
		{
			this.type = type;
			this.reason = reason;
			this.onComplete = onComplete;
		}

		public ResourceEventResultData(Building buildingTarget, Action onResourceAdded = null, Action onComplete = null, string reason = "")
		{
			type = ResultType.AddToBuilding;
			this.buildingTarget = buildingTarget;
			this.reason = reason;
			this.onComplete = onComplete;
			this.onResourceAdded = onResourceAdded;
		}

		public ResourceEventResultData Clone()
		{
			return new ResourceEventResultData
			{
				type = type,
				buildingTarget = buildingTarget,
				reason = reason,
				onComplete = onComplete,
				onResourceAdded = onResourceAdded
			};
		}
	}

	public static event OnResourceAddedEvent OnResourceAdded;

	public static event OnResourceRemovedEvent OnResourceRemoved;

	public static event OnResourceChangedEvent OnResourceChanged;

	public static event OnAddResourceToUIEvent OnAddResourceToUI;

	public static event OnIncomeChangedEvent OnIncomeChanged;

	public static event OnRefreshWalletsEvent OnRefreshWallets;

	public static void ResourceAdded(byte playerId, ResourceManager.Type type, float amount, float total)
	{
		ResourceEvents.OnResourceAdded?.Invoke(playerId, type, amount, total);
		ResourceChanged(playerId);
	}

	public static void ResourceRemoved(byte playerId, ResourceManager.Type type, float amount, float total)
	{
		ResourceEvents.OnResourceRemoved?.Invoke(playerId, type, amount, total);
		ResourceChanged(playerId);
	}

	public static void ResourceChanged(byte playerId)
	{
		ResourceEvents.OnResourceChanged?.Invoke(playerId);
	}

	public static void AddResourceToUI(ResourceEventData data)
	{
		ResourceEvents.OnAddResourceToUI?.Invoke(data);
	}

	public static void IncomeChanged(byte playerId)
	{
		ResourceEvents.OnIncomeChanged?.Invoke(playerId);
	}

	public static void RefreshWallets(byte playerId)
	{
		ResourceEvents.OnRefreshWallets?.Invoke(playerId);
	}
}
