public static class TileExtensions
{
	public static Tile GetInstance(this TileData tile)
	{
		if (tile == null)
		{
			return null;
		}
		return MapRenderer.Current.GetTileInstance(tile.coordinates);
	}
}
