using System.IO;
using Polytopia.Data;

public class InfiltrateAction : ActionBase
{
	public WorldCoordinates Target { get; protected set; }

	public WorldCoordinates Origin { get; protected set; }

	public InfiltrateAction()
	{
	}

	public InfiltrateAction(byte playerId, WorldCoordinates origin, WorldCoordinates target)
		: base(playerId)
	{
		Target = target;
		Origin = origin;
	}

	public override void Execute(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Origin);
		UnitData.Type type = ((tile.unit != null) ? tile.unit.type : UnitData.Type.Cloak);
		ActionUtils.KillUnit(gameState, tile);
		gameState.ActionStack.Add(new InfiltrationRewardAction(base.PlayerId, CityReward.Rebellion, Target, type));
	}

	public override ActionType GetActionType()
	{
		return ActionType.Infiltrate;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		Target.Serialize(writer, version);
		writer.Write(base.HoldForCommand);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Target = new WorldCoordinates(reader, version);
		base.HoldForCommand = reader.ReadBoolean();
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Coordinates: {Target}, Hold: {base.HoldForCommand})";
	}
}
