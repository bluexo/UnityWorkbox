
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

public class WorkItemBase
{
    public volatile bool completed;
}

#endif
[Serializable]
public class TaskWorkItem : WorkItemBase
{
    public Action worker;
}

[Serializable]
public class TaskWorkItem<TResult> : WorkItemBase
{
    public Func<TResult> worker;
    public TResult result;
}

/// <summary>
/// 基于线程池的多线程使用方式，可以将密集运算在子线程中运行，并将结果返回到主线程中，可以在检视窗口中查看线程的运行状态
/// </summary>
public class TaskWorker : SingletonBehaviour<TaskWorker>
{
    private static readonly List<WorkItemBase> workItems = new List<WorkItemBase>();
    private static readonly List<WorkItemBase> removeList = new List<WorkItemBase>();
    private static object enterLock = new object();

    //public static void AddWork<TResult>(Func<TResult> worker, Action<TResult> func)
    //{
    //    lock (enterLock) {
    //        var item = new TaskWorkItem<TResult>() { completed = false, worker = worker };
    //        workItems.Add(item);
    //        ThreadPool.QueueUserWorkItem(o => {
    //            item.result = worker();
    //            item.completed = true;
    //        });
    //    }
    //}

    public static IEnumerator WaitWork(Action worker)
    {
        var item = new TaskWorkItem() { completed = false, worker = worker };
        ThreadPool.QueueUserWorkItem(o => {
            worker();
            item.completed = true;
        });
        yield return new WaitUntil(() => item.completed);
    }

    private void Update()
    {
        //workItems.ForEach(i => { if (i.completed) removeList.Add(i); });
        //removeList.ForEach(i => workItems.Remove(i));
        //removeList.Clear();
    }
}
