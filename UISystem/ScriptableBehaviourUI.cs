using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public abstract class BaseScriptableInvoker : MonoBehaviour
    {
        public abstract void Initialize();

        public abstract object[] Invoke(string funcName, params object[] parameters);
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

        public static void InitializeComponents()
        {
            var uis = UIManager.Instance.GetComponentsInChildren<ScriptableBehaviourUI>(true);
            for (var i = 0; i < uis.Length; i++) {
                uis[i].Initialize();
            }
        }

        public void Initialize()
        {
            Invoker = GetComponent<BaseScriptableInvoker>();
            if (Invoker != null) {
                Invoker.Initialize();
                Invoker.Invoke("Initialize", this);
            } else
                Debug.LogErrorFormat("Cannot found LuaInvoker on UIGameobject {0}!!!", gameObject.name);
        }

        protected override void Start()
        {
            base.Start();
            if (Invoker != null) Invoker.Invoke("Start");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Invoker != null) Invoker.Invoke("OnEnable");
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Invoker != null) Invoker.Invoke("OnDisable");
        }

        public override void Show()
        {
            UIManager.AddUI(this);
            base.Show();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Down) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Up) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Enter) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Enter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Exit) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Exit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Drag) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Drag);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Drop) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Drop);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Click) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Click);
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Invoker != null && (pointerEventType & PointerEventType.Scroll) != 0)
                Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Scroll);
        }
    }
}
