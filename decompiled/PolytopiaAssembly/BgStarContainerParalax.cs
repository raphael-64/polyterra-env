using UnityEngine;

public class BgStarContainerParalax : MonoBehaviour
{
	public Camera cam;

	public int starCount = 100;

	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private MeshFilter meshFilter;

	[Header("Prefabs")]
	[SerializeField]
	private Material starMaterial;

	private Material materialInstance;

	protected static BgStarContainerParalax instance;

	private bool timeToUpdateStars = true;

	private Vector3 previousCameraPosition;

	private Vector3 previousCameraTranslation;

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		previousCameraPosition = ((Component)cam).transform.position;
		materialInstance = Object.Instantiate<Material>(starMaterial);
		((Renderer)meshRenderer).sharedMaterial = materialInstance;
		((Renderer)meshRenderer).sortingLayerName = "Bg";
		instance = this;
		GameEvents.OnMapLoaded += OnMapLoaded;
	}

	private void OnDestroy()
	{
		GameEvents.OnMapLoaded -= OnMapLoaded;
		instance = null;
	}

	private void OnMapLoaded()
	{
		timeToUpdateStars = true;
	}

	public void GenerateStars()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = cam.OrthographicBounds();
		Vector3 size = ((Bounds)(ref val)).size;
		_ = ((Vector3)(ref size)).magnitude;
		Mesh val2 = new Mesh();
		Vector3[] array = (Vector3[])(object)new Vector3[4 * starCount];
		int[] array2 = new int[6 * starCount];
		Vector3[] array3 = (Vector3[])(object)new Vector3[4 * starCount];
		Vector2[] array4 = (Vector2[])(object)new Vector2[4 * starCount];
		Vector3 val3 = default(Vector3);
		for (int i = 0; i < starCount; i++)
		{
			((Vector3)(ref val3))._002Ector(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(0.5f, 1.8f));
			int num = 4;
			array[i * num] = new Vector3(0f, 0f, 0f);
			array[i * num + 1] = new Vector3(1f, 0f, 0f);
			array[i * num + 2] = new Vector3(0f, 1f, 0f);
			array[i * num + 3] = new Vector3(1f, 1f, 0f);
			int num2 = 6;
			array2[i * num2] = i * num;
			array2[i * num2 + 1] = i * num + 2;
			array2[i * num2 + 2] = i * num + 1;
			array2[i * num2 + 3] = i * num + 2;
			array2[i * num2 + 4] = i * num + 3;
			array2[i * num2 + 5] = i * num + 1;
			array3[i * num] = val3;
			array3[i * num + 1] = val3;
			array3[i * num + 2] = val3;
			array3[i * num + 3] = val3;
			array4[i * num] = new Vector2(0f, 0f);
			array4[i * num + 1] = new Vector2(1f, 0f);
			array4[i * num + 2] = new Vector2(0f, 1f);
			array4[i * num + 3] = new Vector2(1f, 1f);
		}
		val2.vertices = array;
		val2.triangles = array2;
		val2.normals = array3;
		val2.uv = array4;
		if ((Object)(object)meshFilter.sharedMesh != (Object)null)
		{
			Object.Destroy((Object)(object)meshFilter.sharedMesh);
		}
		meshFilter.sharedMesh = val2;
	}

	private void LateUpdate()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)cam).transform.position;
		Vector3 val = position - previousCameraPosition;
		Vector3 val2 = previousCameraTranslation + val / cam.orthographicSize;
		materialInstance.SetVector("_CameraTranslation", new Vector4(val2.x, val2.y, 0f, 0f));
		float num = UIManager.CanvasScaler.scaleFactor * ScalingUtils.GetDPI() / (float)Screen.height;
		materialInstance.SetFloat("_Scale", num);
		previousCameraPosition = position;
		previousCameraTranslation = val2;
		if (timeToUpdateStars)
		{
			GenerateStars();
			timeToUpdateStars = false;
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
