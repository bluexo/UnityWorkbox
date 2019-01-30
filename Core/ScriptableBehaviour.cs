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

        [SerializeField]
        public GeneralVisualConfig data;

        protected virtual void Awake()
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
                Debug.LogErrorFormat("Cannot found LuaInvoker on Gameobject {0}!!!", gameObject.name);
        }

        protected virtual void Start() => Invoker?.Invoke(nameof(Start));

        protected virtual void OnEnable() => Invoker?.Invoke(nameof(OnEnable));

        protected virtual void OnDisable() => Invoker?.Invoke(nameof(OnDisable));

        private void Update()
        {
            if (enableUpdate) Invoker?.Invoke(nameof(Update));
        }

        private void FixedUpdate()
        {
            if (enableFixedUpdate) Invoker?.Invoke(nameof(FixedUpdate));
        }

        private void LateUpdate()
        {
            if (enableLateUpdate) Invoker.Invoke(nameof(LateUpdate));
        }

        private void OnApplicationFocus(bool focus) => Invoker.Invoke(nameof(OnApplicationFocus), focus);

        private void OnApplicationPause(bool pause) => Invoker?.Invoke(nameof(OnApplicationPause), pause);

        private void OnApplicationQuit() => Invoker?.Invoke(nameof(OnApplicationQuit));

        public object Invoke(string methodName, params object[] param) => Invoker?.Invoke(methodName, param);

        public object InvokeStatic(string methodName, params object[] param) => Invoker?.InvokeStatic(methodName, param);

        protected virtual void OnDestroy()
        {
            Invoker?.Invoke(nameof(OnDestroy));
            Invoker?.Dispose();
        }
    }
}
