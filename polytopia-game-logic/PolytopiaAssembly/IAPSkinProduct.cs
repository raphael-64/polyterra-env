using System;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]
public class IAPSkinProduct
{
	[SerializeField]
	public ProductType ProductType = (ProductType)1;

	[field: SerializeField]
	public SkinType SkinType { get; private set; }

	[field: SerializeField]
	public string ID { get; private set; }

	public string GetGoogleID()
	{
		return "com.midjiwan.polytopia." + ID;
	}

	public string GetAppleID()
	{
		if (Application.identifier.EndsWith(".alpha"))
		{
			return ID + ".alpha";
		}
		return ID;
	}
}
