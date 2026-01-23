using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class City : Building
{
	[Header("City")]
	public CityRenderer cityRenderer;

	private int depth;

	private CityStatusDisplay cityOverlay;

	[Info]
	[SerializeField]
	protected string info = "None";

	public override int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
			cityRenderer.sortOrder = depth;
			spriteRenderer.SortingOrder = value + 98;
		}
	}

	public bool IsCapital
	{
		get
		{
			if (Owner != null)
			{
				return Tile.Data.capitalOf != 0;
			}
			return false;
		}
	}

	public override void Destroy()
	{
		cityRenderer.Clear();
		if ((Object)(object)cityOverlay != (Object)null)
		{
			cityOverlay.ReturnToPool();
		}
		cityOverlay = null;
		base.Destroy();
	}

	public override void SetData(ImprovementData data)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.SetData(data);
		cityRenderer.SetVisible(visible: false);
		if ((Object)(object)cityOverlay == (Object)null)
		{
			cityOverlay = ObjectPool.GetPooledObject<CityStatusDisplay>("CityStatusDisplay");
			cityOverlay.IsUsed = true;
			((Component)cityOverlay).transform.SetParent(((Component)this).transform);
			((Component)cityOverlay).transform.localPosition = Vector3.zero;
		}
	}

	public override void SetVisible(bool value)
	{
		if (Owner == null)
		{
			((Component)spriteRenderer).gameObject.SetActive(value);
			cityRenderer.SetVisible(visible: false);
			if (Object.op_Implicit((Object)(object)cityOverlay))
			{
				((Component)cityOverlay).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)spriteRenderer).gameObject.SetActive(false);
			cityRenderer.SetVisible(value);
			if (Object.op_Implicit((Object)(object)cityOverlay))
			{
				((Component)cityOverlay).gameObject.SetActive(value);
			}
		}
	}

	public override void UpdateObject()
	{
		bool flag = !Tile.IsHidden;
		if (Owner == null)
		{
			cityRenderer.SetVisible(visible: false);
			((Component)spriteRenderer).gameObject.SetActive(flag);
			if (Object.op_Implicit((Object)(object)cityOverlay))
			{
				cityOverlay.SetCity(null);
				((Component)cityOverlay).gameObject.SetActive(false);
			}
		}
		else
		{
			cityRenderer.SetVisible(flag);
			((Component)spriteRenderer).gameObject.SetActive(false);
			cityRenderer.Coordinates = Tile.Coordinates;
			cityRenderer.HaveWall = HasReward(CityReward.CityWall);
			cityRenderer.HaveWorkshop = HasReward(CityReward.Workshop);
			cityRenderer.ParkCount = RewardCount(CityReward.Park);
			cityRenderer.Level = base.State.level;
			cityRenderer.IsCapitalOf = Tile.Data.capitalOf;
			cityRenderer.Tribe = ((Owner == null) ? 1 : Owner.GetTribeStyle(GameManager.GameState));
			cityRenderer.SkinType = ((Owner != null) ? Owner.skinType : SkinType.Default);
			cityRenderer.IsEnemyCity = Owner != null && !GameManager.IsPlayerViewing(Owner.Id);
			bool flag2 = Tile.Data.capitalOf != 0 && Tile.Data.capitalOf == Owner.Id;
			cityRenderer.PlayerEmbassies = (flag2 ? Owner.GetEmbassiesInCapitalOf(GameManager.GameState) : new List<byte>());
			if (Owner != null && Tile.Data.capitalOf == Owner.Id && GameManager.GameState.GameLogicData.TryGetData(Owner.tribe, out var tribeData) && tribeData.bonus == TribeData.BonusEnum.CityLevel3)
			{
				cityRenderer.Level++;
				cityRenderer.ParkCount++;
			}
			cityRenderer.RefreshCity();
			if (Object.op_Implicit((Object)(object)cityOverlay))
			{
				cityOverlay.SetCity(this);
				((Component)cityOverlay).gameObject.SetActive(flag);
			}
		}
		UpdateInfo();
	}

	public override List<PolytopiaSpriteRenderer> GetSpriteRenderers()
	{
		if (Owner == null)
		{
			if (spriteRenderers == null)
			{
				spriteRenderers = new List<PolytopiaSpriteRenderer>();
				spriteRenderers.Add(base.SpriteRenderer);
			}
			return spriteRenderers;
		}
		return cityRenderer.GetSpriteRenderers();
	}

	private void UpdateInfo()
	{
	}

	public override bool IsInteractableByPlayer(byte id)
	{
		if (Owner == null || Owner.Id != id)
		{
			return false;
		}
		if ((Object)(object)Tile.Unit == (Object)null && tile.Data.CanSupportMoreUnits(GameManager.GameState) && GameManager.GameState.TryGetPlayer(id, out var playerState))
		{
			List<UnitData> unlockedUnits = GameManager.GameState.GameLogicData.GetUnlockedUnits(playerState, GameManager.GameState, shouldIncludeHidden: false);
			if (unlockedUnits != null && unlockedUnits.Count > 0)
			{
				foreach (UnitData item in unlockedUnits)
				{
					if (!item.hidden && playerState.CanAfford(item))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void OnCitySelected()
	{
		bool flag = !Tile.IsHidden;
		if ((Object)(object)cityOverlay != (Object)null && flag)
		{
			cityOverlay.Selected = true;
		}
	}

	public void OnCityDeselected()
	{
		bool flag = !Tile.IsHidden;
		if ((Object)(object)cityOverlay != (Object)null && flag)
		{
			cityOverlay.Selected = false;
		}
	}

	public int GetFreeUnits()
	{
		return Level + 1 - GameManager.GameState.Map.GetCityUnitCount(Tile.Coordinates);
	}

	public bool IsCapitalOf(byte id)
	{
		if (Owner != null)
		{
			return Tile.Data.capitalOf == id;
		}
		return false;
	}
}
