using System.IO;
using System.Linq;
using PolytopiaBackendBase.Game;

public class ResignCommand : CommandBase
{
	public byte ResignedPlayerId { get; set; }

	public byte KickerPlayerId { get; set; }

	public bool WasKicked { get; set; }

	public ResignCommand()
	{
	}

	public ResignCommand(byte playerId, byte resignedPlayerId, byte kickerPlayerId, bool wasKicked)
		: base(playerId)
	{
		ResignedPlayerId = resignedPlayerId;
		KickerPlayerId = kickerPlayerId;
		WasKicked = wasKicked;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (state.CurrentPlayer != base.PlayerId)
		{
			validationError = CommandBase.VALIDATION_ERROR_PLAYER_MISMATCH;
			return false;
		}
		validationError = null;
		return true;
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.Version < 92)
		{
			ExecuteV91(gameState);
		}
		else if (gameState.Version < 93)
		{
			ExecuteV92(gameState);
		}
		else
		{
			ExecuteDefault(gameState);
		}
	}

	public void ExecuteDefault(GameState gameState)
	{
		base.Execute(gameState);
		PlayerState playerState = gameState.PlayerStates.FirstOrDefault((PlayerState player) => player.Id == ResignedPlayerId);
		if (playerState != null)
		{
			playerState.AutoPlay = true;
			playerState.resignedAtCommandIndex = gameState.CommandStack.Count - 1;
			playerState.resignedTurn = (int)gameState.CurrentTurn;
			if (gameState.Settings.GameType == GameType.Competitive || gameState.Settings.GameType == GameType.Multiplayer)
			{
				playerState.handicap = GameSettings.HandicapFromDifficulty(GameSettings.Difficulties.Normal);
			}
			gameState.ActionStack.Add(new ResignAction(base.PlayerId, ResignedPlayerId, KickerPlayerId, WasKicked, gameState.PopAllPendingCommandTriggers()));
			if (gameState.TryGetWinner(out var winner))
			{
				gameState.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
			}
		}
	}

	public void ExecuteV92(GameState gameState)
	{
		base.Execute(gameState);
		PlayerState playerState = gameState.PlayerStates.FirstOrDefault((PlayerState player) => player.Id == ResignedPlayerId);
		if (playerState != null)
		{
			playerState.AutoPlay = true;
			playerState.resignedAtCommandIndex = gameState.CommandStack.Count - 1;
			if (gameState.Settings.GameType == GameType.Competitive || gameState.Settings.GameType == GameType.Multiplayer)
			{
				playerState.handicap = GameSettings.HandicapFromDifficulty(GameSettings.Difficulties.Normal);
			}
			gameState.ActionStack.Add(new ResignAction(base.PlayerId, ResignedPlayerId, KickerPlayerId, WasKicked, gameState.PopAllPendingCommandTriggers()));
			if (gameState.TryGetWinner(out var winner))
			{
				gameState.ActionStack.Add(new GameOverAction(base.PlayerId, winner.Id));
			}
		}
	}

	public void ExecuteV91(GameState gameState)
	{
		base.Execute(gameState);
		PlayerState playerState = gameState.PlayerStates.FirstOrDefault((PlayerState player) => player.Id == ResignedPlayerId);
		if (playerState != null)
		{
			playerState.AutoPlay = true;
			playerState.resignedAtCommandIndex = gameState.CommandStack.Count - 1;
			if (gameState.Settings.GameType == GameType.Competitive || gameState.Settings.GameType == GameType.Multiplayer)
			{
				playerState.handicap = GameSettings.HandicapFromDifficulty(GameSettings.Difficulties.Normal);
			}
			gameState.ActionStack.Add(new ResignAction(base.PlayerId, ResignedPlayerId, KickerPlayerId, WasKicked, gameState.PopAllPendingCommandTriggers()));
		}
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Resign;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(ResignedPlayerId);
		if (version >= 93)
		{
			writer.Write(KickerPlayerId);
			writer.Write(WasKicked);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		ResignedPlayerId = reader.ReadByte();
		if (version >= 93)
		{
			KickerPlayerId = reader.ReadByte();
			WasKicked = reader.ReadBoolean();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, ResignedPlayerId {ResignedPlayerId}, KickerPlayerId {KickerPlayerId}, WasKicked {WasKicked})";
	}
}
