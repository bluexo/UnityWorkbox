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
                Invoker.Invoke("Initialize", this);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogFormat("{0} Initialized!", name);
#endif
            }
            else
                Debug.LogErrorFormat("Cannot found LuaInvoker on UIGameobject {0}!!!", gameObject.name);

            foreach (var child in GetComponentsInChildren<ScriptableBehaviourUI>(true))
            {
                if (child.Equals(this)) continue;
                child.Initialize();
            }
        }

        protected override void Start()
        {
            base.Start();
            if (Invoker) Invoker.Invoke(nameof(Start));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Invoker) Invoker.Invoke(nameof(OnEnable));
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Invoker) Invoker.Invoke(nameof(OnDisable));
        }

        public override void Show()
        {
            UIManager.AddUI(this);
            base.Show();
        }

        private void OnApplicationFocus(bool focus) => Invoker.Invoke(nameof(OnApplicationFocus), focus);

        private void OnApplicationPause(bool pause) => Invoker?.Invoke(nameof(OnApplicationPause), pause);

        private void OnApplicationQuit() => Invoker?.Invoke(nameof(OnApplicationQuit));

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Down) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Up) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Enter) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Enter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Exit) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Exit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Drag) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Drag);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Drop) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Drop);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Click) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Click);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Invoker && (pointerEventType & PointerEventType.Scroll) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Scroll);
        }

        public object Invoke(string methodName, params object[] param)
        {
            return Invoker.Invoke(methodName, param);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Invoker) Invoker.Dispose();
        }
    }
}
