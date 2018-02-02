using Arthas.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Common
{
    public interface IResetable<T> where T : Component
    {
        event System.Action<T> ResetEvent;

        bool IsCollected { get; }

        void ResetObject();
    }

    public class ObjectPool<TComponent> : SingletonBehaviour<ObjectPool<TComponent>>
        where TComponent : Component, IResetable<TComponent>
    {
        public IDictionary<int, Queue<TComponent>> ObjectQueue { get { return objectQueue; } }
        private readonly Dictionary<int, Queue<TComponent>> objectQueue = new Dictionary<int, Queue<TComponent>>();
        private readonly WaitForEndOfFrame waitForEnd = new WaitForEndOfFrame();
        private WaitForSeconds waitForCollect;

        [SerializeField] protected PoolObjectConfig objectArray;
        [SerializeField, Range(1, 20)] protected int initCount = 10;
        [SerializeField, Range(1, 100)] protected int maxOverload = 16;
        [SerializeField] private float collectInterval = .2f;
        [SerializeField] private bool isCollected = false;

        protected IEnumerator Start()
        {
            waitForCollect = new WaitForSeconds(collectInterval);
            if (objectArray == null || objectArray.Items == null)
            {
                Debug.LogError("Cannot found prefab");
                yield break;
            }
            for (var i = 0; i < objectArray.Items.Length; i++)
            {
                var item = objectArray.Items[i];
                yield return SpawnAsync(item);
            }
            if (isCollected) StartCoroutine(CollectPollingAsync());
        }

        protected IEnumerator CollectPollingAsync()
        {
            while (true)
            {
                yield return waitForCollect;
                foreach (var pair in objectQueue)
                {
                    if (pair.Value.Count < maxOverload) continue;
                    var comp = pair.Value.Dequeue();
                    if (comp && !comp.gameObject.activeSelf)
                    {
                        comp.ResetObject();
                        Destroy(comp.gameObject);
                    }
                }
            }
        }

        protected IEnumerator SpawnAsync(PoolObjectInfo item)
        {
            if (item == null) yield break;
            if (!objectQueue.ContainsKey(item.id)) objectQueue.Add(item.id, new Queue<TComponent>());
            for (var j = 0; j < initCount; j++)
            {
                var comp = Instantiate(item.prefab).GetComponent<TComponent>();
                if (!comp)
                {
                    Debug.LogErrorFormat("Cannot found [{0}] from object pool!", typeof(TComponent));
                    continue;
                }
                comp.transform.SetParent(transform);
                comp.gameObject.SetActive(false);
                objectQueue[item.id].Enqueue(comp);
                yield return waitForEnd;
            }
        }

        protected void Spawn(PoolObjectInfo item)
        {
            if (!objectQueue.ContainsKey(item.id)) objectQueue.Add(item.id, new Queue<TComponent>());
            for (var j = 0; j < initCount; j++)
            {
                var comp = Instantiate(item.prefab).GetComponent<TComponent>();
                if (!comp)
                {
                    Debug.LogErrorFormat("Cannot found [{0}] from object pool!", typeof(TComponent));
                    continue;
                }
                comp.transform.SetParent(transform);
                comp.gameObject.SetActive(false);
                objectQueue[item.id].Enqueue(comp);
            }
        }

        protected IEnumerator CollectAsync(int id, IEnumerable<TComponent> bullets, YieldInstruction yieldInstruction)
        {
            if (yieldInstruction != null) yield return yieldInstruction;
            if (!objectQueue.ContainsKey(id))
                objectQueue.Add(id, new Queue<TComponent>());
            foreach (var obj in bullets)
            {
                yield return waitForEnd;
                obj.ResetObject();
                obj.gameObject.SetActive(false);
                objectQueue[id].Enqueue(obj);
            }
        }

        public void Put(int id, TComponent comp, bool disable = true)
        {
            if (!comp || !comp.gameObject) return;
            comp.ResetObject();
            if (!objectQueue.ContainsKey(id)) objectQueue.Add(id, new Queue<TComponent>());
            if (disable) comp.gameObject.SetActive(false);
            objectQueue[id].Enqueue(comp);
        }

        public void DelayPut(int id, TComponent obj, YieldInstruction waitFor)
        {
            StartCoroutine(instance.CollectAsync(id, new TComponent[] { obj }, waitFor));
        }

        public TComponent Get(int id, bool active = true)
        {
#if UNITY_EDITOR
            Debug.LogFormat("Get {0} , Id:{1} from ObjectPool", typeof(TComponent), id);
#endif
            if (!objectQueue.ContainsKey(id))
                objectQueue.Add(id, new Queue<TComponent>());
            var queue = objectQueue[id];
            if (queue.Count < initCount)
            {
                var item = objectArray.Items.FirstOrDefault(o => o.id == id);
                if (item == null)
                {
                    Debug.LogErrorFormat("Cannot found Item : {0} ,Type:{1}", id, typeof(TComponent));
                    return default(TComponent);
                }
                StartCoroutine(SpawnAsync(item));
            }
            TComponent obj = queue.Dequeue();
            obj.gameObject.SetActive(active);
            return obj;
        }

        public void Clear()
        {
            foreach (var array in objectQueue.Values)
            {
                foreach (var obj in array)
                    if (obj) Destroy(obj.gameObject);
            }
            objectQueue.Clear();
            StopAllCoroutines();
        }
    }
}
