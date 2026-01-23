using System;
using System.IO;

public static class SerializationHelpers
{
	public static int ReadVersion(BinaryReader reader)
	{
		return reader.ReadInt32();
	}

	public static byte[] ToByteArray<T>(T serializable, int version) where T : IBinarySerializable
	{
		if (serializable == null)
		{
			return null;
		}
		using MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
		{
			binaryWriter.Write(version);
			serializable.Serialize(binaryWriter, version);
		}
		return memoryStream.ToArray();
	}

	public static bool PeekVersion(byte[] data, out int version)
	{
		version = -1;
		try
		{
			using (MemoryStream input = new MemoryStream(data))
			{
				BinaryReader reader = new BinaryReader(input);
				version = ReadVersion(reader);
			}
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Failed to peek version with error {0}", new object[1] { ex });
			return false;
		}
	}

	public static bool FromByteArray<T>(byte[] data, out T result) where T : IBinarySerializable, new()
	{
		int version;
		return FromByteArray<T>(data, out result, out version);
	}

	public static bool FromByteArray<T>(byte[] data, out T result, out int version) where T : IBinarySerializable, new()
	{
		result = default(T);
		version = VersionManager.GameVersion;
		if (data == null)
		{
			return false;
		}
		try
		{
			T val = new T();
			using (MemoryStream input = new MemoryStream(data))
			{
				BinaryReader reader = new BinaryReader(input);
				version = ReadVersion(reader);
				val.Deserialize(reader, version);
			}
			result = val;
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to deserialize with error {0}", new object[1] { ex });
			return false;
		}
	}
}
