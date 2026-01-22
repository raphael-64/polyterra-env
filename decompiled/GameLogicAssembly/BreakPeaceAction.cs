using System.Collections.Generic;
using System.IO;

public class BreakPeaceAction : ActionBase
{
	public byte OpponentId { get; protected set; }

	public BreakPeaceAction()
	{
	}

	public BreakPeaceAction(byte playerId, byte opponentId)
		: base(playerId)
	{
		OpponentId = opponentId;
	}

	public override void Execute(GameState gameState)
	{
		gameState.TryGetPlayer(OpponentId, out var playerState);
		gameState.TryGetPlayer(base.PlayerId, out var playerState2);
		DiplomacyRelation relation = playerState2.GetRelation(OpponentId);
		DiplomacyRelation relation2 = playerState.GetRelation(base.PlayerId);
		if (gameState.Version < 80)
		{
			relation.State = DiplomacyRelationState.War;
			playerState2.SetLastAttack(playerState.Id, (int)gameState.CurrentTurn, gameState);
			relation2.State = DiplomacyRelationState.Neutral;
		}
		else
		{
			relation.State = DiplomacyRelationState.BrokenPeace;
			relation.LastPeaceBrokenTurn = (int)gameState.CurrentTurn;
			relation2.LastPeaceBrokenTurn = (int)gameState.CurrentTurn;
			relation2.State = DiplomacyRelationState.BrokenPeace;
		}
		List<UnitState> list = new List<UnitState>(10);
		gameState.Map.GetPlayerUnits(playerState2.Id, list);
		for (int i = 0; i < list.Count; i++)
		{
			UnitState unitState = list[i];
			TileData tile = gameState.Map.GetTile(unitState.coordinates);
			if (tile.owner == OpponentId)
			{
				gameState.ActionStack.Add(new DisbandUnitAction(base.PlayerId, tile.coordinates));
				continue;
			}
			unitState.attacked = true;
			unitState.moved = true;
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.BreakPeace;
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
