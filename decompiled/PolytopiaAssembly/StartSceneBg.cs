using UnityEngine;

public class StartSceneBg : MonoBehaviour
{
	[SerializeField]
	protected Camera cam;

	[SerializeField]
	protected SpriteRenderer nature;

	[SerializeField]
	protected Transform gradientTransform;

	[SerializeField]
	protected SpriteRenderer gradientBgSprite;

	[Header("Stars")]
	[SerializeField]
	protected Transform starContainer;

	[SerializeField]
	protected float starDensity;

	[SerializeField]
	protected float starFieldHeight;

	[SerializeField]
	protected float starScale = 1f;

	[Header("Offsets")]
	[SerializeField]
	protected float natureMinScale = 0.65f;

	[SerializeField]
	protected Vector3 positionOffset;

	[SerializeField]
	protected float bgScaleMultiplier = 1f;

	[SerializeField]
	protected float natureWaterRatio = 0.8f;

	[Header("Prefabs")]
	[SerializeField]
	protected SpriteRenderer starPrefab;

	protected Bounds cameraBounds;

	protected float lastStarFieldHeight;

	protected float lastStarDensity;

	private Vector2 natureSize;

	private float brightnessAnimationStart = 1f;

	private static bool isBright = true;

	public static StartSceneBg instance;

	public static bool Bright
	{
		set
		{
			if (Object.op_Implicit((Object)(object)instance) && isBright != value)
			{
				isBright = value;
				instance.brightnessAnimationStart = Time.time;
			}
		}
	}

	private void Start()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		instance = this;
		brightnessAnimationStart = Time.time - 10f;
		Bounds bounds = ((Renderer)nature).bounds;
		natureSize = Vector2.op_Implicit(((Bounds)(ref bounds)).size);
		lastStarFieldHeight = starFieldHeight;
		lastStarDensity = starDensity;
		cameraBounds = cam.OrthographicBounds();
		PopulateStars();
		RefreshLayout();
	}

	private void OnEnable()
	{
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
	}

	private void OnDisable()
	{
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		cameraBounds = cam.OrthographicBounds();
		RefreshLayout();
		PopulateStars();
	}

	private void Update()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		if (lastStarFieldHeight != starFieldHeight || lastStarDensity != starDensity)
		{
			lastStarFieldHeight = starFieldHeight;
			lastStarDensity = starDensity;
			PopulateStars();
		}
		float num = Mathf.Min(1f, (Time.time - brightnessAnimationStart) / 0.5f);
		if (isBright)
		{
			num = 1f - num;
		}
		num = Mathf.SmoothStep(0f, 1f, num);
		nature.color = Color.Lerp(new Color(1f, 1f, 1f), new Color(0.7f, 0.7f, 0.7f), num);
		gradientBgSprite.color = Color.Lerp(new Color(1f, 1f, 1f), new Color(1f, 0.8f, 0.8f), num);
	}

	private void PopulateStars()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.RoundToInt(((Bounds)(ref cameraBounds)).size.x * starFieldHeight * starDensity);
		int childCount = starContainer.childCount;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(((Bounds)(ref cameraBounds)).size.x * 0.5f, starFieldHeight * 0.5f);
		for (int i = 0; i < Mathf.Max(num, childCount); i++)
		{
			Transform val2 = ((childCount > i) ? starContainer.GetChild(i) : null);
			SpriteRenderer val3;
			if ((Object)(object)val2 != (Object)null)
			{
				val3 = ((Component)val2).GetComponent<SpriteRenderer>();
			}
			else
			{
				val3 = Object.Instantiate<SpriteRenderer>(starPrefab, starContainer);
				((Renderer)val3).sortingOrder = 1;
			}
			((Component)val3).transform.localPosition = new Vector3(Random.Range(0f - val.x, val.x), Random.Range(0f - val.y, val.y), 0f);
			((Component)val3).transform.localScale = new Vector3(starScale, starScale, 1f);
			((Component)val3).gameObject.SetActive(i < num);
		}
	}

	private void RefreshLayout()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Max(natureMinScale, ((Bounds)(ref cameraBounds)).size.x / natureSize.x);
		((Component)nature).transform.localScale = new Vector3(num, num, 1f);
		((Component)nature).transform.position = ((Bounds)(ref cameraBounds)).min + positionOffset;
		float num2 = num * natureSize.y * natureWaterRatio;
		float num3 = ((Bounds)(ref cameraBounds)).size.y - num2;
		gradientTransform.position = ((Bounds)(ref cameraBounds)).center + positionOffset + new Vector3(0f, (0f - ((Bounds)(ref cameraBounds)).size.y) * 0.5f + num3 * 0.5f + num2, 0f);
		gradientTransform.localScale = new Vector3(((Bounds)(ref cameraBounds)).size.x * bgScaleMultiplier, num3 * bgScaleMultiplier, 1f);
	}
}
