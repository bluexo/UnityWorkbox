using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace Arthas
{
    [System.Serializable]
    public class SerializeWrapper<T> : ISerializable
    {
        public T obj;

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) { }
    }
}
