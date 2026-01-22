using System.IO;

public class AttackAction : ActionBase
{
	public enum AnimationType
	{
		None,
		Normal,
		Splash
	}

	public WorldCoordinates Origin { get; private set; }

	public WorldCoordinates Target { get; private set; }

	public int Damage { get; private set; }

	public AnimationType Animation { get; private set; }

	public bool ShouldMoveToTarget { get; private set; }

	public ushort Delay { get; private set; }

	public AttackAction()
	{
	}

	public AttackAction(byte playerId, WorldCoordinates origin, WorldCoordinates target, int damage, bool shouldMoveToTarget, AnimationType animation = AnimationType.Normal, ushort delay = 100)
		: base(playerId)
	{
		Origin = origin;
		Target = target;
		Damage = damage;
		Animation = animation;
		ShouldMoveToTarget = shouldMoveToTarget;
		Delay = delay;
	}

	public override void Execute(GameState state)
	{
		ActionUtils.PerformAttack(state, base.PlayerId, Origin, Target, Damage);
	}

	public override ActionType GetActionType()
	{
		return ActionType.Attack;
	}

	public override void Serialize(BinaryWriter writer, int version)
	{
		base.Serialize(writer, version);
		writer.Write(Damage);
		Origin.Serialize(writer, version);
		Target.Serialize(writer, version);
		writer.Write((ushort)Animation);
		writer.Write(ShouldMoveToTarget);
		if (version >= 10)
		{
			writer.Write(Delay);
		}
	}

	public override void Deserialize(BinaryReader reader, int version)
	{
		base.Deserialize(reader, version);
		Damage = reader.ReadInt32();
		Origin = new WorldCoordinates(reader, version);
		Target = new WorldCoordinates(reader, version);
		Animation = (AnimationType)reader.ReadUInt16();
		ShouldMoveToTarget = reader.ReadBoolean();
		if (version < 10)
		{
			Delay = 100;
		}
		else
		{
			Delay = reader.ReadUInt16();
		}
	}

	public override string ToString()
	{
		return $"{GetType()} (PlayerId: {base.PlayerId}, Origin: {Origin}, Target: {Target}, Damage: {Damage}, Delay {Delay})";
	}
}
