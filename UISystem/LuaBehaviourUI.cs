using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arthas.UI
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(LuaBehaviourUI), isFallback = true)]
    public class LuaBehaviourUIEditor : Editor
    {
        private void OnEnable()
        {
            var types = typeof(BaseLuaInvoker).Assembly.GetTypes();
            var invokerType = Array.Find(types, t => t.IsSubclassOf(typeof(BaseLuaInvoker)));
            var go = (target as Component).gameObject;
            var comp = go.GetComponent(typeof(BaseLuaInvoker));
            if (!comp && invokerType != null) go.AddComponent(invokerType);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

    public enum PointerEventType { Click, Down, Up, Enter, Exit, Drag, Drop }

    [DisallowMultipleComponent]
    public abstract class BaseLuaInvoker : MonoBehaviour
    {
        public abstract void Initialize();

        public abstract bool TryInvoke(string funcName, params object[] parameters);

        public virtual object[] Invoke(string funcName, params object[] parameters) { throw new NotImplementedException(); }
    }

    public sealed class LuaBehaviourUI : BaseUI,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IDragHandler,
        IDropHandler
    {
        private readonly HashSet<string> funcs = new HashSet<string>();

        public BaseLuaInvoker Invoker { get; private set; }

        protected override void Start()
        {
            base.Start();
            Invoker = GetComponent<BaseLuaInvoker>();
            if (Invoker != null) {
                Invoker.Initialize();
                Invoker.TryInvoke("Start");
            } else
                Debug.LogErrorFormat("Cannot found LuaInvoker on UIGameobject {0}!!!", gameObject.name);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Invoker != null) Invoker.TryInvoke("OnEnable");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Invoker != null) Invoker.TryInvoke("OnDisable");
        }

        public override void Show()
        {
            UIManager.AddUI(this);
            base.Show();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Enter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Exit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Drag);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Drop);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.TryInvoke("OnPointerEvent", eventData, PointerEventType.Click);
        }
    }
}
