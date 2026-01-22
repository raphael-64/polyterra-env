using System.Collections.Generic;
using System.IO;
using Polytopia.Data;

public class ExamineRuinsAction : ActionBase
{
	public RuinsReward Reward { get; private set; }

	public WorldCoordinates Coordinates { get; private set; }

	public TechData Tech { get; private set; }

	public ExamineRuinsAction()
	{
	}

	public ExamineRuinsAction(byte playerId, RuinsReward reward, WorldCoordinates coordinates)
		: base(playerId)
	{
		Reward = reward;
		Coordinates = coordinates;
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.Version < 50)
		{
			ExecuteV50(gameState);
		}
		else if (gameState.Version < 85)
		{
			ExecuteV84(gameState);
		}
		else
		{
			ExecuteDefault(gameState);
		}
	}

	public void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		if (!gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		tile.improvement = null;
		if (tile.unit != null)
		{
			tile.unit.MakeExhauseted(gameState);
		}
		switch (Reward)
		{
		case RuinsReward.FreeTech:
		{
			List<TechData> unlockableTech = gameState.GameLogicData.GetUnlockableTech(playerState);
			Tech = unlockableTech[gameState.RandomHash.Range(0, unlockableTech.Count, tile.coordinates.X, tile.coordinates.Y)];
			TechData.Type type = Tech.type;
			gameState.ActionStack.Add(new ResearchAction(base.PlayerId, type, 0));
			break;
		}
		case RuinsReward.Explorer:
			gameState.ActionStack.Add(new ScoutMoveAction(base.PlayerId, gameState.GetNextUnitId(), 15u, gameState.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y), Coordinates, new List<WorldCoordinates>()));
			break;
		case RuinsReward.Seamonster:
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Navalon, tile);
			break;
		case RuinsReward.Battleship:
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Battleship, tile);
			break;
		case RuinsReward.SuperUnit:
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Giant, tile);
			break;
		case RuinsReward.Swordsman:
			gameState.ActionStack.Add(new PromoteAction(base.PlayerId, tile.coordinates));
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Swordsman, tile);
			if (tile.unit != null)
			{
				tile.unit.home = WorldCoordinates.NULL_COORDINATES;
			}
			break;
		case RuinsReward.Resources:
		{
			for (uint num = 0u; num < 10; num++)
			{
				int num2 = 40;
				if (num == 9)
				{
					num2 += 150;
				}
				gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, num2));
			}
			break;
		}
		case RuinsReward.PopulationGrowth:
		{
			WorldCoordinates currentCapitalCoordinates = playerState.GetCurrentCapitalCoordinates(gameState);
			if (!(currentCapitalCoordinates == WorldCoordinates.NULL_COORDINATES))
			{
				for (int i = 0; i < 3; i++)
				{
					gameState.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, Coordinates, currentCapitalCoordinates, i * 60));
				}
			}
			break;
		}
		}
	}

	private void ExecuteV84(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Coordinates);
		if (!gameState.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		tile.improvement = null;
		if (tile.unit != null)
		{
			tile.unit.MakeExhauseted(gameState);
		}
		switch (Reward)
		{
		case RuinsReward.FreeTech:
		{
			List<TechData> unlockableTech = gameState.GameLogicData.GetUnlockableTech(playerState);
			Tech = unlockableTech[gameState.RandomHash.Range(0, unlockableTech.Count, tile.coordinates.X, tile.coordinates.Y)];
			TechData.Type type = Tech.type;
			gameState.ActionStack.Add(new ResearchAction(base.PlayerId, type, 0));
			break;
		}
		case RuinsReward.Explorer:
			gameState.ActionStack.Add(new ScoutMoveAction(base.PlayerId, gameState.GetNextUnitId(), 15u, gameState.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y), Coordinates, new List<WorldCoordinates>()));
			break;
		case RuinsReward.Seamonster:
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Navalon, tile);
			break;
		case RuinsReward.Battleship:
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Battleship, tile);
			break;
		case RuinsReward.SuperUnit:
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Giant, tile);
			break;
		case RuinsReward.Swordsman:
			gameState.ActionStack.Add(new PromoteAction(base.PlayerId, tile.coordinates));
			ActionUtils.TrainUnitOnOccupiedSpace(gameState, base.PlayerId, UnitData.Type.Swordsman, tile);
			break;
		case RuinsReward.Resources:
		{
			for (uint num = 0u; num < 10; num++)
			{
				int num2 = 40;
				if (num == 9)
				{
					num2 += 150;
				}
				gameState.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, num2));
			}
			break;
		}
		case RuinsReward.PopulationGrowth:
		{
			WorldCoordinates currentCapitalCoordinates = playerState.GetCurrentCapitalCoordinates(gameState);
			if (!(currentCapitalCoordinates == WorldCoordinates.NULL_COORDINATES))
			{
				for (int i = 0; i < 3; i++)
				{
					gameState.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, Coordinates, currentCapitalCoordinates, i * 60));
				}
			}
			break;
		}
		}
	}

	public void ExecuteV50(GameState state)
	{
		TileData tile = state.Map.GetTile(Coordinates);
		if (!state.TryGetPlayer(base.PlayerId, out var playerState))
		{
			return;
		}
		tile.improvement = null;
		if (tile.unit != null)
		{
			tile.unit.moved = true;
			tile.unit.attacked = true;
		}
		switch (Reward)
		{
		case RuinsReward.FreeTech:
		{
			List<TechData> unlockableTech = state.GameLogicData.GetUnlockableTech(playerState);
			Tech = unlockableTech[state.RandomHash.Range(0, unlockableTech.Count, tile.coordinates.X, tile.coordinates.Y)];
			TechData.Type type = Tech.type;
			state.ActionStack.Add(new ResearchAction(base.PlayerId, type, 0));
			break;
		}
		case RuinsReward.Explorer:
			state.ActionStack.Add(new ScoutMoveAction(base.PlayerId, state.GetNextUnitId(), 15u, state.RandomHash.GetHash(tile.coordinates.X, tile.coordinates.Y), Coordinates, new List<WorldCoordinates>()));
			break;
		case RuinsReward.Seamonster:
			ActionUtils.TrainUnitOnOccupiedSpace(state, base.PlayerId, UnitData.Type.Navalon, tile);
			break;
		case RuinsReward.Battleship:
			ActionUtils.TrainUnitOnOccupiedSpace(state, base.PlayerId, UnitData.Type.Battleship, tile);
			break;
		case RuinsReward.SuperUnit:
			ActionUtils.TrainUnitOnOccupiedSpace(state, base.PlayerId, UnitData.Type.Giant, tile);
			break;
		case RuinsReward.Resources:
		{
			for (uint num = 0u; num < 10; num++)
			{
				int num2 = 40;
				if (num == 9)
				{
					num2 += 150;
				}
				state.ActionStack.Add(new IncreaseCurrencyAction(base.PlayerId, Coordinates, num2));
			}
			break;
		}
		case RuinsReward.PopulationGrowth:
		{
			WorldCoordinates currentCapitalCoordinates = playerState.GetCurrentCapitalCoordinates(state);
			if (!(currentCapitalCoordinates == WorldCoordinates.NULL_COORDINATES))
			{
				for (int i = 0; i < 3; i++)
				{
					state.ActionStack.Add(new IncreasePopulationAction(base.PlayerId, Coordinates, currentCapitalCoordinates, i * 60));
				}
			}
			break;
		}
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.ExamineRuins;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
		writer.Write((ushort)Reward);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
		Reward = (RuinsReward)reader.ReadUInt16();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates}, Reward: {Reward})";
	}
}
