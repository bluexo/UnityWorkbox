/*******************************************************
 * Class  : ConsoleLogger 
 * Date   : 2014/1/31
 * Author : Alvin
 * ******************************************************/

using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System.Collections;
using System;

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

        private static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>()
        {
            { LogType.Assert, Color.white },
            { LogType.Error, Color.red },
            { LogType.Exception, Color.red },
            { LogType.Log, Color.white },
            { LogType.Warning, Color.yellow },
        };

        public KeyCode toggleKey = KeyCode.Tab;
        private List<Log> logs = new List<Log>();
        private Vector2 scrollPosition;
        private bool collapse;
        public bool showConsole;


        private static HashSet<UnityEngine.Object> sampleObjects = new HashSet<UnityEngine.Object>();
        private Rect windowRect = new Rect(margin, margin * 2, Screen.width * .9f, Screen.height - (margin * 3));
        private Rect titleBarRect = new Rect(0, 0, 10000, 45);
        private WaitForSeconds waitForProfile = new WaitForSeconds(3f);
        private const int margin = 20, bytesUnit = 1000000;
        private readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console."),
            collapseLabel = new GUIContent("Collapse", "Hide repeated messages."),
            closeLabel = new GUIContent("Close", "close window");

        private const float fpsMeasurePeriod = 0.5f;
        private int fpsAccumulator = 0, currentFps;
        private float fpsNextPeriod = 0, prevToggleTime;


        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            fpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
            StartCoroutine(EnableProfile());
        }

        private void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        private IEnumerator EnableProfile()
        {
            Profiler.enabled = true;
            while (true)
            {
                yield return waitForProfile;
                Profiler.BeginSample(DateTime.Now.ToLongDateString());
            }
        }

        private void Update()
        {
            //在移动设备上通过四指同时触摸来激活
#if !UNITY_STANDALONE && !UNITY_EDITOR
        if (Input.touches.Length >= 4 
            && (Time.time - prevToggleTime > .5f)) {
                prevToggleTime = Time.time;
                ToggleShowDebug();
        }
#endif
            if (Input.GetKeyDown(toggleKey))
            {
                showConsole = !showConsole;
            }
        }

        public void ToggleShowDebug()
        {
            showConsole = !showConsole;
        }

        private void OnGUI()
        {
            fpsAccumulator++;
            if (Time.realtimeSinceStartup > fpsNextPeriod)
            {
                currentFps = (int)(fpsAccumulator / fpsMeasurePeriod);
                fpsAccumulator = 0;
                fpsNextPeriod += fpsMeasurePeriod;
            }
            using (var scope = new GUILayout.HorizontalScope())
            {
                GUILayout.Box(string.Format("FPS:[{0}] ", currentFps));
                GUILayout.Box(string.Format("UsedHeapSize:[{0}]", LongToMb(Profiler.usedHeapSizeLong)));
                GUILayout.Box(string.Format("Reserved:[{0}]", LongToMb(Profiler.GetTotalReservedMemoryLong())));
                GUILayout.Box(string.Format("Used:[{0}]", LongToMb(Profiler.GetTotalAllocatedMemoryLong())));
                GUILayout.Box(string.Format("Unused:[{0}]", LongToMb(Profiler.GetTotalUnusedReservedMemoryLong())));
                GUILayout.Box(string.Format("MonoHeapSize:[{0}]", LongToMb(Profiler.GetMonoHeapSizeLong())));
                GUILayout.Box(string.Format("MonoUsed:[{0}]", LongToMb(Profiler.GetMonoUsedSizeLong())));
            }

            if (showConsole)
            {
                windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
            }
        }

        private string LongToMb(long bytes)
        {
            var real = Math.Round(bytes / bytesUnit * 1f, 2);
            return real.ToString() + "M";
        }

        public static void RegisterObjectProfile(UnityEngine.Object obj)
        {
            sampleObjects.Add(obj);
        }

        public static void UnregisterObjectProfile(UnityEngine.Object obj)
        {
            sampleObjects.Remove(obj);
        }

        private void ConsoleWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (collapse)
                {
                    var messageSameAsPrevious = i > 0 && log.message == logs[i - 1].message;

                    if (messageSameAsPrevious)
                    {
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