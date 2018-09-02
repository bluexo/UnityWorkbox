using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Arthas.Common;

namespace Arthas.UI
{
    [Flags]
    public enum PointerEventType
    {
        Nothing = 0,
        Click = 2,
        Down = 4,
        Up = 8,
        Enter = 16,
        Exit = 32,
        Drag = 64,
        Drop = 128,
        Scroll = 256,
        Everything = -1
    }

    [DisallowMultipleComponent]
    public class ScriptableBehaviourUI : BaseUI,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IScrollHandler,
        IDragHandler,
        IDropHandler
    {
        public BaseScriptableInvoker Invoker { get; private set; }

        [SerializeField, EnumMaskField]
        private PointerEventType pointerEventType;

        public void Initialize()
        {
            Invoker = GetComponent<BaseScriptableInvoker>();
            if (Invoker)
            {
                Invoker.Initialize();
                Invoker.InvokeScript("Initialize", this);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogFormat("{0} Initialized!", name);
#endif
            }
            else
                Debug.LogErrorFormat("Cannot found LuaInvoker on UIGameobject {0}!!!", gameObject.name);
        }

        protected override void Start()
        {
            base.Start();
            if (Invoker) Invoker.InvokeScript("Start", this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Invoker) Invoker.InvokeScript("OnEnable");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Invoker) Invoker.InvokeScript("OnDisable");
        }

        public override void Show()
        {
            UIManager.AddUI(this);
            base.Show();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Down) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Up) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Enter) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Enter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Exit) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Exit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Drag) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Drag);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Drop) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Drop);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Click) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Click);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Scroll) != 0)
                Invoker.InvokeScript("OnPointerEvent", eventData, PointerEventType.Scroll);
        }

        public object Invoke(string methodName, params object[] param)
        {
            return Invoker.InvokeScript(methodName, param);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Invoker) Invoker.Dispose();
        }
    }
}
