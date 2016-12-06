using UnityEngine;
using System.Collections;

public static class UIHelper
{
    public static T GetComponentFromChild<T>(this Component mono, string name) where T : Component
    {
        T com = null;
        var go = mono.transform.FindChild(name);
        if (go)  com = go.GetComponent<T>();
        else Debug.LogErrorFormat("Cannot found gameObject :{0}", name);
        return com;
    }

    public static RectTransform RectTransform(this Component mono)
    {
        return mono.transform as RectTransform;
    }
}