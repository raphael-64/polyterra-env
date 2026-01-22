using System.Collections.Generic;

namespace AirMigrations;

public class GameMetadata
{
	public string AppVersion { get; set; }

	public string CurrentPlayer { get; set; }

	public long? Date { get; set; }

	public int? GameState { get; set; }

	public string Id { get; set; }

	public int? MapSize { get; set; }

	public int? Turn { get; set; }

	public Dictionary<string, Player> Players { get; set; }
}
