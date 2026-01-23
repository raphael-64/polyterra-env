using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class Resource : WorldObject
{
	[Header("Resource")]
	[Tooltip("If the sprite will change the style depending on climate or style")]
	public bool spriteIsVariant;

	public PolytopiaSpriteRenderer outline;

	protected ResourceData data;

	protected StylePicker stylePicker;

	protected bool outlineEnabled;

	protected bool isTechUnlocked;

	protected bool isVisibleForPlayer;

	protected byte ownerId;

	private List<PolytopiaSpriteRenderer> spriteRenderers;

	public virtual ResourceData Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
		}
	}

	public override Sprite Sprite
	{
		get
		{
			return base.Sprite;
		}
		set
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			base.Sprite = value;
			((Component)spriteRenderer).transform.localScale = Vector3.one;
		}
	}

	public override int Depth
	{
		get
		{
			return spriteRenderer.SortingOrder - 5;
		}
		set
		{
			spriteRenderer.SortingOrder = value + 5;
			if ((Object)(object)outline != (Object)null)
			{
				outline.SortingOrder = value + 4;
			}
		}
	}

	protected StylePicker StylePicker
	{
		get
		{
			if ((Object)(object)stylePicker == (Object)null)
			{
				stylePicker = ((Component)spriteRenderer).GetComponent<StylePicker>();
			}
			return stylePicker;
		}
	}

	public bool OutlineEnabled
	{
		get
		{
			if ((Object)(object)outline != (Object)null)
			{
				return ((Component)outline).gameObject.activeSelf;
			}
			return false;
		}
		set
		{
			outlineEnabled = value;
			if ((Object)(object)outline != (Object)null)
			{
				((Component)outline).gameObject.SetActive(outlineEnabled);
			}
			else if (spriteIsVariant && (Object)(object)StylePicker != (Object)null)
			{
				StylePicker.OutlineEnabled = outlineEnabled;
			}
		}
	}

	protected bool TechUnlocked
	{
		get
		{
			if (!isTechUnlocked && Owner != null && Data != null)
			{
				isTechUnlocked = GameManager.GameState.GameLogicData.GetActionableImprovementForResource(data.type, Owner) != null;
			}
			return isTechUnlocked;
		}
	}

	public bool IsVisibleToPlayer => isVisibleForPlayer;

	public override void UpdateObject()
	{
		if (data != null)
		{
			if (Owner != null && Owner.Id != ownerId)
			{
				ownerId = Owner.Id;
				isTechUnlocked = false;
			}
			if (spriteIsVariant)
			{
				StylePicker.SetStyleAndSkin(Tile.Climate.ToString(), Tile.SkinType);
			}
			if (!tile.IsHidden)
			{
				UpdateVisibility();
			}
			SetVisible(isVisibleForPlayer);
			OutlineEnabled = Owner != null && GameManager.IsPlayerViewing(Owner.Id) && TechUnlocked;
			TerrainMaterialHelper.SetSpriteSaturated(base.SpriteRenderer, ShouldChangeSaturation(data.type) && Owner != null && !GameManager.IsPlayerViewing(Owner.Id));
		}
	}

	private void UpdateVisibility()
	{
		isVisibleForPlayer = false;
		if (GameManager.GameState.GameLogicData.GetUnlockedImprovements(GameManager.LocalPlayer).ContainsRequiredImprovement(data.type))
		{
			isVisibleForPlayer = true;
		}
		else if (GameManager.GameState.GameLogicData.GetUnlockableImprovements(GameManager.LocalPlayer).ContainsRequiredImprovement(data.type))
		{
			isVisibleForPlayer = true;
		}
	}

	private bool ShouldChangeSaturation(ResourceData.Type resourceType)
	{
		if ((uint)(resourceType - 3) <= 1u)
		{
			return false;
		}
		return true;
	}

	public void SetData(ResourceData data)
	{
		this.data = data;
	}

	public override void SetVisible(bool value)
	{
		if (!value || isVisibleForPlayer)
		{
			((Component)spriteRenderer).gameObject.SetActive(value);
			if (value)
			{
				OutlineEnabled = outlineEnabled;
			}
			else if ((Object)(object)outline != (Object)null)
			{
				((Component)outline).gameObject.SetActive(false);
			}
			else if (spriteIsVariant && (Object)(object)StylePicker != (Object)null)
			{
				StylePicker.OutlineEnabled = false;
			}
		}
	}

	public bool IsInteractableByPlayer(byte id)
	{
		if (Owner == null || Owner.Id != id)
		{
			return false;
		}
		bool flag = false;
		if (GameManager.GameState.TryGetPlayer(id, out var playerState))
		{
			ImprovementData actionableImprovementForResource = GameManager.GameState.GameLogicData.GetActionableImprovementForResource(Data.type, playerState);
			if (actionableImprovementForResource != null)
			{
				flag = playerState.CanAfford(actionableImprovementForResource);
			}
		}
		return outlineEnabled && flag;
	}

	public void SetOutlineColor(Color color)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)outline != (Object)null)
		{
			outline.Color = color;
		}
		if ((Object)(object)stylePicker != (Object)null)
		{
			stylePicker.SetOutlineColor(color);
		}
	}

	public List<PolytopiaSpriteRenderer> GetSpriteRenderers()
	{
		if ((Object)(object)stylePicker != (Object)null)
		{
			return stylePicker.GetPolytopiaSpriteRenderers();
		}
		if (spriteRenderers == null)
		{
			spriteRenderers = new List<PolytopiaSpriteRenderer>();
			if ((Object)(object)outline != (Object)null)
			{
				spriteRenderers.Add(outline);
			}
			if ((Object)(object)spriteRenderer != (Object)null)
			{
				spriteRenderers.Add(spriteRenderer);
			}
		}
		return spriteRenderers;
	}
}
