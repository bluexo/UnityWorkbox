using System.Collections.Generic;
#if WINDOWS_UWP
using System.Reflection;
#endif

namespace Arthas.Client.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private Dictionary<string, BaseUI> windows = new Dictionary<string, BaseUI>();
        private List<BaseUI> showedWindows = new List<BaseUI>();

        /// <summary>
        /// 上一个显示的窗口
        /// </summary>
        public static BaseUI CurrentWindow { get; private set; }

        public static BaseUI PrevWindow { get; private set; }

        /// <summary>
        /// 添加UI
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ui"></param>
        public static void AddUI(string name, BaseUI ui)
        {
            if (!Instance.windows.ContainsKey(name)) {
                Instance.windows.Add(name, ui);
                ui.UIShowEvent += Instance.OnShow;
                ui.UIHideEvent += Instance.OnHide;
            }
        }

        internal static bool ContainsUI(string name)
        {
            return Instance.windows.ContainsKey(name);
        }

        /// <summary>
        /// 当UI显示
        /// </summary>
        /// <param name="name"></param>
        private void OnShow(BaseUI ui)
        {
            if (windows.ContainsKey(ui.name)) {
                var window = windows[ui.name];
#if WINDOWS_UWP
                bool isExclusive = window.GetType().GetTypeInfo().IsDefined(typeof(UIHeaderAttribute));
#else
                var isExclusive = window.GetType().IsDefined(typeof(UIHeaderAttribute), false);
#endif
                if (isExclusive) {
                    if (showedWindows.Count <= 0)
                        showedWindows = new List<BaseUI>(UICanvas.Instance.GetComponentsInChildren<BaseUI>());
                    var arr = showedWindows.ToArray();
                    foreach (var item in arr) item.Hide();
                }
                PrevWindow = CurrentWindow;
                showedWindows.Add(window);
                CurrentWindow = window;
                CurrentWindow.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 当UI隐藏
        /// </summary>
        /// <param name="name"></param>
        private void OnHide(BaseUI ui)
        {
            if (windows.ContainsKey(ui.name)) {
                var window = windows[ui.name];
                showedWindows.Remove(window);
            }
        }
    }
}