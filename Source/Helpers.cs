using System;
using System.Linq;
using System.Collections;

namespace System.Collections.Generic
{

static class ExtensionMethods
{
    public static U GetValueOr<T, U>(
        this Dictionary<T, U> dict,
        T key,
        U defaultValue)
    {
        if (dict.TryGetValue(key, out var value))
        {
            return value;
        }
        return defaultValue;
    }

    public static U GetValue<T, U>(
        this Dictionary<T, U> dict,
        T key)
    {
        dict.TryGetValue(key, out var value);
        return value;
    }

    public static U GetOrAdd<T, U>(
        this IDictionary<T, U> dict,
        T key)
        where U : new()
    {
        if (!dict.ContainsKey(key))
        {
            return dict[key] = new U();
        }
        return dict[key];
    }

    public static LinkedList<T> ForEach<T>(this LinkedList<T> list, Action<T> action)
    {
        foreach (var element in list)
        {
            action(element);
        }

        return list;
    }

    public static HashSet<T> ForEach<T>(this HashSet<T> list, Action<T> action)
    {
        foreach (var element in list)
        {
            action(element);
        }

        return list;
    }

    public static bool Empty<T>(this HashSet<T> collection)
    {
        return collection.Count == 0;
    }

    public static bool IsAnyOf<T>(this T t, params T[] values)
    {
        foreach (T value in values)
        {
            if (t.Equals(value))
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsAnyOf<T>(this T t, List<T> values)
    {
        foreach (T value in values)
        {
            if (t.Equals(value))
            {
                return true;
            }
        }
        return false;
    }

    public static void Pop<T>(this Stack<T> stack, int count)
    {
        for (var i = 0; i < count; ++i)
        {
            stack.Pop();
        }
    }
}

}
