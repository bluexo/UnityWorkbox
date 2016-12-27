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


    public static class CollectionExtentions
    {
        public static T First<T>(this IList<T> arr)
        {
            if (arr.Count > 0)
                return arr[0];
            else
                return default(T);
        }

        public static T Last<T>(this IList<T> arr)
        {
            if (arr.Count > 0)
                return arr[arr.Count - 1];
            else return default(T);
        }
    }
}
