using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Arthas
{
    [Serializable]
    public class SerializeWrapper<T> : ISerializable
    {
        public T obj;

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var formatter = new BinaryFormatter();

        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SerializeLinqEntities : Attribute
    {
    }

    public class LinqEntitiesSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(
          object obj, SerializationInfo info, StreamingContext context)
        {
            //EntitySerializer.Serialize(this, obj.GetType(), info, context);
        }

        public object SetObjectData(
          object obj, SerializationInfo info,
          StreamingContext context, ISurrogateSelector selector)
        {
            //EntitySerializer.Deserialize(obj, obj.GetType(), info, context);
            return obj;
        }
    }


    /// <summary>
    /// Returns LinqEntitySurrogate for all types marked SerializeLinqEntities
    /// </summary>
    public class NonSerializableSurrogateSelector : ISurrogateSelector
    {
        public void ChainSelector(ISurrogateSelector selector)
        {
            throw new NotImplementedException();
        }

        public ISurrogateSelector GetNextSelector()
        {
            throw new NotImplementedException();
        }

        public ISerializationSurrogate GetSurrogate(
          Type type, StreamingContext context, out ISurrogateSelector selector)
        {
            if (!type.IsDefined(typeof(SerializeLinqEntities), false))
            {
                //type not marked SerializeLinqEntities
                selector = null;
                return null;
            }
            selector = this;
            return new LinqEntitiesSurrogate();
        }

    }

}
