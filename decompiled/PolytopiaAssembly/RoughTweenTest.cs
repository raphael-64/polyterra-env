using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class RoughTweenTest : MonoBehaviour
{
	public Image target;

	public float animTime;

	public float endScale;

	private float m_value;

	public float Value
	{
		get
		{
			return m_value;
		}
		set
		{
			m_value = value;
			Log.Verbose("m_value: {0}", new object[1] { m_value });
		}
	}

	private void Start()
	{
	}

	public void OnTestTween()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Expected O, but got Unknown
		((Transform)((Graphic)target).rectTransform).localScale = Vector3.one;
		Value = 0f;
		((Graphic)target).color = Color.white;
		TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(target.DOFade(0f, 1f), TweenUtils.GetRoughEase()), new TweenCallback(OnAnimComplete));
	}

	private void OnAnimComplete()
	{
	}
}
