using System.Collections.Generic;
using System.Linq;

namespace PolytopiaBackendBase;

public static class EnumerableExtensions
{
	public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
	{
		TSource[] array = null;
		int num = 0;
		foreach (TSource item in source)
		{
			if (array == null)
			{
				array = new TSource[size];
			}
			array[num++] = item;
			if (num == size)
			{
				yield return array;
				array = null;
				num = 0;
			}
		}
		if (array != null && num > 0)
		{
			yield return array.Take(num).ToArray();
		}
	}
}
