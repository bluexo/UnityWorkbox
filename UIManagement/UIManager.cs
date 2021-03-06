﻿using UnityWorkbox.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if WINDOWS_UWP
using System.Reflection;
#endif

namespace UnityWorkbox.UI
{
    public struct WindowInfo : IComparable<WindowInfo>
    {
        public int? Order { get; set; }
        public bool IsFloating { get; set; }
        public bool IsExclusive { get; set; }
        public BaseUI UI { get; set; }

        public void SetOrder(int floatingCount, int amountCount, int index)
        {
            var i = (IsFloating ? amountCount : amountCount - floatingCount) - index - 1;
            UI.transform.SetSiblingIndex(i);
            UI.gameObject.SetActive(true);
        }

        public bool ContainBrother(BaseUI ui)
        {
            return UI && UI.UIGroup.Contains(ui);
        }

        public int CompareTo(WindowInfo other)
        {
            return other.Order.Value.CompareTo(Order.Value);
        }
    }

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIManager : SingletonBehaviour<UIManager>
    {
        private static readonly Dictionary<BaseUI, WindowInfo> windows = new Dictionary<BaseUI, WindowInfo>();
        private static readonly List<WindowInfo> showedFloatingWindows = new List<WindowInfo>();
        private static readonly List<WindowInfo> showedWindows = new List<WindowInfo>();

        public Canvas Canvas { get; private set; }
        [SerializeField]
        private BaseUI startUI;
        [SerializeField, ArrayField]
        private BaseUI[] preloadPanels = { };

        [SerializeField, Tooltip("Overwrite camera on Awake")]
        private bool overwriteCamera = false;
        [SerializeField, HideInInspector]
        private string cameraTag = "MainCamera";

        protected override void Awake()
        {
            base.Awake();
            Canvas = GetComponent<Canvas>();
            if (!Canvas)
            {
                Debug.LogError("[UIManager] Cannot found [Canvas] Component!");
                return;
            }
            if (overwriteCamera)
            {
                Camera camera = null;
                var cameraObj = GameObject.FindGameObjectWithTag(cameraTag);
                if (cameraObj && (camera = cameraObj.GetComponent<Camera>()))
                {
                    Canvas.worldCamera = camera;
                }
                else
                {
                    Debug.LogErrorFormat("[UIManager] Cannot found [Camera] Component with tag:<color=cyan>{0}</color>", cameraTag);
                }
            }
            for (var i = 0; i < preloadPanels.Length; i++)
            {
                if (!preloadPanels[i]) continue;
                var orgin = preloadPanels[i].RectTransform;
                var go = Instantiate(preloadPanels[i].gameObject);
                go.name = orgin.name;
                var rect = go.GetComponent<RectTransform>();
                rect.SetParent(transform);
                rect.localPosition = Vector3.zero;
                rect.Overwrite(orgin);
            }
            var uis = GetComponentsInChildren<BaseUI>(true);
            for (var i = 0; i < uis.Length; i++) AddUI(uis[i]);

            var scriptableUIs = GetComponentsInChildren<ScriptableBehaviourUI>(true);
            for (var i = 0; i < scriptableUIs.Length; i++)
            {
                InitializeInternal(scriptableUIs[i]);
            }
        }

        private void InitializeInternal(ScriptableBehaviourUI ui)
        {
            if (!ui) return;
            ui.Initialize();
            var uis = ui.GetComponentsInChildren<ScriptableBehaviourUI>(true);
            for (var i = 0; i < uis.Length; i++)
            {
                if (uis[i] == ui) continue;
                InitializeInternal(uis[i]);
            }
        }

        protected void Start()
        {
            if (!startUI)
            {
                Debug.LogError(@"[UIManager] initialize fail , Cannot found a <color=cyan>startUI</color> shown as first one!");
#if UNITY_EDITOR
                UnityEditor.Selection.activeGameObject = gameObject;
                Debug.DebugBreak();
#endif
                return;
            }
            startUI.Show();
            var uis = GetComponentsInChildren<BaseUI>(true);
            for (var i = 0; i < uis.Length; i++)
            {
                if (!uis[i].Equals(startUI)) uis[i].gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 当前窗口
        /// </summary>
        public static WindowInfo CurrentWindow { get; private set; }

        /// <summary>
        /// 上一个显示的窗口
        /// </summary>
        public static WindowInfo PrevWindow { get; private set; }

        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ui"></param>
        public static void AddUI(BaseUI ui)
        {
            if (!windows.ContainsKey(ui))
            {
                var window = CreateWindowInfo(ui);
                windows.Add(ui, window);
                ui.UIShowEvent += OnShow;
                ui.UIHideEvent += OnHide;
            }
        }

        private static WindowInfo CreateWindowInfo(BaseUI ui)
        {
            var uiType = ui.GetType();
#if WINDOWS_UWP
            var floating = uiType.GetTypeInfo().IsDefined(typeof(UIFloatingAttribute),false);
            var exclusive = uiType.GetTypeInfo().IsDefined(typeof(UIExclusiveAttribute), false);
#else
            var floating = uiType.IsDefined(typeof(UIFloatingAttribute), false);
            var exclusive = uiType.IsDefined(typeof(UIExclusiveAttribute), false);
#endif
            return new WindowInfo()
            {
                IsFloating = floating ? floating : ui.Floating,
                IsExclusive = exclusive ? exclusive : ui.IsExclusive,
                Order = ui.SortOrder,
                UI = ui
            };
        }

        /// <summary>
        /// 当UI显示
        /// </summary>
        /// <param name="name"></param>
        private static void OnShow(BaseUI ui)
        {
            if (windows.ContainsKey(ui))
            {
                var window = windows[ui];
                var windowList = window.IsFloating ? showedFloatingWindows : showedWindows;
                if (window.IsExclusive)
                {
                    var array = windowList.ToArray();
                    for (var i = 0; i < array.Length; i++)
                    {
                        if (window.ContainBrother(array[i].UI) || array[i].UI.Equals(ui)) continue;
                        array[i].UI.Hide();
                    }
                }
                PrevWindow = CurrentWindow;
                CurrentWindow = window;
                var sortWindows = new List<WindowInfo>(new WindowInfo[] { CurrentWindow });
                var brothers = CurrentWindow.UI.UIGroup;
                for (var i = 0; i < brothers.Count; i++)
                {
                    if (brothers[i] && windows.ContainsKey(brothers[i]))
                    {
                        var brotherWindow = windows[brothers[i]];
                        sortWindows.Add(brotherWindow);
                    }
                }
                sortWindows.Sort();
                for (var i = 0; i < sortWindows.Count; i++)
                {
                    var sortWindow = sortWindows[i];
                    if (!windowList.Contains(sortWindow))
                    {
                        windowList.Add(sortWindow);
                    }
                    sortWindow.SetOrder(showedFloatingWindows.Count, Instance.transform.childCount, i);
                }
            }
        }

        /// <summary>
        /// 当UI隐藏
        /// </summary>
        /// <param name="name"></param>
        private static void OnHide(BaseUI ui)
        {
            if (windows.ContainsKey(ui))
            {
                var window = windows[ui];
                var willRemoveWindows = window.IsFloating ? showedWindows : showedFloatingWindows;
                willRemoveWindows.Remove(window);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            windows.Clear();
            showedWindows.Clear();
            showedFloatingWindows.Clear();
        }
    }
}