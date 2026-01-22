using System.IO;

public class MapData
{
	private ushort width;

	private ushort height;

	private TileData[] tiles;

	private WorldContinent[] continents;

	public ushort Width => width;

	public ushort Height => height;

	public TileData[] Tiles => tiles;

	public WorldContinent[] Continents
	{
		get
		{
			return continents;
		}
		set
		{
			continents = value;
		}
	}

	public MapData()
	{
	}

	public MapData(ushort width, ushort height)
	{
		this.width = width;
		this.height = height;
		tiles = new TileData[width * height];
	}

	public MapData(MapData map)
	{
		width = map.width;
		height = map.height;
		tiles = map.tiles;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(width);
		writer.Write(height);
		for (int i = 0; i < tiles.Length; i++)
		{
			tiles[i].Serialize(writer, version);
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		width = reader.ReadUInt16();
		height = reader.ReadUInt16();
		if (tiles == null || tiles.Length < width * height)
		{
			tiles = new TileData[width * height];
		}
		for (int i = 0; i < tiles.Length; i++)
		{
			if (tiles[i] != null)
			{
				tiles[i].Deserialize(reader, version);
				continue;
			}
			tiles[i] = new TileData();
			tiles[i].Deserialize(reader, version);
		}
	}
}
