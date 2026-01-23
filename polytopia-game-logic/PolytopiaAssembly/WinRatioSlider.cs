using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinRatioSlider : UIBasicComponent
{
	[Header("Avatars")]
	[SerializeField]
	private AvatarView leftAvatarView;

	[SerializeField]
	private AvatarView rightAvatarView;

	[SerializeField]
	private TextMeshProUGUI leftAvatarNameText;

	[SerializeField]
	private TextMeshProUGUI rightAvatarNameText;

	[Header("Slider contents")]
	[SerializeField]
	private RectTransform sliderContainer;

	[SerializeField]
	private Image sliderFillImage;

	[SerializeField]
	private Image sliderBackgroundImage;

	[SerializeField]
	private TextMeshProUGUI leftWinCountText;

	[SerializeField]
	private TextMeshProUGUI rightWinCountText;

	[SerializeField]
	private RectTransform LeftWinCountContainer;

	[SerializeField]
	private RectTransform RightWinCountContainer;

	[Header("Slider colors")]
	[SerializeField]
	private Color biggerWinCountPanelColor = Color.green;

	[SerializeField]
	private Color lowerWinCountPanelColor = Color.red;

	[SerializeField]
	private Color zeroWinCountPanelColor = Color.grey;

	private const float MINIMAL_SLIDER_RATIO = 0.2f;

	private const float MAXIMAL_SLIDER_RATIO = 0.8f;

	public void SetUp(WinRatioSliderData data)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Max(data.leftPlayerWinCount, 0);
		int num2 = Mathf.Max(data.rightPlayerWinCount, 0);
		int num3 = data.leftPlayerWinCount + data.rightPlayerWinCount;
		float num4 = 0f;
		num4 = ((num3 != 0) ? Mathf.Clamp((float)num / (float)num3, 0.2f, 0.8f) : 0.5f);
		Rect rect = sliderContainer.rect;
		float width = ((Rect)(ref rect)).width;
		rect = RightWinCountContainer.rect;
		float num5 = 0f - ((Rect)(ref rect)).height;
		float num6 = width * num4;
		LeftWinCountContainer.anchoredPosition = Vector2.zero;
		LeftWinCountContainer.sizeDelta = new Vector2(num6, num5);
		float num7 = num6;
		float num8 = width - num7;
		RightWinCountContainer.anchoredPosition = Vector2.op_Implicit(new Vector3(num7, 0f));
		RightWinCountContainer.sizeDelta = new Vector2(num8, num5);
		((TMP_Text)leftWinCountText).text = num.ToString();
		((TMP_Text)rightWinCountText).text = num2.ToString();
		((Graphic)sliderFillImage).color = biggerWinCountPanelColor;
		((Graphic)sliderBackgroundImage).color = lowerWinCountPanelColor;
		if (num == 0)
		{
			((Graphic)sliderFillImage).color = zeroWinCountPanelColor;
		}
		if (num2 == 0)
		{
			((Graphic)sliderBackgroundImage).color = zeroWinCountPanelColor;
		}
		leftAvatarView.SetState(data.leftAvatarState);
		((TMP_Text)leftAvatarNameText).text = data.leftAvatarName;
		rightAvatarView.SetState(data.rightAvatarState);
		((TMP_Text)rightAvatarNameText).text = data.rightAvatarName;
	}
}
