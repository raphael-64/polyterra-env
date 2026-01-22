using System.IO;
using Polytopia.Data;

public class TaskCompletedAction : ActionBase
{
	public TaskData.Type Type { get; protected set; }

	public TaskCompletedAction()
	{
	}

	public TaskCompletedAction(byte playerId, TaskData.Type type)
		: base(playerId)
	{
		Type = type;
	}

	public override ActionType GetActionType()
	{
		return ActionType.TaskCompleted;
	}

	public override void Execute(GameState state)
	{
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		if (version <= 42)
		{
			SerializeV42(writer, version);
		}
		else
		{
			SerializeDefault(writer, version);
		}
	}

	public void SerializeDefault(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((short)Type);
	}

	public void SerializeV42(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		if (version <= 42)
		{
			DeserializeV42(reader, version);
		}
		else
		{
			DeserializeDefault(reader, version);
		}
	}

	public void DeserializeDefault(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (TaskData.Type)reader.ReadUInt16();
	}

	public void DeserializeV42(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} Type: {Type}";
	}
}
