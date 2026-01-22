using System.IO;

public class WipePlayerAction : ActionBase
{
	public byte TargetPlayerId { get; protected set; }

	public WipePlayerAction()
	{
	}

	public WipePlayerAction(byte playerId, byte targetPlayerId)
		: base(playerId)
	{
		TargetPlayerId = targetPlayerId;
	}

	public override void Execute(GameState state)
	{
		if (state.Version <= 17)
		{
			ExecuteV17(state);
		}
		else if (state.Version <= 28)
		{
			ExecuteV18(state);
		}
		else if (state.Version < 60)
		{
			ExecuteV59(state);
		}
		else if (state.Version <= 81)
		{
			ExecuteV81(state);
		}
		else
		{
			ExecuteDefault(state);
		}
	}

	private void ExecuteDefault(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		playerState.wipeOuts++;
		state.TryGetPlayer(TargetPlayerId, out var playerState2);
		playerState2.killedTurn = state.CurrentTurn;
		playerState2.killerId = base.PlayerId;
		state.ActionStack.Add(new WipePlayerEndAction(base.PlayerId, TargetPlayerId));
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == TargetPlayerId)
			{
				state.ActionStack.Add(new DisbandUnitAction(TargetPlayerId, tileData.coordinates));
			}
		}
		foreach (PlayerState playerState3 in state.PlayerStates)
		{
			if (playerState3.KnowsPlayer(TargetPlayerId))
			{
				int num = playerState3.GetAggression(TargetPlayerId, state) - 1000;
				playerState3.ModifyAggression(base.PlayerId, num * -1);
			}
			state.ActionStack.Add(new DestroyEmbassyAction(TargetPlayerId, playerState3.Id));
			state.ActionStack.Add(new DestroyEmbassyAction(playerState3.Id, TargetPlayerId));
		}
	}

	private void ExecuteV81(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		playerState.wipeOuts++;
		state.TryGetPlayer(TargetPlayerId, out var playerState2);
		playerState2.killedTurn = state.CurrentTurn;
		playerState2.killerId = base.PlayerId;
		state.ActionStack.Add(new WipePlayerEndAction(base.PlayerId, TargetPlayerId));
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == TargetPlayerId)
			{
				state.ActionStack.Add(new DisbandUnitAction(TargetPlayerId, tileData.coordinates));
			}
		}
		foreach (PlayerState playerState3 in state.PlayerStates)
		{
			if (playerState3.KnowsPlayer(TargetPlayerId))
			{
				int num = playerState3.GetAggression(TargetPlayerId, state) - 1000;
				playerState3.ModifyAggression(base.PlayerId, num * -1);
				state.ActionStack.Add(new DestroyEmbassyAction(TargetPlayerId, playerState3.Id));
				state.ActionStack.Add(new DestroyEmbassyAction(playerState3.Id, TargetPlayerId));
			}
		}
	}

	private void ExecuteV59(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		playerState.wipeOuts++;
		state.ActionStack.Add(new WipePlayerEndAction(base.PlayerId, TargetPlayerId));
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == TargetPlayerId)
			{
				state.ActionStack.Add(new DisbandUnitAction(base.PlayerId, tileData.coordinates));
			}
		}
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (playerState2.KnowsPlayer(TargetPlayerId))
			{
				int num = playerState2.GetAggression(TargetPlayerId, state) - 1000;
				playerState2.ModifyAggression(base.PlayerId, num * -1);
			}
		}
	}

	private void ExecuteV18(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		playerState.wipeOuts++;
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == TargetPlayerId)
			{
				state.ActionStack.Add(new DisbandUnitAction(base.PlayerId, tileData.coordinates));
			}
		}
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (playerState2.KnowsPlayer(TargetPlayerId))
			{
				int num = playerState2.GetAggression(TargetPlayerId, state) - 1000;
				playerState2.ModifyAggression(base.PlayerId, num * -1);
			}
		}
	}

	private void ExecuteV17(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		playerState.wipeOuts++;
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.unit != null && tileData.unit.owner == TargetPlayerId)
			{
				state.ActionStack.Add(new KillUnitAction(base.PlayerId, tileData.coordinates));
			}
		}
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			if (playerState2.KnowsPlayer(TargetPlayerId))
			{
				int num = playerState2.GetAggression(TargetPlayerId, state) - 1000;
				playerState2.ModifyAggression(base.PlayerId, num * -1);
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.WipePlayer;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(TargetPlayerId);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		TargetPlayerId = reader.ReadByte();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, TargetPlayerId: {TargetPlayerId})";
	}
}
