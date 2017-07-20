using UnityEngine;

using System.Threading;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR

#endif

public class TaskWorker : MonoBehaviour
{
    private volatile bool isCompleted = false;
    private List<int> arr = new List<int>();

    // Use this for initialization
    private IEnumerator Start()
    {
        Debug.Log("EnterThread");
        ThreadPool.QueueUserWorkItem(o => {
            for (var i = 0; i < 10000; i++) arr.Add(i);
            Thread.Sleep(10000);
            isCompleted = true;
        });
        yield return new WaitUntil(() => isCompleted);
        Debug.Log("QuitThread: " + arr.Count);
    }
}
