using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UDebug = UnityEngine.Debug;

namespace Arthas
{
    using System;
    using System.Diagnostics;
#if UNITY_2018
    using UnityEditor.Build.Reporting;

    public class PackageBuidler : EditorWindow
    {
        [MenuItem("Builder/Build/Client")]
        public static bool BuildClient()
        {
            return BuildInternal(BuildTargetType.Client) != null;
        }

        [MenuItem("Builder/Build/Server")]
        public static bool BuildServer()
        {
#if !UNITY_STANDALONE
            Debug.LogWarning("Cannot build server with Non standalone platform");
            return false;
#endif
            return BuildInternal(BuildTargetType.Server) != null;
        }

        [MenuItem("Builder/Build And Run", priority = 1)]
        public static void BuildAndRunAll()
        {
            var client = BuildInternal(BuildTargetType.Client);
            var server = BuildInternal(BuildTargetType.Server);
            if (!string.IsNullOrEmpty(client))
                Process.Start(client);
            if (!string.IsNullOrEmpty(server))
                Process.Start(server);
        }

        [MenuItem("Builder/Configuration")]
        public static void BuildConfiguration()
        {
            Selection.activeObject = PackageBuildConfig.Instance;
        }

        private static string BuildInternal(BuildTargetType type)
        {
            var current = Array.Find(PackageBuildConfig.Instance.Items,
                r => r.BuildTarget == EditorUserBuildSettings.activeBuildTarget && r.BuildTargetType == type);

            if (current == null)
            {
                UDebug.LogErrorFormat("Cannot found build target config : {0} .", EditorUserBuildSettings.activeBuildTarget);
                return null;
            }

            var path = current.Path
                + PackageBuildConfig.Instance.PackageName 
                + DateTime.Now.ToString("_yyyyMMdd_HHmmss")
                + current.ExtensionName;

            var report = BuildPipeline.BuildPlayer(current.Scenes,
                path,
                current.BuildTarget,
                current.BuildOptions);

            if (report.summary.result != BuildResult.Succeeded) return null;
            return path;
        }
    }
#endif
}
