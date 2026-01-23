using System.IO;
using Polytopia.Data;

public class IncreasePopulationAction : ActionBase
{
	public const int DEFAULT_DELAY_MILLISECONDS = 60;

	public const int LONG_DELAY_MILLISECONDS = 200;

	public int Delay { get; protected set; }

	public WorldCoordinates Source { get; protected set; }

	public WorldCoordinates Target { get; protected set; }

	public IncreasePopulationAction()
	{
	}

	public IncreasePopulationAction(byte playerId, WorldCoordinates source, WorldCoordinates target, int delay = 0)
		: base(playerId)
	{
		Delay = delay;
		Source = source;
		Target = target;
	}

	public override void Execute(GameState state)
	{
		if (state.Version > 40)
		{
			ExecuteDefault(state);
		}
		else
		{
			ExecuteV1(state);
		}
	}

	public void ExecuteDefault(GameState state)
	{
		TileData tile = state.Map.GetTile(Target);
		if (tile.HasImprovement(ImprovementData.Type.City))
		{
			tile.improvement.AddPopulation(1);
			if (tile.improvement.ShouldLevelUp() && (tile.owner == state.CurrentPlayer || state.Version < 41))
			{
				state.ActionStack.Add(new CityLevelUpAction(base.PlayerId, tile.coordinates));
			}
			state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, ScoreSheet.cityXPScore, tile.coordinates));
		}
	}

	public void ExecuteV1(GameState state)
	{
		TileData tile = state.Map.GetTile(Target);
		if (tile.HasImprovement(ImprovementData.Type.City))
		{
			tile.improvement.AddPopulation(1);
			if (tile.improvement.ShouldLevelUp())
			{
				state.ActionStack.Add(new CityLevelUpAction(base.PlayerId, tile.coordinates));
			}
			state.ActionStack.Add(new IncreaseScoreAction(base.PlayerId, ScoreSheet.cityXPScore, tile.coordinates));
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.IncreasePopulation;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Delay);
		Source.Serialize(writer, version);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Delay = reader.ReadInt32();
		Source = new WorldCoordinates(reader, version);
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Source: {Source}, Target: {Target}, Delay: {Delay})";
	}
}
