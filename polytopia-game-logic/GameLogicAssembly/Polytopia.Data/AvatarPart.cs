using Newtonsoft.Json;

namespace Polytopia.Data;

public class AvatarPart
{
	public enum Type
	{
		None = 0,
		Part1Layer0 = 12,
		Part1Layer1 = 13,
		Part2Layer1 = 14,
		Part3Layer1 = 15,
		Part4Layer1 = 16,
		Part5Layer1 = 17,
		Part6Layer1 = 18,
		Part1Layer2 = 19,
		Part10Layer2 = 20,
		Part11Layer2 = 21,
		Part12Layer2 = 22,
		Part2Layer2 = 23,
		Part3Layer2 = 24,
		Part4Layer2 = 25,
		Part5Layer2 = 26,
		Part6Layer2 = 27,
		Part7Layer2 = 28,
		Part8Layer2 = 29,
		Part9Layer2 = 30,
		Part1Layer3 = 31,
		Part2Layer3 = 32,
		Part3Layer3 = 33,
		Part4Layer3 = 34,
		Part5Layer3 = 35,
		Part6Layer3 = 36,
		Part7Layer3 = 37,
		Part1Layer4 = 38,
		Part10Layer4 = 39,
		Part11Layer4 = 40,
		Part12Layer4 = 41,
		Part13Layer4 = 42,
		Part2Layer4 = 43,
		Part3Layer4 = 44,
		Part4Layer4 = 45,
		Part5Layer4 = 46,
		Part6Layer4 = 47,
		Part7Layer4 = 48,
		Part8Layer4 = 49,
		Part9Layer4 = 50,
		Part14Layer4 = 51,
		Part15Layer4 = 52,
		Part16Layer4 = 53,
		Part17Layer4 = 54
	}

	public int idx;

	[JsonConverter(typeof(StringIDToObjectConverter<ColorPalette, ColorPalette.Type>))]
	public ColorPalette colorPalette;

	public string sprite;

	public string tintSprite;

	public Type type => (Type)idx;
}
