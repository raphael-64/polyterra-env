using System.Threading.Tasks;
using PolytopiaBackendBase;
using PolytopiaBackendBase.Auth;
using PolytopiaBackendBase.Game;
using UnityEngine;

public class UpdateAvatarPopup : BasicPopup
{
	[SerializeField]
	private PlayerButton playerButton;

	private AvatarState currentAvatarState;

	public void SetData(AvatarState avatarState)
	{
		currentAvatarState = avatarState;
		playerButton.SetAvatarState(avatarState);
	}

	public async Task UpdateAvatar()
	{
		PolytopiaUserViewModel user = PolytopiaBackendAdapter.Instance?.ClientUserData?.User;
		if (user != null)
		{
			byte[] avatarData = SerializationHelpers.ToByteArray(currentAvatarState, VersionManager.AvatarVersion);
			ServerResponse<ResponseViewModel> serverResponse = await PolytopiaBackendAdapter.Instance.UpdateAvatar(new AvatarBindingModel
			{
				AvatarStateData = avatarData
			});
			if (!serverResponse.Success)
			{
				PopupManager.ShowErrorPopup(Localization.GetErrorMessage(serverResponse));
			}
			else
			{
				user.AvatarStateData = avatarData;
			}
		}
	}

	public void OnRandomize()
	{
		SetData(AvatarExtensions.CreateRandomState(VersionManager.AvatarVersion));
	}
}
