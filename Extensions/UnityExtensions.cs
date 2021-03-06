﻿using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public static class UnityExtensions
    {
        public static void Trace(this Component component, string message)
        {
            Debug.LogFormat("<color=green>[Trace] {0} : {1}.</color>", component.name, message);
        }

        public static void Trace(this Component component, params object[] args)
        {
            var message = args.ToArrayString(' ');
            Debug.LogFormat("<color=green>[Trace] {0} : {1}.</color>", component.name, message);
        }

        public static Coroutine Invoke(this MonoBehaviour comp, Action action, float time = 0)
        {
            return comp.StartCoroutine(InvokeAsync(comp, action, time));
        }

        public static Coroutine Invoke(this MonoBehaviour comp, Action action, YieldInstruction yieldInstruction)
        {
            return comp.StartCoroutine(InvokeAsync(comp, action, yieldInstruction));
        }

        public static Coroutine InvokeRepeating(this MonoBehaviour comp, Action action, float delay = 0, float duration = 0)
        {
            return comp.StartCoroutine(InvokeRepatingAsync(comp, action, delay, duration));
        }

        private static IEnumerator InvokeAsync(MonoBehaviour comp, Action action, float time)
        {
            yield return new WaitForSeconds(time);
            if (comp)
                action?.Invoke();
        }

        private static IEnumerator InvokeAsync(MonoBehaviour comp, Action action, YieldInstruction yieldInstruction)
        {
            yield return yieldInstruction;
            if (comp)
                action?.Invoke();
        }

        private static IEnumerator InvokeRepatingAsync(MonoBehaviour comp, Action action, float delay, float rate)
        {
            yield return new WaitForSeconds(delay);
            var duration = new WaitForSeconds(rate);
            while (true)
            {
                yield return duration;
                if (comp)
                    action?.Invoke();
            }
        }

        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        public static void DestroyObject(this MonoBehaviour comp)
        {
            if (comp) Object.Destroy(comp.gameObject);
        }

        public static void SetEnum(this Animator animator, string name, Enum value)
        {
            animator.SetInteger(name, value.Int());
        }

        public static void SetEnum(this Animator animator, int id, Enum value)
        {
            animator.SetInteger(id, value.Int());
        }
    }
}