using System;
using System.Collections;
using UnityEngine;

namespace Arthas.UI
{
    public interface ILuaInvoker
    {
        void Initialize();

        void Invoke(string funcName, params object[] parameters);
    }

    public class LuaBehaviourUI : BaseUI
    {
        public ILuaInvoker Invoker { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Invoker = GetComponent<ILuaInvoker>();
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
    }
}
