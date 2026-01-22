using System.IO;

public class HotseatGameData : IBinarySerializable
{
	public GameState initialGameState;

	public GameState currentGameState;

	public GameState lastTurnGameState;

	public ushort[] lastSeenCommands;

	public HotseatGameData()
	{
		initialGameState = new GameState();
		currentGameState = new GameState();
		lastTurnGameState = new GameState();
		lastSeenCommands = null;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		initialGameState.Serialize(writer, version);
		currentGameState.Serialize(writer, version);
		lastTurnGameState.Serialize(writer, version);
		writer.Write((ushort)((lastSeenCommands != null) ? ((uint)lastSeenCommands.Length) : 0u));
		if (lastSeenCommands != null)
		{
			for (int i = 0; i < lastSeenCommands.Length; i++)
			{
				writer.Write(lastSeenCommands[i]);
			}
		}
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (initialGameState == null)
		{
			initialGameState = new GameState();
		}
		initialGameState.Deserialize(reader, version);
		if (currentGameState == null)
		{
			currentGameState = new GameState();
		}
		currentGameState.Deserialize(reader, version);
		if (lastTurnGameState == null)
		{
			lastTurnGameState = new GameState();
		}
		lastTurnGameState.Deserialize(reader, version);
		ushort num = reader.ReadUInt16();
		if (lastSeenCommands == null || lastSeenCommands.Length < num)
		{
			lastSeenCommands = new ushort[num];
		}
		for (int i = 0; i < num; i++)
		{
			lastSeenCommands[i] = reader.ReadUInt16();
		}
	}
}
