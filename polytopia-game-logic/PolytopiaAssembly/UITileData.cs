using System;
using Polytopia.Data;
using UnityEngine;

[Serializable]
public class UITileData
{
	public Vector2Int Position;

	public TerrainData.Type terrainType;

	public ResourceData.Type resourceType;

	public UnitData.Type unitType;

	public ImprovementData.Type improvementType;
}
