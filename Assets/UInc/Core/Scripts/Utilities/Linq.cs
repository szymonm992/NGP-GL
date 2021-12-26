using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UInc.Core.Utilities
{
	public static class Linq
	{
		//Source: https://stackoverflow.com/a/489421/1817727

		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			HashSet<TKey> seenKeys = new HashSet<TKey>();
			foreach (TSource element in source)
			{
				if (seenKeys.Add(keySelector(element)))
				{
					yield return element;
				}
			}
		}

		public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
				action(item);
		}

		public static void SetOrIncrement<K>(this Dictionary<K, int> dictionary, K key, int add = 1)
		{
			int value;
			if (dictionary.TryGetValue(key, out value))
			{
				dictionary[key] = value + add;
			}
			else
			{
				dictionary[key] = add;
			}
		}

		public static void UpdateKey<TKey, TValue>(this IDictionary<TKey, TValue> dic,
									  TKey fromKey, TKey toKey)
		{
			TValue value = dic[fromKey];
			dic.Remove(fromKey);
			dic[toKey] = value;
		}

		public static void Merge<TKey, TValue>(this IDictionary<TKey, TValue> to, IDictionary<TKey, TValue> data)
		{
			foreach (var item in data)
			{
				if (to.ContainsKey(item.Key) == false)
				{
					to.Add(item.Key, item.Value);
				}
			}
		}
	}
}