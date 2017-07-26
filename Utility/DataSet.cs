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
        [MenuItem("Tools/PlayerPref/DataSet Viewer")]
        public static void OpenOrInit()
        {
            var window = GetWindow<DataSetEditor>("DataSet");
            window.autoRepaintOnSceneChange = true;
            DataSet.Initialize();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            for (var i = 0; i < DataSet.Keys.Count; i++) {
                if (string.IsNullOrEmpty(DataSet.Keys[i])) continue;
                EditorGUILayout.BeginHorizontal();
                var key = DataSet.Keys[i].Substring(DataSet.DataSetPrefix.Length);
                key = EditorGUILayout.TextField(key);
                DataSet.Keys[i] = DataSet.DataSetPrefix + key;
                var json = PlayerPrefs.GetString(DataSet.Keys[i]);
                var item = JsonUtility.FromJson<DataItem>(json);
                if (item != null) {
                    EditorGUILayout.BeginVertical();
                    item.key = EditorGUILayout.TextField(item.key);
                    EditorGUILayout.LabelField(item.typeName);
                    if (item.value != null) EditorGUILayout.TextArea(item.value.ToString());
                    EditorGUILayout.EndVertical();
                }
                PlayerPrefs.SetString(DataSet.Keys[i], JsonUtility.ToJson(item));
                if (GUILayout.Button("-", GUILayout.Width(20f))) DataSet.Keys.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+")) DataSet.Set("NULL", string.Empty);
            GUILayout.Space(10);
            DataSet.Store();
            EditorGUILayout.EndVertical();
        }
    }

#endif
    [Serializable]
    public class DataItem
    {
        public string key;
        public string typeName;
        public object value;
    }

    public static class DataSet
    {
        public const string DataSetKey = "7C068AE7-3EA3-4AE7-83FA-2025533DBF61",
            DataSetPrefix = "DataSet-";

        public static List<string> Keys { get; private set; }
        private readonly static Dictionary<string, string> dicts = new Dictionary<string, string>();

        //[RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            if (!PlayerPrefs.HasKey(DataSetKey)) {
                PlayerPrefs.SetString(DataSetKey, string.Empty);
            }
            var jsonString = PlayerPrefs.GetString(DataSetKey);
            if (string.IsNullOrEmpty(jsonString)) {
                PlayerPrefs.SetString(DataSetKey, string.Format("[\"{0}NULL\"]", DataSetPrefix));
            }
            Keys = new JsonList<string>().Overwrite(PlayerPrefs.GetString(DataSetKey));
        }

        static DataSet()
        {
            Initialize();
        }

        /// <summary>
        /// 将数据存储
        /// </summary>
        public static void Store()
        {
            var keyJson = new JsonList<string>(Keys).ToJson();
            PlayerPrefs.SetString(DataSetKey, keyJson);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 添加并存储
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static bool Set<T>(string key, T value)
        {
            key = DataSetPrefix + key;
            if (!Keys.Contains(key)) {
                Keys.Add(key);
                var item = new DataItem()
                {
                    key = key,
                    typeName = typeof(T).FullName,
                    value = value
                };
                var json = JsonUtility.ToJson(item);
                PlayerPrefs.SetString(key, json);
                return true;
            } else return false;
        }

        public static bool SetAndStore<T>(string key, T value)
        {
            key = DataSetPrefix + key;
            var contains = Set(key, value);
            if (contains) Store();
            return contains;
        }

        public static T Get<T>(string key)
        {
            key = DataSetPrefix + key;
            if (Keys.Contains(key)) return JsonUtility.FromJson<T>(PlayerPrefs.GetString(key));
            return default(T);
        }

        public static void Clear()
        {
            for (var i = 0; i < Keys.Count; i++) {
                var key = DataSetPrefix + Keys[i];
                if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.DeleteKey(DataSetKey);
        }
    }
}
