using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public static class UIExtensions
{
    public static T GetComponentFromChild<T>(this Component current, string name) where T : Component
    {
        T component = null;
        var go = current.transform.Find(name);
        if (go) component = go.GetComponent<T>();
        else Debug.LogErrorFormat("Cannot found <color=cyan>[{0}]</color> child <color=cyan>[{1}]</color> or childs component <color=cyan>[{2}]</color>",
            current.name, name, typeof(T).ToString());
        return component;
    }

    public static T GetComponentFromChild<T>(this GameObject obj, string name) where T : Component
    {
        return obj.transform.GetComponentFromChild<T>(name);
    }


    public static RectTransform RectTransform(this UIBehaviour comp)
    {
        return comp.transform as RectTransform;
    }

    public static void Overwrite(this RectTransform rect, RectTransform orgin)
    {
        rect.anchoredPosition = orgin.anchoredPosition;
        rect.localScale = orgin.localScale;
        rect.anchorMin = orgin.anchorMin;
        rect.anchorMax = orgin.anchorMax;
        rect.offsetMin = orgin.offsetMin;
        rect.offsetMax = orgin.offsetMax;
        rect.pivot = orgin.pivot;
        rect.sizeDelta = orgin.sizeDelta;
    }
}