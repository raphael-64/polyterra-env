using TMPro;
using UnityEngine;

public class UpdateHotseatPlayerPopup : BasicPopup
{
	[SerializeField]
	private PlayerButton playerButton;

	[SerializeField]
	private TMP_InputField playerNameInputField;

	public PlayerProfileState playerState;

	private AvatarState currentAvatarState;

	public void SetData(PlayerProfileState playerState)
	{
		this.playerState = playerState;
		SetAvatarState(playerState.avatarState);
		playerNameInputField.text = playerState.name;
	}

	private void SetAvatarState(AvatarState avatarState)
	{
		currentAvatarState = avatarState;
		playerButton.SetAvatarState(avatarState);
	}

	public void OnMainButtonClicked(int id)
	{
		UpdateHotseatPlayer();
	}

	private void UpdateHotseatPlayer()
	{
		playerState.avatarState = currentAvatarState;
		string text = playerNameInputField.text;
		playerState.name = (string.IsNullOrEmpty(text) ? null : text);
		DiskSerializationHelpers.ToDisk(GameManager.GetHotseatProfilesState(), Paths.GetHotseatProfilesStatePath(), VersionManager.AvatarVersion, out var _);
	}

	public void OnRandomize()
	{
		SetAvatarState(AvatarExtensions.CreateRandomState(VersionManager.AvatarVersion));
	}
}
