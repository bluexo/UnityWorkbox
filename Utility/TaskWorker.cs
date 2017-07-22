
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

#endif
[Serializable]
public class TaskWorkerItem
{
    public volatile bool completed;
    public Action<TaskWorkerItem> worker;
}

public class TaskWorker : MonoBehaviour
{
    private readonly Queue<TaskWorkerItem> items = new Queue<TaskWorkerItem>();
    private volatile bool isCompleted = false;
    private List<int> arr = new List<int>();

    // Use this for initialization
    private IEnumerator Start()
    {
        ThreadPool.QueueUserWorkItem(o => {
            for (var i = 0; i < 10000; i++) arr.Add(i);
            Thread.Sleep(10000);
            isCompleted = true;
        });
        yield return new WaitUntil(() => isCompleted);
    }

    public void AddWork(Action<TaskWorkerItem> worker)
    {
        var item = new TaskWorkerItem() { completed = false, worker = worker };
        items.Enqueue(item);
        ThreadPool.QueueUserWorkItem(o => {
            worker(item);
            item.completed = true;
        });
    }

    public IEnumerator WaitWork(Action<TaskWorkerItem> worker)
    {
        var item = new TaskWorkerItem() { completed = false, worker = worker };
        items.Enqueue(item);
        ThreadPool.QueueUserWorkItem(o => {
            worker(item);
            item.completed = true;
        });
        yield return new WaitUntil(() => item.completed);
    }

    private void Update()
    {

    }
}
