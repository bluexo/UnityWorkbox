namespace UnityEngine
{
    /// <summary>
    /// MonoBehaviour Singleton , That will be create root GameObject which has a name equals <see cref="typeof(T).FullName"/> , if subclass hadn't attach to GameObject.
    /// MonoBehaviour 单例基类，如果子类没有附加到物体上，将会自动创建全局对象 , 以  <see cref="typeof(T).FullName"/> 来命名
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
                        var go = new GameObject(typeof(T).FullName);
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }

        protected static T instance;

        /// <summary>
        /// The subclass must be call base.Awake() , if it override Awake()
        /// 如果子类重写Awake方法，必须调用基类的Awake
        /// </summary>
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