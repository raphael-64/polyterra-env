using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceWidget : UIBasicComponent
{
	public Image icon;

	public TextMeshProUGUI label;

	public ResourceManager.Type type = ResourceManager.Type.Currency;

	public bool AllowUpdateColors = true;

	[SerializeField]
	protected HorizontalLayoutGroup layoutGroup;

	protected float amount;

	protected bool warnIfLow = true;

	public float Amount
	{
		get
		{
			return amount;
		}
		set
		{
			if (amount != value)
			{
				amount = value;
				((TMP_Text)label).text = LocalizationUtils.FormatNumber(amount);
				UpdateColors();
				ResizeComponent();
			}
		}
	}

	public bool WarnIfLow
	{
		get
		{
			return warnIfLow;
		}
		set
		{
			warnIfLow = value;
			UpdateColors();
		}
	}

	private void OnEnable()
	{
		((TMP_Text)label).autoSizeTextContainer = true;
		UpdateColors();
		ResourceEvents.OnResourceChanged += OnResourceChanged;
	}

	private void OnDisable()
	{
		ResourceEvents.OnResourceChanged -= OnResourceChanged;
	}

	private void OnResourceChanged(byte playerId)
	{
		if (playerId == GameManager.LocalPlayer.Id)
		{
			UpdateColors();
		}
	}

	protected void ResizeComponent()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = base.rectTransform.sizeDelta;
		TextMeshProUGUI obj = label;
		string text = ((TMP_Text)label).text;
		Rect rect = ((TMP_Text)label).rectTransform.rect;
		float width = ((Rect)(ref rect)).width;
		rect = ((TMP_Text)label).rectTransform.rect;
		Vector2 preferredValues = ((TMP_Text)obj).GetPreferredValues(text, width, ((Rect)(ref rect)).height);
		((TMP_Text)label).rectTransform.sizeDelta = preferredValues;
		sizeDelta.x = ((Graphic)icon).rectTransform.sizeDelta.x + ((HorizontalOrVerticalLayoutGroup)layoutGroup).spacing + preferredValues.x;
		base.rectTransform.sizeDelta = sizeDelta;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.rectTransform);
	}

	protected void UpdateColors()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (AllowUpdateColors)
		{
			if (WarnIfLow && GameManager.Client != null && GameManager.LocalPlayer != null)
			{
				((Graphic)label).color = (ResourceManager.HaveEnoughResources(GameManager.LocalPlayer.Id, type, Amount) ? Color.white : ColorConstants.red);
			}
			else
			{
				((Graphic)label).color = Color.white;
			}
		}
	}
}
