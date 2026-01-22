using UnityEngine;

public class SettingsToggleContainer : UIBasicComponent
{
	public delegate void ContainerSizeChange(float newSize);

	public TMPLocalizer header;

	[SerializeField]
	protected UIToggleButton button;

	public UIToggleButton Button => button;

	public bool Value
	{
		get
		{
			return Button.Value;
		}
		set
		{
			Button.Value = value;
		}
	}

	public event ContainerSizeChange OnSizeChanged;

	private void OnEnable()
	{
		header.OnTextUpdated += OnTextUpdated;
		button.OnSizeChanged += OnButtonSizeChanged;
	}

	private void OnDisable()
	{
		header.OnTextUpdated -= OnTextUpdated;
		button.OnSizeChanged -= OnButtonSizeChanged;
	}

	private void OnButtonSizeChanged(float newSize)
	{
		UpdateSize();
	}

	private void OnTextUpdated()
	{
		UpdateSize();
	}

	private void UpdateSize()
	{
		float num = Mathf.Max(button.MaxPossibleWidth, 110f);
		base.rectTransform.SetWidth(num);
		this.OnSizeChanged?.Invoke(num);
	}
}
