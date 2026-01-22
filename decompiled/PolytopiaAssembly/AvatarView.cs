using Polytopia.Data;
using UnityEngine;

public class AvatarView : UIBasicComponent
{
	[SerializeField]
	private TintedImage layer0 = new TintedImage();

	[SerializeField]
	private TintedImage layer1 = new TintedImage();

	[SerializeField]
	private TintedImage layer2 = new TintedImage();

	[SerializeField]
	private TintedImage layer3 = new TintedImage();

	[SerializeField]
	private TintedImage layer4 = new TintedImage();

	private bool isInitialized;

	public override void Init()
	{
		base.Init();
		isInitialized = true;
		layer0.Init();
		layer1.Init();
		layer2.Init();
		layer3.Init();
		layer4.Init();
	}

	public void SetState(AvatarState avatarState)
	{
		if (!isInitialized)
		{
			Init();
		}
		SetAvatarPartState(layer0, avatarState.layer0, AvatarExtensions.GetDefaultAvatarPartState(AvatarCategory.Type.Layer0));
		SetAvatarPartState(layer1, avatarState.layer1, AvatarExtensions.GetDefaultAvatarPartState(AvatarCategory.Type.Layer1));
		SetAvatarPartState(layer2, avatarState.layer2, AvatarExtensions.GetDefaultAvatarPartState(AvatarCategory.Type.Layer2));
		SetAvatarPartState(layer3, avatarState.layer3, AvatarExtensions.GetDefaultAvatarPartState(AvatarCategory.Type.Layer3));
		SetAvatarPartState(layer4, avatarState.layer4, AvatarExtensions.GetDefaultAvatarPartState(AvatarCategory.Type.Layer4));
	}

	private void SetAvatarPartState(TintedImage tintedImage, AvatarPartState avatarPartState, AvatarPartState categoryDefault)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		PolytopiaDataManager.GetAvatarData(VersionManager.AvatarDataVersion).TryGetData(avatarPartState.id, out var data);
		int color = avatarPartState.color;
		if (data == null)
		{
			PolytopiaDataManager.GetAvatarData(VersionManager.AvatarDataVersion).TryGetData(categoryDefault.id, out data);
			color = categoryDefault.color;
		}
		((Behaviour)tintedImage.main).enabled = false;
		((Behaviour)tintedImage.tint).enabled = false;
		tintedImage.tintColor = ColorUtil.ColorFromInt((uint)color);
		if (data != null && !string.IsNullOrEmpty(data.sprite))
		{
			tintedImage.mainSpriteHandle.Request(SpriteData.GetAvatarPartSpriteAddress(data.sprite));
		}
		if (data != null && !string.IsNullOrEmpty(data.tintSprite))
		{
			tintedImage.tintSpriteHandle.Request(SpriteData.GetAvatarPartSpriteAddress(data.tintSprite));
		}
	}
}
