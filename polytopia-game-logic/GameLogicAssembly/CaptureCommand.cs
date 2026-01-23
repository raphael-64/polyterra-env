using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class CaptureCommand : CommandBase
{
	public uint UnitId { get; protected set; }

	public WorldCoordinates Coordinates { get; protected set; }

	public CaptureCommand()
	{
	}

	public CaptureCommand(byte playerId, uint unitId, WorldCoordinates coordinates)
		: base(playerId)
	{
		UnitId = unitId;
		Coordinates = coordinates;
	}

	public override bool IsValid(GameState state, out string validationError)
	{
		if (!PassesBasicValidation(state, out validationError))
		{
			return false;
		}
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile == null)
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_TILE;
			return false;
		}
		if (!tile.HasImprovement(ImprovementData.Type.City))
		{
			validationError = CommandBase.VALIDATION_ERROR_MISSING_CITY;
			return false;
		}
		if (!state.TryGetUnit(UnitId, out var unit))
		{
			validationError = CommandBase.VALIDATION_ERROR_UNIT_MISSING;
			return false;
		}
		if (!unit.CanCapture(state, tile))
		{
			validationError = CommandBase.VALIDATION_ERROR_CANT_CAPTURE;
			return false;
		}
		return true;
	}

	public override void Execute(GameState state)
	{
		base.Execute(state);
		switch (state.Version)
		{
		case 6:
		case 7:
		case 8:
		case 9:
			ExecuteV9(state);
			break;
		case 10:
		case 11:
			ExecuteV11(state);
			break;
		case 12:
			ExecuteV12(state);
			break;
		default:
			ExecuteDefault(state);
			break;
		}
	}

	private void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null || !state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		if (state.TryGetUnit(UnitId, out var unit))
		{
			unit.moved = true;
			unit.attacked = true;
			unit.RemoveEffect(UnitEffect.Boosted);
			unit.DisableFollowers(state);
		}
		byte owner = tile.owner;
		if (tile.owner != 0)
		{
			state.TryGetPlayer(tile.owner, out var playerState2);
			playerState2.RemoveNonagression(base.PlayerId);
			playerState2.ModifyAggression(base.PlayerId, (5 + tile.improvement.level) * 1000);
			foreach (PlayerState playerState3 in state.PlayerStates)
			{
				if (tile.GetExplored(playerState3.Id))
				{
					playerState3.ModifyAggression(base.PlayerId, (1000 - playerState.GetAggression(tile.owner, state)) / 2);
				}
			}
			if (playerState.GetAggression(tile.owner, state) > 1000)
			{
				playerState.ModifyAggression(tile.owner, -(5 + tile.improvement.level) * 333);
			}
		}
		state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte>(2) { base.PlayerId, tile.owner }));
		state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
		state.ActionStack.Add(new CaptureCityAction(base.PlayerId, Coordinates, owner));
		if (unit != null && unit.HasEffect(UnitEffect.Invisible))
		{
			state.ActionStack.Add(new RevealAction(base.PlayerId, Coordinates));
		}
	}

	private void ExecuteV12(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null)
		{
			return;
		}
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.GameLogicData.TryGetData(playerState.tribe, out var data);
		if (data.HasAbility(TribeAbility.Type.AlienClimate) && playerState.tribe == TribeData.Type.Polaris)
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, Coordinates, tile.improvement.borderSize, freezeUnits: false, onlyOwnedTiles: true));
		}
		if (state.TryGetPlayer(tile.owner, out var playerState2) && state.GameLogicData.TryGetData(playerState2.tribe, out var data2) && data2.HasAbility(TribeAbility.Type.AlienClimate))
		{
			TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
			if (areaSorted != null && areaSorted.Length != 0)
			{
				for (int num = areaSorted.Length - 1; num >= 0; num--)
				{
					TileData tileData = areaSorted[num];
					if (tileData != null)
					{
						state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData.coordinates, playerState.GetTribeClimate(state)));
					}
				}
			}
		}
		if (state.TryGetUnit(UnitId, out var unit))
		{
			unit.moved = true;
			unit.attacked = true;
		}
		if (tile.owner != 0)
		{
			state.TryGetPlayer(tile.owner, out var playerState3);
			playerState3.RemoveNonagression(base.PlayerId);
			playerState3.ModifyAggression(base.PlayerId, (5 + tile.improvement.level) * 1000);
			foreach (PlayerState playerState4 in state.PlayerStates)
			{
				if (tile.GetExplored(playerState4.Id))
				{
					playerState4.ModifyAggression(base.PlayerId, (1000 - playerState.GetAggression(tile.owner, state)) / 2);
				}
			}
			if (playerState.GetAggression(tile.owner, state) > 1000)
			{
				playerState.ModifyAggression(tile.owner, -(5 + tile.improvement.level) * 333);
			}
		}
		byte oldOwner = playerState2?.Id ?? 0;
		state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte>(2) { base.PlayerId, tile.owner }));
		state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
		state.ActionStack.Add(new CaptureCityAction(base.PlayerId, Coordinates, oldOwner));
	}

	private void ExecuteV11(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null)
		{
			return;
		}
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.GameLogicData.TryGetData(playerState.tribe, out var data);
		if (data.HasAbility(TribeAbility.Type.AlienClimate) && playerState.tribe == TribeData.Type.Polaris)
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, Coordinates, tile.improvement.borderSize, freezeUnits: false, onlyOwnedTiles: true));
		}
		if (state.TryGetPlayer(tile.owner, out var playerState2) && state.GameLogicData.TryGetData(playerState2.tribe, out var data2) && data2.HasAbility(TribeAbility.Type.AlienClimate))
		{
			TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
			if (areaSorted != null && areaSorted.Length != 0)
			{
				for (int num = areaSorted.Length - 1; num >= 0; num--)
				{
					TileData tileData = areaSorted[num];
					if (tileData != null)
					{
						state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData.coordinates, playerState.GetTribeClimate(state)));
					}
				}
			}
		}
		if (state.TryGetUnit(UnitId, out var unit))
		{
			unit.moved = true;
			unit.attacked = true;
		}
		if (tile.owner != 0)
		{
			state.TryGetPlayer(tile.owner, out var playerState3);
			playerState3.RemoveNonagression(base.PlayerId);
			playerState3.ModifyAggression(base.PlayerId, (5 + tile.improvement.level) * 1000);
			foreach (PlayerState playerState4 in state.PlayerStates)
			{
				if (tile.GetExplored(playerState4.Id))
				{
					playerState4.ModifyAggression(base.PlayerId, (1000 - playerState.GetAggression(tile.owner, state)) / 2);
				}
			}
			if (playerState.GetAggression(tile.owner, state) > 1000)
			{
				playerState.ModifyAggression(tile.owner, -(5 + tile.improvement.level) * 333);
			}
		}
		byte oldOwner = playerState2?.Id ?? 0;
		state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte>(2) { base.PlayerId, tile.owner }));
		state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
		state.ActionStack.Add(new CaptureCityAction(base.PlayerId, Coordinates, oldOwner));
		state.ActionStack.Add(new RuleAreaAction(base.PlayerId, Coordinates));
	}

	private void ExecuteV9(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (tile.improvement == null)
		{
			return;
		}
		state.TryGetPlayer(base.PlayerId, out var playerState);
		state.GameLogicData.TryGetData(playerState.tribe, out var data);
		if (data.HasAbility(TribeAbility.Type.AlienClimate) && playerState.tribe == TribeData.Type.Polaris)
		{
			state.ActionStack.Add(new FreezeAreaAction(base.PlayerId, Coordinates, 1));
		}
		if (state.TryGetPlayer(tile.owner, out var playerState2) && state.GameLogicData.TryGetData(playerState2.tribe, out var data2) && data2.HasAbility(TribeAbility.Type.AlienClimate))
		{
			TileData[] areaSorted = state.Map.GetAreaSorted(Coordinates, tile.improvement.borderSize, allowDiagonal: true);
			if (areaSorted != null && areaSorted.Length != 0)
			{
				for (int num = areaSorted.Length - 1; num >= 0; num--)
				{
					TileData tileData = areaSorted[num];
					if (tileData != null)
					{
						state.ActionStack.Add(new ClimateChangeAction(base.PlayerId, tileData.coordinates, playerState.GetTribeClimate(state)));
					}
				}
			}
		}
		if (state.TryGetUnit(UnitId, out var unit))
		{
			unit.moved = true;
			unit.attacked = true;
		}
		if (tile.owner != 0)
		{
			state.TryGetPlayer(tile.owner, out var playerState3);
			playerState3.RemoveNonagression(base.PlayerId);
			playerState3.ModifyAggression(base.PlayerId, (5 + tile.improvement.level) * 1000);
			foreach (PlayerState playerState4 in state.PlayerStates)
			{
				if (tile.GetExplored(playerState4.Id))
				{
					playerState4.ModifyAggression(base.PlayerId, (1000 - playerState.GetAggression(tile.owner, state)) / 2);
				}
			}
			if (playerState.GetAggression(tile.owner, state) > 1000)
			{
				playerState.ModifyAggression(tile.owner, -(5 + tile.improvement.level) * 333);
			}
		}
		byte oldOwner = playerState2?.Id ?? 0;
		state.ActionStack.Add(new UpdateTransportConnectionAction(base.PlayerId, new List<byte>(2) { base.PlayerId, tile.owner }));
		state.ActionStack.Add(new UpdateRoutesAction(base.PlayerId));
		state.ActionStack.Add(new CaptureCityAction(base.PlayerId, Coordinates, oldOwner));
		state.ActionStack.Add(new RuleAreaAction(base.PlayerId, Coordinates));
	}

	public override CommandType GetCommandType()
	{
		return CommandType.Capture;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(UnitId);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		UnitId = reader.ReadUInt32();
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, UnitId: {UnitId}, Coordinates: {Coordinates})";
	}
}
