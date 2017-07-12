namespace UnityEngine
{
    /// <summary>
    /// MonoBehaviour Singleton , if subclass has not attach to GameObject , will be create root GameObject that has a name equals <see cref="typeof(T).FullName"/>
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

        private static T instance;

        /// <summary>
        /// 如果子类重写 Awake() 方法，必须调用基类的 Awake()
        /// if subclass override Awake() , must be call base.Awake() 
        /// </summary>
        protected virtual void Awake()
        {
            if (!instance) {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            } else {
                if (instance && !instance.Equals(this)) {
                    Destroy(instance);
                    Debug.LogErrorFormat("The {0} not allow running multiple instances , the redundant will be Destroy!", typeof(T).FullName);
                }
            }
        }
    }
}