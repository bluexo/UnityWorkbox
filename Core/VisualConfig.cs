using System;
using UnityEngine;

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
}