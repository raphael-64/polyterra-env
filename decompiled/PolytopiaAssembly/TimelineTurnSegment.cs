using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class TimelineTurnSegment : MonoBehaviour
{
	public enum Type
	{
		Left,
		Center,
		Right,
		Both
	}

	public const float MINIMUM_WIDTH = 64f;

	public const float COMMAND_WIDTH = 10f;

	public RectTransform rectTransform;

	public Image icon;

	public Image background;

	public Sprite leftBackground;

	public Sprite centerBackground;

	public Sprite rightBackground;

	public Sprite fullBackground;

	private float width = 64f;

	private float iconSizeMultiplier = 2f;

	private SpriteHandle iconSpriteHandle = new SpriteHandle();

	private void Awake()
	{
		iconSpriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			SetIcon(spriteHandle.sprite);
		});
	}

	public void SetTribe(TribeData.Type type)
	{
		iconSpriteHandle.Request(SpriteData.GetHeadSpriteAddress(type));
	}

	public void LoadIcon(SpriteAddress[] iconSpriteAddress)
	{
		iconSpriteHandle.Request(iconSpriteAddress);
	}

	public void SetIcon(Sprite sprite)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		icon.sprite = sprite;
		icon.useSpriteMesh = true;
		((Graphic)icon).SetNativeSize();
		Vector2 sizeDelta = ((Graphic)icon).rectTransform.sizeDelta;
		((Graphic)icon).rectTransform.sizeDelta = sizeDelta * iconSizeMultiplier;
		RectTransform obj = ((Graphic)icon).rectTransform;
		float x = sprite.pivot.x;
		Rect rect = sprite.rect;
		float num = x / ((Rect)(ref rect)).width;
		float y = sprite.pivot.y;
		rect = sprite.rect;
		obj.pivot = new Vector2(num, y / ((Rect)(ref rect)).height);
	}

	public void SetColor(Color color)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		color.a = 0.8f;
		((Graphic)background).color = color;
	}

	public Color GetColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Graphic)background).color;
	}

	public void SetWidth(float width)
	{
		this.width = Mathf.Max(64f, width);
		rectTransform.SetWidth(this.width);
	}

	public float GetWidth()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			return width;
		}
		return 0f;
	}

	public void SetType(Type type)
	{
		switch (type)
		{
		case Type.Left:
			background.sprite = leftBackground;
			break;
		case Type.Center:
			background.sprite = centerBackground;
			break;
		case Type.Right:
			background.sprite = rightBackground;
			break;
		case Type.Both:
			background.sprite = fullBackground;
			break;
		default:
			background.sprite = centerBackground;
			break;
		}
	}
}
