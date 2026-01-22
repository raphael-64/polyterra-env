using System.IO;

public class DecomposeAction : ActionBase
{
	public WorldCoordinates Coordinates { get; protected set; }

	public DecomposeAction()
	{
	}

	public DecomposeAction(byte playerId, WorldCoordinates coordinates)
		: base(playerId)
	{
		Coordinates = coordinates;
	}

	public override void Execute(GameState state)
	{
		ExecuteDefault(state);
	}

	private void ExecuteDefault(GameState state)
	{
		state.Map.GetTile(Coordinates).improvement.AddEffect(ImprovementEffect.decomposing);
	}

	public override ActionType GetActionType()
	{
		return ActionType.Decompose;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Coordinates.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Coordinates = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Coordinates})";
	}
}
