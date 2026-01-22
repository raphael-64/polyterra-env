using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CityStatusNameContainer : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer bg;

	[Space(5f)]
	[SerializeField]
	protected TextMeshPro label;

	[Space(5f)]
	[SerializeField]
	protected Transform LeftIconContainer;

	[SerializeField]
	protected SpriteRenderer leftIcon;

	[SerializeField]
	protected SpriteRenderer leftIconBackground;

	[Space(5f)]
	[SerializeField]
	protected Transform workContainer;

	[SerializeField]
	protected TextMeshPro workLabel;

	[SerializeField]
	protected SpriteRenderer workIcon;

	[Space(5f)]
	[SerializeField]
	protected Transform riotIconContainer;

	[SerializeField]
	protected SpriteRenderer riotIcon;

	[Space(5f)]
	[SerializeField]
	protected Transform contentTransform;

	protected City city;

	private bool isDirty;

	private bool isLabelDirty = true;

	private bool isWorkLabelDirty = true;

	public void OnEnable()
	{
		if (isDirty)
		{
			UpdateSize();
		}
	}

	public void SetCity(City city)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		this.city = city;
		FontStyles val = (FontStyles)(city.IsCapital ? 4 : 0);
		if (((TMP_Text)label).text != city.State.name || ((TMP_Text)label).fontStyle != val)
		{
			((TMP_Text)label).text = city.State.name;
			((TMP_Text)label).fontStyle = val;
			isLabelDirty = true;
		}
		bool flag = city.Tile.Data.improvement.HasEffect(ImprovementEffect.robbed);
		((Graphic)label).color = (flag ? ColorConstants.red : Color.white);
		bg.color = ColorUtil.SetAlphaOnColor(this.city.Owner.GetPlayerColor(GameManager.GameState), bg.color.a);
		bool num = GameManager.IsPlayerViewing(this.city.Owner.Id);
		bool flag2 = GameManager.IsPlayerViewing(this.city.State.connectedToCapitalOfPlayer);
		PlayerState localPlayer = GameManager.LocalPlayer;
		bool flag3 = localPlayer.HasEmbassyWith(this.city.Owner) && this.city.IsCapitalOf(this.city.Owner.Id);
		bool flag4 = localPlayer.HasActiveEmbassyWith(this.city.Owner, GameManager.GameState);
		bool flag5 = (num && (this.city.IsCapitalOf(city.Owner.Id) || flag2)) || flag3;
		((Component)LeftIconContainer).gameObject.SetActive(flag5);
		if (flag5)
		{
			if (flag3)
			{
				leftIcon.sprite = UIManager.IconData.GetSprite("embassy");
				leftIconBackground.color = (flag4 ? ColorConstants.blue : Color.black);
			}
			else if (this.city.IsCapitalOf(this.city.Owner.Id))
			{
				leftIcon.sprite = UIManager.IconData.GetSprite("Capital");
				leftIconBackground.color = ColorConstants.blue;
			}
			else if (flag2)
			{
				leftIcon.sprite = UIManager.IconData.GetSprite("Roads");
				leftIconBackground.color = ColorConstants.blue;
			}
			Rect rect = leftIcon.sprite.rect;
			Vector2 size = ((Rect)(ref rect)).size;
			rect = leftIconBackground.sprite.rect;
			float num2 = ((Rect)(ref rect)).size.x * leftIcon.sprite.pixelsPerUnit / (size.x * leftIconBackground.sprite.pixelsPerUnit) * ((Component)leftIconBackground).transform.localScale.x;
			((Component)leftIcon).transform.localScale = new Vector3(num2, num2, 1f);
		}
		if (this.city.Tile.Data.improvement != null && !flag)
		{
			int work = this.city.Tile.Data.CalculateWork(GameManager.GameState, localPlayer, this.city.Tile.Data.improvement.level);
			SetWork(work);
		}
		else
		{
			((Component)workContainer).gameObject.SetActive(false);
		}
		((Component)riotIconContainer).gameObject.SetActive(flag);
		UpdateSize();
	}

	private void SetWork(int work)
	{
		((Component)workContainer).gameObject.SetActive(work > 0);
		string text = work.ToString();
		if (text != ((TMP_Text)workLabel).text)
		{
			((TMP_Text)workLabel).text = text;
			isWorkLabelDirty = true;
		}
	}

	protected void UpdateSize()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)this).gameObject.activeInHierarchy)
		{
			isDirty = true;
			return;
		}
		isDirty = false;
		float num = 0f;
		Bounds bounds = ((Renderer)leftIconBackground).bounds;
		float x = ((Bounds)(ref bounds)).size.x;
		float num2 = 0.012f;
		if (((Component)LeftIconContainer).gameObject.activeSelf)
		{
			num += x + num2;
			Vector3 localPosition = ((Component)leftIcon).transform.localPosition;
			localPosition.x = num2 + x * 0.5f;
			((Component)LeftIconContainer).transform.localPosition = localPosition;
		}
		if (isLabelDirty)
		{
			((TMP_Text)label).ForceMeshUpdate(false, false);
			isLabelDirty = false;
		}
		bounds = ((TMP_Text)label).bounds;
		float num3 = ((Bounds)(ref bounds)).size.x * label.transform.localScale.x;
		float num4 = 0.03f;
		Vector3 localPosition2 = label.transform.localPosition;
		localPosition2.x = num + num4;
		label.transform.localPosition = localPosition2;
		num += num4 + num3;
		float num5 = 0.012f;
		if (((Component)workContainer).gameObject.activeSelf)
		{
			float num6 = 0.015f;
			Vector3 localPosition3 = workContainer.localPosition;
			localPosition3.x = num + num6;
			workContainer.localPosition = localPosition3;
			if (isWorkLabelDirty)
			{
				((TMP_Text)workLabel).ForceMeshUpdate(false, false);
				isWorkLabelDirty = false;
			}
			bounds = ((Renderer)workIcon).bounds;
			float x2 = ((Bounds)(ref bounds)).size.x;
			bounds = ((TMP_Text)workLabel).bounds;
			float num7 = ((Bounds)(ref bounds)).size.x * workLabel.transform.localScale.x;
			num += x2 + num7 + num5 + num6;
		}
		else
		{
			num += num4;
		}
		if (((Component)riotIconContainer).gameObject.activeSelf)
		{
			float num8 = 0.08f;
			Vector3 localPosition4 = riotIconContainer.localPosition;
			localPosition4.x = num + num8;
			riotIconContainer.localPosition = localPosition4;
			num += num5 + 2f * num8;
		}
		Vector3 localPosition5 = contentTransform.localPosition;
		localPosition5.x = (0f - num) * 0.5f;
		contentTransform.localPosition = localPosition5;
		Vector3 localScale = ((Component)bg).transform.localScale;
		localScale.x = num * 26.4f;
		((Component)bg).transform.localScale = localScale;
	}
}
