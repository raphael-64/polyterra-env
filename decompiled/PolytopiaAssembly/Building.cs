using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class Building : WorldObject
{
	[Tooltip("If the sprite will change the style depending on climate or style")]
	public bool spriteIsVariant;

	[Tooltip("If the sprite will grow with the buildings level")]
	public bool spriteGrows;

	protected ImprovementData data;

	protected ImprovementState state;

	protected bool hasFetchedLevelPicker;

	protected bool hasFetchedStylePicker;

	protected LevelPicker levelPicker;

	protected StylePicker stylePicker;

	protected List<PolytopiaSpriteRenderer> spriteRenderers;

	private short population;

	public override int Depth
	{
		get
		{
			return spriteRenderer.SortingOrder - 98;
		}
		set
		{
			spriteRenderer.SortingOrder = value + 98;
		}
	}

	public virtual ImprovementData Data => data;

	public ImprovementState State => state;

	public string DisplayName => state.name;

	public virtual string Info => BuildingUtils.GetInfo(Data, this);

	public virtual ushort Level => State.level;

	public virtual ushort MaxLevel
	{
		get
		{
			if (data.maxLevel > 0)
			{
				return (ushort)data.maxLevel;
			}
			if (HasAbility(ImprovementAbility.Type.Patina))
			{
				return 5;
			}
			if (data.adjacencyImprovements != null && data.adjacencyImprovements.Count > 0)
			{
				return 8;
			}
			if (data.type == ImprovementData.Type.City)
			{
				return 100;
			}
			return 1;
		}
	}

	public PlayerState CapitalOf
	{
		get
		{
			for (int i = 0; i < GameManager.GameState.PlayerStates.Count; i++)
			{
				if (GameManager.GameState.PlayerStates[i].startTile == Tile.Coordinates)
				{
					return GameManager.GameState.PlayerStates[i];
				}
			}
			return null;
		}
	}

	public bool Occupied
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)Tile))
			{
				return false;
			}
			if (Object.op_Implicit((Object)(object)Tile.Unit) && Tile.Unit.Owner != Owner)
			{
				return true;
			}
			return false;
		}
	}

	public virtual void SetData(ImprovementData data)
	{
		this.data = data;
	}

	public virtual void SetState(ImprovementState state)
	{
		this.state = state;
		population = state.population;
	}

	public override void SetVisible(bool value)
	{
		((Component)spriteRenderer).gameObject.SetActive(value);
	}

	public override void UpdateObject()
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		if (data == null)
		{
			return;
		}
		if (spriteIsVariant && GameManager.GameState.GameLogicData.TryGetData(Owner.tribe, out var _))
		{
			if (data.type.IsMonument() && GameManager.GameState.TryGetPlayer(state.founder, out var playerState))
			{
				GetStylePicker().SetStyleAndSkin(playerState.GetTribeStyle(GameManager.GameState).ToString(), playerState.skinType);
			}
			else
			{
				GetStylePicker().SetStyleAndSkin(Owner.GetTribeStyle(GameManager.GameState).ToString(), Owner.skinType);
			}
		}
		int num;
		if (spriteGrows)
		{
			if (data.HasAbility(ImprovementAbility.Type.Patina))
			{
				GameState gameState = GameManager.GameState;
				if (gameState != null && gameState.Version < 40)
				{
					num = -1;
					goto IL_00e2;
				}
			}
			num = 0;
			goto IL_00e2;
		}
		goto IL_00fb;
		IL_00fb:
		if (state.HasEffect(ImprovementEffect.decomposing))
		{
			TerrainMaterialHelper.SetSpriteTint(base.SpriteRenderer, new Color(0.4f, 0.9f, 0f));
		}
		else
		{
			TerrainMaterialHelper.SetSpriteSaturated(base.SpriteRenderer, Owner != null && !GameManager.IsPlayerViewing(Owner.Id));
		}
		return;
		IL_00e2:
		int num2 = num;
		GetLevelPicker().Level = state.level + num2;
		goto IL_00fb;
	}

	public virtual List<PolytopiaSpriteRenderer> GetSpriteRenderers()
	{
		StylePicker stylePicker = GetStylePicker();
		if ((Object)(object)stylePicker != (Object)null)
		{
			return stylePicker.GetPolytopiaSpriteRenderers();
		}
		if (spriteRenderers == null)
		{
			spriteRenderers = new List<PolytopiaSpriteRenderer>();
			spriteRenderers.Add(base.SpriteRenderer);
		}
		return spriteRenderers;
	}

	public LevelPicker GetLevelPicker()
	{
		if ((Object)(object)levelPicker == (Object)null && !hasFetchedLevelPicker)
		{
			levelPicker = ((Component)base.SpriteRenderer).GetComponent<LevelPicker>();
			hasFetchedLevelPicker = true;
		}
		return levelPicker;
	}

	public StylePicker GetStylePicker()
	{
		if ((Object)(object)stylePicker == (Object)null && !hasFetchedStylePicker)
		{
			stylePicker = ((Component)base.SpriteRenderer).GetComponent<StylePicker>();
			hasFetchedStylePicker = true;
		}
		return stylePicker;
	}

	public override void OnSelected()
	{
		base.OnSelected();
		InputEvents.BuildingSelected(this);
	}

	public virtual bool HasAbility(ImprovementAbility.Type ability)
	{
		return data.HasAbility(ability);
	}

	public virtual bool HasReward(CityReward reward)
	{
		return State.HasReward(reward);
	}

	public int RewardCount(CityReward reward)
	{
		return State.RewardCount(reward);
	}

	public short GetPopulation()
	{
		return population;
	}

	public virtual bool IsInteractableByPlayer(byte id)
	{
		if (Owner == null || Owner.Id != id)
		{
			return false;
		}
		if (GameManager.GameState.GameLogicData.IsUnlocked(PlayerAbility.Type.Destroy, Owner))
		{
			return true;
		}
		List<ImprovementData> unlockedImprovements = GameManager.GameState.GameLogicData.GetUnlockedImprovements(Owner);
		if (unlockedImprovements != null && unlockedImprovements.Count > 0)
		{
			foreach (ImprovementData item in unlockedImprovements)
			{
				if (GameManager.GameState.GameLogicData.CanBuild(GameManager.GameState, tile.Data, Owner, item))
				{
					return true;
				}
			}
		}
		return false;
	}
}
