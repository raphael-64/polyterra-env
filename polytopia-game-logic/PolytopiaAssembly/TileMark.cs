using UnityEngine;

public class TileMark : MonoBehaviour
{
	public void Show(Tile tile)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Component)this).gameObject.transform.position = ((Component)tile).transform.position + (Vector3)((tile.Data.IsWater && !tile.IsHidden) ? new Vector3(0f, -0.067f, 0f) : Vector3.zero);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}
}
