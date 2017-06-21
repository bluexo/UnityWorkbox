namespace UnityEngine
{
    /// <summary>
    /// Singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
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

        private static T instance;

        protected virtual void Awake()
        {
            if (!instance) {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            } else {
                if (instance && !instance.Equals(this))
                    Destroy(instance);
            }
        }
    }
}