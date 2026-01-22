using NaughtyAttributes;
using UnityEngine;

public class TileTester : MonoBehaviour
{
	public Tile tile;

	[Button(null)]
	private void SpawnPuff()
	{
		tile.SpawnPuff();
	}

	[Button(null)]
	private void SpawnDarkPuff()
	{
		tile.SpawnDarkPuff();
	}

	[Button(null)]
	private void SpawnExplosion()
	{
		tile.SpawnExplosion();
	}

	[Button(null)]
	private void SpawnShine()
	{
		tile.SpawnShine();
	}

	[Button(null)]
	private void SpawnSparkles()
	{
		tile.SpawnSparkles();
	}

	[Button(null)]
	private void SpawnEmbers()
	{
		tile.SpawnEmbers();
	}

	[Button(null)]
	private void SpawnFire()
	{
		tile.SpawnFire();
	}

	[Button(null)]
	private void SpawnRainbowFire()
	{
		tile.SpawnRainbowFire();
	}

	[Button(null)]
	private void RemoveOwner()
	{
		tile.Owner = null;
	}

	[Button(null)]
	private void RemoveRulingCity()
	{
		tile.Data.rulingCityCoordinates = WorldCoordinates.NULL_COORDINATES;
	}

	[Button(null)]
	private void SpawnWorldDamage()
	{
		WorldIconContainer.SpawnWorldEdgeDamage(tile.Coordinates);
	}
}
