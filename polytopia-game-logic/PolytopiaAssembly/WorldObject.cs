using System;
using DG.Tweening;
using UnityEngine;

public class WorldObject : MonoBehaviour, IInteractable
{
	[Header("Components")]
	[SerializeField]
	protected PolytopiaSpriteRenderer spriteRenderer;

	protected Tile tile;

	public virtual PlayerState Owner => Tile.Owner;

	public virtual Tile Tile
	{
		get
		{
			return tile;
		}
		set
		{
			tile = value;
			Depth = tile.Depth;
		}
	}

	public virtual Sprite Sprite
	{
		get
		{
			return spriteRenderer.Sprite;
		}
		set
		{
			if ((Object)(object)value == (Object)null)
			{
				Log.Warning("Trying to assign null value sprite", Array.Empty<object>());
			}
			else
			{
				spriteRenderer.Sprite = value;
			}
		}
	}

	public virtual int Depth
	{
		get
		{
			return spriteRenderer.SortingOrder - 2;
		}
		set
		{
			spriteRenderer.SortingOrder = value + 2;
		}
	}

	public PolytopiaSpriteRenderer SpriteRenderer => spriteRenderer;

	public virtual float Value => 0f;

	public virtual void UpdateObject()
	{
	}

	public virtual void SetVisible(bool value)
	{
	}

	public virtual bool GetVisible()
	{
		return false;
	}

	public virtual void Destroy()
	{
		ShortcutExtensions.DOKill((Component)(object)((Component)this).transform, false);
		ShortcutExtensions.DOKill((Component)(object)spriteRenderer, false);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public virtual void OnHoverStart()
	{
	}

	public virtual void OnHoverEnd()
	{
	}

	public virtual void OnPress()
	{
	}

	public virtual void OnRelease()
	{
	}

	public virtual void OnSelected()
	{
	}

	public virtual void OnDeselected()
	{
	}
}
