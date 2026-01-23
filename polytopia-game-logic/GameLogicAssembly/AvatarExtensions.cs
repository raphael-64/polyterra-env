using System;
using Polytopia.Data;

public static class AvatarExtensions
{
	public static AvatarState CreateRandomState(int version, int seed = -1)
	{
		Random random = ((seed != -1) ? new Random(seed) : new Random());
		return new AvatarState
		{
			layer0 = CreateRandomPartState(AvatarCategory.Type.Layer0, version, random),
			layer1 = CreateRandomPartState(AvatarCategory.Type.Layer1, version, random),
			layer2 = CreateRandomPartState(AvatarCategory.Type.Layer2, version, random),
			layer3 = CreateRandomPartState(AvatarCategory.Type.Layer3, version, random),
			layer4 = CreateRandomPartState(AvatarCategory.Type.Layer4, version, random)
		};
	}

	public static void SetDefaultState(AvatarState state)
	{
		state.layer0 = GetDefaultAvatarPartState(AvatarCategory.Type.Layer0);
		state.layer1 = GetDefaultAvatarPartState(AvatarCategory.Type.Layer1);
		state.layer2 = GetDefaultAvatarPartState(AvatarCategory.Type.Layer2);
		state.layer3 = GetDefaultAvatarPartState(AvatarCategory.Type.Layer3);
		state.layer4 = GetDefaultAvatarPartState(AvatarCategory.Type.Layer4);
	}

	public static AvatarPartState GetDefaultAvatarPartState(AvatarCategory.Type categoryType)
	{
		PolytopiaDataManager.GetAvatarData(VersionManager.AvatarDataVersion).TryGetData(categoryType, out var data);
		AvatarPart avatarPart = data.parts[0];
		int color = 0;
		if (avatarPart.colorPalette.colors.Count > 0)
		{
			color = avatarPart.colorPalette.colors[0];
		}
		return new AvatarPartState
		{
			id = avatarPart.type,
			color = color
		};
	}

	private static AvatarPartState CreateRandomPartState(AvatarCategory.Type categoryType, int version, Random random = null)
	{
		PolytopiaDataManager.GetAvatarData(VersionManager.AvatarDataVersion).TryGetData(categoryType, out var data);
		if (random == null)
		{
			random = new Random();
		}
		int index = random.Range(0, data.parts.Count);
		AvatarPart avatarPart = data.parts[index];
		int color = 0;
		if (avatarPart.colorPalette.colors.Count > 0)
		{
			int index2 = random.Range(0, avatarPart.colorPalette.colors.Count);
			color = avatarPart.colorPalette.colors[index2];
		}
		return new AvatarPartState
		{
			id = avatarPart.type,
			color = color
		};
	}
}
