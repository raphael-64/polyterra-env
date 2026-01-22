using System;
using System.IO;

namespace Polytopia.IO;

public static class PolytopiaFile
{
	public static void WriteAllBytes(string path, byte[] bytes)
	{
		File.WriteAllBytes(path, bytes);
	}

	public static byte[] ReadAllBytes(string path)
	{
		return File.ReadAllBytes(path);
	}

	public static string ReadAllText(string path)
	{
		return File.ReadAllText(path);
	}

	public static Stream Open(string path, FileMode mode, FileAccess access = FileAccess.ReadWrite)
	{
		return File.Open(path, mode);
	}

	public static DateTime GetLastWriteTime(string path)
	{
		return File.GetLastWriteTime(path);
	}

	public static bool Exists(string path)
	{
		return File.Exists(path);
	}

	public static void Move(string fromName, string toName)
	{
		File.Move(fromName, toName);
	}

	public static void Delete(string path)
	{
		File.Delete(path);
	}
}
