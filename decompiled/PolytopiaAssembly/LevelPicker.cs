using UnityEngine;

public class LevelPicker : MonoBehaviour
{
	[SerializeField]
	protected PolytopiaSpriteRenderer spriteRenderer;

	[SerializeField]
	protected Sprite[] sprites;

	protected int currLevel;

	public int Level
	{
		get
		{
			return currLevel;
		}
		set
		{
			currLevel = value;
			if (currLevel >= sprites.Length - 1)
			{
				spriteRenderer.Sprite = sprites[sprites.Length - 1];
			}
			else if (currLevel <= 0)
			{
				spriteRenderer.Sprite = sprites[0];
			}
			else
			{
				spriteRenderer.Sprite = sprites[currLevel];
			}
		}
	}

	public PolytopiaSpriteRenderer SpriteRenderer => spriteRenderer;

	public Sprite[] AllSprites => sprites;

	public Sprite GetSprite(int level)
	{
		if (level - 1 > 0 && level - 1 < sprites.Length)
		{
			return sprites[level - 1];
		}
		return null;
	}
}
