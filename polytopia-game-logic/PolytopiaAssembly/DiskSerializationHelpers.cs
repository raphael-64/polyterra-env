using System;
using System.IO;
using Polytopia.IO;

public static class DiskSerializationHelpers
{
	public static bool ToDisk<T>(T serializable, string filename, int version, out Exception exception) where T : IBinarySerializable
	{
		exception = null;
		try
		{
			string text = filename + "_tmp";
			using (BinaryWriter binaryWriter = new BinaryWriter(PolytopiaFile.Open(text, FileMode.Create)))
			{
				binaryWriter.Write(version);
				serializable.Serialize(binaryWriter, version);
			}
			if (PolytopiaFile.Exists(filename))
			{
				PolytopiaFile.Delete(filename);
			}
			PolytopiaFile.Move(text, filename);
			return true;
		}
		catch (Exception ex)
		{
			Log.Error("Failed to save data with error {0}", new object[1] { ex });
			exception = ex;
			return false;
		}
	}

	public static bool IsDiskFullException(Exception ex)
	{
		if (ex.HResult != -2147024857)
		{
			return ex.HResult == -2147024784;
		}
		return true;
	}

	public static bool FromDisk<T>(string filename, out T result) where T : IBinarySerializable, new()
	{
		int version;
		return FromDisk<T>(filename, out result, out version);
	}

	public static bool FromDisk<T>(string filename, out T result, out int version) where T : IBinarySerializable, new()
	{
		result = default(T);
		version = VersionManager.GameVersion;
		if (string.IsNullOrEmpty(filename) || !PolytopiaFile.Exists(filename))
		{
			return false;
		}
		try
		{
			T val = new T();
			using (BinaryReader reader = new BinaryReader(PolytopiaFile.Open(filename, FileMode.Open)))
			{
				version = SerializationHelpers.ReadVersion(reader);
				val.Deserialize(reader, version);
			}
			result = val;
			return true;
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to load from disk with error {0}", new object[1] { ex });
			return false;
		}
	}
}
