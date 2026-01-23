using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using UnityEngine;

public class Tile : MonoBehaviour, IInteractable, ISpriteRendererProvider
{
	public const float WATER_SELECTION_Y_OFFSET = -0.067f;

	[Header("Components")]
	[SerializeField]
	protected TerrainRenderer terrainRenderer;

	[SerializeField]
	protected StylePicker mountainPicker;

	[SerializeField]
	protected StylePicker forestPicker;

	[SerializeField]
	protected PolytopiaSpriteRenderer fogOfWarRenderer;

	[SerializeField]
	protected SpriteRenderer overlayRenderer;

	[SerializeField]
	protected BorderContainer border;

	[SerializeField]
	protected TransportContainer transport;

	[SerializeField]
	protected Transform visualCenter;

	[SerializeField]
	protected SpriteRenderer debugOverlay;

	[SerializeField]
	protected MeshFilter combinedMeshFilter;

	[SerializeField]
	protected MeshRenderer combinedMeshRenderer;

	[Header("References")]
	[SerializeField]
	protected Sprite walkOverlay;

	[SerializeField]
	protected Sprite attackOverlay;

	[SerializeField]
	protected Sprite killOverlay;

	protected TileData data;

	protected UnitState unitState;

	protected Resource resource;

	protected Building improvement;

	protected Unit unit;

	protected Fire fireFx;

	protected Fire rainbowFireFx;

	protected Tween swayTween;

	protected bool fogIsPressed;

	protected Tween fogTween;

	protected bool hoverRegistered;

	public float HoverStartedTime = -1f;

	public bool UnitVisible;

	[NonSerialized]
	public bool isDirty;

	private List<PolytopiaSpriteRenderer> sortedSpriteRenderers = new List<PolytopiaSpriteRenderer>();

	private List<PolytopiaSpriteRenderer> batchedSpriteRenderers = new List<PolytopiaSpriteRenderer>();

	public TileData Data
	{
		get
		{
			return data;
		}
		internal set
		{
			data = value;
		}
	}

	public WorldCoordinates Coordinates => data.coordinates;

	public PlayerState Owner
	{
		get
		{
			if (GameManager.GameState.TryGetPlayer(data.owner, out var playerState))
			{
				return playerState;
			}
			return null;
		}
		set
		{
			data.owner = value.Id;
		}
	}

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((Component)this).transform.position = value;
		}
	}

	public int Depth
	{
		get
		{
			return terrainRenderer.SpriteRenderer.SortingOrder - 1;
		}
		set
		{
			terrainRenderer.SpriteRenderer.SortingOrder = value + 1;
			mountainPicker.PolytopiaSpriteRenderer.SortingOrder = value + 3;
			forestPicker.PolytopiaSpriteRenderer.SortingOrder = value + 3;
			fogOfWarRenderer.SortingOrder = value + 1;
			border.Depth = value;
			transport.Depth = value;
			((Renderer)combinedMeshRenderer).sortingLayerID = MeshCache.TERRAIN_LAYER_ID;
			((Renderer)combinedMeshRenderer).sortingOrder = Depth;
		}
	}

	public TerrainData.Type Terrain
	{
		get
		{
			return data.terrain;
		}
		set
		{
			data.terrain = value;
		}
	}

	public int Altitude
	{
		get
		{
			return data.altitude;
		}
		set
		{
			data.altitude = value;
		}
	}

	public int Climate
	{
		get
		{
			return data.climate;
		}
		set
		{
			data.climate = value;
		}
	}

	public SkinType SkinType
	{
		get
		{
			return data.skinType;
		}
		set
		{
			data.skinType = value;
		}
	}

	public Building Improvement => improvement;

	public Resource Resource => resource;

	public Unit Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			if ((Object)(object)unit != (Object)(object)value)
			{
				LevelManager.GetClientInteraction().TryUpdateSelection(value);
			}
		}
	}

	public bool IsHidden
	{
		get
		{
			if (GameManager.Client != null && GameManager.Client is ReplayClient { doShowAllPlayers: not false })
			{
				return false;
			}
			return !Data.GetExplored(GameManager.LocalPlayer.Id);
		}
	}

	public Vector3 VisualCenter => visualCenter.position;

	public Transform VisualCenterObject => visualCenter;

	public void DestroyInstance()
	{
		StopFire();
		StopRainbowFire();
		KillSway();
		KillFogAnimation();
		Object.Destroy((Object)(object)((Component)this).gameObject);
		if ((Object)(object)improvement != (Object)null)
		{
			improvement.Destroy();
		}
		if ((Object)(object)resource != (Object)null)
		{
			resource.Destroy();
		}
		if ((Object)(object)unit != (Object)null)
		{
			unit.Destroy();
		}
	}

	public void SetMaterial(Material material)
	{
		((Renderer)combinedMeshRenderer).sharedMaterial = material;
	}

	public void SetMaterialPropertyBlock(MaterialPropertyBlock propertyBlock)
	{
		((Renderer)combinedMeshRenderer).SetPropertyBlock(propertyBlock);
	}

	public void Render()
	{
		if (Data != null)
		{
			RenderTerrain();
			RenderResource();
			RenderImprovement();
			RenderUnit();
			RenderBorder();
			RenderTransportPaths();
			RenderDebug();
			RefreshHoverState();
			if ((Object)(object)Unit != (Object)null)
			{
				LevelManager.GetClientInteraction().TryUpdateSelection(Unit);
			}
		}
	}

	public void Unbatch()
	{
		UnbatchRenderers();
		((Component)combinedMeshFilter).gameObject.SetActive(false);
	}

	private void UnbatchRenderers()
	{
		for (int i = 0; i < batchedSpriteRenderers.Count; i++)
		{
			batchedSpriteRenderers[i].SetBatched(isBatched: false);
		}
		batchedSpriteRenderers.Clear();
	}

	public void BatchSprites()
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		isDirty = false;
		UnbatchRenderers();
		UpdateSortedSpriteRenderers();
		for (int i = 0; i < sortedSpriteRenderers.Count; i++)
		{
			PolytopiaSpriteRenderer polytopiaSpriteRenderer = sortedSpriteRenderers[i];
			if (!((Object)(object)polytopiaSpriteRenderer == (Object)null) && polytopiaSpriteRenderer.SortingLayer == MeshCache.TERRAIN_LAYER_ID && polytopiaSpriteRenderer.HasSprite())
			{
				if (polytopiaSpriteRenderer.GetAtlasName() != "TerrainFeatures")
				{
					break;
				}
				batchedSpriteRenderers.Add(polytopiaSpriteRenderer);
			}
		}
		if (batchedSpriteRenderers.Count > 1)
		{
			CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[batchedSpriteRenderers.Count];
			for (int j = 0; j < batchedSpriteRenderers.Count; j++)
			{
				PolytopiaSpriteRenderer polytopiaSpriteRenderer2 = batchedSpriteRenderers[j];
				Mesh mesh = polytopiaSpriteRenderer2.GetMesh();
				((CombineInstance)(ref array[j])).mesh = mesh;
				Matrix4x4 transform = ((Component)this).transform.worldToLocalMatrix * ((Component)polytopiaSpriteRenderer2).transform.localToWorldMatrix;
				((CombineInstance)(ref array[j])).transform = transform;
				polytopiaSpriteRenderer2.SetBatched(isBatched: true);
			}
			if ((Object)(object)combinedMeshFilter.sharedMesh == (Object)null)
			{
				combinedMeshFilter.sharedMesh = new Mesh();
			}
			else
			{
				combinedMeshFilter.sharedMesh.Clear();
			}
			combinedMeshFilter.sharedMesh.CombineMeshes(array);
			((Component)combinedMeshFilter).gameObject.SetActive(true);
		}
		else
		{
			Unbatch();
		}
	}

	private void RenderTerrain()
	{
		isDirty = true;
		bool desaturated = Owner != null && !GameManager.IsPlayerViewing(Owner.Id);
		switch (Terrain)
		{
		case TerrainData.Type.Mountain:
			mountainPicker.SetStyleAndSkin(Climate.ToString(), SkinType);
			TerrainMaterialHelper.SetSpriteSaturated(mountainPicker.PolytopiaSpriteRenderer, desaturated);
			((Component)mountainPicker).gameObject.SetActive(true);
			((Component)forestPicker).gameObject.SetActive(false);
			break;
		case TerrainData.Type.Forest:
			forestPicker.SetStyleAndSkin(Climate.ToString(), SkinType);
			TerrainMaterialHelper.SetSpriteSaturated(forestPicker.PolytopiaSpriteRenderer, desaturated);
			((Component)forestPicker).gameObject.SetActive(true);
			((Component)mountainPicker).gameObject.SetActive(false);
			break;
		default:
			((Component)mountainPicker).gameObject.SetActive(false);
			((Component)forestPicker).gameObject.SetActive(false);
			break;
		}
		terrainRenderer.UpdateGraphics(this);
		bool flag = !IsHidden;
		((Component)terrainRenderer).gameObject.SetActive(flag);
		((Component)fogOfWarRenderer).gameObject.SetActive(!flag);
		if (flag)
		{
			KillFogAnimation();
		}
	}

	public void RenderResource()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		isDirty = true;
		if (Object.op_Implicit((Object)(object)resource) && (Data.resource == null || resource.Data.type != Data.resource.type || Data.resource.type == ResourceData.Type.None || Data.improvement != null))
		{
			resource.Destroy();
			resource = null;
		}
		if (Data.resource != null && Data.improvement == null && GameManager.GameState.GameLogicData.TryGetData(Data.resource.type, out var resourceData) && PrefabManager.HavePrefab(Data.resource.type))
		{
			if ((Object)(object)resource == (Object)null)
			{
				resource = Object.Instantiate<Resource>(PrefabManager.GetPrefab(resourceData.type), ((Component)this).transform);
			}
			resource.Tile = this;
			resource.Depth = Depth;
			resource.SetData(resourceData);
			resource.SetOutlineColor(GetOutlineColor());
			((Component)resource).transform.localPosition = visualCenter.localPosition;
			resource.UpdateObject();
		}
		if ((Object)(object)resource != (Object)null)
		{
			resource.SetVisible(!IsHidden);
		}
	}

	public void RenderImprovement()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		isDirty = true;
		if (Object.op_Implicit((Object)(object)improvement) && (Data.improvement == null || improvement.Data.type != Data.improvement.type))
		{
			improvement.Destroy();
			improvement = null;
		}
		if (Data.improvement != null)
		{
			if (GameManager.GameState.GameLogicData.TryGetData(Data.improvement.type, out var improvementData) && PrefabManager.HavePrefab(Data.improvement.type))
			{
				if ((Object)(object)improvement == (Object)null)
				{
					improvement = Object.Instantiate<Building>(PrefabManager.GetPrefab(improvementData.type), ((Component)this).transform);
				}
				improvement.Tile = this;
				improvement.Depth = Depth;
				improvement.SetData(improvementData);
				improvement.SetState(Data.improvement);
				((Component)improvement).transform.localPosition = visualCenter.localPosition + (Vector3)(Data.IsWater ? new Vector3(0f, -0.067f, 0f) : Vector3.zero);
				improvement.UpdateObject();
			}
			ImprovementData.Type type = Data.improvement.type;
			if ((type == ImprovementData.Type.Farm || type == ImprovementData.Type.LumberHut || type == ImprovementData.Type.Mine) && (Object)(object)resource != (Object)null)
			{
				resource.SetVisible(value: false);
			}
		}
		if ((Object)(object)improvement != (Object)null)
		{
			improvement.SetVisible(!IsHidden);
			if (Data.IsBeingCaptured(GameManager.GameState))
			{
				SpawnFire();
			}
			else if ((Object)(object)fireFx != (Object)null)
			{
				StopFire();
			}
			if (Data.improvement.type == ImprovementData.Type.Ruin)
			{
				if (GameManager.LocalPlayer.GetTribeData(GameManager.GameState).HasAbility(TribeAbility.Type.RainbowVision))
				{
					SpawnRainbowFire();
				}
				else
				{
					StopRainbowFire();
				}
			}
		}
		else
		{
			StopFire();
			StopRainbowFire();
		}
	}

	public void RenderUnit()
	{
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		unitState = Data.unit;
		if (Object.op_Implicit((Object)(object)unit) && (unitState == null || unit.State.health == 0 || (unitState != null && unit.Data.type != unitState.type)))
		{
			unit.Destroy();
			unit = null;
		}
		if (unitState != null && GameManager.GameState.GameLogicData.TryGetData(unitState.type, out var unitData))
		{
			if ((Object)(object)unit == (Object)null)
			{
				Unit instance = unitState.GetInstance();
				if ((Object)(object)instance != (Object)null)
				{
					unit = instance;
				}
				else
				{
					unit = Unit.CreateUnit(unitData);
					((Object)unit).name = $"P{unitState.owner}_{unitState.type.ToString()}_{unitState.id}";
				}
			}
			unit.Tile = this;
			unit.SetData(unitData);
			unit.SetState(unitState);
			((Component)unit).transform.position = Vector2.op_Implicit(Coordinates.ToPosition());
			unit.SetOutlineColor(GetOutlineColor());
			unit.UpdateObject();
		}
		if ((Object)(object)unit != (Object)null)
		{
			UnitVisible = true;
			if (IsHidden)
			{
				UnitVisible = false;
			}
			if (unit.IsInvisibleForLocalPlayer)
			{
				UnitVisible = false;
			}
			if (Config.alwaysRenderUnits.IntValue == 1)
			{
				UnitVisible = true;
			}
			unit.SetVisible(UnitVisible);
		}
	}

	private void RenderBorder()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		isDirty = true;
		if (Owner != null)
		{
			border.SetColor(Owner.GetPlayerColor(GameManager.GameState));
		}
		border.SetVisible(!IsHidden);
		border.Render();
	}

	private void RenderTransportPaths()
	{
		isDirty = true;
		transport.SetVisible(!IsHidden);
		transport.Render();
	}

	public void RenderDebug()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (Config.showTileOverlay.IntValue != 0 && Config.showTileOverlay.IntValue == 1)
		{
			if (Owner != null)
			{
				TileOverlayColor(Owner.GetPlayerColor(GameManager.GameState));
			}
			else
			{
				TileOverlayColor(Color.white);
			}
		}
		((Component)debugOverlay).gameObject.SetActive(Config.showTileOverlay.IntValue != 0);
	}

	public Tile GetNeighbor(GridDirection direction)
	{
		WorldCoordinates coordinates = Data.coordinates;
		coordinates += direction.ToCoordinates();
		Tile tileInstance = MapRenderer.Current.GetTileInstance(coordinates);
		if ((Object)(object)tileInstance != (Object)null)
		{
			return tileInstance;
		}
		return null;
	}

	public void ShowOverlay(OverlayType type)
	{
		switch (type)
		{
		case OverlayType.Attack:
			overlayRenderer.sprite = attackOverlay;
			break;
		case OverlayType.Walk:
			overlayRenderer.sprite = walkOverlay;
			break;
		case OverlayType.Kill:
			overlayRenderer.sprite = killOverlay;
			break;
		}
		((Component)overlayRenderer).gameObject.SetActive(true);
		RefreshHoverState();
	}

	public void HideOverlay()
	{
		((Component)overlayRenderer).gameObject.SetActive(false);
		RefreshHoverState();
	}

	public void TileOverlayColor(Color color)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)debugOverlay))
		{
			debugOverlay.color = color;
		}
	}

	public bool ShouldLevelUpCity()
	{
		City city = improvement as City;
		if ((Object)(object)city == (Object)null)
		{
			return false;
		}
		return city.cityRenderer.Level < Data.improvement.level;
	}

	public void LevelUpCity(Action onComplete)
	{
		KillSway();
		TweenUtils.AnimateGrow(this, LevelUpCityDone, onComplete);
		AudioManager.PlaySFXAtTile(SFXTypes.Grow, Coordinates);
	}

	protected void LevelUpCityDone()
	{
		improvement?.UpdateObject();
		SpawnPuff();
		AudioManager.PlaySFXAtTile(SFXTypes.Plop, Coordinates);
		SpawnSparkles();
	}

	public void Damage(int damage)
	{
		SpawnExplosion();
		if ((Object)(object)Unit != (Object)null && Object.op_Implicit((Object)(object)((Component)Unit).transform) && Unit.State.health > 0)
		{
			Unit.Sway();
		}
		else
		{
			Sway();
		}
		if (damage >= 0)
		{
			WorldIconContainer.SpawnWorldDamage(Coordinates, damage);
			WorldIconContainer.SpawnWorldEdgeDamage(Coordinates);
			float volume = Math.Min(0.5f + (float)damage / 200f, 1f);
			SFXTypes id = ((damage < 20) ? SFXTypes.Hit1 : ((damage >= 50) ? SFXTypes.Hit3 : SFXTypes.Hit2));
			AudioManager.PlaySFXAtTile(id, Coordinates, volume);
		}
	}

	public void Heal(int healAmount)
	{
		SpawnHeal();
		WorldIconContainer.SpawnWorldHeal(Coordinates, healAmount);
	}

	public void SpawnPuff()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		DoPuff("Puff", ((Component)this).transform, VisualCenterObject.localPosition);
	}

	public void SpawnDarkPuff()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		DoPuff("DarkPuff", ((Component)this).transform, VisualCenterObject.localPosition);
	}

	public void SpawnExplosion()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		DoPuff("Explosion", ((Component)this).transform, VisualCenterObject.localPosition);
	}

	public void SpawnHeal()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		DoPuff("Heal", ((Component)this).transform, VisualCenterObject.localPosition);
	}

	public void SpawnHalo()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		DoPuff("Halo", ((Component)Unit).transform, new Vector3(0f, 0.2f, 0f));
	}

	public void SpawnPoison()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		AudioManager.PlaySFXAtTile(SFXTypes.Explore, Coordinates);
		DoPuff("Poison", ((Component)this).transform, VisualCenterObject.localPosition);
		SpawnGreenParticles();
	}

	public void DoPuff(string puffId, Transform parentTransform, Vector3 localPosition)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Puff pooledObject = ObjectPool.GetPooledObject<Puff>(puffId);
		if ((Object)(object)pooledObject != (Object)null)
		{
			pooledObject.StartAnimation(parentTransform, localPosition);
		}
	}

	public void SpawnShine(float animTime = 1f)
	{
		Shine pooledObject = ObjectPool.GetPooledObject<Shine>("Shine");
		if ((Object)(object)pooledObject != (Object)null)
		{
			pooledObject.StartAnimation(this, animTime);
		}
	}

	public void SpawnSparkles(float animTime = 1f)
	{
		Sparkles pooledObject = ObjectPool.GetPooledObject<Sparkles>("Sparkles");
		if ((Object)(object)pooledObject != (Object)null)
		{
			pooledObject.StartAnimation(this, animTime);
		}
	}

	public void SpawnEmbers(float animTime = 1f)
	{
		Sparkles pooledObject = ObjectPool.GetPooledObject<Sparkles>("Embers");
		if ((Object)(object)pooledObject != (Object)null)
		{
			pooledObject.StartAnimation(this, animTime);
		}
	}

	public void SpawnGreenParticles(float animTime = 1f)
	{
		Sparkles pooledObject = ObjectPool.GetPooledObject<Sparkles>("GreenParticles");
		if ((Object)(object)pooledObject != (Object)null)
		{
			pooledObject.StartAnimation(this, animTime);
		}
	}

	public void SpawnFire(float animTime = -1f)
	{
		if (!((Object)(object)fireFx != (Object)null))
		{
			fireFx = ObjectPool.GetPooledObject<Fire>("Fire");
			if ((Object)(object)fireFx != (Object)null)
			{
				fireFx.StartAnimation(this, animTime);
			}
		}
	}

	public void StopFire()
	{
		if ((Object)(object)fireFx != (Object)null)
		{
			fireFx.StopAnimation();
			fireFx = null;
		}
	}

	public void SpawnRainbowFire(float animTime = -1f)
	{
		if (!((Object)(object)rainbowFireFx != (Object)null))
		{
			rainbowFireFx = ObjectPool.GetPooledObject<Fire>("RainbowFire");
			if ((Object)(object)rainbowFireFx != (Object)null)
			{
				rainbowFireFx.StartAnimation(this, animTime);
			}
		}
	}

	public void StopRainbowFire()
	{
		if ((Object)(object)rainbowFireFx != (Object)null)
		{
			rainbowFireFx.StopAnimation();
			rainbowFireFx = null;
		}
	}

	public void Sway()
	{
		KillSway();
		if (!DOTween.IsTweening((object)((Component)this).transform, false))
		{
			swayTween = TweenUtils.Sway(((Component)this).transform);
		}
	}

	public void ShowRateCount(float value)
	{
		Sway();
		WorldIconContainer.SpawnWorldDamage(Coordinates, (int)value * 10);
	}

	public void KillSway()
	{
		TweenUtils.KillTween(swayTween, complete: true);
	}

	public Color GetOutlineColor()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (Data.climate == 11 && Data.terrain == TerrainData.Type.Field)
		{
			return Color.white;
		}
		return ColorConstants.glowColor;
	}

	public virtual void OnExplored(byte playerId, float delay = 0f)
	{
		((MonoBehaviour)this).StartCoroutine(OnExploredInternal(playerId, delay));
	}

	private IEnumerator OnExploredInternal(byte playerId, float delay = 0f)
	{
		if (delay > 0f)
		{
			yield return (object)new WaitForSeconds(delay);
		}
		SpawnPuff();
		Render();
		if ((Object)(object)Improvement != (Object)null && (Improvement.Data.type == ImprovementData.Type.City || Improvement.Data.type == ImprovementData.Type.Ruin))
		{
			SpawnShine(0.5f);
			AudioManager.PlaySFXAtTile(SFXTypes.Discover, Coordinates);
		}
	}

	private void KillFogAnimation()
	{
		TweenUtils.KillTween(fogTween, complete: true);
	}

	public virtual bool CanPerformAnyAction()
	{
		if (IsHidden)
		{
			return false;
		}
		if (((Component)overlayRenderer).gameObject.activeInHierarchy)
		{
			return true;
		}
		PlayerState localPlayer = GameManager.LocalPlayer;
		if (Object.op_Implicit((Object)(object)Unit) && Unit.Owner == localPlayer && Unit.State.CanPerformAnyAction(GameManager.GameState))
		{
			return true;
		}
		if (Owner != localPlayer)
		{
			return false;
		}
		if ((Object)(object)resource != (Object)null && resource.IsInteractableByPlayer(localPlayer.Id))
		{
			return true;
		}
		if ((Object)(object)improvement != (Object)null && improvement.IsInteractableByPlayer(localPlayer.Id))
		{
			return true;
		}
		List<ImprovementData> unlockedImprovements = GameManager.GameState.GameLogicData.GetUnlockedImprovements(localPlayer);
		if (unlockedImprovements != null && unlockedImprovements.Count > 0)
		{
			foreach (ImprovementData item in unlockedImprovements)
			{
				if (GameManager.GameState.GameLogicData.CanBuild(GameManager.GameState, Data, localPlayer, item) && localPlayer.CanAfford(item))
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void RefreshHoverState()
	{
		if (hoverRegistered && !CanPerformAnyAction())
		{
			SystemManager.DecreaseHoveredCounter();
			hoverRegistered = false;
		}
	}

	public virtual void OnHoverStart()
	{
		if (HoverStartedTime == -1f)
		{
			HoverStartedTime = Time.time;
		}
		if (!hoverRegistered && CanPerformAnyAction())
		{
			SystemManager.IncreaseHoveredCounter();
			hoverRegistered = true;
		}
	}

	public virtual void OnHoverEnd()
	{
		HoverStartedTime = -1f;
		if (hoverRegistered)
		{
			SystemManager.DecreaseHoveredCounter();
			hoverRegistered = false;
		}
		if (IsHidden && fogIsPressed)
		{
			KillFogAnimation();
			fogTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)fogOfWarRenderer).transform, 0f, 1f, false), (Ease)24);
			fogIsPressed = false;
		}
	}

	public virtual void OnPress()
	{
		if (IsHidden)
		{
			KillFogAnimation();
			fogTween = (Tween)(object)ShortcutExtensions.DOLocalMoveY(((Component)fogOfWarRenderer).transform, -0.05f, 0.2f, false);
			fogIsPressed = true;
		}
	}

	public virtual void OnRelease()
	{
		if (IsHidden)
		{
			KillFogAnimation();
			fogTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)fogOfWarRenderer).transform, 0f, 1f, false), (Ease)24);
			fogIsPressed = false;
		}
	}

	public void OnSelected()
	{
		PopupManager.HideCurrentPopup();
		InputEvents.TileSelected(this);
		AudioManager.PlaySFXAtTile(SFXTypes.Press, Coordinates);
		if (Owner != null && Owner.Id == GameManager.LocalPlayer.Id)
		{
			WorldCoordinates rulingCityCoordinates = GameManager.GameState.Map.GetTile(Coordinates).rulingCityCoordinates;
			if (rulingCityCoordinates != Coordinates)
			{
				Tile tileInstance = MapRenderer.Current.GetTileInstance(rulingCityCoordinates);
				if ((Object)(object)tileInstance != (Object)null)
				{
					tileInstance.Sway();
				}
			}
		}
		if (Owner != null && (Object)(object)improvement != (Object)null && improvement.Data.type == ImprovementData.Type.City)
		{
			(improvement as City).OnCitySelected();
		}
	}

	public void OnDeselected()
	{
		InputEvents.TileSelected(null);
		if (Owner != null && (Object)(object)improvement != (Object)null && improvement.Data.type == ImprovementData.Type.City)
		{
			(improvement as City).OnCityDeselected();
		}
	}

	public short GetPopulation()
	{
		if ((Object)(object)improvement != (Object)null)
		{
			return improvement.GetPopulation();
		}
		return 0;
	}

	public bool IsInteractableByPlayer(byte id)
	{
		bool flag = false;
		bool flag2 = false;
		if ((Object)(object)resource != (Object)null)
		{
			flag = resource.IsInteractableByPlayer(id);
		}
		if ((Object)(object)improvement != (Object)null)
		{
			flag2 = improvement.IsInteractableByPlayer(id);
		}
		return flag || flag2;
	}

	public bool OwnedBy(byte playerId)
	{
		if (Owner != null)
		{
			return Owner.Id == playerId;
		}
		return false;
	}

	public List<PolytopiaSpriteRenderer> GetSpriteRenderers()
	{
		return sortedSpriteRenderers;
	}

	private void AddActive(List<PolytopiaSpriteRenderer> spriteRenderers)
	{
		for (int i = 0; i < spriteRenderers.Count; i++)
		{
			PolytopiaSpriteRenderer polytopiaSpriteRenderer = spriteRenderers[i];
			if (((Component)polytopiaSpriteRenderer).gameObject.activeInHierarchy)
			{
				sortedSpriteRenderers.Add(polytopiaSpriteRenderer);
			}
		}
	}

	private void UpdateSortedSpriteRenderers()
	{
		sortedSpriteRenderers.Clear();
		if (IsHidden)
		{
			sortedSpriteRenderers.Add(fogOfWarRenderer);
			return;
		}
		AddActive(border.GetBackSpriteRenderers());
		sortedSpriteRenderers.Add(terrainRenderer.SpriteRenderer);
		AddActive(transport.GetSpriteRenderers());
		if (((Component)forestPicker).gameObject.activeInHierarchy)
		{
			sortedSpriteRenderers.AddRange(forestPicker.GetPolytopiaSpriteRenderers());
		}
		if (((Component)mountainPicker).gameObject.activeInHierarchy)
		{
			sortedSpriteRenderers.AddRange(mountainPicker.GetPolytopiaSpriteRenderers());
		}
		if ((Object)(object)resource != (Object)null)
		{
			AddActive(resource.GetSpriteRenderers());
		}
		if ((Object)(object)improvement != (Object)null)
		{
			sortedSpriteRenderers.AddRange(improvement.GetSpriteRenderers());
		}
		AddActive(border.GetFrontSpriteRenderers());
	}

	public override string ToString()
	{
		return ((Object)this).name;
	}
}
