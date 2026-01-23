using System.Collections.Generic;
using System.IO;

public class ResignAction : ActionBase
{
	public List<CommandTrigger> PendingCommandTriggers;

	public byte ResignedPlayerId;

	public byte KickerPlayerId;

	public bool WasKicked;

	public ResignAction()
	{
	}

	public ResignAction(byte playerId, byte resignedPlayerId, byte kickerPlayerId, bool wasKicked, List<CommandTrigger> pendingCommandTriggers)
		: base(playerId)
	{
		PendingCommandTriggers = pendingCommandTriggers;
		ResignedPlayerId = resignedPlayerId;
		KickerPlayerId = kickerPlayerId;
		WasKicked = wasKicked;
	}

	public override void Execute(GameState state)
	{
		ExecuteDefault(state);
	}

	public void ExecuteDefault(GameState state)
	{
		if (PendingCommandTriggers != null)
		{
			state.pendingCommandTriggers.AddRange(PendingCommandTriggers);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Resign;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)((PendingCommandTriggers != null) ? ((uint)PendingCommandTriggers.Count) : 0u));
		if (PendingCommandTriggers != null)
		{
			for (int i = 0; i < PendingCommandTriggers.Count; i++)
			{
				PendingCommandTriggers[i].Serialize(writer, version);
			}
		}
		if (version >= 93)
		{
			writer.Write(ResignedPlayerId);
			writer.Write(KickerPlayerId);
			writer.Write(WasKicked);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		int num = reader.ReadUInt16();
		if (PendingCommandTriggers == null)
		{
			PendingCommandTriggers = new List<CommandTrigger>(num);
		}
		else
		{
			PendingCommandTriggers.Clear();
		}
		for (int i = 0; i < num; i++)
		{
			CommandTrigger item = default(CommandTrigger);
			item.Deserialize(reader, version);
			PendingCommandTriggers.Add(item);
		}
		if (version >= 93)
		{
			ResignedPlayerId = reader.ReadByte();
			KickerPlayerId = reader.ReadByte();
			WasKicked = reader.ReadBoolean();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, ResignedPlayerId {ResignedPlayerId}, KickerPlayerId {KickerPlayerId}, WasKicked {WasKicked})";
	}
}
