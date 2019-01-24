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
        [MenuItem("Builder/Build")]
        public static void Build()
        {
            var items = PackageBuildConfig.Instance.Items;
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                if (!item.Build) continue;
                BuildInternal(item);
            }
        }

        [MenuItem("Builder/Configuration")]
        public static void Configuration()
        {
            Selection.activeObject = PackageBuildConfig.Instance;
        }

        private static string BuildInternal(BuildItem current)
        {
            if (current == null)
            {
                UDebug.LogErrorFormat("Cannot found build target config : {0} .", EditorUserBuildSettings.activeBuildTarget);
                return null;
            }

            var path = current.OutputPath
                + PackageBuildConfig.Instance.PackageName
                + current.NamePrefix
                + DateTime.Now.ToString("_yyyyMMdd_HHmmss")
                + "." 
                + current.ExtensionName;

            var report = BuildPipeline.BuildPlayer(Array.ConvertAll(current.Scenes, s => AssetDatabase.GetAssetOrScenePath(s)),
                path,
                current.BuildTarget,
                current.BuildOptions);

            if (report.summary.result != BuildResult.Succeeded) return null;
            return path;
        }
    }
#endif
}
