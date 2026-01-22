using UnityEngine;
using UnityEngine.UI;

public class UIImageButton : UIBasicButton
{
	[SerializeField]
	protected Image icon;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	public Sprite Icon
	{
		get
		{
			return icon.sprite;
		}
		set
		{
			icon.sprite = value;
		}
	}

	public override void Awake()
	{
		base.Awake();
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetImage(spriteHandle.sprite);
		});
	}

	public void LoadImage(string spriteId)
	{
		iconSpriteHandle.Request(SpriteData.GetIconSpriteAddress(spriteId));
	}

	public void SetImage(Sprite sprite)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		icon.sprite = sprite;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		Vector2 sizeDelta = ((Graphic)icon).rectTransform.sizeDelta;
		((Graphic)icon).rectTransform.sizeDelta = sizeDelta;
		RectTransform obj = ((Graphic)icon).rectTransform;
		float x = sprite.pivot.x;
		Rect rect = sprite.rect;
		float num = x / ((Rect)(ref rect)).width;
		float y = sprite.pivot.y;
		rect = sprite.rect;
		obj.pivot = new Vector2(num, y / ((Rect)(ref rect)).height);
		UIUtils.FitImageContentInParent(((Graphic)icon).rectTransform);
	}
}
