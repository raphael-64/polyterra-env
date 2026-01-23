using UnityEngine;

public class UIWorldIconContainer : UIBasicComponent
{
	protected static UIWorldIconContainer instance;

	public override void Init()
	{
		base.Init();
		instance = this;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public static HintIcon GetHintIcon(WorldCoordinates coordinate)
	{
		HintIcon hintIcon = GetHintIcon();
		hintIcon.Coordinates = coordinate;
		return hintIcon;
	}

	public static HintIcon GetHintIcon(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		HintIcon hintIcon = GetHintIcon();
		hintIcon.Position = position;
		return hintIcon;
	}

	public static HintIcon GetHintIcon(RectTransform uiElement)
	{
		HintIcon hintIcon = GetHintIcon();
		hintIcon.UIElement = uiElement;
		return hintIcon;
	}

	protected static HintIcon GetHintIcon()
	{
		HintIcon pooledObject = ObjectPool.GetPooledObject<HintIcon>("HintIcon");
		((Component)pooledObject).transform.SetParent(((Component)instance).transform, false);
		return pooledObject;
	}
}
