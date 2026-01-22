using System;
using System.Collections.Generic;
using Polytopia.Data;

[Serializable]
public class PreviewData
{
	public TribeData.Type tribeType;

	public List<UITileData> TileDataList = new List<UITileData>();
}
