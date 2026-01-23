using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CMConnectInfoRow : MonoBehaviour
{
	[SerializeField]
	protected UIButtonBase mobile;

	[SerializeField]
	protected UIButtonBase desktop;

	public TextMeshProUGUI description;

	private void Awake()
	{
		((Component)desktop).gameObject.SetActive(true);
		((Component)mobile).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		mobile.OnClicked += OnHeaderIconClicked;
		desktop.OnClicked += OnHeaderIconClicked;
	}

	private void OnDisable()
	{
		mobile.OnClicked -= OnHeaderIconClicked;
		desktop.OnClicked -= OnHeaderIconClicked;
	}

	private void OnHeaderIconClicked(int id, BaseEventData eventData)
	{
		NativeHelpers.OpenURL("https://www.challengermode.com/portal?game=polytopia");
	}
}
