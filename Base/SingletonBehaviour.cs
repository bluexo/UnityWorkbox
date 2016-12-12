using UnityEngine;

/// <summary>
///Singleton
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object enterLock = new object();

    public static T Instance {
        get {
            lock (enterLock) {
                if (!instance) {
                    instance = FindObjectOfType<T>();
                    if (!instance) {
                        var go = new GameObject(typeof(T).ToString());
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
    }

    private static T instance;

    protected virtual void Awake()
    {
        if (!instance) {
            instance = this as T;
            DontDestroyOnLoad(instance.gameObject);
        }
        else {
            if (!instance.Equals(this))
                Destroy(gameObject);
        }
    }
}