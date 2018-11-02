using Arthas;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YongeGames
{
    using Arthas.Common;
    using UnityEngine.SceneManagement;

    public enum BuildTargetType
    {
        Client,
        Server
    }

    [System.Serializable]
    public class BuildItem
    {
        [Path(Type = PathType.Folder, Relative = false)]
        public string Path;
        public string ExtensionName;
        public BuildTarget BuildTarget;
        [EnumMaskField]
        public BuildOptions BuildOptions;
        public BuildTargetType BuildTargetType;
        public string[] Scenes;
    }

    public class PackageBuildConfig : SingletonVisualConfig<PackageBuildConfig, BuildItem>
    {
        public string PackageName;
    }
}
