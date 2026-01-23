using System;
using System.Collections.Generic;
using System.Text;

public static class ArrayExtensions
{
	public static T[] Splice<T>(this T[] array, int from, int to)
	{
		int num = to - from;
		T[] array2 = new T[num];
		Array.Copy(array, from, array2, 0, num);
		return array2;
	}

	public static List<T> Splice<T>(this List<T> list, int from, int to)
	{
		int num = to - from;
		List<T> list2 = new List<T>(num);
		list2.AddRange(list.GetRange(from, num));
		return list2;
	}

	public static bool Contains<T>(this T[] list, T item)
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i] == null)
			{
				return item == null;
			}
			ref readonly T reference = ref list[i];
			object obj = item;
			if (reference.Equals(obj))
			{
				return true;
			}
		}
		return false;
	}

	public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
	{
		if (items == null)
		{
			return;
		}
		if (queue == null)
		{
			queue = new Queue<T>();
		}
		foreach (T item in items)
		{
			queue.Enqueue(item);
		}
	}

	public static string CombineToString<T>(this T[] array, string deliniator = null)
	{
		StringBuilder stringBuilder = new StringBuilder(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i]);
			if (deliniator != null)
			{
				stringBuilder.Append(deliniator);
			}
		}
		return stringBuilder.ToString();
	}
}
