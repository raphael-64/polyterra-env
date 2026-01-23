using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class TribeButtonStar : UIBasicComponent
{
	[SerializeField]
	public Image emptyStar;

	[SerializeField]
	public Image fullStar;

	protected bool m_active;

	protected bool isBig;

	protected float animationTime;

	protected Vector2 emptyOgSize;

	protected Vector2 starOgSize;

	public bool Active
	{
		get
		{
			return m_active;
		}
		set
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			if (value == ((Component)fullStar).gameObject.activeSelf)
			{
				return;
			}
			m_active = value;
			((Component)fullStar).gameObject.SetActive(m_active);
			if (m_active && animationTime > 0f)
			{
				((Transform)((Graphic)fullStar).rectTransform).localScale = new Vector3(0.5f, 0.5f, 1f);
				TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale((Transform)(object)((Graphic)fullStar).rectTransform, 1f, animationTime), (Ease)24), (TweenCallback)delegate
				{
					((Component)emptyStar).gameObject.SetActive(!m_active);
				});
			}
			else
			{
				((Component)emptyStar).gameObject.SetActive(!m_active);
			}
		}
	}

	public bool IsBig
	{
		get
		{
			return isBig;
		}
		set
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			isBig = value;
			if (isBig)
			{
				((Graphic)emptyStar).SetNativeSize();
				((Graphic)fullStar).SetNativeSize();
				RectTransform obj = ((Graphic)emptyStar).rectTransform;
				obj.sizeDelta *= 0.5f;
				RectTransform obj2 = ((Graphic)fullStar).rectTransform;
				obj2.sizeDelta *= 0.5f;
			}
			else
			{
				((Graphic)emptyStar).rectTransform.sizeDelta = emptyOgSize;
				((Graphic)fullStar).rectTransform.sizeDelta = starOgSize;
			}
			base.rectTransform.sizeDelta = Vector2.Max(((Graphic)emptyStar).rectTransform.sizeDelta, ((Graphic)fullStar).rectTransform.sizeDelta);
		}
	}

	public float AnimationTime
	{
		get
		{
			return animationTime;
		}
		set
		{
			animationTime = value;
		}
	}

	public override void Init()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		base.Init();
		emptyOgSize = ((Graphic)emptyStar).rectTransform.sizeDelta;
		starOgSize = ((Graphic)fullStar).rectTransform.sizeDelta;
		((Component)fullStar).gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		DOTween.Kill((object)((Graphic)fullStar).rectTransform, false);
	}
}
