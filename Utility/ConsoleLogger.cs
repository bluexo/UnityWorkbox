/*******************************************************
 * Class  : ConsoleLogger 
 * Date   : 2014/1/31
 * Author : Alvin
 * ******************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace Arthas.Common
{
    public class ConsoleLogger : SingletonBehaviour<ConsoleLogger>
    {
        private struct Log
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        public KeyCode toggleKey = KeyCode.Tab;

        private List<Log> logs = new List<Log>();
        private Vector2 scrollPosition;
        public bool showConsole, showFPS;
        private bool collapse;


        private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

        private const int margin = 20;

        private Rect windowRect = new Rect(margin, margin * 2, Screen.width * .9f, Screen.height - (margin * 3));
        private Rect titleBarRect = new Rect(0, 0, 10000, 45);
        private readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console."),
            collapseLabel = new GUIContent("Collapse", "Hide repeated messages."),
            closeLabel = new GUIContent("Close", "close window");

        private void OnEnable()
        {
#if UNITY_4_6
		    Application.RegisterLogCallback(HandleLog);
#else
            Application.logMessageReceived += HandleLog;
#endif
        }

        private void OnDisable()
        {
#if !UNITY_4_6
            Application.logMessageReceived -= HandleLog;
#endif
        }

        const float fpsMeasurePeriod = 0.5f;
        private int m_FpsAccumulator = 0;
        private float m_FpsNextPeriod = 0, prevToggleTime;
        private int m_CurrentFps;
        const string display = "{0} FPS";


        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }

        private void Update()
        {
            //在移动设备上通过五指同时触摸来激活
#if !UNITY_STANDALONE && !UNITY_EDITOR
        if (Input.touches.Length >= 5 
            && (Time.time - prevToggleTime > .5f)) {
                prevToggleTime = Time.time;
                ToggleShowDebug();
        }
#endif
            if (Input.GetKeyDown(toggleKey)) {
                showConsole = !showConsole;
            }
        }

        public void ToggleShowDebug()
        {
            showConsole = !showConsole;
        }

        private void OnGUI()
        {
            // measure average frames per second
            if (showFPS) {
                m_FpsAccumulator++;
                if (Time.realtimeSinceStartup > m_FpsNextPeriod) {
                    m_CurrentFps = (int)(m_FpsAccumulator / fpsMeasurePeriod);
                    m_FpsAccumulator = 0;
                    m_FpsNextPeriod += fpsMeasurePeriod;
                }
                GUI.Box(new Rect(10, 5, 60, 25), string.Format(display, m_CurrentFps));
            }
            if (showConsole) {
                windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
            }

        }

        private void ConsoleWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < logs.Count; i++) {
                var log = logs[i];
                if (collapse) {
                    var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

                    if (messageSameAsPrevious) {
                        continue;
                    }
                }
                GUI.contentColor = logTypeColors[log.type];
                GUILayout.Label(log.message);
            }
            GUILayout.EndScrollView();

            GUI.contentColor = Color.white;

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(closeLabel, GUILayout.Height(45f)))
                showConsole = false;

            if (GUILayout.Button(clearLabel, GUILayout.Height(45f)))
                logs.Clear();

            if (GUILayout.Button(collapseLabel, GUILayout.Height(45f)))
                collapse = !collapse;

            GUILayout.EndHorizontal();

            GUI.DragWindow(titleBarRect);
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            var log = new Log()
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
            };
            logs.Add(log);
        }
    }
}