using System.IO;

public class EndCommandAction : ActionBase
{
	public CommandType commandType { get; protected set; }

	public EndCommandAction()
	{
	}

	public EndCommandAction(byte playerId, CommandType commandType)
		: base(playerId)
	{
		this.commandType = commandType;
	}

	public override void Execute(GameState state)
	{
	}

	public override ActionType GetActionType()
	{
		return ActionType.EndCommand;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)commandType);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		commandType = (CommandType)reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, CommandType: {commandType})";
	}
}
