using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoIcon : MonoBehaviour
{
	public enum Mood
	{
		None,
		DataBased,
		Happy,
		Angry
	}

	[SerializeField]
	public RectTransform rectTransform;

	[SerializeField]
	public Image HeadImage;

	[SerializeField]
	public Image DiplomacyStateImage;

	[SerializeField]
	public Image MoodImage;

	[SerializeField]
	public Image BackgroundImage;

	public const float REFERENCE_SIZE = 512f;

	private SpriteHandle headSpriteHandle;

	public SpriteHandle GetHeadSpriteHandle()
	{
		if (headSpriteHandle == null)
		{
			headSpriteHandle = new SpriteHandle();
			headSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_001c: Unknown result type (might be due to invalid IL or missing references)
				//IL_001f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				HeadImage.sprite = spriteHandle.sprite;
				Rect rect = spriteHandle.sprite.rect;
				Vector2 size = ((Rect)(ref rect)).size;
				((Graphic)HeadImage).rectTransform.sizeDelta = size * rectTransform.GetHeight() / 512f;
			});
		}
		return headSpriteHandle;
	}

	public void SetData(PlayerState player, PlayerState otherPlayer, Mood mood = Mood.DataBased, bool shouldShowDiplomaticState = true, bool forceKnownPlayer = false)
	{
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		GameState gameState = GameManager.GameState;
		bool flag = player.IsAlive(gameState);
		PlayerState localPlayer = GameManager.LocalPlayer;
		bool flag2 = localPlayer.KnowsPlayer(player.Id) || forceKnownPlayer;
		if (!flag)
		{
			SpriteAddress headSpriteAddress = SpriteData.GetHeadSpriteAddress("dead");
			GetHeadSpriteHandle().Request(headSpriteAddress);
		}
		else if (localPlayer.Id != player.Id && !flag2)
		{
			SpriteAddress headSpriteAddress = SpriteData.GetHeadSpriteAddress("neutral");
			GetHeadSpriteHandle().Request(headSpriteAddress);
		}
		else
		{
			GetHeadSpriteHandle().Request(SpriteData.GetHeadSpriteAddresses(GameManager.GameState, player));
		}
		if (flag && player.AutoPlay && mood == Mood.DataBased)
		{
			int aggression = player.GetAggression(otherPlayer.Id, GameManager.GameState);
			bool flag3 = (float)aggression > OpinionManager.HateLimit * -1000f;
			bool flag4 = (float)aggression < OpinionManager.LoveLimit * -1000f;
			MoodImage.sprite = (flag3 ? UIManager.IconData.GetSprite("Angry") : (flag4 ? UIManager.IconData.GetSprite("Happy") : null));
		}
		else
		{
			switch (mood)
			{
			case Mood.Happy:
				MoodImage.sprite = UIManager.IconData.GetSprite("Happy");
				break;
			case Mood.Angry:
				MoodImage.sprite = UIManager.IconData.GetSprite("Angry");
				break;
			default:
				MoodImage.sprite = null;
				break;
			}
		}
		((Component)MoodImage).gameObject.SetActive((Object)(object)MoodImage.sprite != (Object)null);
		if ((Object)(object)MoodImage.sprite != (Object)null)
		{
			Rect rect = MoodImage.sprite.rect;
			Vector2 size = ((Rect)(ref rect)).size;
			Vector2 pivot = MoodImage.sprite.pivot;
			((Graphic)MoodImage).rectTransform.pivot = pivot / size;
			((Graphic)MoodImage).rectTransform.sizeDelta = size * (rectTransform.GetHeight() / 512f) * 3f;
		}
		Color color = ((flag2 || !flag) ? player.GetPlayerColor(GameManager.GameState) : ColorConstants.gray);
		color.a = 1f;
		((Graphic)BackgroundImage).color = color;
		if (flag && shouldShowDiplomaticState && player.HasPeaceWith(otherPlayer.Id))
		{
			DiplomacyStateImage.sprite = UIManager.IconData.GetSprite("peace");
			((Component)DiplomacyStateImage).gameObject.SetActive(true);
		}
		else if (flag && shouldShowDiplomaticState && player.HasWarWith(otherPlayer, gameState))
		{
			DiplomacyStateImage.sprite = UIManager.IconData.GetSprite("breakpeace");
			((Component)DiplomacyStateImage).gameObject.SetActive(true);
		}
		else
		{
			((Component)DiplomacyStateImage).gameObject.SetActive(false);
		}
	}
}
