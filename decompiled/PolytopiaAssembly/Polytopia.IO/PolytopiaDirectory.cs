using System.IO;
using UnityEngine;

namespace Polytopia.IO;

public static class PolytopiaDirectory
{
	public static readonly string PERSISTENT_DATA_PATH = Application.persistentDataPath;

	public static readonly string IMAGE_CACHE_ROOT = Application.persistentDataPath;

	public static string[] GetFiles(string path, string pattern = null)
	{
		if (!string.IsNullOrEmpty(pattern))
		{
			return Directory.GetFiles(path, pattern);
		}
		return Directory.GetFiles(path);
	}

	public static string[] GetDirectories(string path, string pattern = null)
	{
		if (!string.IsNullOrEmpty(pattern))
		{
			return Directory.GetDirectories(path, pattern);
		}
		return Directory.GetDirectories(path);
	}

	public static void Move(string fromPath, string toPath)
	{
		Directory.Move(fromPath, toPath);
	}

	public static void Delete(string path, bool recursively)
	{
		Directory.Delete(path, recursively);
	}

	public static bool Exists(string path)
	{
		return Directory.Exists(path);
	}

	public static void CreateDirectory(string path)
	{
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}
}
