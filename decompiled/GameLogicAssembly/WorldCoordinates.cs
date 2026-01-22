using System;
using System.Collections.Generic;
using System.IO;

public struct WorldCoordinates
{
	public static WorldCoordinates NULL_COORDINATES = new WorldCoordinates(-1, -1);

	private int x;

	private int y;

	public int X
	{
		get
		{
			return x;
		}
		set
		{
			x = value;
		}
	}

	public int Y
	{
		get
		{
			return y;
		}
		set
		{
			y = value;
		}
	}

	public int SqrMagnitude => x * x + y * y;

	public float Magnitude => (float)Math.Sqrt(x * x + y * y);

	public WorldCoordinates(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public WorldCoordinates(BinaryReader reader, int version)
	{
		x = reader.ReadInt32();
		y = reader.ReadInt32();
	}

	public static bool IsAdjacent(WorldCoordinates a, WorldCoordinates b)
	{
		return (b - a).SqrMagnitude <= 2;
	}

	public static float Distance(WorldCoordinates a, WorldCoordinates b)
	{
		return (b - a).Magnitude;
	}

	public static WorldCoordinates operator +(WorldCoordinates a, WorldCoordinates b)
	{
		return new WorldCoordinates(a.x + b.x, a.y + b.y);
	}

	public static WorldCoordinates operator -(WorldCoordinates a, WorldCoordinates b)
	{
		return new WorldCoordinates(a.x - b.x, a.y - b.y);
	}

	public static WorldCoordinates operator /(WorldCoordinates a, int divisor)
	{
		return new WorldCoordinates(a.x / divisor, a.y / divisor);
	}

	public static bool operator ==(WorldCoordinates a, WorldCoordinates b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(WorldCoordinates a, WorldCoordinates b)
	{
		if (a.x == b.x)
		{
			return a.y != b.y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is WorldCoordinates worldCoordinates)
		{
			if (worldCoordinates.x == x)
			{
				return worldCoordinates.y == y;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public bool IsValid(int width, int height)
	{
		if (x >= 0 && x < width && y >= 0)
		{
			return y < height;
		}
		return false;
	}

	public int ToIndex(int width)
	{
		return ToIndex(x, y, width);
	}

	public static int ToIndex(int x, int y, int width)
	{
		return y * width + x;
	}

	public static WorldCoordinates FromIndex(int index, int width)
	{
		return new WorldCoordinates(index % width, index / width);
	}

	public static GridDirection GetDirectionFromDistanceVector(WorldCoordinates distanceVector)
	{
		GridDirection result = GridDirection.SW;
		int num = 0;
		for (int i = 0; i < GridDirections.COUNT; i++)
		{
			GridDirection gridDirection = (GridDirection)i;
			WorldCoordinates worldCoordinates = gridDirection.ToCoordinates();
			int num2 = ((worldCoordinates.x * worldCoordinates.x + worldCoordinates.y * worldCoordinates.y == 1) ? 1393 : 985);
			int num3 = worldCoordinates.x * distanceVector.x * num2 + worldCoordinates.y * distanceVector.y * num2;
			if (num3 > num)
			{
				result = gridDirection;
				num = num3;
			}
		}
		return result;
	}

	public static GridDirection? GetDirection(WorldCoordinates from, WorldCoordinates to)
	{
		WorldCoordinates worldCoordinates = to - from;
		if (MapDataExtensions.ChebyshevDistance(to, from) > 1)
		{
			return GetDirectionFromDistanceVector(worldCoordinates);
		}
		return ToDirection(worldCoordinates);
	}

	public static GridDirection? ToDirection(WorldCoordinates coordinates)
	{
		if (coordinates.X == -1)
		{
			if (coordinates.Y == -1)
			{
				return GridDirection.SW;
			}
			if (coordinates.Y == 0)
			{
				return GridDirection.W;
			}
			if (coordinates.Y == 1)
			{
				return GridDirection.NW;
			}
		}
		else if (coordinates.X == 0)
		{
			if (coordinates.Y == -1)
			{
				return GridDirection.S;
			}
			if (coordinates.Y == 1)
			{
				return GridDirection.N;
			}
		}
		else if (coordinates.X == 1)
		{
			if (coordinates.Y == -1)
			{
				return GridDirection.SE;
			}
			if (coordinates.Y == 0)
			{
				return GridDirection.E;
			}
			if (coordinates.Y == 1)
			{
				return GridDirection.NE;
			}
		}
		return null;
	}

	public override string ToString()
	{
		return $"({x},{y})";
	}

	public static List<WorldCoordinates> GetArea(WorldCoordinates center, int radius, bool allowDiagonal = true, bool includeCenter = true)
	{
		List<WorldCoordinates> list = new List<WorldCoordinates>();
		GetAreaPreallocated(list, center, radius, allowDiagonal, includeCenter);
		return list;
	}

	public static void GetAreaPreallocated(List<WorldCoordinates> area, WorldCoordinates center, int radius, bool allowDiagonal = true, bool includeCenter = true)
	{
		area.Clear();
		for (int i = center.Y - radius; i <= center.Y + radius; i++)
		{
			for (int j = center.X - radius; j <= center.X + radius; j++)
			{
				if ((includeCenter || j != center.X || i != center.Y) && (allowDiagonal || j == center.X || i == center.Y))
				{
					area.Add(new WorldCoordinates(j, i));
				}
			}
		}
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(x);
		writer.Write(y);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		x = reader.ReadInt32();
		y = reader.ReadInt32();
	}
}
