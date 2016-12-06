using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单例脚本
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance
    {
        get
        {
            if (!instance)
            {
                var others = FindObjectsOfType<T>();
                if (others.Length == 1)
                {
                    instance = others[0];
                }
                else
                {
                    if (others.Length > 1)
                        foreach (var other in others) DestroyImmediate(other);
                    var go = new GameObject(typeof(T).ToString());
                    instance = go.AddComponent<T>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }
    private static T instance;
}
