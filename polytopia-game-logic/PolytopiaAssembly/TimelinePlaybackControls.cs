using UnityEngine;
using UnityEngine.UI;

public class TimelinePlaybackControls : UIBasicButton
{
	public Image playGlyph;

	public Image pauseGlyph;

	public void SetIsPlaying(bool isPlaying)
	{
		((Component)playGlyph).gameObject.SetActive(!isPlaying);
		((Component)pauseGlyph).gameObject.SetActive(isPlaying);
	}
}
