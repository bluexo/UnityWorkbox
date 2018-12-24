using Arthas.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arthas
{
#if USE_XLUA
    [RequireComponent(typeof(XLuaInvoker))]
#endif
    /// <summary>
    /// 用于和脚本语言交互的组件
    /// </summary>
    public class ScriptableBehaviour : MonoBehaviour
    {
        public BaseScriptableInvoker Invoker { get; private set; }

        [SerializeField]
        private bool enableUpdate = false,
            enableFixedUpdate = false,
            enableLateUpdate = false;

        protected virtual void Awake()
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
                Debug.LogErrorFormat("Cannot found LuaInvoker on Gameobject {0}!!!", gameObject.name);
        }

        protected virtual void Start() => Invoker?.InvokeScript(nameof(Start), this);

        protected virtual void OnEnable() => Invoker?.InvokeScript(nameof(OnEnable));

        protected virtual void OnDisable() => Invoker?.InvokeScript(nameof(OnDisable));

        private void Update()
        {
            if (enableUpdate) Invoker?.InvokeScript(nameof(Update));
        }

        private void FixedUpdate()
        {
            if (enableFixedUpdate) Invoker?.InvokeScript(nameof(FixedUpdate));
        }

        private void LateUpdate()
        {
            if (enableLateUpdate) Invoker.InvokeScript(nameof(LateUpdate));
        }

        private void OnApplicationFocus(bool focus) => Invoker.InvokeScript(nameof(OnApplicationFocus), focus);

        private void OnApplicationPause(bool pause) => Invoker?.InvokeScript(nameof(OnApplicationPause), pause);

        private void OnApplicationQuit() => Invoker?.InvokeScript(nameof(OnApplicationQuit));

        public object Invoke(string methodName, params object[] param) => Invoker?.InvokeScript(methodName, param);

        protected virtual void OnDestroy()
        {
            Invoker?.InvokeScript(nameof(OnDestroy));
            Invoker?.Dispose();
        }
    }
}
