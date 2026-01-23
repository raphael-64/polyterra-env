using System;
using System.Collections.Generic;
using Polytopia.Data;
using UnityEngine;

public class StylePicker : MonoBehaviour
{
	[SerializeField]
	protected PickerType pickerType;

	[Space(20f)]
	[SerializeField]
	protected SpriteRenderer spriteRenderer;

	[SerializeField]
	protected PolytopiaSpriteRenderer polytopiaSpriteRenderer;

	[Header("Outlines")]
	[SerializeField]
	protected SpriteRenderer outlineRenderer;

	[SerializeField]
	protected PolytopiaSpriteRenderer polytopiaOutlineRenderer;

	protected List<PolytopiaSpriteRenderer> polytopiaSpriteRenderers;

	protected string styleId = "";

	protected SkinType skinType;

	protected string defaultSpriteName = "";

	public SpriteRenderer SpriteRenderer => spriteRenderer;

	public PolytopiaSpriteRenderer PolytopiaSpriteRenderer => polytopiaSpriteRenderer;

	public bool OutlineEnabled
	{
		get
		{
			if ((Object)(object)outlineRenderer != (Object)null)
			{
				return ((Component)outlineRenderer).gameObject.activeSelf;
			}
			if ((Object)(object)polytopiaOutlineRenderer != (Object)null)
			{
				return ((Component)polytopiaOutlineRenderer).gameObject.activeSelf;
			}
			return false;
		}
		set
		{
			if ((Object)(object)outlineRenderer != (Object)null)
			{
				((Component)outlineRenderer).gameObject.SetActive(value);
			}
			else if ((Object)(object)polytopiaOutlineRenderer != (Object)null)
			{
				((Component)polytopiaOutlineRenderer).gameObject.SetActive(value);
			}
		}
	}

	public int Style
	{
		set
		{
			if (!(styleId == value.ToString()))
			{
				styleId = value.ToString();
				SetSprites();
			}
		}
	}

	public void SetStyleAndSkin(string styleId, SkinType skinType)
	{
		if (!(this.styleId == styleId) || this.skinType != skinType)
		{
			this.styleId = styleId;
			this.skinType = skinType;
			defaultSpriteName = GetDefaultSpriteName();
			SetSprites();
		}
	}

	protected string GetDefaultSpriteName()
	{
		if (string.IsNullOrEmpty(defaultSpriteName))
		{
			if ((Object)(object)spriteRenderer != (Object)null)
			{
				if (!Object.op_Implicit((Object)(object)spriteRenderer.sprite))
				{
					return "";
				}
				return ((Object)spriteRenderer.sprite).name;
			}
			if ((Object)(object)polytopiaSpriteRenderer != (Object)null)
			{
				if (!Object.op_Implicit((Object)(object)polytopiaSpriteRenderer.Sprite))
				{
					return "";
				}
				return ((Object)polytopiaSpriteRenderer.Sprite).name;
			}
		}
		return defaultSpriteName;
	}

	protected void SetSprites()
	{
		string skinId = ((pickerType == PickerType.Unit) ? defaultSpriteName : styleId);
		SpriteAddress address = GetAddress(skinId);
		SpriteHandle spriteHandle = new SpriteHandle();
		spriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle2)
		{
			SetSprite(spriteHandle2.sprite);
		});
		if (skinType != SkinType.Default)
		{
			spriteHandle.Request(GetAdresses());
		}
		else
		{
			spriteHandle.Request(address);
		}
		if (!((Object)(object)outlineRenderer != (Object)null) && !((Object)(object)polytopiaOutlineRenderer != (Object)null))
		{
			return;
		}
		SpriteHandle outlineSpriteHandle = new SpriteHandle();
		outlineSpriteHandle.SetCompletion(delegate
		{
			SetOutlineSprite(outlineSpriteHandle.sprite);
		});
		if (skinType != SkinType.Default)
		{
			SpriteAddress[] adresses = GetAdresses();
			for (int num = 0; num < adresses.Length; num++)
			{
				adresses[num].sprite += "_Outline";
			}
			outlineSpriteHandle.Request(adresses);
		}
		else
		{
			SpriteAddress spriteAddress = address;
			spriteAddress.sprite += "_Outline";
			outlineSpriteHandle.Request(spriteAddress);
		}
		void SetOutlineSprite(Sprite sprite)
		{
			if ((Object)(object)outlineRenderer != (Object)null)
			{
				outlineRenderer.sprite = sprite;
			}
			else if ((Object)(object)polytopiaOutlineRenderer != (Object)null)
			{
				polytopiaOutlineRenderer.Sprite = sprite;
			}
		}
		void SetSprite(Sprite sprite)
		{
			if ((Object)(object)spriteRenderer != (Object)null)
			{
				spriteRenderer.sprite = sprite;
			}
			else if ((Object)(object)polytopiaSpriteRenderer != (Object)null)
			{
				polytopiaSpriteRenderer.Sprite = sprite;
			}
		}
	}

	public void SetOutlineColor(Color color)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)outlineRenderer != (Object)null)
		{
			outlineRenderer.color = color;
		}
	}

	public void GetSprite(SkinType skinType, string style, Action<Sprite> action = null)
	{
		SpriteHandle spriteHandle = new SpriteHandle();
		spriteHandle.SetCompletion(delegate(SpriteHandle spriteHandle2)
		{
			action?.Invoke(spriteHandle2.sprite);
		});
		spriteHandle.Request(GetAdresses(skinType, style));
	}

	public List<PolytopiaSpriteRenderer> GetPolytopiaSpriteRenderers()
	{
		if (polytopiaSpriteRenderers == null)
		{
			polytopiaSpriteRenderers = new List<PolytopiaSpriteRenderer>();
			if ((Object)(object)polytopiaOutlineRenderer != (Object)null)
			{
				polytopiaSpriteRenderers.Add(polytopiaOutlineRenderer);
			}
			if ((Object)(object)polytopiaSpriteRenderer != (Object)null)
			{
				polytopiaSpriteRenderers.Add(polytopiaSpriteRenderer);
			}
		}
		return polytopiaSpriteRenderers;
	}

	private SpriteAddress[] GetAdresses(SkinType skinType, string defaultStyle)
	{
		string text = GetDefaultSpriteName();
		if (pickerType != PickerType.Unit)
		{
			return new SpriteAddress[2]
			{
				GetAddress(skinType.GetName()),
				GetAddress(defaultStyle)
			};
		}
		return new SpriteAddress[2]
		{
			GetAddress(text + "_" + skinType.GetName()),
			GetAddress(text)
		};
	}

	private SpriteAddress[] GetAdresses()
	{
		if (pickerType != PickerType.Unit)
		{
			return new SpriteAddress[2]
			{
				GetAddress(skinType.GetName()),
				GetAddress(styleId)
			};
		}
		return new SpriteAddress[2]
		{
			GetAddress(defaultSpriteName + "_" + skinType.GetName()),
			GetAddress(defaultSpriteName)
		};
	}

	private SpriteAddress GetAddress(string skinId)
	{
		return SpriteData.GetAddress(pickerType, skinId);
	}
}
