using TMPro;
using UnityEngine;

public class ResourceContainerBase : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI headerLabel;

	[SerializeField]
	protected TextMeshProUGUI amountLabel;

	[SerializeField]
	protected float minWidth = 100f;

	public float width
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return base.rectTransform.sizeDelta.x;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			Vector2 sizeDelta = base.rectTransform.sizeDelta;
			sizeDelta.x = Mathf.Max(value, minWidth);
			base.rectTransform.sizeDelta = sizeDelta;
		}
	}

	private void OnEnable()
	{
		LocalizationEvents.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDisable()
	{
		LocalizationEvents.OnLanguageChanged -= OnLanguageChanged;
	}

	protected virtual void OnLanguageChanged(Localization.Languages language)
	{
	}
}
