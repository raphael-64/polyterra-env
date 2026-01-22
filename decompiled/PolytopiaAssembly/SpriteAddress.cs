public struct SpriteAddress
{
	public string atlas;

	public string sprite;

	public SpriteAddress(string atlas, string sprite)
	{
		this.atlas = atlas;
		this.sprite = sprite;
	}

	public override string ToString()
	{
		return atlas + " " + sprite;
	}
}
