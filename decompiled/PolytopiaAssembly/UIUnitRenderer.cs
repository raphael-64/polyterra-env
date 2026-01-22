using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UIUnitRenderer : MonoBehaviour
{
	public Unit sourceUnit;

	protected TribeData tribeData;

	protected PlayerState playerState;

	protected RectTransform m_rectTransform;

	public UnitData.Type UnitType { get; set; }

	public SkinType SkinType { get; set; }

	public Unit SourceUnit
	{
		get
		{
			return sourceUnit;
		}
		set
		{
			sourceUnit = value;
			UnitType = sourceUnit.Data.type;
		}
	}

	public TribeData TribeData
	{
		get
		{
			return tribeData;
		}
		set
		{
			tribeData = value;
		}
	}

	public PlayerState PlayerState
	{
		get
		{
			return playerState;
		}
		set
		{
			playerState = value;
			TribeData = playerState.GetTribeData(GameManager.GameState);
			SkinType = PlayerState.skinType;
		}
	}

	protected Color TintColor
	{
		get
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (PlayerState != null)
			{
				return PlayerState.GetPlayerColor(GameManager.GameState);
			}
			return ColorUtil.ColorFromInt((uint)TribeData.color);
		}
	}

	public RectTransform rectTransform
	{
		get
		{
			if ((Object)(object)m_rectTransform == (Object)null)
			{
				m_rectTransform = ((Component)this).GetComponent<RectTransform>();
			}
			return m_rectTransform;
		}
	}

	public void RefreshGraphics()
	{
		if (TribeData == null)
		{
			Log.Error($"Failed to refresh ui unit renderer unitdata {0}, tribedata {1} ", new object[1] { TribeData });
		}
		else
		{
			CreateUnit();
		}
	}

	private void CreateUnit()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		rectTransform.sizeDelta = new Vector2(100f, 100f);
		if ((Object)(object)sourceUnit == (Object)null)
		{
			sourceUnit = PrefabManager.GetPrefab(UnitType);
		}
		((Object)((Component)this).gameObject).name = UnitType.ToString();
		SpriteRenderer[] componentsInChildren = ((Component)sourceUnit).GetComponentsInChildren<SpriteRenderer>();
		if (componentsInChildren.Length == 0)
		{
			Log.Error("Found no spriterenderers on {0}", new object[1] { ((Object)sourceUnit).name });
		}
		List<int> list = new List<int>();
		SpriteRenderer[] array = componentsInChildren;
		foreach (SpriteRenderer val in array)
		{
			if (((Component)val).gameObject.activeSelf && ((Renderer)val).enabled)
			{
				list.Add(((Renderer)val).sortingOrder);
			}
		}
		Array.Sort(list.ToArray(), componentsInChildren);
		int num = LayerMask.NameToLayer("HideInUI");
		array = componentsInChildren;
		foreach (SpriteRenderer val2 in array)
		{
			if (((Renderer)val2).sortingLayerID != MeshCache.STATUS_DISPLAYS_LAYER_ID && ((Component)val2).gameObject.layer != num)
			{
				CreateImage(val2, sourceUnit, ((Component)this).transform, 1f);
			}
		}
		Bounds imageContentOffset = UIUtils.GetImageContentOffset(rectTransform);
		rectTransform.sizeDelta = new Vector2(((Bounds)(ref imageContentOffset)).size.x, ((Bounds)(ref imageContentOffset)).size.y);
	}

	protected void CreateImage(SpriteRenderer spriteRenderer, Unit source, Transform newParent, float scale)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		float parentScale = (((Object)(object)((Component)spriteRenderer).transform.parent != (Object)(object)source) ? ((Component)spriteRenderer).transform.parent.localScale.x : 1f);
		Vector3 spriteRendererScale = ((Component)spriteRenderer).transform.localScale;
		string style = ((PlayerState != null) ? PlayerState.GetTribeStyle(GameManager.GameState).ToString() : tribeData.style.ToString());
		string style2 = ((PlayerState != null) ? PlayerState.GetTribeClimate(GameManager.GameState).ToString() : tribeData.climate.ToString());
		List<Unit.LayerData?> layerDataList = GetLayersDataForSpriteRenderer(spriteRenderer, source);
		Unit.LayerData? layerData = layerDataList.Find((Unit.LayerData? x) => x.HasValue && (Object)(object)x.Value.stylePicker != (Object)null);
		Image image = GetImage();
		if (layerData.HasValue && layerData.Value.stylePicker is ClimatePicker climatePicker)
		{
			climatePicker.GetSprite(SkinType, style2, delegate(Sprite s)
			{
				SetImage(image, s);
			});
		}
		else if (layerData.HasValue)
		{
			if (layerData.Value.part == Unit.LayerData.Part.Head && PlayerState != null && SeasonManager.TryGetHeadVariant(PlayerState, out var seasonalSkin))
			{
				layerData.Value.stylePicker.GetSprite(seasonalSkin, style, delegate(Sprite sprite)
				{
					SetImage(image, sprite);
				});
			}
			else
			{
				layerData.Value.stylePicker.GetSprite(SkinType, style, delegate(Sprite sprite)
				{
					SetImage(image, sprite);
				});
			}
		}
		else
		{
			SetImage(image, spriteRenderer.sprite);
		}
		Image GetImage()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject
			{
				name = ((Object)((Component)spriteRenderer).gameObject).name
			};
			val.transform.SetParent(newParent);
			return val.AddComponent<Image>();
		}
		static List<Unit.LayerData?> GetLayersDataForSpriteRenderer(SpriteRenderer val, Unit unit)
		{
			List<Unit.LayerData?> list = new List<Unit.LayerData?>();
			Unit.LayerData[] layerData2 = unit.layerData;
			for (int i = 0; i < layerData2.Length; i++)
			{
				Unit.LayerData value = layerData2[i];
				if ((Object)(object)value.GetSpriteRenderer() == (Object)(object)val)
				{
					list.Add(value);
				}
			}
			return list;
		}
		void SetImage(Image val, Sprite sprite)
		{
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0198: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01eb: Expected O, but got Unknown
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)sprite == (Object)null)
			{
				Log.Error("Sprite is null for {0} in unit {1}", new object[2]
				{
					((Object)spriteRenderer).name,
					((Object)source).name
				});
			}
			val.sprite = sprite;
			val.useSpriteMesh = true;
			((Graphic)val).raycastTarget = false;
			((Graphic)val).SetNativeSize();
			RectTransform val2 = ((Graphic)val).rectTransform;
			float uI_PIXELS_PER_UNIT = UIConstants.UI_PIXELS_PER_UNIT;
			val2.sizeDelta *= scale * parentScale;
			float x = val2.sizeDelta.x;
			Rect rect = sprite.rect;
			float num = x / ((Rect)(ref rect)).size.x;
			Vector2 pivot = sprite.pivot;
			rect = sprite.rect;
			Vector3 val3 = Vector2.op_Implicit((pivot - ((Rect)(ref rect)).size / 2f) * spriteRendererScale.x * num);
			val2.anchoredPosition = Vector2.op_Implicit(((Component)source).transform.InverseTransformPoint(((Component)spriteRenderer).transform.position) * uI_PIXELS_PER_UNIT * scale - val3);
			if (val2.sizeDelta.x <= 0f || val2.sizeDelta.y <= 0f)
			{
				Log.Error("Invalid size of image {0}, scale {1}, parentScale {2}", new object[3] { val2.sizeDelta, scale, parentScale });
			}
			((Transform)val2).localScale = new Vector3(spriteRendererScale.x, spriteRendererScale.y, 1f);
			if (spriteRendererScale.x == 0f || spriteRendererScale.y == 0f)
			{
				Log.Error("Invalid scale {0}", new object[1] { spriteRendererScale });
			}
			MaterialPropertyBlock val4 = new MaterialPropertyBlock();
			((Renderer)spriteRenderer).GetPropertyBlock(val4);
			((Graphic)val).material = UIManager.Instance.SpriteDuplicator.GetCachedCopiedMaterial(((Renderer)spriteRenderer).sharedMaterial, val4.GetColor("_OverlayColor"), val4.GetFloat("_OverlayStrength"));
			if (layerDataList.Find((Unit.LayerData? layerData2) => layerData2.HasValue && layerData2.Value.layerType == Unit.LayerData.LayerTypes.Tint).HasValue)
			{
				((Graphic)val).color = TintColor;
			}
			else if (!val4.isEmpty && val4.GetColor("_Color") != Color.clear)
			{
				((Graphic)val).color = val4.GetColor("_Color");
			}
			else
			{
				((Graphic)val).color = spriteRenderer.color;
			}
		}
	}
}
