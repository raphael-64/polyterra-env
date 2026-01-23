using UnityEngine;

public class SpriteHandle
{
	public SpriteAddress address;

	public Sprite sprite;

	private SpriteHandleCallback completion;

	private bool isCompleted;

	public void SetCompletion(SpriteHandleCallback completion)
	{
		this.completion = completion;
		if (isCompleted)
		{
			completion?.Invoke(this);
		}
	}

	public void Complete(string atlasName, string spriteName, Sprite sprite)
	{
		if ((!(atlasName != address.atlas) || !(spriteName != address.sprite)) && !((Object)(object)sprite == (Object)null))
		{
			isCompleted = true;
			this.sprite = sprite;
			completion?.Invoke(this);
		}
	}

	public void Request(SpriteAddress spriteAddress)
	{
		sprite = null;
		address = spriteAddress;
		isCompleted = false;
		GameManager.GetSpriteAtlasManager().LoadSprite(this);
	}

	public void Request(SpriteAddress[] spriteAddresses)
	{
		sprite = null;
		address = default(SpriteAddress);
		isCompleted = false;
		GameManager.GetSpriteAtlasManager().LoadSprite(this, spriteAddresses);
	}
}
