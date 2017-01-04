using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public static class UIExtensions
{
    public static T GetComponentFromChild<T>(this Component comp, string name) where T : Component
    {
        T com = null;
        var go = comp.transform.FindChild(name);
        if (go)  com = go.GetComponent<T>();
        else Debug.LogErrorFormat("Cannot found <color=cyan>[{0}]</color> child <color=cyan>[{1}]</color> or childs component <color=cyan>[{2}]</color>",
            comp.name,
            name,
            typeof(T).ToString());
        return com;
    }

    public static RectTransform RectTransform(this UIBehaviour comp)
    {
        return comp.transform as RectTransform;
    }
}