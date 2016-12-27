using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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