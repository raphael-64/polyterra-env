using System.IO;
using PolytopiaBackendBase.Game;

public class GameRules
{
	public enum DeathCondition
	{
		Cities,
		Units
	}

	public int TurnLimit { get; set; }

	public int ScoreLimit { get; set; }

	public bool WinByCapital { get; set; }

	public bool WinByExtermination { get; set; }

	public bool AllowMirrorPick { get; set; }

	public bool AllowSpecialTribes { get; set; }

	public bool AllowTechSharing { get; set; }

	public DeathCondition PlayerDeathCondition { get; set; }

	public GameRules()
	{
	}

	public GameRules(GameMode gameMode)
	{
		LoadPreset(gameMode);
	}

	public void LoadPreset(GameMode gameMode)
	{
		switch (gameMode)
		{
		case GameMode.Perfection:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 0;
			TurnLimit = 30;
			WinByCapital = false;
			WinByExtermination = false;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		case GameMode.Domination:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 0;
			TurnLimit = 0;
			WinByCapital = false;
			WinByExtermination = true;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		case GameMode.Glory:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 10000;
			TurnLimit = 0;
			WinByCapital = false;
			WinByExtermination = false;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		case GameMode.Might:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 0;
			TurnLimit = 0;
			WinByCapital = true;
			WinByExtermination = true;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		case GameMode.Sandbox:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 0;
			TurnLimit = 0;
			WinByCapital = false;
			WinByExtermination = false;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		case GameMode.Tutorial:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 0;
			TurnLimit = 30;
			WinByCapital = false;
			WinByExtermination = false;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		default:
			AllowMirrorPick = false;
			AllowTechSharing = true;
			AllowSpecialTribes = true;
			ScoreLimit = 0;
			TurnLimit = 0;
			WinByCapital = false;
			WinByExtermination = false;
			PlayerDeathCondition = DeathCondition.Cities;
			break;
		}
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		if (version < 9)
		{
			Serialize8(writer, version);
		}
		else
		{
			Serialize9(writer, version);
		}
	}

	public void Serialize8(BinaryWriter writer, int version)
	{
		writer.Write(TurnLimit);
		writer.Write(ScoreLimit);
		writer.Write(1440);
		writer.Write(WinByCapital);
		writer.Write(AllowMirrorPick);
		writer.Write(AllowSpecialTribes);
		writer.Write(WinByExtermination);
		writer.Write(AllowTechSharing);
		writer.Write((ushort)PlayerDeathCondition);
	}

	public void Serialize9(BinaryWriter writer, int version)
	{
		writer.Write(TurnLimit);
		writer.Write(ScoreLimit);
		writer.Write(WinByCapital);
		writer.Write(AllowMirrorPick);
		writer.Write(AllowSpecialTribes);
		writer.Write(WinByExtermination);
		writer.Write(AllowTechSharing);
		writer.Write((ushort)PlayerDeathCondition);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		if (version < 9)
		{
			Deserialize8(reader, version);
		}
		else
		{
			Deserialize9(reader, version);
		}
	}

	private void Deserialize8(BinaryReader reader, int version)
	{
		TurnLimit = reader.ReadInt32();
		ScoreLimit = reader.ReadInt32();
		reader.ReadInt32();
		WinByCapital = reader.ReadBoolean();
		AllowMirrorPick = reader.ReadBoolean();
		AllowSpecialTribes = reader.ReadBoolean();
		WinByExtermination = reader.ReadBoolean();
		AllowTechSharing = reader.ReadBoolean();
		PlayerDeathCondition = (DeathCondition)reader.ReadUInt16();
	}

	private void Deserialize9(BinaryReader reader, int version)
	{
		TurnLimit = reader.ReadInt32();
		ScoreLimit = reader.ReadInt32();
		WinByCapital = reader.ReadBoolean();
		AllowMirrorPick = reader.ReadBoolean();
		AllowSpecialTribes = reader.ReadBoolean();
		WinByExtermination = reader.ReadBoolean();
		AllowTechSharing = reader.ReadBoolean();
		PlayerDeathCondition = (DeathCondition)reader.ReadUInt16();
	}
}
