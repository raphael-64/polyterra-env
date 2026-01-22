using UnityEngine;

public class DebugInfoPopup : BasicPopup
{
	[Header("Debug Info Popup")]
	[SerializeField]
	private DebugContainer debugContainer;

	public override void Init()
	{
		debugContainer.Initialize();
		base.Init();
	}

	public override void Show()
	{
		debugContainer.UpdateScreenSizeData();
		base.Show();
	}
}
