using System;
using System.Collections.Generic;
using Polytopia.Data;
using Polytopia.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]
public class PurchaseManager : IStoreListener
{
	private const int INIT_TIMEOUT = 10;

	public static readonly GameSettings.Difficulties[] UnlockedDifficulties = new GameSettings.Difficulties[4]
	{
		GameSettings.Difficulties.Easy,
		GameSettings.Difficulties.Normal,
		GameSettings.Difficulties.Hard,
		GameSettings.Difficulties.Crazy
	};

	private bool shouldUseDebug;

	private List<TribeData.Type> debugUnlockedTribes = new List<TribeData.Type>();

	private PurchaseCache purchaseCache;

	private bool isInitialized;

	private TribePurchaseCallback OnTribePurchaseComplete;

	private SkinPurchaseCallback OnSkinPurchaseComplete;

	private RestoreCallback OnRestoreComplete;

	private IStoreController controller;

	private IExtensionProvider extensions;

	private float initStartTime;

	private int preRestoreUnlockedTribesCount = -1;

	public bool IsInitialized => isInitialized;

	public void Init()
	{
		string purchaseCachePatch = Paths.GetPurchaseCachePatch();
		if (!DiskSerializationHelpers.FromDisk<PurchaseCache>(purchaseCachePatch, out purchaseCache))
		{
			if (PolytopiaFile.Exists(Paths.GetPurchaseCachePatch()))
			{
				GameManager.GetAnalyticsManager().SendEvent("PurchaseCacheLoadFailed", new Dictionary<string, object> { 
				{
					"Version",
					VersionManager.SemanticVersion.ToString()
				} });
			}
			purchaseCache = new PurchaseCache();
			UpdateUnlockedProducts();
		}
		else if (purchaseCache.HasNewSerialisationVersion)
		{
			DiskSerializationHelpers.ToDisk(purchaseCache, purchaseCachePatch, 4, out var _);
		}
		RetryInit();
	}

	public void RetryInit()
	{
		initStartTime = Time.time;
		isInitialized = true;
		UpdateUnlockedProducts();
	}

	public bool IsInitTimedOut()
	{
		if (!IsInitialized)
		{
			return Time.time - initStartTime >= 10f;
		}
		return false;
	}

	public List<TribeData.Type> GetUnlockedTribes(bool forceUpdate = false)
	{
		if (!purchaseCache.HasUpdated || forceUpdate)
		{
			UpdateUnlockedProducts();
		}
		return purchaseCache.GetUnlockedTribes();
	}

	public void UpdateUnlockedProducts()
	{
		List<TribeData.Type> allTribeTypes = PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).GetAllTribeTypes();
		for (int i = 0; i < allTribeTypes.Count; i++)
		{
			if (!IsTribeUnlockedInternal(allTribeTypes[i]))
			{
				Log.Spam("Locking tribe {0}", new object[1] { allTribeTypes[i] });
				allTribeTypes.RemoveAt(i--);
			}
		}
		List<SkinType> allSkinsTypes = SkinTypeExtensions.GetAllSkinsTypes();
		for (int j = 0; j < allSkinsTypes.Count; j++)
		{
			if (!IsSkinUnlockedInternal(allSkinsTypes[j]))
			{
				Log.Spam("Locking skin {0}", new object[1] { allSkinsTypes[j] });
				allSkinsTypes.RemoveAt(j--);
			}
		}
		bool flag = Config.purchaseDebug.IntValue == 1 || isInitialized;
		bool flag2 = false;
		if (allTribeTypes.Count > purchaseCache.GetUnlockedTribes().Count || flag)
		{
			flag2 = purchaseCache.TryUpdateIAPUnlockedTribes(allTribeTypes);
		}
		bool flag3 = false;
		if (allSkinsTypes.Count > purchaseCache.GetUnlockedSkins().Count || flag)
		{
			flag3 = purchaseCache.TryUpdateIAPUnlockedSkins(allSkinsTypes);
		}
		if (flag3 || flag2)
		{
			DiskSerializationHelpers.ToDisk(purchaseCache, Paths.GetPurchaseCachePatch(), 4, out var _);
			SystemEvents.PurchaseManagerUpdated();
		}
	}

	public void UpdateServerUnlockedTribes(int[] unlockedTribes)
	{
		List<TribeData.Type> list = new List<TribeData.Type>();
		int num = 0;
		while (unlockedTribes != null && num < unlockedTribes.Length)
		{
			int item = unlockedTribes[num];
			list.Add((TribeData.Type)item);
			num++;
		}
		if (purchaseCache.TryUpdateServerUnlockedTribes(list))
		{
			DiskSerializationHelpers.ToDisk(purchaseCache, Paths.GetPurchaseCachePatch(), 4, out var _);
			SystemEvents.PurchaseManagerUpdated();
		}
	}

	public int GetUnlockedTribeCount()
	{
		return GetUnlockedTribes().Count - 2;
	}

	public bool IsTribeUnlocked(TribeData.Type type)
	{
		return GetUnlockedTribes().Contains(type);
	}

	private bool IsTribeUnlockedInternal(TribeData.Type tribe)
	{
		if (tribe == TribeData.Type.None || tribe == TribeData.Type.Nature)
		{
			return true;
		}
		return IsTribeUnlockedSteam((uint)Config.steamAppId.IntValue, tribe);
	}

	public decimal? GetPrice(TribeData.Type tribeType)
	{
		return null;
	}

	public string GetCurrency(TribeData.Type tribeType)
	{
		return null;
	}

	public string GetPriceString(TribeData.Type tribeType)
	{
		return GetPriceStringSteam(tribeType);
	}

	public void OnPurchaseProduct(TribeData.Type tribeType, TribePurchaseCallback callback)
	{
		OnPurchaseProductSteam(callback);
	}

	public void RestorePurchases(RestoreCallback callback)
	{
		preRestoreUnlockedTribesCount = GetUnlockedTribeCount();
		RestorePurchasesSteam();
	}

	private List<TribeData.Type> GetPreunlockedTribes()
	{
		return GameManager.IAPData.preunlockedTribesStandalone;
	}

	public bool IsAnythingBought()
	{
		foreach (TribeData.Type unlockedTribe in purchaseCache.GetUnlockedTribes())
		{
			if (unlockedTribe != TribeData.Type.Nature && unlockedTribe != TribeData.Type.None && !GetPreunlockedTribes().Contains(unlockedTribe))
			{
				return true;
			}
		}
		return false;
	}

	private void CompletePurchase(bool success, IAPProduct product, PurchaseFailureReason reason)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (OnTribePurchaseComplete != null)
		{
			OnTribePurchaseComplete(success, product, reason);
			if (!PolytopiaBackendAdapter.Instance.IsConnected)
			{
				GameManager.GetLoginManager().Login();
			}
		}
		OnTribePurchaseComplete = null;
	}

	private void CompleteRestore(bool success, int restoredTribesCount)
	{
		if (OnRestoreComplete != null)
		{
			OnRestoreComplete(success, restoredTribesCount);
		}
		OnRestoreComplete = null;
	}

	private static bool IsTribeUnlockedSteam(uint appId, TribeData.Type tribe)
	{
		return tribe switch
		{
			TribeData.Type.Polaris => FacepunchHelpers.IsDLCPurchased(appId, 1237430u), 
			TribeData.Type.Elyrion => FacepunchHelpers.IsDLCPurchased(appId, 982201u), 
			TribeData.Type.Aquarion => FacepunchHelpers.IsDLCPurchased(appId, 982200u), 
			TribeData.Type.Cymanti => FacepunchHelpers.IsDLCPurchased(appId, 1529660u), 
			_ => GameManager.IAPData.preunlockedTribesStandalone.Contains(tribe), 
		};
	}

	private string GetPriceStringSteam(TribeData.Type tribeType)
	{
		return "tribepicker.getdlc";
	}

	private void OnPurchaseProductSteam(TribePurchaseCallback callback)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		SteamFriends.OpenStoreOverlay(AppId.op_Implicit((uint)Config.steamAppId.IntValue), (OverlayToStoreFlag)0);
		SteamFriends.OnGameOverlayActivated += delegate
		{
			callback(success: true, null, (PurchaseFailureReason)7);
		};
	}

	private void RestorePurchasesSteam()
	{
	}

	private bool IsTribeUnlockedMobile(TribeData.Type tribe)
	{
		if (isInitialized)
		{
			IAPProduct product = GameManager.IAPData.GetProduct(tribe);
			if (product != null)
			{
				Product val = controller.products.WithID(product.id);
				if (val != null)
				{
					return val.hasReceipt;
				}
			}
		}
		return GameManager.IAPData.preunlockedTribesMobile.Contains(tribe);
	}

	private decimal GetPriceMobile(TribeData.Type tribe)
	{
		if (isInitialized)
		{
			IAPProduct product = GameManager.IAPData.GetProduct(tribe);
			if (product != null)
			{
				Product val = controller.products.WithID(product.id);
				if (val != null && val.availableToPurchase)
				{
					return val.metadata.localizedPrice;
				}
			}
		}
		return -1m;
	}

	private string GetCurrencyMobile(TribeData.Type tribe)
	{
		if (isInitialized)
		{
			IAPProduct product = GameManager.IAPData.GetProduct(tribe);
			if (product != null)
			{
				Product val = controller.products.WithID(product.id);
				if (val != null && val.availableToPurchase)
				{
					return val.metadata.isoCurrencyCode;
				}
			}
		}
		return null;
	}

	private string GetPriceStringMobile(TribeData.Type tribe)
	{
		if (isInitialized)
		{
			IAPProduct product = GameManager.IAPData.GetProduct(tribe);
			if (product != null)
			{
				Product val = controller.products.WithID(product.id);
				if (val != null && val.availableToPurchase)
				{
					return val.metadata.localizedPriceString;
				}
			}
		}
		return "tribepicker.buy";
	}

	private void OnPurchaseProductMobile(TribeData.Type tribeType)
	{
		Log.Verbose($"PurchaseManager :: OnPurchaseProductMobile :: {tribeType}", Array.Empty<object>());
		if (isInitialized)
		{
			IAPProduct product = GameManager.IAPData.GetProduct(tribeType);
			if (product != null)
			{
				Product val = controller.products.WithID(product.id);
				if (val != null && val.availableToPurchase)
				{
					controller.InitiatePurchase(val);
				}
				else if (val == null)
				{
					SendErrorEvent("Purchase Failed", "Missing product in unity iap", tribeType.ToString());
					CompletePurchase(success: false, product, (PurchaseFailureReason)2);
					Log.Verbose("PurchaseManager :: OnPurchaseProductMobile :: couldn't find product", Array.Empty<object>());
				}
				else if (val.availableToPurchase)
				{
					SendErrorEvent("Purchase Failed", "Not available to purchase", tribeType.ToString());
					CompletePurchase(success: false, product, (PurchaseFailureReason)2);
					Log.Verbose("PurchaseManager :: OnPurchaseProductMobile :: product is not available for purchase", Array.Empty<object>());
				}
			}
			else
			{
				SendErrorEvent("Purchase Failed", "Missing product in iap data", tribeType.ToString());
				CompletePurchase(success: false, null, (PurchaseFailureReason)0);
				Log.Error("PurchaseManager :: OnPurchaseProductMobile :: Failed to initiate purchase since the product does not exist in IAPData", Array.Empty<object>());
			}
		}
		else
		{
			SendErrorEvent("Purchase Failed", "Not initialised");
			CompletePurchase(success: false, null, (PurchaseFailureReason)0);
			Log.Error("PurchaseManager :: OnPurchaseProductMobile :: Failed to initiate purchase since purchasing is not fully initted", Array.Empty<object>());
		}
	}

	private void RestorePurchasesMobile()
	{
		if (!isInitialized)
		{
			Log.Verbose("PurchaseManager :: RestorePurchases :: Purchase manager is not initialized", Array.Empty<object>());
			return;
		}
		Log.Verbose("PurchaseManager :: RestorePurchases", Array.Empty<object>());
		NetworkUtils.ShowLoader(Localization.Get("tribepicker.restoring"), 0);
	}

	private static void SendErrorEvent(string eventName, string error = "", string product = "")
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		GameManager.GetAnalyticsManager().SendEvent(eventName, new Dictionary<string, object>
		{
			{
				"version",
				VersionManager.SemanticVersion.ToString()
			},
			{
				"installerName",
				Application.installerName
			},
			{
				"installMode",
				Application.installMode
			},
			{
				"os",
				SystemInfo.operatingSystem
			},
			{ "error", error },
			{ "product", product },
			{
				"language",
				Localization.GetSystemLanguage()
			},
			{
				"bundleId",
				Application.identifier
			},
			{ "isAlpha", false }
		});
	}

	public static void ShowRestoreErrorPopup()
	{
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("purchasing.restore.error.title");
		basicPopup.Description = Localization.Get("purchasing.restore.error.message");
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show();
	}

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		isInitialized = true;
		this.controller = controller;
		this.extensions = extensions;
		UpdateUnlockedProducts();
		Log.Verbose("PurchaseManager :: OnInitialized", Array.Empty<object>());
		SystemEvents.PurchaseManagerInitialized(success: true, null);
	}

	public bool HasReceipt()
	{
		return true;
	}

	public unsafe void OnInitializeFailed(InitializationFailureReason error)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		isInitialized = false;
		SendErrorEvent("Init Failed", ((object)(*(InitializationFailureReason*)(&error))/*cast due to .constrained prefix*/).ToString());
		Log.Verbose($"PurchaseManager :: OnInitializeFailed :: {error}", Array.Empty<object>());
		SystemEvents.PurchaseManagerInitialized(success: false, GetDefaultInitErrorKey());
	}

	public void OnAndroidInitializeFailed()
	{
		isInitialized = false;
		SendErrorEvent("Init Failed", "Google Player Service Connect Failed");
		Log.Verbose("PurchaseManager :: OnInitializeFailed :: Google Player Service Connect Failed", Array.Empty<object>());
		SystemEvents.PurchaseManagerInitialized(success: false, "purchasing.error.androidinitializationfailed");
	}

	public static string GetDefaultInitErrorKey()
	{
		return "purchasing.error.initializationfailed";
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
	{
		Log.Verbose("PurchaseManager :: ProcessPurchase :: Transaction ID: " + e.purchasedProduct.transactionID, Array.Empty<object>());
		UpdateUnlockedProducts();
		IAPProduct productWithId = GameManager.IAPData.GetProductWithId(e.purchasedProduct.definition.id);
		CompletePurchase(success: true, productWithId, (PurchaseFailureReason)7);
		return (PurchaseProcessingResult)0;
	}

	public unsafe void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		SendErrorEvent("Purchase Failed", ((object)(*(PurchaseFailureReason*)(&reason))/*cast due to .constrained prefix*/).ToString());
		IAPProduct productWithId = GameManager.IAPData.GetProductWithId(product.definition.id);
		CompletePurchase(success: false, productWithId, reason);
		Log.Verbose($"PurchaseManager :: OnPurchaseFailed :: Purchase Failure Reason: {reason}", Array.Empty<object>());
		UpdateUnlockedProducts();
	}

	public static void ShowPurchaseErrorPopup(PurchaseFailureReason reason)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected I4, but got Unknown
		string text = "";
		switch ((int)reason)
		{
		case 0:
			text = "purchasing.error.purchasingunavailable";
			break;
		case 1:
			text = "purchasing.error.existingpurchasepending";
			break;
		case 2:
			text = "purchasing.error.productunavailable";
			break;
		case 3:
			text = "purchasing.error.signatureinvalid";
			break;
		case 4:
			return;
		case 5:
			text = "purchasing.error.paymentdeclined";
			break;
		case 6:
			text = "purchasing.error.duplicatetransaction";
			break;
		default:
			text = "purchasing.error.unknown";
			break;
		}
		BasicPopup basicPopup = PopupManager.GetBasicPopup();
		basicPopup.Header = Localization.Get("purchasing.error.title");
		basicPopup.Description = Localization.Get(text);
		basicPopup.buttonData = new PopupBase.PopupButtonData[1]
		{
			new PopupBase.PopupButtonData("buttons.ok", PopupBase.PopupButtonData.States.Selected)
		};
		basicPopup.Show();
	}

	public void CmdUnlockTribe(string[] args)
	{
		if (args.Length < 1)
		{
			foreach (TribeData.Type allTribeType in PolytopiaDataManager.GetGameLogicData(VersionManager.GameLogicDataVersion).GetAllTribeTypes())
			{
				DebugUnlockTribe(allTribeType);
			}
			return;
		}
		TribeData.Type? type = TribeExtensions.GetType(args[0]);
		if (type.HasValue)
		{
			DebugUnlockTribe(type.Value);
		}
		else
		{
			DebugConsole.Write("Invalid tribe '" + args[0] + "'. Valid tribes " + string.Join(", ", (TribeData.Type[])Enum.GetValues(typeof(TribeData.Type))), Array.Empty<object>());
		}
	}

	public void CmdLockTribe(string[] args)
	{
		if (args.Length < 1)
		{
			debugUnlockedTribes.Clear();
			UpdateUnlockedProducts();
			SaveDebugUnlockedTribes();
			return;
		}
		TribeData.Type? type = TribeExtensions.GetType(args[0]);
		if (type.HasValue)
		{
			DebugLockTribe(type.Value);
		}
		else
		{
			DebugConsole.Write("Invalid tribe '" + args[0] + "'. Valid tribes " + string.Join(", ", (TribeData.Type[])Enum.GetValues(typeof(TribeData.Type))), Array.Empty<object>());
		}
	}

	public void CmdPrintUnlockedTribes(string[] args)
	{
		DebugConsole.Write(DebugUnlockedTribesString(), Array.Empty<object>());
	}

	public void DebugLockTribe(TribeData.Type tribe)
	{
		debugUnlockedTribes.Remove(tribe);
		UpdateUnlockedProducts();
		SaveDebugUnlockedTribes();
	}

	public void DebugUnlockTribe(TribeData.Type tribe)
	{
		if (!debugUnlockedTribes.Contains(tribe))
		{
			debugUnlockedTribes.Add(tribe);
			UpdateUnlockedProducts();
			SaveDebugUnlockedTribes();
		}
	}

	private string DebugUnlockedTribesString()
	{
		return string.Join(",", debugUnlockedTribes);
	}

	private void SaveDebugUnlockedTribes()
	{
		PolytopiaPlayerPrefs.SetString("polytopia_purchase_debug_unlocked_tribes", DebugUnlockedTribesString());
		PlayerPrefsUtils.Save();
	}

	private void LoadDebugUnlockedTribes()
	{
		string text = PolytopiaPlayerPrefs.GetString("polytopia_purchase_debug_unlocked_tribes");
		Log.Spam("Reading unlocked tribes {0}", new object[1] { text });
		string[] array = text.Split(',');
		debugUnlockedTribes.Clear();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			TribeData.Type? type = TribeExtensions.GetType(array2[i]);
			if (type.HasValue)
			{
				debugUnlockedTribes.Add(type.Value);
			}
		}
		UpdateUnlockedProducts();
	}

	public void UpdateServerUnlockedSkins(int[] unlockedTribes)
	{
		List<SkinType> list = new List<SkinType>();
		int num = 0;
		while (unlockedTribes != null && num < unlockedTribes.Length)
		{
			int item = unlockedTribes[num];
			list.Add((SkinType)item);
			num++;
		}
		if (purchaseCache.TryUpdateServerUnlockedSkins(list))
		{
			DiskSerializationHelpers.ToDisk(purchaseCache, Paths.GetPurchaseCachePatch(), 4, out var _);
			SystemEvents.PurchaseManagerUpdated();
		}
	}

	public List<SkinType> GetUnlockedSkins()
	{
		return purchaseCache.GetUnlockedSkins();
	}

	public bool IsSkinUnlocked(SkinType skinType)
	{
		if (skinType != SkinType.Default)
		{
			return GetUnlockedSkins().Contains(skinType);
		}
		return true;
	}

	private bool IsSkinUnlockedInternal(SkinType skinType)
	{
		if (skinType == SkinType.Default)
		{
			return true;
		}
		if (SystemManager.IsSteam)
		{
			return IsSkinUnlockedSteam((uint)Config.steamAppId.IntValue, skinType);
		}
		if (SystemManager.IsSwitch)
		{
			return false;
		}
		if (SystemManager.IsTesla)
		{
			return true;
		}
		if (SystemManager.IsMobile)
		{
			return IsSkinUnlockedMobile(skinType);
		}
		return false;
		bool IsSkinUnlockedMobile(SkinType skinType2)
		{
			if (isInitialized)
			{
				IAPSkinProduct skinProduct = GameManager.IAPData.GetSkinProduct(skinType2);
				if (skinProduct != null)
				{
					Product val = controller.products.WithID(skinProduct.ID);
					if (val != null)
					{
						return val.hasReceipt;
					}
				}
			}
			return false;
		}
		static bool IsSkinUnlockedSteam(uint appId, SkinType skinType2)
		{
			switch (skinType2)
			{
			case SkinType.Default:
			case SkinType.Test:
				return true;
			case SkinType.Ranger:
			case SkinType.Baerion:
			case SkinType.Skeleton:
				return FacepunchHelpers.IsDLCPurchased(appId, 2186650u);
			default:
				return false;
			}
		}
	}

	public void OnPurchaseProduct(SkinType skinType, SkinPurchaseCallback callback)
	{
		if (SystemManager.IsMobile)
		{
			OnSkinPurchaseComplete = callback;
			OnPurchaseProductMobile(skinType);
		}
		else if (SystemManager.IsSteam)
		{
			OnPurchaseProductSteam(callback);
		}
		else
		{
			_ = SystemManager.IsSwitch;
		}
	}

	private void OnPurchaseProductMobile(SkinType skinType)
	{
		Log.Verbose($"PurchaseManager :: OnPurchaseProductMobile :: {skinType}", Array.Empty<object>());
		if (isInitialized)
		{
			IAPSkinProduct skinProduct = GameManager.IAPData.GetSkinProduct(skinType);
			if (skinProduct != null)
			{
				Product val = controller.products.WithID(skinProduct.ID);
				if (val != null && val.availableToPurchase)
				{
					controller.InitiatePurchase(val);
				}
				else if (val == null)
				{
					SendErrorEvent("Purchase Failed", "Missing product in unity iap", skinType.ToString());
					CompleteSkinPurchase(success: false, skinProduct, (PurchaseFailureReason)2);
					Log.Verbose("PurchaseManager :: OnPurchaseProductMobile :: couldn't find product", Array.Empty<object>());
				}
				else if (val.availableToPurchase)
				{
					SendErrorEvent("Purchase Failed", "Not available to purchase", skinType.ToString());
					CompleteSkinPurchase(success: false, skinProduct, (PurchaseFailureReason)2);
					Log.Verbose("PurchaseManager :: OnPurchaseProductMobile :: product is not available for purchase", Array.Empty<object>());
				}
			}
			else
			{
				SendErrorEvent("Purchase Failed", "Missing product in iap data", skinType.ToString());
				CompleteSkinPurchase(success: false, null, (PurchaseFailureReason)0);
				Log.Error("PurchaseManager :: OnPurchaseProductMobile :: Failed to initiate purchase since the product does not exist in IAPData", Array.Empty<object>());
			}
		}
		else
		{
			SendErrorEvent("Purchase Failed", "Not initialised");
			CompleteSkinPurchase(success: false, null, (PurchaseFailureReason)0);
			Log.Error("PurchaseManager :: OnPurchaseProductMobile :: Failed to initiate purchase since purchasing is not fully initted", Array.Empty<object>());
		}
	}

	private void OnPurchaseProductSteam(SkinPurchaseCallback callback)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		SteamFriends.OpenStoreOverlay(AppId.op_Implicit((uint)Config.steamAppId.IntValue), (OverlayToStoreFlag)0);
		SteamFriends.OnGameOverlayActivated += delegate
		{
			callback(success: true, null, (PurchaseFailureReason)7);
		};
	}

	private void CompleteSkinPurchase(bool success, IAPSkinProduct product, PurchaseFailureReason reason)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (OnSkinPurchaseComplete != null)
		{
			OnSkinPurchaseComplete(success, product, reason);
			if (!PolytopiaBackendAdapter.Instance.IsConnected)
			{
				GameManager.GetLoginManager().Login();
			}
		}
		OnSkinPurchaseComplete = null;
	}

	public string GetSkinPriceString(SkinType skinType)
	{
		if (SystemManager.IsMobile)
		{
			if (IsInitialized)
			{
				Product skinProduct = GetSkinProduct(skinType);
				if (skinProduct != null && skinProduct.availableToPurchase)
				{
					return skinProduct.metadata.localizedPriceString;
				}
			}
			return "tribepicker.buy";
		}
		return "tribepicker.getdlc";
	}

	public decimal? GetPrice(SkinType skinType)
	{
		if (!IsInitialized || !SystemManager.IsMobile)
		{
			return null;
		}
		Product skinProduct = GetSkinProduct(skinType);
		if (skinProduct != null && skinProduct.availableToPurchase)
		{
			return skinProduct.metadata.localizedPrice;
		}
		return null;
	}

	public string GetSkinCurrency(SkinType skinType)
	{
		if (!IsInitialized || !SystemManager.IsMobile)
		{
			return "";
		}
		Product skinProduct = GetSkinProduct(skinType);
		if (skinProduct != null && skinProduct.availableToPurchase)
		{
			return skinProduct.metadata.isoCurrencyCode;
		}
		return "";
	}

	private Product GetSkinProduct(SkinType skinType)
	{
		if (IsInitialized)
		{
			IAPSkinProduct skinProduct = GameManager.IAPData.GetSkinProduct(skinType);
			if (skinProduct != null)
			{
				return controller.products.WithID(skinProduct.ID);
			}
		}
		return null;
	}
}
