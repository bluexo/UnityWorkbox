using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Common
{
    [System.Serializable]
    public class PoolObjectInfo
    {
        public int id;
        public string name;
        public GameObject prefab;
    }

    [CreateAssetMenu(menuName = "Configs/ObjectPoolConfig")]
    public class PoolObjectConfig : VisualConfig<PoolObjectInfo> { }
}