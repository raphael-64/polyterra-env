using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TribeInfoButton : UITextButton
{
	private PlayerState playerState;

	private SpriteHandle ownerButtonSpriteHandle;

	protected override void OnEnable()
	{
		base.OnEnable();
		base.OnClicked += ShowPlayerInfo;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.OnClicked -= ShowPlayerInfo;
	}

	public void SetOwner(PlayerState owner)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		playerState = owner;
		BgColorStates.defaultColor = owner.GetPlayerColor(GameManager.GameState);
		BgColorStates = new ColorStates
		{
			defaultColor = owner.GetPlayerColor(GameManager.GameState),
			hoverColor = BgColorStates.hoverColor,
			highlightedColor = BgColorStates.highlightedColor,
			highlightedHoverColor = BgColorStates.highlightedHoverColor,
			disabledColor = BgColorStates.disabledColor
		};
		SpriteAddress[] headSpriteAddresses = SpriteData.GetHeadSpriteAddresses(GameManager.GameState, playerState);
		GetOwnerButtonIconSpriteHandle().Request(headSpriteAddresses);
	}

	private void ShowPlayerInfo(int id, BaseEventData eventData)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (playerState != null && !PopupManager.IsPopupShowing<PlayerInfoPopup>())
		{
			PlayerInfoPopup playerInfoPopup = PopupManager.GetPlayerInfoPopup();
			playerInfoPopup.SetData(playerState);
			playerInfoPopup.Show(InputManager.GetInputPosition());
		}
	}

	private SpriteHandle GetOwnerButtonIconSpriteHandle()
	{
		if (ownerButtonSpriteHandle == null)
		{
			ownerButtonSpriteHandle = new SpriteHandle();
			ownerButtonSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
			{
				icon.sprite = spriteHandle.sprite;
				((Graphic)icon).SetNativeSize();
			});
		}
		return ownerButtonSpriteHandle;
	}
}
