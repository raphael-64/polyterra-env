using UnityEngine;

public class BgStarContainer : MonoBehaviour
{
	public Camera cam;

	public int starCount = 100;

	public float margin = 10f;

	[Tooltip("Closer to 0 the stars will follow the world, closer to 1 the stars will follow the camera")]
	[Range(0f, 1f)]
	public float moveMultiplier = 0.1f;

	[Header("Prefabs")]
	public GameObject starPrefab;

	[Header("Debug")]
	public bool showBounds;

	protected static BgStarContainer instance;

	private bool movementEnabled = true;

	private Vector3 worldCenter;

	private float originalCameraSize;

	private GameObject[] stars;

	private void Awake()
	{
		instance = this;
		stars = (GameObject[])(object)new GameObject[starCount];
		GameEvents.OnMapLoaded += OnMapLoaded;
		originalCameraSize = cam.orthographicSize;
		SystemEvents.OnScreenSizeChanged += OnScreenSizeChanged;
	}

	private void OnDestroy()
	{
		GameEvents.OnMapLoaded -= OnMapLoaded;
		SystemEvents.OnScreenSizeChanged -= OnScreenSizeChanged;
		instance = null;
	}

	private void OnMapLoaded()
	{
		GenerateStars();
	}

	private void OnScreenSizeChanged(Vector2 screenSize)
	{
		if (SystemManager.IsMobile)
		{
			GenerateStars();
		}
	}

	public void GenerateStars()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		Bounds worldBounds = MapRenderer.Current.GetWorldBounds();
		Resolution currentResolution = Screen.currentResolution;
		float num = ((Resolution)(ref currentResolution)).width;
		currentResolution = Screen.currentResolution;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(num, (float)((Resolution)(ref currentResolution)).height, 0f);
		Vector3 val2 = Camera.main.ScreenToWorldPoint(val) - Camera.main.ScreenToWorldPoint(Vector3.zero);
		Vector2 val3 = new Vector2(val2.x + margin, val2.y + margin) * 0.5f;
		worldCenter = ((Bounds)(ref worldBounds)).center;
		((Component)this).transform.position = worldCenter;
		for (int i = 0; i < starCount; i++)
		{
			GameObject val4;
			if ((Object)(object)stars[i] == (Object)null)
			{
				val4 = Object.Instantiate<GameObject>(starPrefab, ((Component)this).transform);
				stars[i] = val4;
			}
			else
			{
				val4 = stars[i];
			}
			val4.transform.localPosition = new Vector3(Random.Range(0f - val3.x, val3.x), Random.Range(0f - val3.y, val3.y), 0f);
			val4.transform.localScale = Vector3.one * 0.2f;
		}
	}

	private void LateUpdate()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (movementEnabled)
		{
			Vector3 val = (worldCenter - ((Component)cam).transform.position) * moveMultiplier;
			val.z = 0f;
			((Component)this).transform.position = worldCenter - val;
			float num = cam.orthographicSize / originalCameraSize;
			((Component)this).transform.localScale = new Vector3(num, num, 1f);
		}
	}

	public static void Show()
	{
		((Component)instance).gameObject.SetActive(true);
	}

	public static void Hide()
	{
		((Component)instance).gameObject.SetActive(false);
	}
}
