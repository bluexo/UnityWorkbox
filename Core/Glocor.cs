using System.Collections;

namespace UnityEngine
{
    /// <summary>
    /// 全局的协程载体 , 用于执行全局异步操作
    /// </summary>
    public class Glocor : SingletonBehaviour<Glocor>
    {
        /// <summary>
        /// 当前运行的全局协程数量
        /// </summary>
        public static int Count { get; private set; }

        /// <summary>
        /// 运行一个协程
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static Coroutine Run(IEnumerator collection)
        {
            Count++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogFormat("<color=cyan>Start a global coroutine , count = <size=14>{0}</size></color> ", Count);
#endif
            return Instance.StartCoroutine(collection);
        }

        /// <summary>
        /// 终止一个协程
        /// </summary>
        /// <param name="collection"></param>
        public static void Stop(IEnumerator collection)
        {
            Count--;
            Instance.StopCoroutine(collection);
        }

        /// <summary>
        /// 终止一个协程
        /// </summary>
        /// <param name="cor"></param>
        public static void Stop(Coroutine cor)
        {
            Count--;
            Instance.StopCoroutine(cor);
        }

        /// <summary>
        /// 停止所有
        /// </summary>
        public static void StopAll()
        {
            Count = 0;
            Instance.StopAllCoroutines();
        }
    }
}
