using System;

public static class GridDirections
{
	public static int COUNT = 8;

	public static GridDirection Opposite(this GridDirection direction)
	{
		if ((int)direction >= COUNT / 2)
		{
			return direction - COUNT / 2;
		}
		return direction + COUNT / 2;
	}

	public static GridDirection Next(this GridDirection direction)
	{
		if (direction != GridDirection.S)
		{
			return direction + 1;
		}
		return GridDirection.SW;
	}

	public static GridDirection Previous(this GridDirection direction)
	{
		if (direction != GridDirection.SW)
		{
			return direction - 1;
		}
		return GridDirection.S;
	}

	public static GridDirection Average(GridDirection firstDirection, GridDirection secondDirection)
	{
		WorldCoordinates distanceVector = firstDirection.ToCoordinates() + secondDirection.ToCoordinates();
		if (distanceVector.X == 0 && distanceVector.Y == 0)
		{
			return (GridDirection)((int)(firstDirection + COUNT / 4) % COUNT);
		}
		return WorldCoordinates.GetDirectionFromDistanceVector(distanceVector);
	}

	public static GridDirectionFlag ToFlag(this GridDirection direction)
	{
		return (GridDirectionFlag)(1 << (int)direction);
	}

	public static bool ContainsDirection(this GridDirectionFlag flag, GridDirectionFlag otherFlag)
	{
		return (flag & otherFlag) == otherFlag;
	}

	public static GridDirection Random()
	{
		return (GridDirection)new Random().Next(0, Enum.GetValues(typeof(GridDirection)).Length);
	}

	public static WorldCoordinates ToCoordinates(this GridDirection direction)
	{
		return direction switch
		{
			GridDirection.SW => new WorldCoordinates(-1, -1), 
			GridDirection.W => new WorldCoordinates(-1, 0), 
			GridDirection.NW => new WorldCoordinates(-1, 1), 
			GridDirection.N => new WorldCoordinates(0, 1), 
			GridDirection.NE => new WorldCoordinates(1, 1), 
			GridDirection.E => new WorldCoordinates(1, 0), 
			GridDirection.SE => new WorldCoordinates(1, -1), 
			GridDirection.S => new WorldCoordinates(0, -1), 
			_ => new WorldCoordinates(0, 0), 
		};
	}
}
