using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arthas.Common
{
    using System;
#if UNITY_EDITOR
    using UnityEditor;

    public class DataSetEditor : EditorWindow
    {
        [MenuItem("Tools/DataSet")]
        public static void OpenOrInit()
        {

        }
    }

#endif
    [Serializable]
    public class DataItem<T>
    {
        public string key;
        public T value;
    }

    public static class DataSet
    {
        public static readonly string DataSetKey = typeof(DataSet).FullName,
            DataItemKeyPrefix = typeof(DataItem<>).FullName;
    }
}
