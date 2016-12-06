using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public static class UIHelper
{
    public static T GetComponentFromChild<T>(this Component comp, string name) where T : Component
    {
        T com = null;
        var go = comp.transform.FindChild(name);
        if (go)  com = go.GetComponent<T>();
        else Debug.LogErrorFormat("Cannot found gameObject :{0}", name);
        return com;
    }

    public static RectTransform RectTransform(this UIBehaviour comp)
    {
        return comp.transform as RectTransform;
    }
}