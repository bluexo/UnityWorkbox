using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Arthas.Common
{
    public interface IJsonSerializable
    {
        string ToJson();
        void FromJson(string json);
    }

    public abstract class VisualConfig<T> : ScriptableObject, IJsonSerializable where T : new()
    {
        [SerializeField, Path(Type = PathType.Folder, Relative = false)]
        protected string backupDirectory;

        [HideInInspector]
        public string backupTag;

        [Space(30)]
        [SerializeField, HideInInspector]
        protected T[] items = { new T() };
        public virtual T[] Items { get { return items; } }

        public virtual string ToJson()
        {
            return new JsonList<T>(items).ToJson(true);
        }

        public virtual void FromJson(string json)
        {
            var jArray = new JsonList<T>().Overwrite(json, true);
            items = jArray.Value.ToArray();
        }
    }

#if UNITY_EDITOR
    public abstract class SingletonVisualConfig<TConfig, TItem> : VisualConfig<TItem>
        where TConfig : VisualConfig<TItem>
        where TItem : new()
    {
        public static TConfig Instance { get { return instance ?? (instance = LoadOrCreate()); } }

        private static TConfig instance;

        public static TConfig LoadOrCreate()
        {
            var assets = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(TConfig).Name));
            if (assets == null || assets.Length == 0)
            {
                var obj = CreateInstance<TConfig>();
                var path = EditorUtility.SaveFilePanel("Save", Application.dataPath, typeof(TConfig).Name, "asset");
                if (string.IsNullOrEmpty(path)) return null;
                AssetDatabase.CreateAsset(obj, PathUtility.ToAssetsPath(path));
                AssetDatabase.Refresh();
                return LoadOrCreate();
            }
            else
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
                return AssetDatabase.LoadAssetAtPath<TConfig>(assetPath);
            }
        }
    }
#endif
}