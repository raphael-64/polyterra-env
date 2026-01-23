using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class MeetAction : ActionBase
{
	public int RewardMultiplier = 3;

	public byte OtherPlayerId { get; protected set; }

	public WorldCoordinates Coordinates { get; protected set; }

	public TechData.Type Tech { get; protected set; }

	public MeetAction()
	{
	}

	public MeetAction(byte playerId, byte otherPlayerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		OtherPlayerId = otherPlayerId;
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state)
	{
		if (!state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return false;
		}
		if (playerState.KnowsPlayer(OtherPlayerId))
		{
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		if (state.Version < 50)
		{
			ExecuteV50(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	public void ExecuteDefault(GameState gameState)
	{
		if (gameState.TryGetPlayer(base.PlayerId, out var playerState) && gameState.TryGetPlayer(OtherPlayerId, out var playerState2))
		{
			ActionUtils.TryRevealCapital(gameState, playerState, playerState2);
			playerState.knownPlayers.Add(OtherPlayerId);
			playerState.GetRelation(OtherPlayerId).FirstMeet = (int)gameState.CurrentTurn;
			playerState.opinions.UpdateOpinions(gameState, playerState);
			if (gameState.Settings.RulesGameMode != GameMode.Domination)
			{
				playerState.ModifyAggression(OtherPlayerId, -1000);
				playerState2.ModifyAggression(base.PlayerId, -1000);
			}
			gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, GetReward(gameState), 40));
		}
	}

	public int GetReward(GameState gameState)
	{
		if (gameState.Version < 83)
		{
			return GetRewardV82(gameState);
		}
		if (gameState.TryGetPlayer(base.PlayerId, out var _) && gameState.TryGetPlayer(OtherPlayerId, out var playerState2))
		{
			return 1 + Math.Min((int)Math.Ceiling((float)playerState2.score / 1000f), 5) * 2;
		}
		return 2;
	}

	public int GetRewardV82(GameState gameState)
	{
		if (gameState.TryGetPlayer(base.PlayerId, out var _) && gameState.TryGetPlayer(OtherPlayerId, out var playerState2))
		{
			return Math.Min((int)Math.Ceiling((float)playerState2.score / 1000f), 4) * RewardMultiplier;
		}
		return 2;
	}

	public void ExecuteV50(GameState gameState)
	{
		if (!gameState.TryGetPlayer(base.PlayerId, out var playerState) || !gameState.TryGetPlayer(OtherPlayerId, out var playerState2))
		{
			return;
		}
		ActionUtils.TryRevealCapital(gameState, playerState, playerState2);
		playerState.knownPlayers.Add(OtherPlayerId);
		playerState.opinions.UpdateOpinions(gameState, playerState);
		if (gameState.Settings.RulesGameMode != GameMode.Domination)
		{
			playerState.ModifyAggression(OtherPlayerId, -1000);
			playerState2.ModifyAggression(base.PlayerId, -1000);
		}
		if (!gameState.Settings.rules.AllowTechSharing)
		{
			return;
		}
		List<TechData> unlockableTech = gameState.GameLogicData.GetUnlockableTech(playerState);
		List<TechData.Type> list = new List<TechData.Type>();
		foreach (TechData item in unlockableTech)
		{
			if (playerState2.HasTech(item.type))
			{
				list.Add(item.type);
			}
		}
		if (list.Count > 0)
		{
			Tech = list[gameState.RandomHash.Range(0, list.Count, Coordinates.X, Coordinates.Y)];
			gameState.ActionStack.Add(new ResearchAction(base.PlayerId, Tech, 0));
			return;
		}
		Tech = TechData.Type.Basic;
		for (int i = 0; i < 5; i++)
		{
			gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, 40));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.Meet;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(OtherPlayerId);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		OtherPlayerId = reader.ReadByte();
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, OtherPlayerId: {OtherPlayerId}, Coordinates: {Coordinates})";
	}
}
