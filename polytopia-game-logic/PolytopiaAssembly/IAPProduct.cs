using System;
using Polytopia.Data;
using UnityEngine;
using UnityEngine.Purchasing;

[Serializable]
public class IAPProduct
{
	public TribeData.Type tribeType;

	public ProductType type = (ProductType)1;

	[SerializeField]
	private string appleId = "";

	public string googleId = "";

	public string id => appleId;

	public string GetAppleId()
	{
		if (Application.identifier.EndsWith(".alpha"))
		{
			return appleId + ".alpha";
		}
		return appleId;
	}
}
