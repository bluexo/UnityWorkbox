using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

namespace Arthas.Common
{
#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(TaskWorker))]
    public class TaskWorkerEditor : Editor
    {
        private readonly HashSet<TaskWorkItem> items = new HashSet<TaskWorkItem>();
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var style = new GUIStyle(EditorStyles.largeLabel) { richText = true, alignment = TextAnchor.MiddleLeft };
            foreach (var i in TaskWorker.WorkItems) items.Add(i);
            foreach (var item in items) {
                var rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(32));
                EditorGUI.DrawRect(rect, (item.completed ? Color.yellow : Color.green) / 1.5f);
                var label = string.Format("<size=15><color=white>WorkThread: </color><color=cyan>{0}</color> {1} </size>", item.threadId, item.completed ? "Completed" : "Running");
                EditorGUI.LabelField(rect, label, style);
            }
        }
    }
#endif

    [Serializable]
    public class TaskWorkItem
    {
        public volatile bool completed;
        public int threadId;
        public object result;
        public Action<TaskWorkItem> worker;
    }

    /// <summary>
    /// 简单的多线程使用方式，可以将MonoBehaviour中的密集运算在子线程中运行，并将结果返回到主线程中 , 从而避免阻塞主线程导致的卡顿现象
    /// 可以在检视窗口中查看线程的运行状态
    /// </summary>
    public class TaskWorker : SingletonBehaviour<TaskWorker>
    {
        public static IEnumerable<TaskWorkItem> WorkItems { get { return workItems.Keys; } }
        private static readonly Dictionary<TaskWorkItem, Action<object>> workItems = new Dictionary<TaskWorkItem, Action<object>>();
        private object enterLock = new object();

        /// <summary>
        /// 将任务添加到工作线程，在线程池内运行，无法手动结束
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="callback"></param>
        public static void AddWork(Action<TaskWorkItem> worker, Action<object> callback)
        {
            lock (Instance.enterLock) {
                var item = new TaskWorkItem() { completed = false, worker = worker };
                workItems.Add(item, callback);
                ThreadPool.QueueUserWorkItem(o => {
                    item.threadId = Thread.CurrentThread.ManagedThreadId;
                    item.worker(item);
                    item.completed = true;
                });
            }
        }

        /// <summary>
        /// 将任务添加到工作线程，新建一个线程实例，可以手动结束
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Thread AddThreadWork(Action<TaskWorkItem> worker, Action<object> callback, ThreadPriority priority = ThreadPriority.Normal)
        {
            lock (Instance.enterLock) {
                var item = new TaskWorkItem() { completed = false, worker = worker };
                workItems.Add(item, callback);
                var thread = new Thread(() => {
                    item.threadId = Thread.CurrentThread.ManagedThreadId;
                    item.worker(item);
                    item.completed = true;
                });
                thread.Priority = priority;
                thread.Start();
                return thread;
            }
        }

        /// <summary>
        /// 以异步的方式将任务加入到工作线程，然后在协程中等待任务结束，推荐的使用方式
        /// </summary>
        /// <param name="worker"></param>
        /// <returns></returns>
        public static IEnumerator WaitWork(Action<TaskWorkItem> worker)
        {
            var item = new TaskWorkItem() { completed = false, worker = worker };
            workItems.Add(item, null);
            ThreadPool.QueueUserWorkItem(o => {
                item.threadId = Thread.CurrentThread.ManagedThreadId;
                item.worker(item);
                item.completed = true;
            });
            yield return new WaitUntil(() => item.completed);
        }

        private void Update()
        {
            var items = new TaskWorkItem[workItems.Count];
            workItems.Keys.CopyTo(items, 0);
            for (var i = 0; i < items.Length; i++) {
                var key = items[i];
                if (key.completed && workItems.ContainsKey(key)) {
                    var item = workItems[key];
                    if (item != null) item.Invoke(key.result);
                    workItems.Remove(key);
                }
            }
        }
    }
}
