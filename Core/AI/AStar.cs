using UnityEngine;
using System.Collections;
using System;

public abstract class IAsyncWaiter : IEnumerator
{
    public virtual object Current { get { return null; } }
    bool IEnumerator.MoveNext() { return !Done; }

    public abstract bool Done { get; protected set; }
    public virtual void Reset() { Done = false; }
    public virtual void Complete() { Done = true; }
}


public class DestoryAsyncWaiter : IAsyncWaiter
{
    public override bool Done { get; protected set; }
}


public class AStar : MonoBehaviour
{
    DestoryAsyncWaiter waiter = new DestoryAsyncWaiter();

    IEnumerator Start()
    {
        yield return waiter;
        Debug.Log("Done");
    }

    void Update()
    {
        if (Time.time > 3) {
            waiter.Complete();
        }
    }
}