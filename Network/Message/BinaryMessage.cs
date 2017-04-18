using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class BinaryMessage : IMessage
{
    public IMessageHead Head { get; private set; }

    private byte[] bodyBuffer;

    private object body;

    public BinaryMessage(IMessageHead head, byte[] buffer)
    {
        Head = head;
        head.ExceptHead(ref buffer);
        using (var stream = new MemoryStream(buffer))
        {
            var formatter = new BinaryFormatter();
            body = formatter.Deserialize(stream);
        }
    }

    public BinaryMessage(IMessageHead head, object body)
    {
        Head = Head;
        using (var stream = new MemoryStream())
        {
            var formater = new BinaryFormatter();
            formater.Serialize(stream, body);
            bodyBuffer = stream.GetBuffer();
        }
    }

    public byte[] GetBuffer(bool containsHead = false)
    {
        if (containsHead)
        {
            var headBuffer = Head.GetBuffer();
            var newBuffer = new byte[headBuffer.Length + bodyBuffer.Length];
            Buffer.BlockCopy(headBuffer, 0, newBuffer, 0, headBuffer.Length);
            Buffer.BlockCopy(bodyBuffer, 0, newBuffer, headBuffer.Length, bodyBuffer.Length);
            return newBuffer;
        }
        return bodyBuffer;
    }

    public byte[] GetBufferWithLength()
    {
        var current = GetBuffer(true);
        var lengthBytes = Head.IsLittleEndian
            ? BitConverter.GetBytes(current.Length)
            : BitConverter.GetBytes(current.Length).ReverseToBytes();
        var bufferWithLength = new byte[current.Length + sizeof(int)];
        Buffer.BlockCopy(lengthBytes, 0, bufferWithLength, 0, sizeof(int));
        Buffer.BlockCopy(current, 0, bufferWithLength, sizeof(int), current.Length);
        return bufferWithLength;
    }

    public T GetValue<T>() { return (T)body; }
}
