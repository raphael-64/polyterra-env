using System.IO;
using Polytopia.Data;

public class DecreasePopulationAction : ActionBase
{
	public const int DEFAULT_DELAY_MILLISECONDS = 200;

	public int Delay { get; protected set; }

	public WorldCoordinates Target { get; protected set; }

	public DecreasePopulationAction()
	{
	}

	public DecreasePopulationAction(byte playerId, WorldCoordinates target, int delay = 0)
		: base(playerId)
	{
		Delay = delay;
		Target = target;
	}

	public override void Execute(GameState state)
	{
		TileData tile = state.Map.GetTile(Target);
		if (tile.HasImprovement(ImprovementData.Type.City))
		{
			state.TryGetPlayer(tile.owner, out var playerState);
			ActionUtils.RemoveScore(playerState, ScoreSheet.cityXPScore);
			tile.improvement.AddPopulation(-1);
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.DecreasePopulation;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Delay);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Delay = reader.ReadInt32();
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Target: {Target}, Delay: {Delay})";
	}
}
