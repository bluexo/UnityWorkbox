using System;
using System.Collections.Generic;

namespace Arthas.Protocol
{
    /// <summary>
    /// 配置
    /// </summary>
    public abstract class ConfigBase
    {
        public bool initialized { get; protected set; }
        public abstract void ToDictionary();
    }

    public abstract class InfoBase
    {
        public int Id { get; set; }
    }

    public class TConfig<T> : ConfigBase where T : InfoBase
    {
        public List<T> Records { get; set; }

        private Dictionary<int, T> dictionary;
       
        public IDictionary<int, T> Dictionary() { return dictionary; }

        public override void ToDictionary()
        {
            dictionary = new Dictionary<int, T>();
            foreach (var conf in Records)
                dictionary.Add(conf.Id, conf);
            initialized = true;
        }

        public T GetConfig(int id)
        {
            T t = null;
            if (initialized && dictionary.ContainsKey(id))
                t = dictionary[id];
            return t;
        }
    }
}
