using System;
using System.Collections.Generic;

namespace Enhancer.Extensions;

public static class CollectionExtensions
{
    /// <summary>Execute code for every element in a collection and pass-through the original value.</summary>
    /// <typeparam name="TSource">The inner type of the collection</typeparam>
    /// <param name="source">The collection</param>
    /// <param name="action">The action to execute</param>
    /// <returns>The collection, unchanged</returns>
    public static IEnumerable<TSource> Tap<TSource>(this IEnumerable<TSource>? source, Action<TSource>? action)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (action is null) throw new ArgumentNullException(nameof(action));
        return TapIterator(source, action);
    }

    static IEnumerable<TSource> TapIterator<TSource>(IEnumerable<TSource> source, Action<TSource> action)
    {
        foreach (TSource element in source) {
            action(element);
            yield return element;
        }
    }
}
