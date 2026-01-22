using UnityEngine;

public class SpriteDuplicatorTest : MonoBehaviour
{
	public UIDuplicatedSprites spriteDuplicator;

	public Transform sprite;

	private void Start()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		UIManager.Instance.SpriteDuplicator.RenderSprites(sprite, spriteDuplicator, 1.3846f);
	}
}
