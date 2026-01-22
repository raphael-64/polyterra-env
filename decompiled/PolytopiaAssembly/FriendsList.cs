using PolytopiaBackendBase;
using UnityEngine;
using UnityEngine.UI;

public class FriendsList : UIScreenBase
{
	[Header("Friends List")]
	[SerializeField]
	protected FriendsListComponent friendsListComponent;

	[SerializeField]
	protected LayoutElement containerLayoutElement;

	public FriendsListType ListType
	{
		get
		{
			return friendsListComponent.ListType;
		}
		set
		{
			friendsListComponent.ListType = value;
		}
	}

	public override void OnScreenUpdated()
	{
		if ((Object)(object)containerLayoutElement != (Object)null)
		{
			containerLayoutElement.minHeight = UIManager.GetUIHeight();
		}
	}

	private void OnBackendConnectionChanged(ConnectionStatus status)
	{
		Show();
		ShowCompleted();
	}

	public override async void Show(bool instant = false)
	{
		friendsListComponent.PlayerPickedCallback = PlayerPickedCallback;
		friendsListComponent.Show();
		base.Show(instant);
		UINavigationManager.Select(CurrentSelectable);
		OnScreenUpdated();
	}

	protected void PlayerPickedCallback(PlayerData friend)
	{
		GameManager.PreliminaryGameSettings.AddPlayer(friend);
		OnBack();
	}
}
