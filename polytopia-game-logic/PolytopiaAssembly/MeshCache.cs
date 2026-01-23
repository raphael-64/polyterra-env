using System.Collections.Generic;
using UnityEngine;

public class MeshCache
{
	public static int TERRAIN_LAYER_ID;

	public static int GFX_LAYER_ID;

	public static int BUILDING_LAYER_ID;

	public static int UNITS_LAYER_ID;

	public static int STATUS_DISPLAYS_LAYER_ID;

	public static int CITY_STATUS_DISPLAY_ID;

	public static int BG_LAYER_ID;

	public Dictionary<MeshDescription, Mesh> cachedSpriteMeshes = new Dictionary<MeshDescription, Mesh>();

	public Dictionary<string, MaterialPropertyBlock> cachedSpriteMaterialPropertyBlocks = new Dictionary<string, MaterialPropertyBlock>();

	public void CacheLayerIDs()
	{
		TERRAIN_LAYER_ID = SortingLayer.NameToID("Terrain");
		GFX_LAYER_ID = SortingLayer.NameToID("GFX");
		BUILDING_LAYER_ID = SortingLayer.NameToID("Building");
		UNITS_LAYER_ID = SortingLayer.NameToID("Units");
		STATUS_DISPLAYS_LAYER_ID = SortingLayer.NameToID("UnitStatusDisplays");
		CITY_STATUS_DISPLAY_ID = SortingLayer.NameToID("CityStatusDisplays");
		BG_LAYER_ID = SortingLayer.NameToID("BG");
	}

	public Mesh GetOrCreateSpriteMesh(SpriteRenderer spriteRenderer)
	{
		MeshDescription description = new MeshDescription(spriteRenderer);
		return GetOrCreateSpriteMesh(description);
	}

	public Mesh GetOrCreateSpriteMesh(PolytopiaSpriteRenderer spriteRenderer)
	{
		MeshDescription description = new MeshDescription(spriteRenderer);
		return GetOrCreateSpriteMesh(description);
	}

	public Mesh GetOrCreateSpriteMesh(MeshDescription description)
	{
		if (!cachedSpriteMeshes.TryGetValue(description, out var value))
		{
			value = SpriteToMesh(description);
			cachedSpriteMeshes[description] = value;
		}
		return value;
	}

	public MaterialPropertyBlock GetOrCreateMaterialPropertyBlock(string atlasName)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(atlasName))
		{
			return null;
		}
		if (!cachedSpriteMaterialPropertyBlocks.TryGetValue(atlasName, out var propertyBlock))
		{
			propertyBlock = new MaterialPropertyBlock();
			propertyBlock.SetVector("_Flip", new Vector4(1f, 1f, 0f, 0f));
			GameManager.GetSpriteAtlasManager().LoadSpriteAtlasTexture(atlasName, delegate(Texture2D texture)
			{
				if ((Object)(object)texture != (Object)null)
				{
					propertyBlock.SetTexture("_MainTex", (Texture)(object)texture);
				}
			});
			cachedSpriteMaterialPropertyBlocks[atlasName] = propertyBlock;
		}
		return propertyBlock;
	}

	public Mesh SpriteToMesh(MeshDescription description)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Sprite sprite = description.sprite;
		if ((Object)(object)sprite == (Object)null)
		{
			return null;
		}
		Mesh val = new Mesh();
		((Object)val).hideFlags = (HideFlags)61;
		val.Clear();
		Vector2[] vertices = sprite.vertices;
		Vector3[] array = (Vector3[])(object)new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i] = Vector2.op_Implicit(vertices[i]);
		}
		val.vertices = array;
		val.uv = sprite.uv;
		Color[] array2 = (Color[])(object)new Color[val.vertices.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = description.color;
		}
		val.colors = array2;
		ushort[] triangles = sprite.triangles;
		int[] array3 = new int[triangles.Length];
		for (int k = 0; k < array3.Length; k++)
		{
			array3[k] = triangles[k];
		}
		val.SetIndices(array3, (MeshTopology)0, 0);
		return val;
	}
}
