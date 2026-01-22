namespace AirMigrations;

public class Player
{
	public int? Handicap { get; set; }

	public string Leader { get; set; }

	public string OnlineId { get; set; }

	public string Type { get; set; }

	public bool IsHuman => Type == "human";

	public bool IsAi => Type == "ai";
}
