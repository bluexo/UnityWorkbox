using System;
using System.Collections;
using UnityEngine;

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


public static class UnityExtentions
{
    public static Coroutine Invoke(this MonoBehaviour comp, Action action, float time = 0)
    {
        return comp.StartCoroutine(InvokeAsync(comp, action, time));
    }

    public static Coroutine InvokeRepeating(this MonoBehaviour comp, Action action, float delay, float rate)
    {
        return comp.StartCoroutine(InvokeRepatingAsync(comp, action, delay, rate));
    }

    private static IEnumerator InvokeAsync(MonoBehaviour comp, Action action, float time)
    {
        yield return new WaitForSeconds(time);
        if (comp) {
            action.Invoke();
        }
    }

    private static IEnumerator InvokeRepatingAsync(MonoBehaviour comp, Action action, float delay, float rate)
    {
        yield return new WaitForSeconds(delay);
        var duration = new WaitForSeconds(rate);
        while (true) {
            yield return duration;
            if (comp) {
                action.Invoke();
            }
        }
    }
}