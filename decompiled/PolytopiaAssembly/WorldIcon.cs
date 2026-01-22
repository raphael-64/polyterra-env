using UnityEngine;
using UnityEngine.UI;

public class WorldIcon : UIWorldScoreBase
{
	[SerializeField]
	protected RectTransform imageContainer;

	[SerializeField]
	protected Image image;

	protected Sprite sprite;

	protected string iconId = string.Empty;

	protected float maxSize = -1f;

	protected float minSize = -1f;

	public override ResourceManager.Type ResourceType
	{
		get
		{
			return base.ResourceType;
		}
		set
		{
			base.ResourceType = value;
			switch (ResourceType)
			{
			case ResourceManager.Type.Score:
				IconIdentifier = "";
				break;
			case ResourceManager.Type.Currency:
			case ResourceManager.Type.Production:
				MinSize = 16f;
				MaxSize = 50f;
				IconIdentifier = "Resource";
				break;
			case ResourceManager.Type.Population:
				MinSize = 16f;
				MaxSize = 50f;
				IconIdentifier = "People";
				break;
			}
		}
	}

	public string IconIdentifier
	{
		get
		{
			return iconId;
		}
		set
		{
			iconId = value;
			Sprite = UIManager.IconData.GetSprite(iconId);
		}
	}

	public Sprite Sprite
	{
		get
		{
			return sprite;
		}
		set
		{
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			sprite = value;
			image.sprite = sprite;
			((Graphic)image).SetNativeSize();
			if ((Object)(object)Sprite != (Object)null)
			{
				if (MaxSize <= 0f || MinSize <= 0f)
				{
					MaxSize = Mathf.Max(((Graphic)image).rectTransform.sizeDelta.x, ((Graphic)image).rectTransform.sizeDelta.y);
				}
				else
				{
					UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
				}
				UpdateSize();
			}
		}
	}

	public float MaxSize
	{
		get
		{
			return maxSize;
		}
		set
		{
			maxSize = value;
		}
	}

	public float MinSize
	{
		get
		{
			return minSize;
		}
		set
		{
			minSize = value;
		}
	}

	public float Size
	{
		get
		{
			float num = 1f - CameraController.NormalizedZoom;
			return Mathf.Lerp(MinSize, MaxSize, num);
		}
	}

	public override void ResetItem()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		base.ResetItem();
		Sprite = null;
		iconId = string.Empty;
		((Transform)imageContainer).localScale = Vector3.one;
		MaxSize = -1f;
		MinSize = -1f;
	}

	protected void UpdateSize()
	{
		UpdateContainerSize();
		if ((Object)(object)Sprite != (Object)null)
		{
			UIUtils.FitImageContentInParent(((Graphic)image).rectTransform);
		}
	}

	protected void UpdateContainerSize()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (MaxSize <= 0f || MinSize <= 0f)
		{
			imageContainer.sizeDelta = new Vector2(100f, 100f);
		}
		else
		{
			imageContainer.sizeDelta = new Vector2(Size, Size);
		}
	}
}
