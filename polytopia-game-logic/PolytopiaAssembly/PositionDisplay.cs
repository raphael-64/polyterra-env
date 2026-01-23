using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PositionDisplay : UIBasicComponent
{
	[SerializeField]
	protected TextMeshProUGUI label;

	[SerializeField]
	protected Image bg;

	protected string[] labels;

	private void Awake()
	{
		SetLocalizedLabels();
	}

	public override void Init()
	{
		base.Init();
		((Component)this).gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		GameEvents.OnPassPlayer += OnPassPlayer;
		GameEvents.OnTurnEnded += OnTurnEnded;
		GameEvents.OnFinishedProcessing += OnFinishedProcessing;
		LocalizationEvents.OnLocalizationUpdated += OnLocalizationUpdated;
		OnLocalizationUpdated();
	}

	private void OnDisable()
	{
		GameEvents.OnPassPlayer -= OnPassPlayer;
		GameEvents.OnTurnEnded -= OnTurnEnded;
		GameEvents.OnFinishedProcessing -= OnFinishedProcessing;
		LocalizationEvents.OnLocalizationUpdated -= OnLocalizationUpdated;
	}

	private void OnPassPlayer()
	{
		UpdateLabel();
	}

	private void OnTurnEnded()
	{
		UpdateLabel();
	}

	private void OnFinishedProcessing()
	{
		UpdateLabel();
	}

	private void SetLocalizedLabels()
	{
		labels = Localization.Get("world.ranks").Split(',');
	}

	private void OnLocalizationUpdated()
	{
		SetLocalizedLabels();
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (GameManager.GameState == null || GameManager.LocalPlayer == null)
		{
			return;
		}
		List<PlayerState> playersSortedByRank = GameManager.GameState.GetPlayersSortedByRank();
		int num = -1;
		int count = playersSortedByRank.Count;
		for (int i = 0; i < count; i++)
		{
			if (playersSortedByRank[i].Id != byte.MaxValue)
			{
				num = i;
				if (playersSortedByRank[i] == GameManager.LocalPlayer)
				{
					break;
				}
			}
		}
		if (num >= 0)
		{
			if (num >= labels.Length)
			{
				((TMP_Text)label).text = num.ToString();
			}
			else
			{
				((TMP_Text)label).text = labels[num];
			}
			((Graphic)bg).color = ((num == 0) ? ColorConstants.green : ColorConstants.blue);
			base.rectTransform.SetWidth(((TMP_Text)label).preferredWidth + 4f);
		}
	}
}
