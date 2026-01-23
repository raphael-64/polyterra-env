using Polytopia.Data;
using UnityEngine;
using UnityEngine.UI;

public class UIBuildingRenderer : MonoBehaviour
{
	protected RectTransform m_rectTransform;

	private SpriteHandle spriteHandle = new SpriteHandle();

	public Sprite sprite
	{
		get
		{
			Image component = ((Component)this).GetComponent<Image>();
			if ((Object)(object)component != (Object)null)
			{
				return component.sprite;
			}
			return null;
		}
		set
		{
			Image obj = ((Component)this).gameObject.AddComponent<Image>();
			obj.sprite = value;
			((Graphic)obj).SetNativeSize();
			obj.useSpriteMesh = true;
		}
	}

	public RectTransform rectTransform => m_rectTransform;

	protected void Awake()
	{
		spriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle)
		{
			sprite = spriteHandle.sprite;
		});
	}

	public void SetBuilding(Building building)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (building is City && building.Owner != null)
		{
			AddRectTransform();
			City city = building as City;
			UIDuplicatedSprites container = ((Component)this).gameObject.AddComponent<UIDuplicatedSprites>();
			UIManager.Instance.SpriteDuplicator.RenderSprites(((Component)city.cityRenderer).transform, container);
		}
		else
		{
			AddRectTransform();
			sprite = building.Sprite;
		}
	}

	public void SetBuildingData(ImprovementData data, SkinType skinType, int climate)
	{
		AddRectTransform();
		spriteHandle.Request(SpriteData.GetBuildingSpriteAddresses(data.type, skinType, climate));
	}

	protected void AddRectTransform()
	{
		m_rectTransform = ((Component)this).gameObject.AddComponent<RectTransform>();
	}

	public static UIBuildingRenderer GetInstance()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return new GameObject
		{
			name = "BuildingRenderer"
		}.AddComponent<UIBuildingRenderer>();
	}
}
