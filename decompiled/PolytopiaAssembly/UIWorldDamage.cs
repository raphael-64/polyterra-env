using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldDamage : UIWorldIconBase
{
	[SerializeField]
	protected CanvasGroup cnvsGrp;

	[SerializeField]
	protected Image image;

	protected bool beyondEdge;

	protected Sequence seq;

	public override Vector2 UIPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return base.UIPosition;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = Vector2.zero;
			Rect rect;
			if (value.x < 0f)
			{
				val += new Vector2(-1f, 0f);
			}
			else
			{
				float x = value.x;
				rect = UIManager.WorldRectTransform.rect;
				if (x > ((Rect)(ref rect)).width)
				{
					val += new Vector2(1f, 0f);
				}
			}
			if (value.y < 0f)
			{
				val += new Vector2(0f, -1f);
			}
			else
			{
				float y = value.y;
				rect = UIManager.WorldRectTransform.rect;
				if (y > ((Rect)(ref rect)).height)
				{
					val += new Vector2(0f, 1f);
				}
			}
			beyondEdge = val != Vector2.zero;
			if (beyondEdge)
			{
				rect = UIManager.WorldRectTransform.rect;
				Vector2 val2 = ((Rect)(ref rect)).size * 0.5f;
				Vector2 uIPosition = val2 + val2 * val;
				if (val.x == 0f)
				{
					uIPosition.x = value.x;
				}
				if (val.y == 0f)
				{
					uIPosition.y = value.y;
				}
				base.UIPosition = uIPosition;
			}
			else
			{
				base.UIPosition = value;
			}
			cnvsGrp.alpha = (beyondEdge ? 1 : 0);
		}
	}

	protected override void InternalShow()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		base.InternalShow();
		((Graphic)image).color = ColorUtil.SetAlphaOnColor(((Graphic)image).color, 1f);
		seq = DOTween.Sequence();
		TweenSettingsExtensions.AppendInterval(seq, 0.5f);
		TweenSettingsExtensions.Append(seq, (Tween)(object)image.DOFade(0f, 1f));
		TweenSettingsExtensions.AppendCallback(seq, new TweenCallback(Hide));
	}

	public override void Hide()
	{
		TweenUtils.KillSequence(seq);
		base.Hide();
	}
}
