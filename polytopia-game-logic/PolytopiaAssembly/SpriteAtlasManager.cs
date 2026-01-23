using System;
using System.Collections.Generic;
using System.IO;
using Polytopia.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class SpriteAtlasManager
{
	public const int SPRITE_ATLAS_LOAD_RETRY_COUNT = 10;

	public static List<string> ALL_ATLASES = new List<string> { "Avatar", "TerrainBuildings", "Heads", "Overlays", "StartScene", "TerrainFeatures", "Units", "UI" };

	public static List<string> MENU_ATLASES = new List<string> { "TerrainFeatures", "Heads" };

	private Dictionary<string, SpriteAtlas> loadedAtlases = new Dictionary<string, SpriteAtlas>();

	private Dictionary<string, Dictionary<string, Sprite>> cachedSprites = new Dictionary<string, Dictionary<string, Sprite>>();

	private Dictionary<Sprite, string> spriteToAtlasName = new Dictionary<Sprite, string>();

	private Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();

	private string[] cachedStreamingAssetsPaths;

	public bool IsSpriteAtlasLoaded(string atlas)
	{
		return loadedAtlases.ContainsKey(atlas);
	}

	public bool AreSpriteAtlasesLoaded(List<string> atlases)
	{
		foreach (string atlase in atlases)
		{
			if (!IsSpriteAtlasLoaded(atlase))
			{
				return false;
			}
		}
		return true;
	}

	public void LoadAssetBundle(string path, Action<AssetBundle> completion)
	{
		if (loadedAssetBundles.TryGetValue(path, out var value))
		{
			completion(value);
			return;
		}
		((AsyncOperation)AssetBundle.LoadFromFileAsync(path)).completed += delegate(AsyncOperation asyncOperation)
		{
			AssetBundleCreateRequest val = (AssetBundleCreateRequest)(object)((asyncOperation is AssetBundleCreateRequest) ? asyncOperation : null);
			if ((Object)(object)val.assetBundle == (Object)null)
			{
				Log.Error("Could not load assetbundle {0}", new object[1] { path });
				completion(null);
			}
			else
			{
				loadedAssetBundles[path] = val.assetBundle;
				completion(val.assetBundle);
			}
		};
	}

	public void LoadSpriteAtlasBackupSolution(string atlas, Action<SpriteAtlas> completion)
	{
		string spriteAtlesesAddressablesPath = Paths.GetSpriteAtlesesAddressablesPath();
		string path = Path.Combine(spriteAtlesesAddressablesPath, "sprites_assets_" + atlas.ToLowerInvariant() + ".bundle");
		LoadAssetBundle(path, delegate(AssetBundle assetBundle)
		{
			if ((Object)(object)assetBundle == (Object)null)
			{
				Log.Error("Could not load assetbundle from path {0}", new object[1] { path });
				completion(null);
			}
			else
			{
				string text = "Assets/Atlases/" + atlas + ".spriteatlas";
				((AsyncOperation)assetBundle.LoadAssetAsync(text)).completed += delegate(AsyncOperation innerAsyncOperation)
				{
					Object asset = ((AssetBundleRequest)((innerAsyncOperation is AssetBundleRequest) ? innerAsyncOperation : null)).asset;
					SpriteAtlas val = (SpriteAtlas)(object)((asset is SpriteAtlas) ? asset : null);
					if ((Object)(object)val == (Object)null)
					{
						Log.Error("Could not load atlas from assetbundle {0}", new object[1] { atlas });
						completion(null);
					}
					loadedAtlases[atlas] = val;
					completion(val);
				};
			}
		});
	}

	public string[] GetCachedStreamingAssetsPaths()
	{
		if (cachedStreamingAssetsPaths == null)
		{
			string spriteAtlesesAddressablesPath = Paths.GetSpriteAtlesesAddressablesPath();
			if (!PolytopiaDirectory.Exists(spriteAtlesesAddressablesPath))
			{
				Log.Error("Can't find directory {0}", new object[1] { spriteAtlesesAddressablesPath });
				return null;
			}
			cachedStreamingAssetsPaths = PolytopiaDirectory.GetFiles(spriteAtlesesAddressablesPath);
			string[] array = cachedStreamingAssetsPaths;
			foreach (string text in array)
			{
				Log.Verbose("Spriteatlas in streaming assets {0}", new object[1] { text });
			}
		}
		return cachedStreamingAssetsPaths;
	}

	public void LoadSpriteAtlasTexture(string atlas, Action<Texture2D> completion)
	{
		if (!cachedSprites.TryGetValue(atlas, out var value))
		{
			value = new Dictionary<string, Sprite>();
			cachedSprites[atlas] = value;
		}
		if (value.Count > 0)
		{
			foreach (Sprite value2 in value.Values)
			{
				if (!((Object)(object)value2 == (Object)null))
				{
					completion(value2.texture);
					return;
				}
			}
		}
		LoadSpriteAtlas(atlas, delegate(SpriteAtlas spriteAtlas)
		{
			if ((Object)(object)spriteAtlas != (Object)null)
			{
				Sprite[] array = (Sprite[])(object)new Sprite[spriteAtlas.spriteCount];
				spriteAtlas.GetSprites(array);
				Texture2D texture = array[0].texture;
				foreach (Sprite val in array)
				{
					cachedSprites[atlas][((Object)val).name] = val;
				}
				completion(texture);
			}
		});
	}

	public void LoadSpriteAtlas(string atlas, Action<SpriteAtlas> completion)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!loadedAtlases.TryGetValue(atlas, out var value))
		{
			AsyncOperationHandle<SpriteAtlas> val = Addressables.LoadAssetAsync<SpriteAtlas>((object)atlas);
			val.Completed += delegate(AsyncOperationHandle<SpriteAtlas> handle)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Invalid comparison between Unknown and I4
				if ((int)handle.Status == 1 && (Object)(object)handle.Result != (Object)null)
				{
					loadedAtlases[atlas] = handle.Result;
					completion(handle.Result);
				}
				else
				{
					Log.Error("Failed to load atlas {0} using addressables", new object[1] { atlas });
					LoadSpriteAtlasBackupSolution(atlas, completion);
				}
			};
		}
		else
		{
			completion(value);
		}
	}

	public void PreloadAllSpriteAtlases()
	{
		PreloadSpriteAtlases(new List<string> { "Avatar", "TerrainBuildings", "Heads", "Overlays", "StartScene", "TerrainFeatures", "Units", "UI" }, null);
	}

	public void PreloadSpriteAtlases(List<string> atlases, int retryCount, Action<bool> completion)
	{
		PreloadSpriteAtlases(atlases, delegate(bool success)
		{
			if (!success && retryCount > 0)
			{
				PreloadSpriteAtlases(atlases, retryCount--, completion);
			}
			else
			{
				completion(success);
			}
		});
	}

	public void PreloadSpriteAtlases(List<string> atlases, Action<bool> completion)
	{
		if (AreSpriteAtlasesLoaded(atlases))
		{
			completion?.Invoke(obj: true);
			return;
		}
		bool isCompleted = false;
		Action<SpriteAtlas> completion2 = delegate(SpriteAtlas spriteAtlas)
		{
			if (!isCompleted && ((Object)(object)spriteAtlas == (Object)null || AreSpriteAtlasesLoaded(atlases)))
			{
				isCompleted = true;
				completion?.Invoke((Object)(object)spriteAtlas != (Object)null);
			}
		};
		foreach (string atlase in atlases)
		{
			if (isCompleted)
			{
				break;
			}
			LoadSpriteAtlas(atlase, completion2);
		}
	}

	public void LoadSprite(SpriteHandle spriteHandle)
	{
		LoadSprite(spriteHandle.address.atlas, spriteHandle.address.sprite, delegate(string atlasName, string spriteName, Sprite sprite)
		{
			spriteHandle.Complete(atlasName, spriteName, sprite);
		});
	}

	public void LoadSprite(SpriteHandle spriteHandle, SpriteAddress[] spriteAddresses)
	{
		GameManager.GetSpriteAtlasManager().LoadSprites(spriteAddresses, delegate(string[] atlasNames, string[] spriteNames, Sprite[] sprites)
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				if ((Object)(object)sprites[i] != (Object)null)
				{
					spriteHandle.address = spriteAddresses[i];
					spriteHandle.Complete(atlasNames[i], spriteNames[i], sprites[i]);
					break;
				}
			}
		});
	}

	public void LoadSprite(SpriteAddress spriteAddress, SpriteCallback completion)
	{
		LoadSprite(spriteAddress.atlas, spriteAddress.sprite, completion);
	}

	public void LoadSprite(SpriteAddress[] spriteAddresses, SingleSpriteCallback spriteCallback)
	{
		GameManager.GetSpriteAtlasManager().LoadSprites(spriteAddresses, delegate(string[] atlasNames, string[] spriteNames, Sprite[] sprites)
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				if ((Object)(object)sprites[i] != (Object)null)
				{
					spriteCallback(sprites[i]);
					break;
				}
			}
		});
	}

	public void LoadSprite(SpriteAddress[] spriteAddresses, SpriteCallback spriteCallback)
	{
		GameManager.GetSpriteAtlasManager().LoadSprites(spriteAddresses, delegate(string[] atlasNames, string[] spriteNames, Sprite[] sprites)
		{
			for (int i = 0; i < sprites.Length; i++)
			{
				if ((Object)(object)sprites[i] != (Object)null)
				{
					spriteCallback(atlasNames[i], spriteNames[i], sprites[i]);
					return;
				}
			}
			spriteCallback(atlasNames[0], spriteNames[0], null);
		});
	}

	public void LoadSprites(SpriteAddress[] spriteAddresses, SpritesCallback completion)
	{
		string[] atlasNames = new string[spriteAddresses.Length];
		string[] spriteNames = new string[spriteAddresses.Length];
		Sprite[] sprites = (Sprite[])(object)new Sprite[spriteAddresses.Length];
		int count = spriteAddresses.Length;
		for (int i = 0; i < spriteAddresses.Length; i++)
		{
			GetSprite(i);
		}
		void GetSprite(int index)
		{
			LoadSprite(spriteAddresses[index].atlas, spriteAddresses[index].sprite, delegate(string atlasName, string spriteName, Sprite sprite)
			{
				atlasNames[index] = atlasName;
				spriteNames[index] = spriteName;
				sprites[index] = sprite;
				int num = count - 1;
				count = num;
				if (num == 0)
				{
					completion(atlasNames, spriteNames, sprites);
				}
			});
		}
	}

	public void LoadSprite(string atlas, string sprite, SpriteCallback completion)
	{
		LoadSpriteAtlas(atlas, delegate(SpriteAtlas spriteAtlas)
		{
			if ((Object)(object)spriteAtlas != (Object)null)
			{
				completion(atlas, sprite, GetSpriteFromAtlas(spriteAtlas, sprite));
			}
			else
			{
				completion(atlas, sprite, null);
			}
		});
	}

	public Sprite GetLoadedSprite(string atlas, string sprite)
	{
		if (loadedAtlases.TryGetValue(atlas, out var value))
		{
			return GetSpriteFromAtlas(value, sprite);
		}
		return null;
	}

	public string GetAtlasNameForSprite(Sprite sprite)
	{
		if ((Object)(object)sprite == (Object)null)
		{
			return null;
		}
		if (!spriteToAtlasName.TryGetValue(sprite, out var value))
		{
			string name = ((Object)sprite.texture).name;
			for (int i = 0; i < ALL_ATLASES.Count; i++)
			{
				string text = ALL_ATLASES[i];
				if (name.Contains(text) && (string.IsNullOrEmpty(value) || value.Length < text.Length))
				{
					value = text;
				}
			}
			if (string.IsNullOrEmpty(value))
			{
				Log.Error("Can't find atlas for raw name {0}", new object[1] { name });
				return null;
			}
			spriteToAtlasName[sprite] = value;
		}
		return value;
	}

	private Sprite GetSpriteFromAtlas(SpriteAtlas spriteAtlas, string sprite)
	{
		string name = ((Object)spriteAtlas).name;
		Sprite value2;
		if (!cachedSprites.TryGetValue(name, out var value))
		{
			value = new Dictionary<string, Sprite>();
			cachedSprites[name] = value;
		}
		else if (value.TryGetValue(sprite, out value2))
		{
			return value2;
		}
		return value[sprite] = spriteAtlas.GetSprite(sprite);
	}
}
