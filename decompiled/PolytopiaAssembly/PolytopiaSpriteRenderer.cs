using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

public class PolytopiaSpriteRenderer : MonoBehaviour, IPooledObject
{
	[Header("Replacing SpriteRenderer")]
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[Header("Standalone")]
	[SerializeField]
	private Sprite sprite;

	[SerializeField]
	private Color color = Color.white;

	private List<MeshDescription> sprites;

	public MeshRenderer meshRenderer;

	public MeshFilter meshFilter;

	[SortingLayer]
	[SerializeField]
	private int sortingLayer;

	[SerializeField]
	private int sortingOrder;

	[SerializeField]
	private Material sharedMaterial;

	private bool isBatched;

	private bool isDirty = true;

	private bool isInitialised;

	private string atlasName;

	public Bounds Bounds => ((Renderer)meshRenderer).bounds;

	public Sprite Sprite
	{
		get
		{
			return sprite;
		}
		set
		{
			sprites = null;
			if (!((Object)(object)sprite == (Object)(object)value))
			{
				if ((Object)(object)spriteRenderer != (Object)null)
				{
					spriteRenderer.sprite = value;
				}
				sprite = value;
				SetDirty(isDirty: true);
			}
		}
	}

	public Color Color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return color;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (!(color == value))
			{
				if ((Object)(object)spriteRenderer != (Object)null)
				{
					spriteRenderer.color = value;
				}
				color = value;
				SetDirty(isDirty: true);
			}
		}
	}

	public List<MeshDescription> Sprites
	{
		get
		{
			return sprites;
		}
		set
		{
			sprite = null;
			if (value == sprites)
			{
				return;
			}
			bool flag = false;
			if (value == null || sprites == null || value.Count != sprites.Count)
			{
				flag = true;
			}
			else
			{
				for (int i = 0; i < sprites.Count; i++)
				{
					if (!value[i].Equals(sprites[i]))
					{
						flag = true;
					}
				}
			}
			sprites = value;
			if (flag)
			{
				SetDirty(flag);
			}
		}
	}

	public int SortingLayer
	{
		get
		{
			return sortingLayer;
		}
		set
		{
			if (sortingLayer != value)
			{
				sortingLayer = value;
				if ((Object)(object)spriteRenderer != (Object)null)
				{
					((Renderer)spriteRenderer).sortingLayerID = value;
				}
				else
				{
					((Renderer)meshRenderer).sortingLayerID = value;
				}
			}
		}
	}

	public int SortingOrder
	{
		get
		{
			return sortingOrder;
		}
		set
		{
			if (sortingOrder != value)
			{
				sortingOrder = value;
				if ((Object)(object)spriteRenderer != (Object)null)
				{
					((Renderer)spriteRenderer).sortingOrder = value;
				}
				else
				{
					((Renderer)meshRenderer).sortingOrder = value;
				}
			}
		}
	}

	public Material SharedMaterial
	{
		get
		{
			return sharedMaterial;
		}
		set
		{
			sharedMaterial = value;
			if ((Object)(object)spriteRenderer != (Object)null)
			{
				((Renderer)spriteRenderer).sharedMaterial = value;
			}
			else
			{
				((Renderer)meshRenderer).sharedMaterial = value;
			}
		}
	}

	public bool IsUsed { get; set; }

	public void Awake()
	{
		Init();
	}

	public void OnEnable()
	{
		if (isDirty)
		{
			GameManager.GetSpriteRendererManager().AddDirty(this);
		}
	}

	public void Init()
	{
		if (isInitialised)
		{
			return;
		}
		isInitialised = true;
		if ((Object)(object)spriteRenderer != (Object)null)
		{
			ReplaceSpriteRendererWithMeshRenderer();
			return;
		}
		if ((Object)(object)meshFilter == (Object)null)
		{
			meshFilter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		}
		if ((Object)(object)meshRenderer == (Object)null)
		{
			meshRenderer = CreateBasicMeshRenderer();
			((Renderer)meshRenderer).sortingLayerID = sortingLayer;
			((Renderer)meshRenderer).sortingOrder = sortingOrder;
			ForceUpdateMesh();
		}
	}

	public Mesh GetMesh()
	{
		if (!isInitialised && (Object)(object)spriteRenderer != (Object)null)
		{
			return GameManager.GetMeshCache().GetOrCreateSpriteMesh(spriteRenderer);
		}
		if (isDirty)
		{
			ForceUpdateMesh();
		}
		return meshFilter.sharedMesh;
	}

	public void ForceUpdateMesh()
	{
		if (!isInitialised)
		{
			Init();
		}
		if (sprites != null && sprites.Count > 0)
		{
			UpdateMultiMesh();
		}
		else
		{
			UpdateSingleMesh();
		}
		MaterialPropertyBlock orCreateMaterialPropertyBlock = GameManager.GetMeshCache().GetOrCreateMaterialPropertyBlock(atlasName);
		((Renderer)meshRenderer).SetPropertyBlock(orCreateMaterialPropertyBlock);
		((Renderer)meshRenderer).sortingLayerID = sortingLayer;
		((Renderer)meshRenderer).sortingOrder = sortingOrder;
		SetDirty(isDirty: false);
	}

	private void UpdateMultiMesh()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[sprites.Count];
		atlasName = null;
		for (int i = 0; i < sprites.Count; i++)
		{
			MeshDescription description = sprites[i];
			string atlasNameForSprite = GameManager.GetSpriteAtlasManager().GetAtlasNameForSprite(description.sprite);
			if (atlasName == null)
			{
				atlasName = atlasNameForSprite;
			}
			else if (atlasNameForSprite != atlasName)
			{
				throw new Exception("Trying to combine sprites from different spriteatlases");
			}
			Mesh orCreateSpriteMesh = GameManager.GetMeshCache().GetOrCreateSpriteMesh(description);
			((CombineInstance)(ref array[i])).mesh = orCreateSpriteMesh;
			((CombineInstance)(ref array[i])).transform = Matrix4x4.identity;
		}
		if ((Object)(object)meshFilter.sharedMesh == (Object)null)
		{
			meshFilter.sharedMesh = new Mesh();
		}
		else
		{
			meshFilter.sharedMesh.Clear();
		}
		meshFilter.sharedMesh.CombineMeshes(array);
	}

	private void UpdateSingleMesh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		Mesh orCreateSpriteMesh = GameManager.GetMeshCache().GetOrCreateSpriteMesh(new MeshDescription(sprite, color, 0));
		if ((Object)(object)orCreateSpriteMesh != (Object)null)
		{
			CombineInstance[] array = (CombineInstance[])(object)new CombineInstance[1];
			((CombineInstance)(ref array[0])).mesh = orCreateSpriteMesh;
			((CombineInstance)(ref array[0])).transform = Matrix4x4.identity;
			if ((Object)(object)meshFilter.sharedMesh == (Object)null)
			{
				meshFilter.sharedMesh = new Mesh();
			}
			else
			{
				meshFilter.sharedMesh.Clear();
			}
			meshFilter.sharedMesh.CombineMeshes(array);
		}
		atlasName = GameManager.GetSpriteAtlasManager().GetAtlasNameForSprite(sprite);
	}

	private MeshRenderer CreateBasicMeshRenderer()
	{
		MeshRenderer obj = ((Component)this).gameObject.AddComponent<MeshRenderer>();
		((Renderer)obj).receiveShadows = false;
		((Renderer)obj).shadowCastingMode = (ShadowCastingMode)0;
		((Renderer)obj).lightProbeUsage = (LightProbeUsage)0;
		((Renderer)obj).reflectionProbeUsage = (ReflectionProbeUsage)0;
		((Renderer)obj).allowOcclusionWhenDynamic = false;
		((Renderer)obj).enabled = !isBatched;
		return obj;
	}

	private void ReplaceSpriteRendererWithMeshRenderer()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Sprite = spriteRenderer.sprite;
		Color = spriteRenderer.color;
		sortingLayer = ((Renderer)spriteRenderer).sortingLayerID;
		sortingOrder = ((Renderer)spriteRenderer).sortingOrder;
		sharedMaterial = ((Renderer)spriteRenderer).sharedMaterial;
		Object.DestroyImmediate((Object)(object)spriteRenderer);
		meshFilter = ((Component)this).gameObject.AddComponent<MeshFilter>();
		meshRenderer = CreateBasicMeshRenderer();
		((Renderer)meshRenderer).sharedMaterial = sharedMaterial;
		((Renderer)meshRenderer).sortingLayerID = sortingLayer;
		((Renderer)meshRenderer).sortingOrder = sortingOrder;
		ForceUpdateMesh();
	}

	public void SetBatched(bool isBatched)
	{
		this.isBatched = isBatched;
		if ((Object)(object)meshRenderer != (Object)null)
		{
			((Renderer)meshRenderer).enabled = !isBatched;
		}
	}

	public void SetDirty(bool isDirty)
	{
		this.isDirty = isDirty;
		if (isDirty)
		{
			GameManager.GetSpriteRendererManager().AddDirty(this);
		}
		else
		{
			GameManager.GetSpriteRendererManager().RemoveDirty(this);
		}
	}

	public string GetAtlasName()
	{
		if (isDirty)
		{
			return GameManager.GetSpriteAtlasManager().GetAtlasNameForSprite(sprite);
		}
		return atlasName;
	}

	public bool HasSprite()
	{
		if ((Object)(object)Sprite != (Object)null)
		{
			return true;
		}
		if (sprites != null)
		{
			return sprites.Count > 0;
		}
		return false;
	}

	public void ReturnToPool()
	{
		isBatched = false;
		ObjectPool.ReturnObject((IPooledObject)this);
	}

	[SpecialName]
	GameObject IPooledObject.get_gameObject()
	{
		return ((Component)this).gameObject;
	}
}
