using System.IO;
using Polytopia.Data;

public class FreezeUnitAction : ActionBase
{
	public WorldCoordinates Origin { get; private set; }

	public WorldCoordinates Target { get; private set; }

	public int Damage { get; private set; }

	public FreezeUnitAction()
	{
	}

	public FreezeUnitAction(byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage)
		: base(playerId)
	{
		Origin = origin;
		Target = target;
		Damage = damage;
	}

	public override void Execute(GameState gameState)
	{
		if (gameState.Version <= 81)
		{
			ExecuteV81(gameState);
		}
		else
		{
			ExecuteDefault(gameState);
		}
	}

	private void ExecuteV81(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Target);
		UnitState unit = tile.unit;
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		gameState.TryGetPlayer(unit.owner, out var playerState2);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, gameState);
		if (Origin != WorldCoordinates.NULL_COORDINATES)
		{
			ActionUtils.PerformAttack(gameState, base.PlayerId, Origin, Target, Damage);
		}
		unit.moved = true;
		unit.attacked = true;
		unit.AddEffect(UnitEffect.Frozen);
	}

	private void ExecuteDefault(GameState gameState)
	{
		TileData tile = gameState.Map.GetTile(Target);
		UnitState unit = tile.unit;
		gameState.TryGetPlayer(base.PlayerId, out var playerState);
		gameState.TryGetPlayer(unit.owner, out var playerState2);
		PlayerExtensions.ReactToAttack(playerState, playerState2, tile, gameState);
		if (Origin != WorldCoordinates.NULL_COORDINATES)
		{
			ActionUtils.PerformAttack(gameState, base.PlayerId, Origin, Target, Damage);
		}
		unit.moved = true;
		unit.attacked = true;
		unit.AddEffect(UnitEffect.Frozen);
		if (gameState.TryGetTask(playerState, TaskData.Type.Pacifist, out var task))
		{
			task.Reset();
		}
	}

	public override ActionType GetActionType()
	{
		return ActionType.FreezeUnit;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Damage);
		Origin.Serialize(writer, version);
		Target.Serialize(writer, version);
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Damage = reader.ReadInt32();
		Origin = new WorldCoordinates(reader, version);
		Target = new WorldCoordinates(reader, version);
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Origin: {Origin}, Target: {Target}, Damage: {Damage})";
	}
}
