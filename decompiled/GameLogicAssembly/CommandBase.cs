using System.IO;

public class CommandBase
{
	public static string VALIDATION_ERROR_MISSING_PLAYER = "Player does not exist";

	public static string VALIDATION_ERROR_MISSING_TECH_DATA = "Missing tech data";

	public static string VALIDATION_ERROR_TECH_NOT_UNLOCKABLE = "Tech not unlockable";

	public static string VALIDATION_ERROR_CANT_AFFORD = "Not enough resources";

	public static string VALIDATION_ERROR_MISSING_UNIT_DATA = "Missing unit data";

	public static string VALIDATION_ERROR_TILE_OCCUPIED = "Tile is occupied by unit";

	public static string VALIDATION_ERROR_CANT_SUPPORT_MORE_UNITS = "City can't support more units";

	public static string VALIDATION_ERROR_UNIT_LOCKED = "Unit can't move to any surrounding tile";

	public static string VALIDATION_ERROR_UNIT_MISSING = "Tile is missing unit";

	public static string VALIDATION_ERROR_CANT_ATTACK = "Can't attack";

	public static string VALIDATION_ERROR_INVALID_ATTACK_TARGET = "Invalid attack target";

	public static string VALIDATION_ERROR_CANT_BREAK_ICE = "Can't break ice";

	public static string VALIDATION_ERROR_MISSING_IMPROVEMENT_DATA = "Missing improvement data";

	public static string VALIDATION_ERROR_NOT_UNLOCKED = "Not unlocked";

	public static string VALIDATION_ERROR_CANT_BUILD = "Can't build";

	public static string VALIDATION_ERROR_MISSING_TILE = "Missing tile";

	public static string VALIDATION_ERROR_MISSING_CITY = "Missing city";

	public static string VALIDATION_ERROR_CANT_CAPTURE = "Can't capture";

	public static string VALIDATION_ERROR_PLAYER_MISMATCH = "Player mismatch";

	public static string VALIDATION_ERROR_COMMAND_TRIGGER_EXISTS = "Pending command trigger exists";

	public static string VALIDATION_ERROR_MISSING_COMMAND_TRIGGER = "No command trigger";

	public static string VALIDATION_ERROR_MISSING_IMPROVEMENT = "Missing improvement";

	public static string VALIDATION_ERROR_CANT_FREEZE = "Can't freeze";

	public static string VALIDATION_ERROR_CANT_HEAL_OTHERS = "Can't heal others";

	public static string VALIDATION_ERROR_CANT_MOVE = "Can't move";

	public static string VALIDATION_ERROR_INVALID_PATH = "Invalid path";

	public static string VALIDATION_ERROR_CANT_PROMOTE = "Can't promote";

	public static string VALIDATION_ERROR_CANT_RECOVER = "Can't recover";

	public static string VALIDATION_ERROR_INVALID_DIPLOMACY_TARGET = "Invalid diplomacy target";

	public static string VALIDATION_ERROR_INVALID_DIPLOMACY_STATE = "Invalid diplomacy state";

	public static string VALIDATION_ERROR_WITHIN_TEMPLE_RANGE = "Withing temple range";

	public byte PlayerId { get; protected set; }

	public string Id => GetCommandType().ToString().ToLowerInvariant();

	public CommandBase()
	{
	}

	public CommandBase(byte playerId)
	{
		PlayerId = playerId;
	}

	public virtual void Execute(GameState state)
	{
		if (state.Version >= 30)
		{
			state.ActionStack.Add(new EndCommandAction(PlayerId, GetCommandType()));
		}
	}

	public bool IsValid(GameState state)
	{
		string validationError;
		return IsValid(state, out validationError);
	}

	public virtual bool IsValid(GameState state, out string validationError)
	{
		return PassesBasicValidation(state, out validationError);
	}

	public virtual bool NeedServerConfirmation()
	{
		return false;
	}

	public virtual bool ShouldAlwaysAskForConfirmation()
	{
		return false;
	}

	public virtual bool ShouldAskForConfirmation()
	{
		return false;
	}

	public bool PassesBasicValidation(GameState state, out string validationError)
	{
		if (state.CurrentPlayer != PlayerId)
		{
			validationError = VALIDATION_ERROR_PLAYER_MISMATCH;
			return false;
		}
		if (state.TryGetPendingCommandTrigger(PlayerId, out var _))
		{
			validationError = VALIDATION_ERROR_COMMAND_TRIGGER_EXISTS;
			return false;
		}
		validationError = null;
		return true;
	}

	public virtual CommandType GetCommandType()
	{
		return CommandType.None;
	}

	public virtual void Serialize(BinaryWriter writer, int version)
	{
		writer.Write(PlayerId);
	}

	public virtual void Deserialize(BinaryReader reader, int version)
	{
		PlayerId = reader.ReadByte();
	}

	public static byte[] ToByteArray(CommandBase command, int version)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			binaryWriter.Write(version);
			GameState.SerializeCommand(command, binaryWriter, version);
		}
		return memoryStream.ToArray();
	}

	public static bool FromByteArray(byte[] commandData, out CommandBase command, out int version)
	{
		using MemoryStream input = new MemoryStream(commandData);
		using BinaryReader reader = new BinaryReader(input);
		version = SerializationHelpers.ReadVersion(reader);
		command = GameState.DeserializeCommand(reader, version);
		return true;
	}
}
