using System.Collections;
using System.Collections.Generic;

namespace System
{
    // Action
    public delegate void Action<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

    public delegate void Action<T1, T2, T3, T4, T5, T6>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

    public delegate void Action<T1, T2, T3, T4, T5, T6, T7>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);

    public delegate void Action<T1, T2, T3, T4, T5, T6, T7, T8>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);

    //Func
    public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

    public delegate TResult Func<T1, T2, T3, T4, T5, T6, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6);

    public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7);

    public delegate TResult Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8);
}

namespace System.Collections
{
    public static class CollectionExtentions
    {
        public static T First<T>(this IList<T> arr)
        {
            if (arr.Count > 0) return arr[0];
            else return default(T);
        }

        public static T Last<T>(this IList<T> arr)
        {
            if (arr.Count > 0) return arr[arr.Count - 1];
            else return default(T);
        }

        public static void Replace<Tkey, TValue>(this IDictionary<Tkey, TValue> dict, Tkey k, TValue v)
        {
            if (dict.ContainsKey(k))
                dict.Remove(k);
            dict.Add(k, v);
        }

        public static T[] Reverse<T>(this T[] arr)
        {
            Array.Reverse(arr);
            return arr;
        }

        public static T Random<T>(this T[] arr, int startIndex = 0)
        {
            if (arr.Length <= 0) return default(T);
            var rand = UnityEngine.Random.Range(startIndex, arr.Length);
            return arr[rand];
        }

        public static void Foreach<Tkey, TValue>(this IDictionary<Tkey, TValue> dict, Action<KeyValuePair<Tkey, TValue>> action)
        {
            foreach (var pair in dict) {
                if (action != null) {
                    action(pair);
                }
            }
        }

        public static void Foreach(this Array arr, Action<int> action)
        {
            int len = arr.Length;
            while (len-- > 0) {
                if (action != null) {
                    action(len);
                }
            }
        }
    }
}
