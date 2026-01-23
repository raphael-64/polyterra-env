using System.IO;
using Polytopia.Data;

public class ResearchAction : ActionBase
{
	public TechData.Type Type { get; private set; }

	public int Cost { get; protected set; }

	public ResearchAction()
	{
	}

	public ResearchAction(byte playerId, TechData.Type type, int cost)
		: base(playerId)
	{
		Type = type;
		Cost = cost;
	}

	public override void Execute(GameState state)
	{
		if (state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			ActionUtils.LearnTech(state, playerState, Type, Cost, shouldUseActions: true);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Research;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write((ushort)Type);
		writer.Write(Cost);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Type = (TechData.Type)reader.ReadUInt16();
		Cost = reader.ReadInt32();
	}

	public override string ToString()
	{
		return $"ResearchAction(PlayerId: {base.PlayerId}, Type: {Type})";
	}
}
