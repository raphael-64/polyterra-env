using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InputSystemCursorOverride : MonoBehaviour
{
	public static InputSystemCursorOverride INSTANCE;

	[SerializeField]
	private Transform thisTransform;

	[SerializeField]
	private Image controllerNormalImage;

	[SerializeField]
	private Image controllerHoverImage;

	[SerializeField]
	private CanvasGroup group;

	private bool isVisible;

	private bool isHover;

	private void Awake()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		INSTANCE = this;
		isVisible = false;
		isHover = false;
		group.alpha = 0f;
		((Graphic)controllerHoverImage).color = new Color(1f, 1f, 1f, 0f);
	}

	public void UserIsMovingCursor()
	{
		if (!isVisible)
		{
			ShortcutExtensions.DOKill((Component)(object)group, false);
			group.DOFade(1f, 0.05f);
			isVisible = true;
		}
	}

	public void Hide()
	{
		ShortcutExtensions.DOKill((Component)(object)group, false);
		group.DOFade(0f, 0.3f);
		isVisible = false;
	}

	public void SetPosition(Vector2 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (Vector2.op_Implicit(thisTransform.position) == position && !isVisible && isHover)
		{
			ShortcutExtensions.DOKill((Component)(object)group, false);
			group.DOFade(1f, 0.05f);
			isVisible = true;
		}
		thisTransform.position = Vector2.op_Implicit(position);
	}

	public void SetHover(bool hover)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Color color = default(Color);
		((Color)(ref color))._002Ector(1f, 1f, 1f, hover ? 0f : 1f);
		Color color2 = default(Color);
		((Color)(ref color2))._002Ector(1f, 1f, 1f, hover ? 1f : 0f);
		isHover = hover;
		((Graphic)controllerNormalImage).color = color;
		((Graphic)controllerHoverImage).color = color2;
	}
}
