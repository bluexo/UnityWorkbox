
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
    using Generic;

    public static class CollectionExtentions
    {
        private readonly static Random random = new Random();

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

        public static T RandomItem<T>(this IList<T> arr)
        {
            if (arr.Count > 0)
            {
                return arr[random.Next(0, arr.Count)];
            }
            else return default(T);
        }

        public static void Replace<Tkey, TValue>(this IDictionary<Tkey, TValue> dict, Tkey k, TValue v)
        {
            if (dict.ContainsKey(k)) dict.Remove(k);
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
            var rand = random.Next(startIndex, arr.Length);
            return arr[rand];
        }

        public static void Foreach<Tkey, TValue>(this IDictionary<Tkey, TValue> dict, Action<Tkey, TValue> action)
        {
            foreach (var pair in dict)
            {
                if (action == null) return;
                action(pair.Key, pair.Value);
            }
        }

        public static void Foreach<Tkey, TValue>(this IDictionary<Tkey, TValue> dict, Func<KeyValuePair<Tkey, TValue>, bool> action)
        {
            foreach (var pair in dict)
            {
                if (action == null) return;
                if (!action(pair)) break;
            }
        }

        public static void Foreach<T>(this IEnumerable<T> arr, Action<T> action)
        {
            if (action == null) return;
            foreach (var t in arr) action(t);
        }

        public static void Foreach<T>(this IEnumerable<T> arr, Func<T, bool> action)
        {
            if (action == null) return;
            foreach (var t in arr) if (!action(t)) break;
        }

        public static T FindFirstOrDefault<T>(this IEnumerable<T> arr, Predicate<T> predicate = null)
        {
            foreach (var a in arr)
            {
                if (predicate == null) return a;
                if (predicate(a)) return a;
            }
            return default(T);
        }

        public static IEnumerable<T> FindAll<T>(this IEnumerable<T> arr, Predicate<T> predicate = null)
        {
            foreach (var a in arr)
            {
                if (!predicate(a)) continue;
                yield return a;
            }
        }

        public static string ToArrayString<T>(this IEnumerable<T> collection, char separator = ' ')
        {
            var str = string.Empty;
            foreach (var item in collection) str += item.ToString() + separator;
            return str;
        }

        public static string ToArrayString<T>(this IEnumerable<T> collection, Func<T, string> toString, char separator = ' ')
        {
            var str = string.Empty;
            foreach (var item in collection) str += toString(item) + separator;
            return str;
        }

        public static int Int(this Enum value)
        {
            return (int)Enum.ToObject(value.GetType(), value);
        }

        public static bool Eq(this Enum left, Enum right)
        {
            var lv = left.Int();
            var rv = right.Int();
            return (lv | rv) == rv;
        }

        public static TEnum ToEnum<TEnum>(this int value)
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), value);
        }
    }
}
