using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

#if LUA

namespace Arthas.UI
{

#if UNITY_EDITOR
    using UnityEditor;

    [InitializeOnLoad]
    [CustomEditor(typeof(LuaBehaviourUI), true)]
    public class LuaBehaviourInitializer : Editor
    {
        static ILuaInvoker invoker;

        static LuaBehaviourInitializer()
        {
            var types = typeof(ILuaInvoker).Assembly.GetTypes();
            var type = Array.Find(types, m => m.IsSubclassOf(typeof(ILuaInvoker)));
            if (type == null) {
                Debug.LogError("Cannot found LuaInvoker SubClass , LuaBehaviourUI will not invok ...!");
                return;
            }
            invoker = (ILuaInvoker)Activator.CreateInstance(type);
        }

    }
#endif

    public interface ILuaInvoker
    {
        void CallMethod(string funcName, params object[] parameters);
    }

    public class LuaBehaviourUI : BaseUI
    {
        [SerializeField]
        public ILuaInvoker luaInvoker;

        protected override void Awake()
        {
            base.Awake();
            luaInvoker.CallMethod("Awake");
        }

        protected override void Start()
        {
            base.Start();
            luaInvoker.CallMethod("Start");
        }

        public override void Hide()
        {
            base.Hide();
            luaInvoker.CallMethod("Show");
        }

        public override void Show()
        {
            base.Show();
            luaInvoker.CallMethod("Hide");
        }
    }
}

#endif