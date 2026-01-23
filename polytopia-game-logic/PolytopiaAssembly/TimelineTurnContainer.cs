using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimelineTurnContainer : MonoBehaviour
{
	public RectTransform rectTransform;

	public TimelineTurnSegment timelineTurnSegmentPrefab;

	public TextMeshProUGUI turnLabel;

	private Timeline.TimelineTurnData data;

	private List<TimelineTurnSegment> segments;

	public Timeline.TimelineTurnData GetData()
	{
		return data;
	}

	public void SetData(Timeline.TimelineTurnData data)
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		this.data = data;
		if (segments == null)
		{
			segments = new List<TimelineTurnSegment>(data.players.Count);
		}
		int num = 0;
		for (int i = 0; i < data.players.Count; i++)
		{
			PlayerState playerState = data.players[i];
			if (data.playerCommands.TryGetValue(playerState.Id, out var value) && (playerState.Id != byte.MaxValue || value.Count != 1))
			{
				num++;
				if (segments.Count <= i)
				{
					TimelineTurnSegment item = Object.Instantiate<TimelineTurnSegment>(timelineTurnSegmentPrefab, ((Component)this).transform);
					segments.Add(item);
				}
				((Component)segments[i]).gameObject.SetActive(true);
				segments[i].SetTribe(playerState.tribe);
				segments[i].LoadIcon(SpriteData.GetHeadSpriteAddresses(GameManager.Client.GameState, playerState));
				segments[i].SetColor(data.playerColors[playerState.Id]);
				segments[i].SetType(TimelineTurnSegment.Type.Center);
				float width = Mathf.Max((float)value.Count * 10f, 64f);
				segments[i].SetWidth(width);
			}
		}
		if (num < segments.Count)
		{
			for (int num2 = segments.Count - 1; num2 >= num; num2--)
			{
				((Component)segments[num2]).gameObject.SetActive(false);
			}
		}
		if (num == 1)
		{
			segments[0].SetType(TimelineTurnSegment.Type.Both);
		}
		else
		{
			segments[0].SetType(TimelineTurnSegment.Type.Left);
			segments[num - 1].SetType(TimelineTurnSegment.Type.Right);
		}
		((TMP_Text)turnLabel).text = Localization.Get("replay.turnlabel", data.turn);
		((TMP_Text)turnLabel).transform.SetAsLastSibling();
	}

	public TimelineTurnSegment GetSegment(int index)
	{
		if (segments == null || segments.Count < index)
		{
			return null;
		}
		return segments[index];
	}

	public int GetTurn()
	{
		if (data == null)
		{
			return -1;
		}
		return data.turn;
	}

	public float GetWidth()
	{
		float num = 0f;
		for (int i = 0; i < segments.Count; i++)
		{
			num += segments[i].GetWidth();
		}
		return num;
	}
}
