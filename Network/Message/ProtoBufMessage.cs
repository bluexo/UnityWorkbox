using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtoBufMessage : IMessage
{

    private object obj;

    public IMessageHead Head { get; private set; }

    public ProtoBufMessage(IMessageHead head, byte[] buffer)
    {

    }

    public ProtoBufMessage(IMessageHead head, object obj)
    {

    }

    public byte[] GetBuffer(bool containsHead = false)
    {
        throw new System.NotImplementedException();
    }

    public T GetValue<T>()
    {
        throw new System.NotImplementedException();
    }

    public byte[] GetBufferWithLength()
    {
        throw new System.NotImplementedException();
    }
}
