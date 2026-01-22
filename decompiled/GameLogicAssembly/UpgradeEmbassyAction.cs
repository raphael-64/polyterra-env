using System.Collections.Generic;
using System.IO;

public class UpgradeEmbassyAction : ActionBase
{
	public byte OpponentId { get; protected set; }

	public UpgradeEmbassyAction()
	{
	}

	public UpgradeEmbassyAction(byte playerId, byte opponentId)
		: base(playerId)
	{
		OpponentId = opponentId;
	}

	public override void Execute(GameState gameState)
	{
		gameState.TryGetPlayer(OpponentId, out var playerState);
		gameState.TryGetPlayer(base.PlayerId, out var playerState2);
		playerState2.Currency -= gameState.GameLogicData.DiplomacyData.embassyUpgradeCost;
		DiplomacyRelation relation = playerState.GetRelation(base.PlayerId);
		relation.EmbassyLevel++;
		if (relation.EmbassyLevel != 2)
		{
			return;
		}
		List<TileData> list = new List<TileData>(4);
		gameState.Map.GetPlayerCityTiles(OpponentId, list);
		foreach (TileData item in list)
		{
			if (!item.GetExplored(base.PlayerId))
			{
				gameState.ActionStack.Add(new ExploreAction(base.PlayerId, item.coordinates));
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.UpgradeEmbassy;
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
