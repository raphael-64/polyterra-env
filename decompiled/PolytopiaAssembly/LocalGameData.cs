using System.IO;

public class LocalGameData : IBinarySerializable
{
	public GameState initialGameState;

	public GameState currentGameState;

	public ushort lastSeenCommand;

	public LocalGameData()
	{
		initialGameState = new GameState();
		currentGameState = new GameState();
		lastSeenCommand = 0;
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		initialGameState.Serialize(writer, version);
		currentGameState.Serialize(writer, version);
		writer.Write(lastSeenCommand);
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
		lastSeenCommand = reader.ReadUInt16();
	}
}
