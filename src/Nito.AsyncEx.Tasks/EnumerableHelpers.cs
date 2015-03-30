using System;
using System.Collections.Generic;

internal static class EnumerableHelpers
{
    public static IEnumerable<T> Return<T>(params T[] values)
    {
        return values;
    }

    public static IEnumerable<T> Append<T>(this IEnumerable<T> @this, T item)
    {
        foreach (var entry in @this)
            yield return entry;
        yield return item;
    }
}