using UnityEngine;
using UnityEngine.EventSystems;

public class PolytopiaInputModule : StandaloneInputModule
{
	[SerializeField]
	private Canvas[] canvases = (Canvas[])(object)new Canvas[0];

	public override void Process()
	{
		try
		{
			PolytopiaInput.Omnicursor.UpdateControllerOverridePosition(((BaseInputModule)this).eventSystem, canvases);
		}
		finally
		{
		}
		((StandaloneInputModule)this).Process();
	}

	public PointerEventData GetPointerEventData(int id)
	{
		((PointerInputModule)this).m_PointerData.TryGetValue(id, out var value);
		return value;
	}
}
