namespace System
{
    public class SingletonBase { }

    public class Singleton<T> : SingletonBase where T : SingletonBase, new()
    {
        private static object enterLock = new object();
        public static T Instance { get { lock (enterLock) return instance ?? (instance = new T()); } }
        private static T instance;
    }
}