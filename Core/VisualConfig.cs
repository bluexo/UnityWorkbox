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

    /// <summary>
    /// 可视化配置
    /// 继承此类之后, 可以在检视窗口中显示和编辑配置操作，如添加、删除项，备份恢复以及导入导出为JSON文件
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

    /// <summary>
    /// 单例的可视化配置基类
    /// 在编辑器模式下第一次使用会弹出保存窗口, 可以保存到 <p>Assets</p> 下的任何地方
    /// 如果在运行模式下使用,必须按照类型名称命名并且保存到 <p>Resources</p> 目录
    /// </summary>
    /// <typeparam name="TConfig"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public abstract class SingletonVisualConfig<TConfig, TItem> : VisualConfig<TItem>
        where TConfig : VisualConfig<TItem>
        where TItem : new()
    {
        public const string RuntimeConfigPath = "Configs/";


        public static TConfig Instance { get { return instance ?? (instance = LoadOrCreate()); } }

        private static TConfig instance;

        public static TConfig LoadOrCreate()
        {
#if UNITY_EDITOR
            var assets = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(TConfig).Name));
            if (assets == null || assets.Length == 0)
            {
                var obj = CreateInstance<TConfig>();
                var path = EditorUtility.SaveFilePanel("Save singleton config", Application.dataPath + "/Resources/", typeof(TConfig).Name, "asset");
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
#else
            return Resources.Load<TConfig>(RuntimeConfigPath + typeof(TConfig).Name);
#endif
        }
    }
}