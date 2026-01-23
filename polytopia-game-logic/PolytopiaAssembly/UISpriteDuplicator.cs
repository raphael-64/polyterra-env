using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISpriteDuplicator
{
	public class SpriteDuplicationData
	{
		public UIDuplicatedSprites container;

		public Transform source;

		public float scale;

		public string excludeSortingLayer = string.Empty;

		public Vector2 offset = Vector2.zero;

		public bool forceFullAlpha;

		public SpriteDuplicationData(Transform source, UIDuplicatedSprites container, float scale = 1f, string excludeSortingLayer = "", Vector2 offset = default(Vector2))
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			this.source = source;
			this.container = container;
			this.scale = scale;
			this.excludeSortingLayer = excludeSortingLayer;
			this.offset = offset;
		}
	}

	private Dictionary<string, Material> cachedNameToMaterials = new Dictionary<string, Material>();

	public unsafe Material GetCachedCopiedMaterial(Material material, Color color, float overlayStrength)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		string key = ((Object)material).name + ((object)(*(Color*)(&color))/*cast due to .constrained prefix*/).ToString() + overlayStrength;
		if (cachedNameToMaterials.ContainsKey(key))
		{
			return cachedNameToMaterials[key];
		}
		Material val = Object.Instantiate<Material>(material);
		val.SetColor("_OverlayColor", color);
		val.SetFloat("_OverlayStrength", overlayStrength);
		cachedNameToMaterials.Add(key, val);
		return val;
	}

	public void RenderSprites(SpriteDuplicationData data)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		RenderSprites(data.source, data.container, data.scale, data.excludeSortingLayer, data.offset, data.forceFullAlpha);
	}

	public void RenderSprites(Transform source, UIDuplicatedSprites container, float scale = 1f, string excludeSortingLayer = "", Vector2 offset = default(Vector2), bool forceFullAlpha = false)
	{
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		container.Clear();
		int num = (string.IsNullOrEmpty(excludeSortingLayer) ? (-1) : SortingLayer.NameToID(excludeSortingLayer));
		List<Component> list = new List<Component>();
		ISpriteRendererProvider component = ((Component)source).GetComponent<ISpriteRendererProvider>();
		if (component != null)
		{
			List<PolytopiaSpriteRenderer> spriteRenderers = component.GetSpriteRenderers();
			list.AddRange((IEnumerable<Component>)spriteRenderers);
		}
		if (list.Count == 0)
		{
			SpriteRenderer[] componentsInChildren = ((Component)source).GetComponentsInChildren<SpriteRenderer>();
			list.AddRange((IEnumerable<Component>)(object)componentsInChildren);
			PolytopiaSpriteRenderer[] componentsInChildren2 = ((Component)source).GetComponentsInChildren<PolytopiaSpriteRenderer>();
			foreach (PolytopiaSpriteRenderer polytopiaSpriteRenderer in componentsInChildren2)
			{
				if ((Object)(object)polytopiaSpriteRenderer.meshRenderer != (Object)null)
				{
					list.Add((Component)(object)polytopiaSpriteRenderer);
				}
			}
			list.Sort(delegate(Component x, Component y)
			{
				int num4 = SortingOrderFromComponent(x);
				int value = SortingOrderFromComponent(y);
				return num4.CompareTo(value);
			});
		}
		int num2 = LayerMask.NameToLayer("HideInUI");
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			Component obj = list[num3];
			SpriteRenderer val = (SpriteRenderer)(object)((obj is SpriteRenderer) ? obj : null);
			if (val != null)
			{
				if (((Renderer)val).sortingLayerID != MeshCache.CITY_STATUS_DISPLAY_ID && ((Component)val).gameObject.layer != num2 && ((Renderer)val).enabled && ((Renderer)val).sortingLayerID != num && (Object)(object)val.sprite != (Object)null)
				{
					CreateImage(val, source, (Transform)(object)container.rectTransform, scale, offset, forceFullAlpha);
				}
			}
			else if (list[num3] is PolytopiaSpriteRenderer polytopiaSpriteRenderer2 && polytopiaSpriteRenderer2.SortingLayer != MeshCache.CITY_STATUS_DISPLAY_ID && ((Component)polytopiaSpriteRenderer2).gameObject.layer != num2 && ((Behaviour)polytopiaSpriteRenderer2).enabled && polytopiaSpriteRenderer2.SortingLayer != num && polytopiaSpriteRenderer2.HasSprite())
			{
				CreateImage(polytopiaSpriteRenderer2, source, container.rectTransform, scale, offset, forceFullAlpha);
			}
		}
		CenterSprites(container.rectTransform, offset);
	}

	public void CenterSprites(RectTransform containerTransform, Vector2 offset = default(Vector2))
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Rect actualRect = UIUtils.GetActualRect(containerTransform, includeRootSize: false);
		Rect rect = containerTransform.rect;
		Vector2 center = ((Rect)(ref rect)).center;
		float y = ((Rect)(ref actualRect)).size.y;
		rect = containerTransform.rect;
		float num = (y - ((Rect)(ref rect)).size.y) * 0.5f;
		num = Mathf.Clamp(num, 0f, num);
		Vector2 val = ((Rect)(ref actualRect)).position - containerTransform.anchoredPosition - center;
		val.y -= num;
		Image[] componentsInChildren = ((Component)containerTransform).GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			RectTransform component = ((Component)componentsInChildren[i]).GetComponent<RectTransform>();
			component.anchoredPosition -= val - offset;
		}
	}

	protected void CreateImage(PolytopiaSpriteRenderer spriteRenderer, Transform source, RectTransform destination, float scale, Vector2 offset = default(Vector2), bool forceFullAlpha = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		MaterialPropertyBlock val = new MaterialPropertyBlock();
		((Renderer)spriteRenderer.meshRenderer).GetPropertyBlock(val);
		GetCachedCopiedMaterial(((Renderer)spriteRenderer.meshRenderer).sharedMaterial, val.GetColor("_OverlayColor"), val.GetFloat("_OverlayStrength"));
		if ((Object)(object)spriteRenderer.Sprite != (Object)null)
		{
			CreateImage(spriteRenderer.Sprite, ((Component)spriteRenderer).transform, (Transform)(object)destination, GetColor(spriteRenderer.Color, forceFullAlpha), spriteRenderer.SharedMaterial, flipX: false, flipY: false, source, scale, offset);
		}
		else
		{
			if (spriteRenderer.Sprites == null)
			{
				return;
			}
			foreach (MeshDescription sprite in spriteRenderer.Sprites)
			{
				CreateImage(sprite.sprite, ((Component)spriteRenderer).transform, (Transform)(object)destination, GetColor(sprite.color, forceFullAlpha), spriteRenderer.SharedMaterial, flipX: false, flipY: false, source, scale, offset);
			}
		}
	}

	protected void CreateImage(SpriteRenderer spriteRenderer, Transform source, Transform destination, float scale, Vector2 offset = default(Vector2), bool forceFullAlpha = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		MaterialPropertyBlock val = new MaterialPropertyBlock();
		((Renderer)spriteRenderer).GetPropertyBlock(val);
		Material cachedCopiedMaterial = GetCachedCopiedMaterial(((Renderer)spriteRenderer).sharedMaterial, val.GetColor("_OverlayColor"), val.GetFloat("_OverlayStrength"));
		CreateImage(spriteRenderer.sprite, ((Component)spriteRenderer).transform, destination, GetColor(spriteRenderer.color, forceFullAlpha), cachedCopiedMaterial, spriteRenderer.flipX, spriteRenderer.flipY, source, scale, offset);
	}

	protected void CreateImage(Sprite sprite, Transform rendererTransform, Transform destinationTransform, Color color, Material material, bool flipX, bool flipY, Transform source, float scale, Vector2 offset = default(Vector2))
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		float num = (((Object)(object)rendererTransform.parent != (Object)(object)source && (Object)(object)rendererTransform.parent != (Object)null) ? rendererTransform.parent.localScale.x : 1f);
		Vector3 localScale = rendererTransform.localScale;
		GameObject val = new GameObject
		{
			name = ((Object)((Component)rendererTransform).gameObject).name
		};
		val.transform.SetParent(destinationTransform);
		Image val2 = val.AddComponent<Image>();
		val2.sprite = sprite;
		val2.useSpriteMesh = true;
		((Graphic)val2).raycastTarget = false;
		((Graphic)val2).SetNativeSize();
		Transform transform = val.transform;
		Transform obj = ((transform is RectTransform) ? transform : null);
		((RectTransform)obj).pivot = new Vector2(0.5f, 0.5f);
		((RectTransform)obj).anchorMin = new Vector2(0.5f, 0.5f);
		((RectTransform)obj).anchorMax = new Vector2(0.5f, 0.5f);
		((RectTransform)obj).sizeDelta = ((RectTransform)obj).sizeDelta * (scale * num);
		float x = ((RectTransform)obj).sizeDelta.x;
		Rect rect = sprite.rect;
		float num2 = x / ((Rect)(ref rect)).size.x;
		Vector2 pivot = sprite.pivot;
		rect = sprite.rect;
		Vector3 val3 = Vector2.op_Implicit((pivot - ((Rect)(ref rect)).size / 2f) * localScale.x * num2);
		((RectTransform)obj).anchoredPosition = Vector2.op_Implicit(source.InverseTransformPoint(rendererTransform.position) * UIConstants.UI_PIXELS_PER_UNIT * scale - val3 + Vector2.op_Implicit(offset));
		obj.localScale = new Vector3(flipX ? (0f - localScale.x) : localScale.x, flipY ? (0f - localScale.y) : localScale.y, 1f);
		((Graphic)val2).color = color;
		((Graphic)val2).material = material;
	}

	private int SortingOrderFromComponent(Component component)
	{
		SpriteRenderer val = (SpriteRenderer)(object)((component is SpriteRenderer) ? component : null);
		if (val != null)
		{
			return ((Renderer)val).sortingOrder;
		}
		if (component is PolytopiaSpriteRenderer polytopiaSpriteRenderer)
		{
			return polytopiaSpriteRenderer.SortingOrder;
		}
		throw new Exception("Component {0} is not a spriteRenderer");
	}

	private Color GetColor(Color color, bool forceFullAlpha)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (forceFullAlpha)
		{
			color.a = 1f;
		}
		return color;
	}
}
