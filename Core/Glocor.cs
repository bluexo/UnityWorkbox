using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arthas.Common
{
    /// <summary>
    /// 全局的协程载体 , 用于执行全局异步操作
    /// </summary>
    public class Glocor : SingletonBehaviour<Glocor>
    {
        private static HashSet<Coroutine> coroutines = new HashSet<Coroutine>();

        /// <summary>
        /// 当前运行的全局协程数量
        /// </summary>
        public static int Count { get { return coroutines.Count; } }

        /// <summary>
        /// 运行一个协程
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static Coroutine Run(IEnumerator collection)
        {
            var cor = Instance.StartCoroutine(collection);
            coroutines.Add(cor);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogFormat("<color=cyan>Start a global coroutine , count = <size=14>{0}</size></color> ", Count);
#endif
            return cor;
        }

        /// <summary>
        /// 终止一个协程
        /// </summary>
        /// <param name="cor"></param>
        public static void Stop(Coroutine cor)
        {
            coroutines.Remove(cor);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogFormat("<color=cyan>Stop a global coroutine , count = <size=14>{0}</size></color> ", Count);
#endif
            Instance.StopCoroutine(cor);
        }

        /// <summary>
        /// 终止一个协程
        /// </summary>
        /// <param name="cors"></param>
        public static void Stop(params Coroutine[] cors)
        {
            for (var i = 0; i < cors.Length; i++)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogFormat("<color=cyan>Stop a global coroutine , count = <size=14>{0}</size></color> ", Count);
#endif
                if (cors[i] != null)
                {
                    coroutines.Remove(cors[i]);
                    Instance.StopCoroutine(cors[i]);
                }
            }
        }

        /// <summary>
        /// 停止所有
        /// </summary>
        public static void StopAll()
        {
            coroutines.Clear();
            Instance.StopAllCoroutines();
        }
    }
}
