using System.Collections.Generic;
using UnityEngine;

public class PolytopiaSpriteRendererManager
{
	private HashSet<PolytopiaSpriteRenderer> dirtySpriteRendererers = new HashSet<PolytopiaSpriteRenderer>();

	public void AddDirty(PolytopiaSpriteRenderer renderer)
	{
		dirtySpriteRendererers.Add(renderer);
	}

	public void RemoveDirty(PolytopiaSpriteRenderer renderer)
	{
		dirtySpriteRendererers.Remove(renderer);
	}

	public void Update()
	{
		int count = dirtySpriteRendererers.Count;
		if (count <= 0)
		{
			return;
		}
		PolytopiaSpriteRenderer[] array = new PolytopiaSpriteRenderer[dirtySpriteRendererers.Count];
		dirtySpriteRendererers.CopyTo(array);
		for (int i = 0; i < count; i++)
		{
			PolytopiaSpriteRenderer polytopiaSpriteRenderer = array[i];
			if ((Object)(object)polytopiaSpriteRenderer == (Object)null)
			{
				RemoveDirty(polytopiaSpriteRenderer);
			}
			else
			{
				polytopiaSpriteRenderer.ForceUpdateMesh();
			}
		}
	}
}
