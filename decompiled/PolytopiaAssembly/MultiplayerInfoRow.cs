using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MultiplayerInfoRow : MonoBehaviour
{
	public TextMeshProUGUI header;

	public TextMeshProUGUI description;

	protected UnityAction<string, string> descriptionLinkCallback;

	public virtual UnityAction<string, string> DescriptionLinkCallback
	{
		get
		{
			return descriptionLinkCallback;
		}
		set
		{
			descriptionLinkCallback = value;
			TMPLinkHelper tMPLinkHelper2 = default(TMPLinkHelper);
			if (descriptionLinkCallback != null)
			{
				TMPLinkHelper tMPLinkHelper = ((Component)description).GetComponent<TMPLinkHelper>();
				if ((Object)(object)tMPLinkHelper == (Object)null)
				{
					tMPLinkHelper = ((Component)description).gameObject.AddComponent<TMPLinkHelper>();
					if (tMPLinkHelper.OnClick == null)
					{
						tMPLinkHelper.OnClick = new TMPLinkHelper.TMPLinkEvent();
					}
				}
				((UnityEvent<string, string>)tMPLinkHelper.OnClick).AddListener(descriptionLinkCallback);
			}
			else if ((Object)(object)description != (Object)null && ((Component)description).TryGetComponent<TMPLinkHelper>(ref tMPLinkHelper2))
			{
				Object.Destroy((Object)(object)tMPLinkHelper2);
			}
		}
	}
}
