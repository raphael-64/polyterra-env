using UnityEngine;

public class SegmentConnector : MonoBehaviour
{
	public Unit master;

	public GameObject connector;

	private Vector3 previousDiff;

	private void Update()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)master == (Object)null || (Object)(object)connector == (Object)null)
		{
			connector.SetActive(false);
			return;
		}
		connector.SetActive(true);
		Vector3 val = ((Component)this).transform.position - ((Component)master).transform.position;
		Vector3 val2 = previousDiff - val;
		if (!(((Vector3)(ref val2)).sqrMagnitude < 1E-05f))
		{
			previousDiff = val;
			float num = Mathf.Atan2(val.y, val.x) * 57.29578f;
			connector.transform.rotation = Quaternion.AngleAxis(num, Vector3.forward);
			connector.transform.localScale = new Vector3(((Vector3)(ref val)).magnitude * 1.7f, 1f, 1f);
		}
	}
}
