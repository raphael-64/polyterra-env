using System.Collections.Generic;
using System.IO;

public class EstablishEmbassyAction : ActionBase
{
	public byte OpponentId { get; protected set; }

	public EstablishEmbassyAction()
	{
	}

	public EstablishEmbassyAction(byte playerId, byte opponentId)
		: base(playerId)
	{
		OpponentId = opponentId;
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(OpponentId, out var playerState);
		state.TryGetPlayer(base.PlayerId, out var playerState2);
		playerState2.Currency -= state.GameLogicData.DiplomacyData.embassyCost;
		DiplomacyRelation relation = playerState.GetRelation(base.PlayerId);
		relation.EmbassyLevel = 1;
		relation.EmbassyBuildTurn = (int)state.CurrentTurn;
		playerState.SendMessage(DiplomacyMessageType.EstablishEmbassy, base.PlayerId);
		List<TileData> area = state.Map.GetArea(playerState.startTile, 1, allowDiagonal: true, includeCenter: false);
		for (int i = 0; i < area.Count; i++)
		{
			TileData tileData = area[i];
			if (!tileData.GetExplored(base.PlayerId))
			{
				state.ActionStack.Add(new ExploreAction(base.PlayerId, tileData.coordinates));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.EstablishEmbassy;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(OpponentId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		OpponentId = reader.ReadByte();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, OpponentId: {OpponentId})";
	}
}
