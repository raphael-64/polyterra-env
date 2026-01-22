using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIToggleButton : UITextButton
{
	public delegate void ToggleValueChange(bool value);

	[Serializable]
	public class ToggleData
	{
		public string labelKey = string.Empty;

		public Color color = Color.white;
	}

	[Header("Toggle Button")]
	[SerializeField]
	protected bool value = true;

	[SerializeField]
	protected ToggleData OnData;

	[SerializeField]
	protected ToggleData OffData;

	public bool Value
	{
		get
		{
			return value;
		}
		set
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			this.value = value;
			ToggleData toggleData = (this.value ? OnData : OffData);
			base.Key = toggleData.labelKey;
			bgColorStates.defaultColor = toggleData.color;
			UpdateColors();
			this.OnValueChanged?.Invoke(this.value);
		}
	}

	public float MaxPossibleWidth
	{
		get
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			float x = ((TMP_Text)labelLocalizer.TextComponent).GetPreferredValues(Localization.Get(OnData.labelKey)).x;
			float x2 = ((TMP_Text)labelLocalizer.TextComponent).GetPreferredValues(Localization.Get(OffData.labelKey)).x;
			return Mathf.Max(x, x2) + margin * 2f;
		}
	}

	public event ToggleValueChange OnValueChanged;

	public override void Start()
	{
		base.Start();
		Value = value;
	}

	public override void PointerUp(PointerEventData eventData)
	{
		if (buttonState != ButtonStates.None)
		{
			Value = !Value;
		}
		base.PointerUp(eventData);
	}

	public override void OnSubmit(BaseEventData data)
	{
		Value = !Value;
		base.OnSubmit(data);
	}
}
