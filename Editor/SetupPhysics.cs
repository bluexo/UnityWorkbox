using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
internal class PhysicsSettler
{
    static bool registered = false;

    static bool active = false;

    static Rigidbody[] workList;

    static bool cachedAutoSimulation;

    const float timeToSettle = 10f;

    static float activeTime = 0f;

    static PhysicsSettler()
    {
        if (registered)
            return;
        EditorApplication.update += Update;
        SceneView.duringSceneGui += OnSceneGUI;
        registered = true;
    }

    [MenuItem("Tools/Physics/Settle Physics")]
    static void Activate()
    {
        if (active) return;
        active = true;
        workList = Object.FindObjectsOfType<Rigidbody>();
        cachedAutoSimulation = Physics.autoSimulation;
        activeTime = 0f;

        foreach (Rigidbody body in workList)
            body.WakeUp();
    }

    [MenuItem("Tools/Physics/Settle Physics", true)]
    static bool checkMenu()
    {
        return !active;
    }

    static void Update()
    {
        if (!active) return;

        activeTime += Time.deltaTime;
        Physics.autoSimulation = false;
        bool allSleeping = true;
        foreach (Rigidbody body in workList)
        {
            if (body != null)
                allSleeping &= body.IsSleeping();
        }

        if (allSleeping || activeTime >= timeToSettle)
        {
            Physics.autoSimulation = cachedAutoSimulation;
            active = false;
        }
        else
        {
            Physics.Simulate(Time.deltaTime);
        }
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        if (!active) return;

        Handles.BeginGUI();
        Color cacheColor = GUI.color;
        GUI.color = Color.red;
        GUILayout.Label("Simulating Physics.", GUI.skin.box, GUILayout.Width(200));
        GUILayout.Label(string.Format("Time Remaining: {0:F2}", (timeToSettle - activeTime)), GUI.skin.box, GUILayout.Width(200));
        Handles.EndGUI();

        foreach (Rigidbody body in workList)
        {
            if (body != null)
            {
                bool isSleeping = body.IsSleeping();
                if (!isSleeping)
                {
                    GUI.color = Color.green;
                    Handles.Label(body.transform.position, "SIMULATING");
                }
            }
        }
        GUI.color = cacheColor;
    }

}