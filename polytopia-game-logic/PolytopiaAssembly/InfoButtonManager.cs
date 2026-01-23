using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class InfoButtonManager : UIBasicComponent
{
	[Serializable]
	public class TribeButtonsData
	{
		public UIBasicComponent UIComponent;

		public List<TribeInfoButton> tribeInfoButtonsList = new List<TribeInfoButton>();
	}

	[Header("Info Button Manager")]
	[SerializeField]
	private List<TribeButtonsData> tribeButtonsDataList = new List<TribeButtonsData>();

	[Header("Prefabs")]
	[SerializeField]
	private TribeInfoButton tribeInfoButtonPrefab;

	public static InfoButtonManager Instance { get; private set; }

	public override void Init()
	{
		base.Init();
		Instance = this;
		((Component)this).gameObject.SetActive(true);
	}

	public void SetTribeInfoButtons(UIBasicComponent basicComponent, TextMeshProUGUI textMeshPro, TribeInfoButtonData tribeInfoButtonData)
	{
		if (((TMP_Text)textMeshPro).textInfo != null)
		{
			((TMP_Text)textMeshPro).textInfo.Clear();
		}
		TribeButtonsData tribeButtonsData = tribeButtonsDataList.FirstOrDefault((TribeButtonsData x) => (Object)(object)x.UIComponent == (Object)(object)basicComponent);
		if (tribeButtonsData == null)
		{
			tribeButtonsData = new TribeButtonsData
			{
				UIComponent = basicComponent
			};
			tribeButtonsDataList.Add(tribeButtonsData);
		}
		((MonoBehaviour)this).StartCoroutine(WaitUntilText(tribeButtonsData, textMeshPro, tribeInfoButtonData));
	}

	protected IEnumerator WaitUntilText(TribeButtonsData tribeButtonsData, TextMeshProUGUI textMeshPro, TribeInfoButtonData tribeInfoButtonData)
	{
		while (((TMP_Text)textMeshPro).textInfo == null || (((TMP_Text)textMeshPro).textInfo.characterCount == 0 && ((TMP_Text)textMeshPro).text.Length > 0))
		{
			yield return null;
		}
		TMP_LinkInfo[] linkInfo = ((TMP_Text)textMeshPro).textInfo.linkInfo;
		for (int i = 0; i < ((TMP_Text)textMeshPro).textInfo.linkCount; i++)
		{
			TMP_LinkInfo linkInfo2 = linkInfo[i];
			SpawnTribeButton(tribeButtonsData, linkInfo2, textMeshPro, tribeInfoButtonData);
		}
	}

	protected TribeInfoButton SpawnTribeButton(TribeButtonsData tribeButtonsData, TMP_LinkInfo linkInfo, TextMeshProUGUI textMesh, TribeInfoButtonData tribeInfoButtonData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		Bounds linkBounds = textMesh.GetLinkBounds(linkInfo);
		TribeInfoButton tribeInfoButton = Object.Instantiate<TribeInfoButton>(tribeInfoButtonPrefab, (Transform)(object)((TMP_Text)textMesh).rectTransform);
		float scaleFactor = UIManager.CanvasScaler.scaleFactor;
		float num = ((Transform)((TMP_Text)textMesh).rectTransform).lossyScale.x / scaleFactor;
		Vector3 center = ((Bounds)(ref linkBounds)).center;
		center.x -= 19.5f * num * scaleFactor;
		if (tribeInfoButtonData.adjustPosition)
		{
			center.y += ((TMP_Text)textMesh).fontSize * 0.1f * num * scaleFactor;
		}
		Vector3 size = ((Bounds)(ref linkBounds)).size;
		size /= num;
		size /= scaleFactor;
		size.x += 39f;
		size.y = tribeInfoButton.rectTransform.sizeDelta.y;
		((Transform)tribeInfoButton.rectTransform).position = center;
		tribeInfoButton.rectTransform.sizeDelta = Vector2.op_Implicit(size);
		tribeInfoButton.text = ((TMP_LinkInfo)(ref linkInfo)).GetLinkText();
		byte tribeID = ((TMP_LinkInfo)(ref linkInfo)).GetLinkID().GetTribeID();
		GameManager.GameState.TryGetPlayer(tribeID, out var playerState);
		tribeInfoButton.SetOwner(playerState);
		((Object)tribeInfoButton).name = "TribeInfoButton_" + ((TMP_LinkInfo)(ref linkInfo)).GetLinkText();
		tribeButtonsData.tribeInfoButtonsList.Add(tribeInfoButton);
		return tribeInfoButton;
	}

	public void DestroyInfoButtons(UIBasicComponent basicComponent)
	{
		TribeButtonsData tribeButtonsData = tribeButtonsDataList.FirstOrDefault((TribeButtonsData x) => (Object)(object)x.UIComponent == (Object)(object)basicComponent);
		if (tribeButtonsData != null)
		{
			for (int num = 0; num < tribeButtonsData.tribeInfoButtonsList.Count; num++)
			{
				Object.Destroy((Object)(object)((Component)tribeButtonsData.tribeInfoButtonsList[num]).gameObject);
			}
			tribeButtonsData.tribeInfoButtonsList.Clear();
		}
	}
}
