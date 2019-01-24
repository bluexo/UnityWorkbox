using Arthas;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Arthas
{
    using Arthas.Common;
    using UnityEngine.SceneManagement;

    [System.Serializable]
    public class BuildItem
    {
        public string NamePrefix;
        public string ExtensionName;
        [Path(Type = PathType.Folder, Relative = false)]
        public string OutputPath;
        public BuildTarget BuildTarget;
        public bool Build;
        [EnumMaskField]
        public BuildOptions BuildOptions;
        public Object[] Scenes;
    }

    public class PackageBuildConfig : SingletonVisualConfig<PackageBuildConfig, BuildItem>
    {
        public string PackageName;
    }
}
