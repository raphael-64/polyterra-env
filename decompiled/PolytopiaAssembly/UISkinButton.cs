using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UISkinButton : MonoBehaviour
{
	[SerializeField]
	private Image padLockImage;

	[field: SerializeField]
	public UIRoundButton RoundButton { get; private set; }

	[field: SerializeField]
	public SkinType SkinType { get; set; }

	public void SetPadlock(bool visible)
	{
		((Component)padLockImage).gameObject.SetActive(visible);
	}
}
