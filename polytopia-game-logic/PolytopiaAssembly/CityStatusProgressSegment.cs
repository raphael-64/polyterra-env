using UnityEngine;

public class CityStatusProgressSegment : MonoBehaviour
{
	public enum Type
	{
		Left,
		Middle,
		Right
	}

	[SerializeField]
	protected SpriteRenderer background;

	[SerializeField]
	protected SpriteRenderer dot;

	[SerializeField]
	protected Sprite segment;

	[SerializeField]
	protected Sprite leftEdge;

	[SerializeField]
	protected Sprite rightEdge;

	private Type type;

	private float width;

	public void SetType(Type type)
	{
		this.type = type;
	}

	public void SetWidth(float width)
	{
		this.width = width;
	}

	public void SetPosition(float position)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = new Vector3(position + width * 0.5f, 0f, 0f);
	}

	public void SetColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		background.color = color;
	}

	public void SetDot(bool visible, Color color)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		((Component)dot).gameObject.SetActive(visible);
		dot.color = color;
	}

	public void Render()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0f, 0.01f);
		switch (type)
		{
		case Type.Left:
			background.sprite = leftEdge;
			val.x = 0.01f;
			break;
		case Type.Middle:
			background.sprite = segment;
			break;
		case Type.Right:
			background.sprite = rightEdge;
			val.x = -0.01f;
			break;
		}
		((Component)dot).transform.localPosition = Vector2.op_Implicit(val);
		background.size = Vector2.op_Implicit(new Vector3(width, 0.16f));
	}
}
