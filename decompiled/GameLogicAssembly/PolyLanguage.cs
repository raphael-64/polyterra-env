using System;
using Polytopia.Data;

public static class PolyLanguage
{
	public static string[] language = new string[26]
	{
		"po", "ly", "lu", "mi", "lo", "da", "bi", "oo", "sa", "ko",
		"me", "da", "to", "pi", "as", "an", "ki", "uh", "fi", "xi",
		"-", "sha", "ji", "us", "za", "um"
	};

	private static string[] templates = new string[8] { "[action] of [place]", "[nature] of [place]", "[place]ian [action]", "[place]ian [nature]", "[super] [action]", "The [nature] of [action]", "The [super] [nature]", "[nature] & [action]" };

	private static string[] actions = new string[52]
	{
		"War", "Spirit", "Faith", "Glory", "Blood", "Empires", "Songs", "Dawn", "Prophecy", "Gold",
		"Fire", "Swords", "Queens", "Knights", "Kings", "Tribes", "Tales", "Quests", "Change", "Games",
		"Throne", "Conquest", "Struggle", "Victory", "Battles", "Legends", "Heroes", "Storms", "Clouds", "Gods",
		"Love", "Lords", "Lights", "Wrath", "Destruction", "Whales", "Ruins", "Monuments", "Wonder", "Giants",
		"Warriors", "Archers", "Defenders", "Catapults", "Riders", "Sleds", "Explorers", "Priests", "Ships", "Dragons",
		"Crabs", "Rebellion"
	};

	private static string[] crazyActions = new string[25]
	{
		"Clowns", "Bongo", "Duh!", "Squeal", "Squirrel", "Confusion", "Gruff", "Moan", "Chickens", "Sponge",
		"Gnomes", "Bell boys", "Gherkins", "Commotion", "LOL", "Shenanigans", "Hullabaloo", "Papercuts", "Eggs", "Mooni",
		"Gaami", "Banjo", "Flowers", "Fiddlesticks", "Fish Sticks"
	};

	private static string[] superLatives = new string[29]
	{
		"Epic", "Endless", "Glorious", "Brave", "Misty", "Mysterious", "Lost", "Cold", "Amazing", "Doomed",
		"Glowing", "Glimmering", "Magical", "Living", "Thriving", "Bold", "Dark", "Bright", "Majestic", "Shimmering",
		"Lucky", "Great", "Everlasting", "Eternal", "Superb", "Frozen", "Magnificent", "Evil", "Beautiful"
	};

	private static string[] crazySuperLatives = new string[18]
	{
		"Gruffy", "Slimy", "Silly", "Unwilling", "Stumbling", "Drunken", "Merry", "Mediocre", "Normal", "Stupid",
		"Moody", "Tipsy", "Trifling", "Rancid", "Numb", "Livid", "Smooth", "Nuclear"
	};

	private static string[] natures = new string[45]
	{
		"Hills", "Fields", "Lands", "Forest", "Ocean", "Fruit", "Mountain", "Lake", "Paradise", "Jungle",
		"Desert", "River", "Sea", "Shores", "Valley", "Garden", "Moon", "Star", "Winter", "Spring",
		"Summer", "Autumn", "Divide", "Square", "Glacier", "Ice", "Plains", "Volcano", "Cliff", "Rapids",
		"Reef", "Plateau", "Basin", "Oasis", "Marsh", "Swamp", "Monsoon", "Atoll", "Fjord", "Tundra",
		"Map", "Strait", "Savanna", "Butte", "Bay"
	};

	private static string[] crazyNatures = new string[14]
	{
		"Custard", "Goon", "Cat", "Spaghetti", "Fish", "Fame", "Popcorn", "Dessert", "Space", "Beasts",
		"Birds", "Bugs", "Food", "Aliens"
	};

	private static string[] forbiddenWords = new string[4] { "anus", "loli", "boner", "niga" };

	public static string MakeWord(TribeData tribeData, float length = 0f, int seed = 0)
	{
		bool flag = true;
		string[] array = language;
		if (tribeData != null)
		{
			array = tribeData.language.Split(',');
			if (tribeData.type == TribeData.Type.Elyrion)
			{
				flag = false;
			}
		}
		Random rng = ((seed != 0) ? new Random(seed) : new Random());
		if (length == 0f)
		{
			length = 3f + 3f * rng.Value();
		}
		string text = string.Empty;
		int num = 0;
		while ((float)text.Length < length && num++ < 100)
		{
			text += array[rng.Range(0, array.Length)];
			if (text.Substring(0, 1) == " " || text.Substring(0, 1) == "-")
			{
				text = string.Empty;
			}
		}
		if (!IsWordAllowed(text))
		{
			text = MakeWord(tribeData, length);
		}
		if (flag)
		{
			text = char.ToUpper(text[0]) + text.Substring(1);
		}
		return text;
	}

	private static bool IsWordAllowed(string word)
	{
		string[] array = forbiddenWords;
		foreach (string text in array)
		{
			if (word == text.ToLower())
			{
				Log.Verbose("[felix] {0} is not allowed", new object[1] { word });
				return false;
			}
		}
		return true;
	}

	public static string MakeGameName(bool makeCrazyName = false, Random random = null)
	{
		if (random == null)
		{
			random = new Random();
		}
		return templates[random.Range(0, templates.Length)].Replace("[action]", GetWord(actions, crazyActions, makeCrazyName, random)).Replace("[place]", MakeWord(null)).Replace("[super]", GetWord(superLatives, crazySuperLatives, makeCrazyName, random))
			.Replace("[nature]", GetWord(natures, crazyNatures, makeCrazyName, random));
	}

	private static string GetWord(string[] words, string[] crazyWords, bool makeCrazyName, Random random)
	{
		if ((makeCrazyName ? 0.5f : 0f) < random.Value())
		{
			return words[random.Range(0, words.Length)];
		}
		return crazyWords[random.Range(0, crazyWords.Length)];
	}
}
