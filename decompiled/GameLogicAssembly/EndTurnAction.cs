using System.IO;
using Polytopia.Data;
using PolytopiaBackendBase.Game;

public class EndTurnAction : ActionBase
{
	public EndTurnAction()
	{
	}

	public EndTurnAction(byte playerId)
		: base(playerId)
	{
	}

	public override void Execute(GameState state)
	{
		state.TryGetPlayer(base.PlayerId, out var playerState);
		if (state.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Bump(state);
		}
		if (base.PlayerId == state.CurrentPlayer)
		{
			state.EndPlayerTurn();
		}
		if (state.TryGetWinner(out var winner))
		{
			state.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
		}
		if (state.Settings.GameType == GameType.PassAndPlay)
		{
			state.ActionStack.Add(new PassPlayerAction(state.CurrentPlayer));
		}
		else
		{
			state.ActionStack.Add(new StartTurnAction(state.CurrentPlayer));
		}
		for (int i = 0; i < state.Map.Tiles.Length; i++)
		{
			TileData tileData = state.Map.Tiles[i];
			if (tileData.owner == base.PlayerId && tileData.improvement != null && tileData.improvement.HasEffect(ImprovementEffect.robbed))
			{
				tileData.improvement.RemoveEffect(ImprovementEffect.robbed);
			}
		}
		foreach (PlayerState playerState2 in state.PlayerStates)
		{
			DiplomacyRelation relation = playerState.GetRelation(playerState2.Id);
			if (relation.State == DiplomacyRelationState.BrokenPeace)
			{
				Log.Verbose("Remove cooldown for {0} against {1}", new object[2] { playerState, playerState2 });
				relation.State = DiplomacyRelationState.Neutral;
			}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.EndTurn;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId})";
	}
}
