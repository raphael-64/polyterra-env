using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
	[SerializeField]
	private Transform container;

	[SerializeField]
	private SpriteRenderer icon;

	[SerializeField]
	private TextMeshPro label;

	protected Tween translationTween;

	protected Tween scaleTween;

	public void Show(Unit unit, float damage, float health, float delay)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)unit.head == (Object)null))
		{
			((Component)this).gameObject.SetActive(true);
			((Component)this).gameObject.transform.position = unit.head.position;
			bool flag = damage >= health;
			((Component)icon).gameObject.SetActive(flag);
			((Component)label).gameObject.SetActive(!flag);
			((TMP_Text)label).text = Mathf.Round(damage * 0.1f).ToString();
			((Component)container).transform.localPosition = new Vector3(0f, 0f, 0f);
			translationTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(container, 0.2f, 0.5f, false), (Ease)24, 1f), delay);
			((Component)container).transform.localScale = new Vector3(0f, 0f, 1f);
			scaleTween = (Tween)(object)TweenSettingsExtensions.SetDelay<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(container, 1f, 0.7f), (Ease)24), delay);
			GameManager.DelayCall(Mathf.RoundToInt(delay * 1000f), delegate
			{
				AudioManager.PlaySFX(SFXTypes.Snap, 0.5f, 0.7f + Mathf.Min(0.5f, damage * 0.01f));
			});
		}
	}

	public void Hide()
	{
		TweenUtils.KillTween(translationTween);
		translationTween = null;
		TweenUtils.KillTween(scaleTween);
		scaleTween = null;
		((Component)this).gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		Hide();
	}
}
