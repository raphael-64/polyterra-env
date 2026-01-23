using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopupScrollerContent : UIBasicComponent, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	public RectTransform parentRT;

	protected Tween dragTween;

	private bool isDragEnabled;

	private Tween positionTween;

	private bool isDragging;

	private void OnDisable()
	{
		TweenUtils.KillTween(positionTween, complete: true);
		if (isDragging)
		{
			UnlockInteraction();
		}
	}

	public void AnimateFrom(Vector2 origin, float duration)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		isDragEnabled = false;
		ShortcutExtensions.DOKill((Component)(object)base.rectTransform, true);
		base.rectTransform.anchoredPosition = origin;
		positionTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(base.rectTransform.DOAnchorPos(Vector2.zero, duration), (Ease)27, 2f), (TweenCallback)delegate
		{
			isDragEnabled = true;
		});
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!PopupManager.GetCurrentPopup().fullscreenVariant && isDragEnabled && !isDragging)
		{
			isDragging = true;
			InputManager.DisableInput(InputManager.InputType.Camera | InputManager.InputType.Map);
			TweenUtils.KillTween(dragTween);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		if (!PopupManager.GetCurrentPopup().fullscreenVariant && isDragEnabled && isDragging)
		{
			Vector2 val = eventData.delta * UICanvasScalerHelper.GetInvertedUIScale();
			Vector2 anchoredPosition = base.rectTransform.anchoredPosition + val;
			anchoredPosition.x -= GetRubberDelta(val.x, base.rectTransform.GetWidth());
			anchoredPosition.y -= GetRubberDelta(val.y, base.rectTransform.GetHeight());
			base.rectTransform.anchoredPosition = anchoredPosition;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (isDragging)
		{
			dragTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(base.rectTransform.DOAnchorPos(Vector2.zero, 0.2f), (Ease)27);
			UnlockInteraction();
		}
	}

	private float GetRubberDelta(float overStretching, float viewSize)
	{
		return (float)(1.0 - 1.0 / ((double)Mathf.Abs(overStretching) * 0.550000011920929 / (double)viewSize + 1.0)) * viewSize * Mathf.Sign(overStretching);
	}

	private void UnlockInteraction()
	{
		InputManager.EnableInput(InputManager.InputType.Camera | InputManager.InputType.Map);
		isDragging = false;
	}
}
