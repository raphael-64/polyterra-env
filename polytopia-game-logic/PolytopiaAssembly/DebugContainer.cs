using UnityEngine;

public class DebugContainer : MonoBehaviour
{
	public UnitStatsRow buildVersionRow;

	public UnitStatsRow osVersionRow;

	public UnitStatsRow deviceRow;

	public UnitStatsRow dpiRow;

	public UnitStatsRow screenSizeRow;

	public void Initialize()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		buildVersionRow.SetData("credits.build", VersionManager.SemanticVersion.ToString());
		string operatingSystem = SystemInfo.operatingSystem;
		osVersionRow.SetData("credits.os", operatingSystem);
		dpiRow.SetData("credits.screendpi", Mathf.RoundToInt(ScalingUtils.GetDPI()));
		if ((Object)(object)deviceRow != (Object)null)
		{
			deviceRow.SetData("credits.device", SystemInfo.deviceModel);
		}
		UpdateScreenSizeData();
		Transform transform = ((Component)this).transform;
		UpdateSizes(((RectTransform)((transform is RectTransform) ? transform : null)).sizeDelta.x);
	}

	public void UpdateSizes(float width)
	{
		buildVersionRow.UpdateSize(width);
		osVersionRow.UpdateSize(width);
		deviceRow.UpdateSize(width);
		dpiRow.UpdateSize(width);
		screenSizeRow.UpdateSize(width);
	}

	public void UpdateScreenSizeData()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Vector2Int.op_Implicit(NativeHelpers.Screen());
		float num = val.x / ScalingUtils.GetDPI();
		float num2 = val.y / ScalingUtils.GetDPI();
		float num3 = num * num + num2 * num2;
		screenSizeRow.SetData("credits.screensize", $"{val.x}x{val.y}, {Mathf.Round(10f * Mathf.Sqrt(num3)) / 10f:F1}\"");
		UnitStatsRow unitStatsRow = screenSizeRow;
		Transform transform = ((Component)this).transform;
		unitStatsRow.UpdateSize(((RectTransform)((transform is RectTransform) ? transform : null)).sizeDelta.x);
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		UpdateScreenSizeData();
	}
}
