using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Polytopia.Data;
using UnityEngine;

public class Unit : WorldObject
{
	[Serializable]
	public struct LayerData
	{
		public enum LayerTypes
		{
			None,
			Tint,
			Style,
			Outline,
			Climate
		}

		public enum Part
		{
			None,
			Head,
			Body
		}

		public enum RendererType
		{
			None,
			SpriteRenderer,
			StylePicker
		}

		public Part part;

		public LayerTypes layerType;

		public SpriteRenderer spriteRenderer;

		public StylePicker stylePicker;

		public static RendererType GetRendererTypeFromLayerType(LayerTypes layerType)
		{
			switch (layerType)
			{
			case LayerTypes.Style:
			case LayerTypes.Climate:
				return RendererType.StylePicker;
			default:
				return RendererType.SpriteRenderer;
			}
		}

		public SpriteRenderer GetSpriteRenderer()
		{
			RendererType rendererTypeFromLayerType = GetRendererTypeFromLayerType(layerType);
			if (rendererTypeFromLayerType != RendererType.SpriteRenderer && rendererTypeFromLayerType == RendererType.StylePicker)
			{
				return stylePicker?.SpriteRenderer;
			}
			return spriteRenderer;
		}
	}

	[Header("Unit settings")]
	public Transform spriteContainer;

	public LayerData[] layerData;

	public Transform head;

	[Header("Special")]
	[Tooltip("Special component for centipedes and other future units with masters")]
	public SegmentConnector connector;

	[Header("Debug")]
	[ToggleButton]
	public bool runAttackAnimation;

	[ToggleButton]
	public bool fakeDamage;

	protected List<WorldCoordinates> walkOptions;

	protected List<WorldCoordinates> attackOptions;

	protected UnitData data;

	protected UnitState state;

	protected UnitStatusDisplay statusDisplay;

	protected HintIcon overHeadIcon;

	protected SweatDrops sweatDrops;

	protected bool boundsSet;

	protected Bounds bounds;

	protected Tween swayTween;

	protected Tween moveTween;

	protected Tween attackMoveTween;

	protected Tween attackReturnTween;

	private List<Tile> overlayTiles = new List<Tile>();

	private List<Unit> overlayUnits = new List<Unit>();

	public virtual UnitData Data => data;

	public UnitState State => state;

	public override PlayerState Owner
	{
		get
		{
			if (State != null && GameManager.GameState.TryGetPlayer(state.owner, out var playerState))
			{
				return playerState;
			}
			Log.Error("Found orphaned unit: {0}", new object[1] { ((Object)this).name });
			return null;
		}
	}

	public override int Depth
	{
		get
		{
			return 0;
		}
		set
		{
		}
	}

	public Vector3 Center
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Bounds val = Bounds;
			return ((Bounds)(ref val)).center;
		}
	}

	public Bounds Bounds
	{
		get
		{
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (!boundsSet)
			{
				bounds = default(Bounds);
				LayerData[] array = this.layerData;
				for (int i = 0; i < array.Length; i++)
				{
					LayerData layerData = array[i];
					SpriteRenderer val = layerData.spriteRenderer;
					if ((Object)(object)val != (Object)null)
					{
						if (layerData.layerType == LayerData.LayerTypes.Style)
						{
							val = layerData.stylePicker.SpriteRenderer;
						}
						Bounds val2 = ((Renderer)val).bounds;
						((Bounds)(ref val2)).center = ((Component)this).transform.InverseTransformPoint(((Bounds)(ref val2)).center);
						((Bounds)(ref bounds)).Encapsulate(val2);
					}
				}
				boundsSet = true;
			}
			return bounds;
		}
	}

	public Vector3 HeadPosition => ((Component)this).transform.InverseTransformPoint(head.position);

	public bool Flipped
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localScale.x < 0f;
		}
		set
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			state.flipped = value;
			if (!state.HasLeader())
			{
				Vector3 localScale = ((Component)this).transform.localScale;
				localScale.x = ((!state.flipped) ? 1 : (-1));
				((Component)this).transform.localScale = localScale;
				if ((Object)(object)statusDisplay != (Object)null)
				{
					((Component)statusDisplay).transform.localScale = localScale;
				}
			}
		}
	}

	public ushort Health => state.health;

	public bool IsInvisibleForLocalPlayer => State.IsInvisible(GameManager.GameState, GameManager.LocalPlayer.Id);

	private void Start()
	{
		OnMoveComplete();
		CheckOverHeads();
		GameEvents.OnFinishedProcessing += OnFinishedProcessing;
		GameEvents.OnStartedProcessing += OnStartedProcessing;
	}

	private void OnStartedProcessing()
	{
		if (GameManager.GameState.CurrentPlayer != Owner.Id)
		{
			HideOverHeadIcon();
		}
	}

	private void OnFinishedProcessing()
	{
		CheckOverHeads();
	}

	private void Update()
	{
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (runAttackAnimation)
		{
			GridDirection direction = GridDirections.Random();
			WorldCoordinates coordinates = Tile.GetNeighbor(direction).Coordinates;
			if (data.GetRange() > 1)
			{
				coordinates = MapRenderer.Current.GetTileInstance(coordinates).GetNeighbor(direction).Coordinates;
			}
			Attack(coordinates);
		}
		else if (fakeDamage)
		{
			Tile.Damage(Random.Range(0, 101));
			Tile.RenderUnit();
		}
		if (!State.HasEffect(UnitEffect.Invisible))
		{
			return;
		}
		LayerData[] array = this.layerData;
		for (int i = 0; i < array.Length; i++)
		{
			LayerData layerData = array[i];
			if (layerData.layerType != LayerData.LayerTypes.Outline)
			{
				SpriteRenderer val = layerData.spriteRenderer;
				if (!((Object)(object)val == (Object)null))
				{
					Color color = val.color;
					color.a = 0.75f + Mathf.Sin(Time.time * 3f) * 0.1f;
					val.color = color;
				}
			}
		}
	}

	public static Unit CreateUnit(UnitData unitData)
	{
		Unit unit = Object.Instantiate<Unit>(PrefabManager.GetPrefab(unitData.type), LevelManager.UnitHolder);
		MapRenderer.Current.AddUnit(unit);
		return unit;
	}

	public void SetData(UnitData data)
	{
		this.data = data;
	}

	public void SetState(UnitState state)
	{
		this.state = state;
	}

	public override void UpdateObject()
	{
		if (!((Object)(object)this != (Object)null))
		{
			return;
		}
		DrawNewUnit();
		UpdateStatusDisplay();
		if (state.HasLeader())
		{
			UpdateConnector();
		}
		if (state.HasFollower())
		{
			Unit unitInstance = MapRenderer.Current.GetUnitInstance(state.follower);
			if ((Object)(object)unitInstance != (Object)null)
			{
				unitInstance.UpdateConnector();
			}
		}
		LevelManager.GetClientInteraction().TryUpdateSelection(this);
	}

	public void UpdateConnector()
	{
		Log.Verbose("Set master to {0}", new object[1] { state.follower });
		if ((Object)(object)connector != (Object)null)
		{
			Unit unitInstance = MapRenderer.Current.GetUnitInstance(state.leader);
			connector.master = unitInstance;
		}
	}

	public override void Destroy()
	{
		Log.Verbose("Destroying {0}", new object[1] { state });
		MapRenderer.Current.RemoveUnit(this);
		if ((Object)(object)Tile != (Object)null && (Object)(object)Tile.Unit == (Object)(object)this)
		{
			Tile.Unit = null;
		}
		ShortcutExtensions.DOKill((Component)(object)spriteContainer, false);
		LevelManager.GetClientInteraction().TryDeselectUnit(this);
		HideDeathIndicator();
		base.Destroy();
	}

	private void OnDestroy()
	{
		if ((Object)(object)statusDisplay != (Object)null)
		{
			statusDisplay.ReturnToPool();
			statusDisplay = null;
		}
		GameEvents.OnFinishedProcessing -= OnFinishedProcessing;
		GameEvents.OnStartedProcessing -= OnStartedProcessing;
		HideOverHeadIcon();
		TweenUtils.KillTween(moveTween);
		TweenUtils.KillTween(swayTween);
		TweenUtils.KillTween(attackMoveTween);
		TweenUtils.KillTween(attackReturnTween);
	}

	public override void SetVisible(bool value)
	{
		if (!((Object)(object)this == (Object)null))
		{
			((Component)spriteContainer).gameObject.SetActive(value);
			UpdateStatusDisplay();
		}
	}

	public override bool GetVisible()
	{
		return ((Component)spriteContainer).gameObject.activeInHierarchy;
	}

	public void SetOutlineColor(Color color)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (this.layerData == null || this.layerData.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this.layerData.Length; i++)
		{
			LayerData layerData = this.layerData[i];
			if (layerData.layerType == LayerData.LayerTypes.Outline && (Object)(object)layerData.spriteRenderer != (Object)null)
			{
				layerData.spriteRenderer.color = color;
			}
			if (layerData.layerType == LayerData.LayerTypes.Style && (Object)(object)layerData.stylePicker != (Object)null)
			{
				layerData.stylePicker.SetOutlineColor(color);
			}
		}
	}

	public WorldCoordinates GetHomeTile()
	{
		if (state != null)
		{
			return state.home;
		}
		return WorldCoordinates.NULL_COORDINATES;
	}

	public void Move(WorldCoordinates from, WorldCoordinates to, bool shouldAnimate = true, Action onComplete = null)
	{
		MoveInternal(new List<WorldCoordinates> { to, from }, shouldAnimate, onComplete);
	}

	public void Move(List<WorldCoordinates> path, bool shouldAnimate = true, Action onComplete = null, Action onStepComplete = null)
	{
		if (path == null || path.Count <= 1)
		{
			onComplete?.Invoke();
		}
		else
		{
			MoveInternal(path, shouldAnimate, onComplete);
		}
	}

	public void Attack(WorldCoordinates target, bool moveToTarget = false, Action onComplete = null)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		float num = MapDataExtensions.ChebyshevDistance(Tile.Coordinates, target);
		float time = 0.2f + num * 0.05f;
		Tile targetTile = MapRenderer.Current.GetTileInstance(target);
		WeaponGFX pooledObject = ObjectPool.GetPooledObject<WeaponGFX>(((Component)PrefabManager.GetPrefab(data.weapon)).gameObject);
		pooledObject.IsUsed = true;
		((Component)pooledObject).transform.position = ((Component)this).transform.position + Center;
		pooledObject.SetSkin(Owner.skinType);
		if (Object.op_Implicit((Object)(object)targetTile.Unit))
		{
			targetTile.Unit.Flipped = targetTile.Unit.State.flipped;
		}
		Flipped = State.flipped;
		switch (data.weapon)
		{
		case UnitData.WeaponEnum.Sting:
			time *= 0.5f;
			break;
		case UnitData.WeaponEnum.IceArrow:
			time *= 1f;
			break;
		case UnitData.WeaponEnum.Gun:
			time *= 1.5f;
			Sway();
			Tile.SpawnDarkPuff();
			break;
		case UnitData.WeaponEnum.FireBlow:
			time *= 2f;
			Tile.SpawnDarkPuff();
			break;
		case UnitData.WeaponEnum.Poison:
			time *= 2f;
			Tile.SpawnPoison();
			break;
		}
		if (data.GetRange() > 1)
		{
			pooledObject.AnimateRanged(time, target, delegate
			{
				switch (data.weapon)
				{
				case UnitData.WeaponEnum.Gun:
					time *= 1.5f;
					targetTile.SpawnEmbers(time);
					break;
				case UnitData.WeaponEnum.FireBlow:
					time *= 2f;
					targetTile.SpawnEmbers(time);
					break;
				case UnitData.WeaponEnum.IceArrow:
					time *= 2f;
					targetTile.SpawnSparkles(time);
					break;
				}
				onComplete?.Invoke();
			});
		}
		else
		{
			Vector3 val = Vector3.Lerp(Vector2.op_Implicit(Tile.Coordinates.ToPosition()), Vector2.op_Implicit(targetTile.Coordinates.ToPosition()), 0.5f);
			if (moveToTarget)
			{
				attackMoveTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, Vector2.op_Implicit(targetTile.Coordinates.ToPosition()), 0.2f, false), GameManager.GetSharedAnimationCurves().unitAttackAndMoveAnimationCurve);
			}
			else
			{
				attackReturnTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMove(((Component)this).transform, val, 0.3f, false), GameManager.GetSharedAnimationCurves().unitAttackAndReturnAnimationCurve);
			}
			pooledObject.AnimateCloseRangeAttack(0.2f);
			GameManager.DelayCall(100, onComplete);
		}
	}

	public bool CanAttack(WorldCoordinates target)
	{
		if (!State.CanAttack())
		{
			return false;
		}
		return HasAttackOption(target);
	}

	public bool HasAttackOption(WorldCoordinates target)
	{
		if (attackOptions == null || attackOptions.Count == 0)
		{
			attackOptions = State.GetAttackOptions(GameManager.GameState, data.GetRange());
		}
		return attackOptions.Contains(target);
	}

	public bool CanAttackAnything()
	{
		if (!State.CanAttack())
		{
			return false;
		}
		attackOptions = State.GetAttackOptions(GameManager.GameState, data.GetRange());
		return attackOptions.Count > 0;
	}

	public bool CanMoveTo(WorldCoordinates target)
	{
		if (!State.CanMove())
		{
			return false;
		}
		if (walkOptions == null || walkOptions.Count == 0)
		{
			walkOptions = State.GetMovementOptions(GameManager.GameState, state.GetMovement(GameManager.GameState));
		}
		return walkOptions.Contains(target);
	}

	public bool CanMoveAnywhere()
	{
		if (!State.CanMove())
		{
			return false;
		}
		if (walkOptions == null || walkOptions.Count == 0)
		{
			walkOptions = State.GetMovementOptions(GameManager.GameState, state.GetMovement(GameManager.GameState));
		}
		return walkOptions.Count > 0;
	}

	public void Sway()
	{
		if (swayTween == null || !swayTween.active)
		{
			swayTween = TweenUtils.Sway(spriteContainer);
		}
	}

	protected void DrawNewUnit()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		MaterialPropertyBlock val = new MaterialPropertyBlock();
		float num = 0f;
		Color white = Color.white;
		bool flag = !State.moved || CanAttackAnything();
		if (State.HasEffect(UnitEffect.Frozen))
		{
			num = 0.8f;
			((Color)(ref white))._002Ector(0.4f, 0.9f, 1f);
		}
		else if (State.HasEffect(UnitEffect.Poisoned))
		{
			num = 0.5f;
			((Color)(ref white))._002Ector(0.4f, 0.9f, 0f);
		}
		else if (State.HasEffect(UnitEffect.Boosted))
		{
			num = 0.5f;
			((Color)(ref white))._002Ector(1f, 0.3f, 0f);
		}
		else if (!flag)
		{
			num = 0.4f;
		}
		bool flag2 = flag && GameManager.IsPlayerViewing(Owner.Id);
		bool flag3 = state.HasDynamicStyle();
		SkinType skinType = (flag3 ? state.skinType : Owner.skinType);
		LayerData[] array = this.layerData;
		for (int i = 0; i < array.Length; i++)
		{
			LayerData layerData = array[i];
			SpriteRenderer val2 = layerData.spriteRenderer;
			if (layerData.layerType == LayerData.LayerTypes.Climate)
			{
				int num2 = Owner.GetTribeClimate(GameManager.GameState);
				if (State.style != -1)
				{
					num2 = State.style;
				}
				val2 = layerData.stylePicker.SpriteRenderer;
				layerData.stylePicker.SetStyleAndSkin(num2.ToString(), Owner.skinType);
				layerData.stylePicker.OutlineEnabled = flag2;
			}
			else if (layerData.layerType == LayerData.LayerTypes.Style)
			{
				string styleId = (flag3 ? state.style.ToString() : Owner.GetTribeStyle(GameManager.GameState).ToString());
				if (layerData.part == LayerData.Part.Head && SeasonManager.TryGetHeadVariant(Owner, out var seasonalSkin))
				{
					layerData.stylePicker.SetStyleAndSkin(styleId, seasonalSkin);
				}
				else
				{
					layerData.stylePicker.SetStyleAndSkin(styleId, skinType);
				}
				layerData.stylePicker.OutlineEnabled = flag2;
				val2 = layerData.stylePicker.SpriteRenderer;
			}
			else if (layerData.layerType == LayerData.LayerTypes.Outline)
			{
				((Component)layerData.spriteRenderer).gameObject.SetActive(flag2);
				val2 = null;
			}
			else if (layerData.layerType == LayerData.LayerTypes.Tint && (Object)(object)layerData.spriteRenderer != (Object)null)
			{
				Color playerColor = Owner.GetPlayerColor(GameManager.GameState);
				if (State.HasEffect(UnitEffect.Invisible))
				{
					playerColor.a = layerData.spriteRenderer.color.a;
				}
				layerData.spriteRenderer.color = playerColor;
			}
			if ((Object)(object)val2 != (Object)null)
			{
				((Renderer)val2).GetPropertyBlock(val);
			}
			val.SetFloat("_OverlayStrength", num);
			val.SetColor("_OverlayColor", white);
			if ((Object)(object)val2 != (Object)null)
			{
				((Renderer)val2).SetPropertyBlock(val);
			}
		}
		Flipped = State.flipped;
	}

	protected void UpdateStatusDisplay()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		_ = GameManager.GameState;
		if (GetVisible() && Health > 0 && Data.type != UnitData.Type.Scout)
		{
			if ((Object)(object)statusDisplay == (Object)null)
			{
				statusDisplay = ObjectPool.GetPooledObject<UnitStatusDisplay>("UnitStatusDisplay");
				((Component)statusDisplay).transform.SetParent(spriteContainer);
				((Component)statusDisplay).transform.localPosition = new Vector3(0f, 0.2f, 0f) - ((Component)spriteContainer).transform.localPosition;
				statusDisplay.SetUnit(this);
			}
			statusDisplay.SetState(State);
			statusDisplay.RefreshColor();
			ShowStatusDisplayDot(ActivateNeighbourDetectors());
			statusDisplay.Show();
		}
		else if ((Object)(object)statusDisplay != (Object)null)
		{
			statusDisplay.ReturnToPool();
			statusDisplay = null;
		}
	}

	public void ShowStatusDisplayDot(bool show)
	{
		if (!((Object)(object)statusDisplay == (Object)null))
		{
			statusDisplay.SetDot(show);
		}
	}

	private bool ActivateNeighbourDetectors()
	{
		if (state.owner == GameManager.LocalPlayer.Id)
		{
			return state.IsDetectingHiddenUnits(GameManager.GameState);
		}
		return false;
	}

	private bool CanDetectHiddenUnits(UnitState unitState)
	{
		return unitState.owner == GameManager.LocalPlayer.Id;
	}

	private void MoveInternal(List<WorldCoordinates> path, bool shouldAnimate, Action onComplete = null)
	{
		if ((Object)(object)Tile != (Object)null)
		{
			Tile.Unit = null;
		}
		Tile = MapRenderer.Current.GetTileInstance(path[0]);
		Tile.Unit = this;
		HideOverHeadIcon();
		if (shouldAnimate)
		{
			AnimatePathMove(path, onComplete);
			return;
		}
		Tile.RenderUnit();
		OnMoveComplete();
		onComplete?.Invoke();
	}

	public void AnimatePathMove(List<WorldCoordinates> path, Action onComplete = null)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		if (swayTween != null)
		{
			TweenUtils.KillTween(swayTween);
		}
		WorldCoordinates coordinates = path[path.Count - 1];
		((Component)this).transform.position = Vector2.op_Implicit(coordinates.ToPosition());
		PlayMoveSFX(path[0]);
		float num = 0.1f + 0.1f * (float)path.Count;
		float progress = 0f;
		WorldCoordinates lastFrom = WorldCoordinates.NULL_COORDINATES;
		WorldCoordinates lastTo = WorldCoordinates.NULL_COORDINATES;
		moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => progress), (DOSetter<float>)delegate(float newProgress)
		{
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			progress = newProgress;
			float num2 = GameManager.GetSharedAnimationCurves().moveUnitAnimationCurve.Evaluate(progress) * (float)(path.Count - 1);
			int num3 = Mathf.FloorToInt(num2);
			int num4 = path.Count - 1 - num3;
			int index = Mathf.Max(num4 - 1, 0);
			float num5 = num2 - (float)num3;
			Tile tileInstance = MapRenderer.Current.GetTileInstance(path[index]);
			if ((Object)(object)tileInstance != (Object)null && !GetVisible() && !tileInstance.IsHidden && !IsInvisibleForLocalPlayer)
			{
				SetVisible(value: true);
			}
			if (path[num4] != lastFrom || path[index] != lastTo)
			{
				if (path[num4] != path[index])
				{
					GridDirection? direction = WorldCoordinates.GetDirection(path[num4], path[index]);
					if (direction.HasValue)
					{
						switch (direction)
						{
						case GridDirection.SW:
						case GridDirection.W:
						case GridDirection.NW:
						case GridDirection.N:
							if (!Flipped)
							{
								Flipped = true;
							}
							break;
						default:
							if (Flipped)
							{
								Flipped = false;
							}
							break;
						}
					}
				}
				lastFrom = path[num4];
				lastTo = path[index];
			}
			((Component)this).transform.position = Vector3.LerpUnclamped(Vector2.op_Implicit(path[num4].ToPosition()), Vector2.op_Implicit(path[index].ToPosition()), num5);
		}, 1f, num), (TweenCallback)delegate
		{
			if ((Object)(object)Tile != (Object)null)
			{
				Tile.RenderUnit();
			}
			OnMoveComplete();
		});
		if (state.HasFollower() && onComplete != null)
		{
			GameManager.DelayCall(80, onComplete);
		}
		else
		{
			GameManager.DelayCall((int)(num * 1000f), onComplete);
		}
	}

	private void PlayMoveSFX(WorldCoordinates to)
	{
		TileData tileData = GameManager.GameState.Map.GetTile(to);
		SFXTypes id = SFXTypes.None;
		if (Data.HasAbility(UnitAbility.Type.Fly))
		{
			id = SFXTypes.MoveAir;
		}
		else if (tileData.IsWater)
		{
			id = SFXTypes.MoveWater;
		}
		else if (state.HasEffect(UnitEffect.Invisible))
		{
			id = SFXTypes.MoveAir;
		}
		else if (Data.HasAbility(UnitAbility.Type.Creep))
		{
			id = SFXTypes.Creep;
		}
		else
		{
			switch (tileData.terrain)
			{
			case TerrainData.Type.Field:
			case TerrainData.Type.Mountain:
				id = SFXTypes.MoveGrass;
				break;
			case TerrainData.Type.Forest:
				id = SFXTypes.MoveForest;
				break;
			case TerrainData.Type.Ice:
				id = SFXTypes.MoveIce;
				break;
			}
		}
		AudioManager.PlaySFXAtTile(id, tileData.coordinates);
	}

	private void OnMoveComplete()
	{
		if ((Object)(object)this != (Object)null)
		{
			attackOptions = null;
			DrawNewUnit();
			UpdateStatusDisplay();
		}
	}

	public void CheckOverHeads()
	{
		if (State != null && (Object)(object)Tile != (Object)null && GameManager.GameState != null && GameManager.IsPlayerViewing(Owner.Id) && GameManager.GameState.CurrentPlayer == Owner.Id && !GameManager.Client.IsRecap)
		{
			if (State.CanExamineRuins(GameManager.GameState, Tile.Data))
			{
				ExamineRuinsCommand command = new ExamineRuinsCommand(GameManager.LocalPlayer.Id, State.coordinates);
				if (!command.IsValid(GameManager.GameState))
				{
					return;
				}
				ShowOverHeadIcon(HintIcon.IconTypes.ExamineRuins, delegate
				{
					overHeadIcon = null;
					if (!SendCommand(command))
					{
						CheckOverHeads();
					}
				});
				return;
			}
			if (State.CanCapture(GameManager.GameState, Tile.Data))
			{
				CaptureCommand command2 = new CaptureCommand(GameManager.LocalPlayer.Id, State.id, State.coordinates);
				if (!command2.IsValid(GameManager.GameState))
				{
					return;
				}
				ShowOverHeadIcon(HintIcon.IconTypes.CaptureCity, delegate
				{
					overHeadIcon = null;
					if (!SendCommand(command2))
					{
						CheckOverHeads();
					}
				});
				return;
			}
			if (State.CanBePromoted(GameManager.GameState))
			{
				PromoteCommand command3 = new PromoteCommand(GameManager.LocalPlayer.Id, State.coordinates);
				if (!command3.IsValid(GameManager.GameState))
				{
					return;
				}
				ShowOverHeadIcon(HintIcon.IconTypes.Promote, delegate
				{
					overHeadIcon = null;
					if (!SendCommand(command3))
					{
						CheckOverHeads();
					}
				});
				return;
			}
		}
		HideOverHeadIcon();
	}

	private bool SendCommand(CommandBase command)
	{
		if (ClientActionManager.CanExecuteCommand(command, GameManager.GameState))
		{
			InputEvents.SelectionCleared();
			GameManager.Client.SendCommand(command);
			return true;
		}
		return false;
	}

	public void HideOverHeadIcon()
	{
		if ((Object)(object)overHeadIcon != (Object)null)
		{
			overHeadIcon.Hide();
			overHeadIcon = null;
		}
	}

	private void ShowOverHeadIcon(HintIcon.IconTypes iconType, Action callback)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)overHeadIcon != (Object)null) || overHeadIcon.Type != iconType)
		{
			if ((Object)(object)overHeadIcon == (Object)null)
			{
				overHeadIcon = UIWorldIconContainer.GetHintIcon(State.coordinates);
				overHeadIcon.WorldOffset = new Vector3(0f, 0.2f, 0f);
			}
			overHeadIcon.Callback = callback;
			overHeadIcon.Type = iconType;
			overHeadIcon.Coordinates = State.coordinates;
			overHeadIcon.Show();
		}
	}

	public override void OnSelected()
	{
		UpdateOverlays();
		PopupManager.HideCurrentPopup();
		InputEvents.UnitSelected(this);
		Sway();
		SwayLinked();
		SwayLinked(up: false);
	}

	public void UpdateSelection()
	{
		UpdateOverlays();
		InputEvents.UnitSelected(this);
	}

	public override void OnDeselected()
	{
		ClearUnitOverlays();
		ClearTileOverlays();
	}

	private void ClearUnitOverlays()
	{
		foreach (Unit overlayUnit in overlayUnits)
		{
			overlayUnit.HideDeathIndicator();
		}
		overlayUnits.Clear();
	}

	private void ClearTileOverlays()
	{
		foreach (Tile overlayTile in overlayTiles)
		{
			overlayTile.HideOverlay();
		}
		overlayTiles.Clear();
	}

	private void UpdateOverlays()
	{
		ClearUnitOverlays();
		ClearTileOverlays();
		if (Owner.Id != GameManager.LocalPlayer.Id || GameManager.LocalPlayer.Id != GameManager.Client.GameState.CurrentPlayer || GameManager.LocalPlayer.AutoPlay)
		{
			return;
		}
		walkOptions = State.GetMovementOptions(GameManager.GameState, state.GetMovement(GameManager.GameState));
		if (walkOptions != null && !State.moved)
		{
			for (int i = 0; i < walkOptions.Count; i++)
			{
				Tile tileInstance = MapRenderer.Current.GetTileInstance(walkOptions[i]);
				tileInstance.ShowOverlay(OverlayType.Walk);
				overlayTiles.Add(tileInstance);
			}
		}
		attackOptions = State.GetAttackOptions(GameManager.GameState, data.GetRange());
		if (attackOptions == null || !State.CanAttack())
		{
			return;
		}
		for (int j = 0; j < attackOptions.Count; j++)
		{
			Tile tileInstance2 = MapRenderer.Current.GetTileInstance(attackOptions[j]);
			if (state.HasAbility(UnitAbility.Type.Infiltrate, GameManager.GameState))
			{
				tileInstance2.ShowOverlay(OverlayType.Kill);
				overlayTiles.Add(tileInstance2);
				continue;
			}
			BattleResults battleResults = BattleHelpers.GetBattleResults(GameManager.GameState, State, tileInstance2.Unit.State);
			bool flag = battleResults.attackDamage >= tileInstance2.Unit.State.health;
			if (battleResults.retaliationDamage >= State.health)
			{
				tileInstance2.ShowOverlay(OverlayType.Kill);
				overlayTiles.Add(tileInstance2);
			}
			else
			{
				tileInstance2.ShowOverlay(OverlayType.Attack);
				overlayTiles.Add(tileInstance2);
			}
			if (flag)
			{
				tileInstance2.Unit.ShowDeathIndicator();
				overlayUnits.Add(tileInstance2.Unit);
			}
		}
	}

	public void SwayLinked(bool up = true)
	{
		uint num = (up ? state.leader : state.follower);
		if (num == 0 || GameManager.GameState == null || !GameManager.GameState.TryGetUnit(num, out var unit))
		{
			return;
		}
		WorldCoordinates coordinates = unit.coordinates;
		if (!(coordinates != state.coordinates))
		{
			return;
		}
		Tile masterTile = MapRenderer.Current.GetTileInstance(coordinates);
		if (!((Object)(object)masterTile != (Object)null) || !((Object)(object)masterTile.Unit != (Object)null))
		{
			return;
		}
		GameManager.DelayCall(50, delegate
		{
			if (!((Object)(object)masterTile == (Object)null) && !((Object)(object)masterTile.Unit == (Object)null) && masterTile.Unit.State != null)
			{
				masterTile.Unit.Sway();
				masterTile.Unit.SwayLinked(up);
			}
		});
	}

	public void ShowDeathIndicator()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)head == (Object)null))
		{
			if ((Object)(object)sweatDrops == (Object)null)
			{
				sweatDrops = ObjectPool.GetPooledObject<SweatDrops>("Drops");
			}
			((Component)sweatDrops).gameObject.SetActive(true);
			sweatDrops.IsUsed = true;
			((Component)sweatDrops).transform.position = ((Component)head).transform.position;
			((Component)sweatDrops).transform.parent = ((Component)this).transform;
		}
	}

	public void HideDeathIndicator()
	{
		if (!((Object)(object)sweatDrops == (Object)null))
		{
			((Component)sweatDrops).transform.parent = ((Component)ObjectPool.instance).transform;
			sweatDrops.IsUsed = false;
			sweatDrops.ReturnToPool();
			sweatDrops = null;
		}
	}

	public bool IsInteractableByPlayer(byte id)
	{
		if (!State.moved || CanAttackAnything())
		{
			return Owner.Id == id;
		}
		return false;
	}

	public override string ToString()
	{
		return $"{Owner.tribe} ({Owner.UserName})";
	}
}
