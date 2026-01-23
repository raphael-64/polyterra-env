using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Polytopia.Data;
using UnityEngine;

public class PurchaseCache : IBinarySerializable
{
	public const int PURCHASE_CACHE_VERSION = 4;

	public bool HasNewSerialisationVersion;

	public bool HasUpdated;

	private List<TribeData.Type> unlockedTribes = new List<TribeData.Type>();

	private List<TribeData.Type> serverUnlockedTribes = new List<TribeData.Type>();

	private List<TribeData.Type> iapUnlockedTribes = new List<TribeData.Type>();

	private List<SkinType> unlockedSkins = new List<SkinType>();

	private List<SkinType> iapUnlockedSkins = new List<SkinType>();

	private List<SkinType> serverUnlockedSkins = new List<SkinType>();

	public List<TribeData.Type> GetUnlockedTribes()
	{
		return unlockedTribes;
	}

	public bool TryUpdateServerUnlockedTribes(List<TribeData.Type> serverUnlockedTribes)
	{
		if (!HaveUnlocksChanged(serverUnlockedTribes, this.serverUnlockedTribes))
		{
			return false;
		}
		this.serverUnlockedTribes = serverUnlockedTribes;
		UpdateUnlockedTribes();
		return true;
	}

	public bool TryUpdateIAPUnlockedTribes(List<TribeData.Type> iapUnlockedTribes)
	{
		HasUpdated = true;
		if (!HaveUnlocksChanged(iapUnlockedTribes, this.iapUnlockedTribes))
		{
			return false;
		}
		this.iapUnlockedTribes = iapUnlockedTribes;
		UpdateUnlockedTribes();
		return true;
	}

	private void UpdateUnlockedTribes()
	{
		unlockedTribes.Clear();
		unlockedTribes.AddRange(iapUnlockedTribes);
		for (int i = 0; i < serverUnlockedTribes.Count; i++)
		{
			TribeData.Type item = serverUnlockedTribes[i];
			if (!unlockedTribes.Contains(item))
			{
				unlockedTribes.Add(item);
			}
		}
	}

	private bool HaveUnlocksChanged(List<TribeData.Type> newUnlockedTribes, List<TribeData.Type> unlockedTribes)
	{
		if (unlockedTribes.Count != newUnlockedTribes.Count)
		{
			return true;
		}
		foreach (TribeData.Type newUnlockedTribe in newUnlockedTribes)
		{
			if (!unlockedTribes.Contains(newUnlockedTribe))
			{
				return true;
			}
		}
		return false;
	}

	private static string GetSalt()
	{
		return "+&865He115H5w4r3Y5u".Replace('5', '0');
	}

	private static void WriteUnlockedTribes(List<TribeData.Type> unlockedTribes, BinaryWriter writer)
	{
		writer.Write(unlockedTribes.Count);
		for (int i = 0; i < unlockedTribes.Count; i++)
		{
			writer.Write((byte)unlockedTribes[i]);
		}
	}

	private static void WriteUnlockedSkins(List<SkinType> unlockedSkins, BinaryWriter writer)
	{
		writer.Write(unlockedSkins.Count);
		for (int i = 0; i < unlockedSkins.Count; i++)
		{
			writer.Write((byte)unlockedSkins[i]);
		}
	}

	private List<TribeData.Type> ReadTribes(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		List<TribeData.Type> list = new List<TribeData.Type>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add((TribeData.Type)reader.ReadByte());
		}
		return list;
	}

	private List<SkinType> ReadSkins(BinaryReader reader)
	{
		int num = reader.ReadInt32();
		List<SkinType> list = new List<SkinType>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add((SkinType)reader.ReadByte());
		}
		return list;
	}

	private static byte[] GetChecksum1(List<TribeData.Type> iapUnlockedTribes)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		WriteUnlockedTribes(iapUnlockedTribes, binaryWriter);
		binaryWriter.Write(SystemInfo.deviceUniqueIdentifier);
		binaryWriter.Write(GetSalt());
		byte[] buffer = memoryStream.ToArray();
		using HashAlgorithm hashAlgorithm = SHA256.Create();
		return hashAlgorithm.ComputeHash(buffer);
	}

	private static byte[] GetChecksum2(List<TribeData.Type> serverUnlockedTribes, List<TribeData.Type> iapUnlockedTribes)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		WriteUnlockedTribes(serverUnlockedTribes, binaryWriter);
		WriteUnlockedTribes(iapUnlockedTribes, binaryWriter);
		binaryWriter.Write(SystemInfo.deviceUniqueIdentifier);
		binaryWriter.Write(GetSalt());
		byte[] buffer = memoryStream.ToArray();
		using HashAlgorithm hashAlgorithm = SHA256.Create();
		return hashAlgorithm.ComputeHash(buffer);
	}

	private static byte[] GetChecksum3(List<TribeData.Type> serverUnlockedTribes, List<TribeData.Type> iapUnlockedTribes)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		WriteUnlockedTribes(serverUnlockedTribes, binaryWriter);
		WriteUnlockedTribes(iapUnlockedTribes, binaryWriter);
		binaryWriter.Write(SettingsUtils.GetOrCreateRandomSeed());
		binaryWriter.Write(GetSalt());
		byte[] buffer = memoryStream.ToArray();
		using HashAlgorithm hashAlgorithm = SHA256.Create();
		return hashAlgorithm.ComputeHash(buffer);
	}

	private static byte[] GetChecksum4(List<TribeData.Type> serverUnlockedTribes, List<TribeData.Type> iapUnlockedTribes, List<SkinType> serverUnlockedSkins, List<SkinType> iapUnlockedSkins)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		WriteUnlockedTribes(serverUnlockedTribes, binaryWriter);
		WriteUnlockedTribes(iapUnlockedTribes, binaryWriter);
		WriteUnlockedSkins(serverUnlockedSkins, binaryWriter);
		WriteUnlockedSkins(iapUnlockedSkins, binaryWriter);
		binaryWriter.Write(SettingsUtils.GetOrCreateRandomSeed());
		binaryWriter.Write(GetSalt());
		byte[] buffer = memoryStream.ToArray();
		using HashAlgorithm hashAlgorithm = SHA256.Create();
		return hashAlgorithm.ComputeHash(buffer);
	}

	public void Serialize(BinaryWriter writer, int version)
	{
		WriteUnlockedTribes(serverUnlockedTribes, writer);
		WriteUnlockedTribes(iapUnlockedTribes, writer);
		WriteUnlockedSkins(serverUnlockedSkins, writer);
		WriteUnlockedSkins(iapUnlockedSkins, writer);
		writer.Write(SystemInfo.deviceUniqueIdentifier);
		byte[] checksum = GetChecksum4(serverUnlockedTribes, iapUnlockedTribes, serverUnlockedSkins, iapUnlockedSkins);
		writer.Write(checksum.Length);
		writer.Write(checksum);
	}

	public void Deserialize(BinaryReader reader, int version)
	{
		HasNewSerialisationVersion = version != 4;
		if (version < 2)
		{
			Deserialize1(reader, version);
		}
		else if (version < 3)
		{
			Deserialize2(reader, version);
		}
		else if (version < 4)
		{
			Deserialize3(reader, version);
		}
		else
		{
			Deserialize4(reader, version);
		}
	}

	private void Deserialize4(BinaryReader reader, int version)
	{
		List<TribeData.Type> list = ReadTribes(reader);
		List<TribeData.Type> list2 = ReadTribes(reader);
		List<SkinType> list3 = ReadSkins(reader);
		List<SkinType> list4 = ReadSkins(reader);
		string text = reader.ReadString();
		if (text != SystemInfo.deviceUniqueIdentifier)
		{
			GameManager.GetAnalyticsManager().SendEvent("DeviceIdMismatch", new Dictionary<string, object>
			{
				{ "old", text },
				{
					"current",
					SystemInfo.deviceUniqueIdentifier
				}
			});
		}
		int count = reader.ReadInt32();
		byte[] fileChecksum = reader.ReadBytes(count);
		byte[] checksum = GetChecksum4(list, list2, list3, list4);
		VerifyChecksum(fileChecksum, checksum, version);
		serverUnlockedTribes = list;
		iapUnlockedTribes = list2;
		serverUnlockedSkins = list3;
		iapUnlockedSkins = list4;
		UpdateUnlockedTribes();
		UpdateUnlockedSkins();
	}

	private void Deserialize3(BinaryReader reader, int version)
	{
		List<TribeData.Type> list = ReadTribes(reader);
		List<TribeData.Type> list2 = ReadTribes(reader);
		string text = reader.ReadString();
		if (text != SystemInfo.deviceUniqueIdentifier)
		{
			GameManager.GetAnalyticsManager().SendEvent("DeviceIdMismatch", new Dictionary<string, object>
			{
				{ "old", text },
				{
					"current",
					SystemInfo.deviceUniqueIdentifier
				}
			});
		}
		int count = reader.ReadInt32();
		byte[] fileChecksum = reader.ReadBytes(count);
		byte[] checksum = GetChecksum3(list, list2);
		VerifyChecksum(fileChecksum, checksum, version);
		serverUnlockedTribes = list;
		iapUnlockedTribes = list2;
		UpdateUnlockedTribes();
	}

	private void Deserialize2(BinaryReader reader, int version)
	{
		List<TribeData.Type> list = ReadTribes(reader);
		List<TribeData.Type> list2 = ReadTribes(reader);
		int count = reader.ReadInt32();
		byte[] fileChecksum = reader.ReadBytes(count);
		byte[] checksum = GetChecksum2(list, list2);
		VerifyChecksum(fileChecksum, checksum, version);
		serverUnlockedTribes = list;
		iapUnlockedTribes = list2;
		UpdateUnlockedTribes();
	}

	private void Deserialize1(BinaryReader reader, int version)
	{
		List<TribeData.Type> list = ReadTribes(reader);
		int count = reader.ReadInt32();
		byte[] fileChecksum = reader.ReadBytes(count);
		byte[] checksum = GetChecksum1(list);
		VerifyChecksum(fileChecksum, checksum, version);
		iapUnlockedTribes = list;
		UpdateUnlockedTribes();
	}

	private void VerifyChecksum(byte[] fileChecksum, byte[] checksum, int version)
	{
		if (!StructuralComparisons.StructuralEqualityComparer.Equals(fileChecksum, checksum))
		{
			GameManager.GetAnalyticsManager().SendEvent("ChecksumMismatch", new Dictionary<string, object>
			{
				{ "version", version },
				{
					"fileChecksum",
					fileChecksum.ToString()
				},
				{
					"checksum",
					checksum.ToString()
				}
			});
			Log.Error("Failed to read purchase cache, checksum mismatch {0} {1} {2}", new object[3]
			{
				version,
				fileChecksum.CombineToString(),
				checksum.CombineToString()
			});
		}
	}

	public List<SkinType> GetUnlockedSkins()
	{
		return unlockedSkins;
	}

	public bool TryUpdateServerUnlockedSkins(List<SkinType> serverUnlockedSkins)
	{
		HaveUnlocksChanged(serverUnlockedSkins, this.serverUnlockedSkins);
		if (1 == 0)
		{
			return false;
		}
		this.serverUnlockedSkins = serverUnlockedSkins;
		UpdateUnlockedSkins();
		return true;
	}

	public bool TryUpdateIAPUnlockedSkins(List<SkinType> iapUnlockedSkins)
	{
		HasUpdated = true;
		if (!HaveUnlocksChanged(iapUnlockedSkins, this.iapUnlockedSkins))
		{
			return false;
		}
		this.iapUnlockedSkins = iapUnlockedSkins;
		UpdateUnlockedSkins();
		return true;
	}

	private void UpdateUnlockedSkins()
	{
		unlockedSkins.Clear();
		unlockedSkins.AddRange(iapUnlockedSkins);
		for (int i = 0; i < serverUnlockedSkins.Count; i++)
		{
			SkinType item = serverUnlockedSkins[i];
			if (!unlockedSkins.Contains(item))
			{
				unlockedSkins.Add(item);
			}
		}
	}

	private bool HaveUnlocksChanged(List<SkinType> newUnlockedSkins, List<SkinType> unlockedSkins)
	{
		if (unlockedSkins.Count != newUnlockedSkins.Count)
		{
			return true;
		}
		foreach (SkinType newUnlockedSkin in newUnlockedSkins)
		{
			if (!unlockedSkins.Contains(newUnlockedSkin))
			{
				return true;
			}
		}
		return false;
	}
}
