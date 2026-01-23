using System;
using System.Collections.Generic;
using System.Linq;
using Polytopia.Data;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "IAPData", menuName = "In App Purchase Data", order = 9004)]
public class IAPData : ScriptableObject
{
	public List<IAPProduct> products;

	public List<IAPSkinProduct> skinProducts = new List<IAPSkinProduct>();

	public List<TribeData.Type> preunlockedTribesMobile;

	public List<TribeData.Type> preunlockedTribesStandalone;

	public IAPProduct GetProduct(TribeData.Type tribeType)
	{
		foreach (IAPProduct product in products)
		{
			if (product.tribeType == tribeType)
			{
				return product;
			}
		}
		return null;
	}

	public IAPSkinProduct GetSkinProduct(SkinType skinType)
	{
		return skinProducts.FirstOrDefault((IAPSkinProduct x) => x.SkinType == skinType);
	}

	public IAPSkinProduct GetSkinProduct(string id)
	{
		return skinProducts.FirstOrDefault((IAPSkinProduct x) => x.ID == id);
	}

	public IAPProduct GetProductWithId(string id)
	{
		foreach (IAPProduct product in products)
		{
			if (product.id == id)
			{
				return product;
			}
		}
		return null;
	}
}
