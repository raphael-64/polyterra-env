using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class StylePickerAddressable : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer spriteRenderer;

	private Sprite[] sprites;

	[SerializeField]
	protected string baseID = "";

	[Header("Outlines")]
	[SerializeField]
	protected SpriteRenderer outlineRenderer;

	[SerializeField]
	protected Sprite[] outlines;

	protected int currStyle;

	protected int maxStyles = 16;

	public int Style
	{
		get
		{
			return currStyle;
		}
		set
		{
			Log.Info("Set style to {0}", new object[1] { value });
			currStyle = value;
			LoadAsset(value);
		}
	}

	public SpriteRenderer SpriteRenderer => spriteRenderer;

	public Sprite[] AllSprites => sprites;

	public bool OutlineEnabled
	{
		get
		{
			if ((Object)(object)outlineRenderer != (Object)null)
			{
				return ((Component)outlineRenderer).gameObject.activeSelf;
			}
			return false;
		}
		set
		{
			if ((Object)(object)outlineRenderer != (Object)null)
			{
				((Component)outlineRenderer).gameObject.SetActive(value);
			}
		}
	}

	private void Awake()
	{
		Log.Spam("{0} Style picker Awake!", new object[1] { baseID });
		sprites = (Sprite[])(object)new Sprite[maxStyles];
		_ = baseID != "";
	}

	private void LoadAsset(int id)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Log.Spam("load asset {0}_{1}", new object[2] { baseID, id });
		AsyncOperationHandle<Sprite> val = Addressables.LoadAssetAsync<Sprite>((object)(baseID + "_" + id));
		val.Completed += SpriteLoaded;
	}

	private void SpriteLoaded(AsyncOperationHandle<Sprite> obj)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Invalid comparison between Unknown and I4
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		AsyncOperationStatus status = obj.Status;
		if ((int)status != 1)
		{
			if ((int)status == 2)
			{
				Log.Info("Sprite load failed. {0}", new object[1] { obj.Status });
			}
		}
		else
		{
			Log.Info("Sprite loaded", Array.Empty<object>());
			spriteRenderer.sprite = obj.Result;
		}
	}

	public Sprite GetSprite(int style)
	{
		if (Object.op_Implicit((Object)(object)sprites[style - 1]))
		{
			return sprites[style - 1];
		}
		Log.Warning("{0}, Couldn't find sprite. index: {1}", new object[2]
		{
			((Object)((Component)this).gameObject).name,
			style - 1
		});
		return spriteRenderer.sprite;
	}
}
