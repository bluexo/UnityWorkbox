using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arthas.UI
{
    public enum PointerEventType { Click, Down, Up, Enter, Exit, Drag, Drop }

    public abstract class BaseLuaInvoker : MonoBehaviour
    {
        public abstract void Initialize();

        public abstract object[] Invoke(string funcName, params object[] parameters);
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
        public BaseLuaInvoker Invoker { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Invoker = GetComponent<BaseLuaInvoker>();
            if (Invoker != null) {
                Invoker.Initialize();
                Invoker.Invoke("Awake");
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
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Down);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Up);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Enter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Exit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Drag);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Drop);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Invoker != null) Invoker.Invoke("OnPointerEvent", eventData, PointerEventType.Click);
        }
    }
}
