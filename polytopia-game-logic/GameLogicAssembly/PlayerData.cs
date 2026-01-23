using Polytopia.Data;

public class PlayerData
{
	public enum Type
	{
		None,
		Bot,
		Local,
		Friend,
		Player
	}

	public enum State
	{
		None,
		IsYou,
		Accepted,
		SentRequest,
		ReceivedRequest,
		Rejected
	}

	public Type type;

	public State state;

	public bool knownTribe;

	public TribeData.Type tribe;

	public TribeData.Type tribeMix;

	public GameSettings.Difficulties botDifficulty;

	public SkinType skinType;

	public PlayerProfileState profile = new PlayerProfileState();

	public string defaultName;

	public PlayerData Clone()
	{
		return new PlayerData
		{
			type = type,
			knownTribe = knownTribe,
			tribe = tribe,
			botDifficulty = botDifficulty,
			state = state,
			profile = 
			{
				id = profile.id,
				name = profile.name,
				avatarState = profile.avatarState
			},
			defaultName = defaultName
		};
	}

	public string GetName()
	{
		if (!string.IsNullOrEmpty(profile.name))
		{
			return profile.name;
		}
		return defaultName;
	}
}
