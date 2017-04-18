using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

public enum MessageFormat { Binary, Json, ProtoBuf }

public class DefaultMessageHead : IMessageHead
{
    public bool IsLittleEndian { get; private set; }

    public DefaultMessageHead(bool littleEndian = true)
    {
        IsLittleEndian = littleEndian;
    }

    public int SerialNumber { get; set; }

    public short Dest { get; set; }

    public short CommandId { get; set; }

    public virtual byte[] GetBuffer()
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            if (IsLittleEndian && BitConverter.IsLittleEndian)
            {
                writer.Write(SerialNumber);
                writer.Write(Dest);
                writer.Write(CommandId);
            }
            else
            {
                writer.Write(BitConverter.GetBytes(SerialNumber).ReverseToBytes());
                writer.Write(BitConverter.GetBytes(Dest).ReverseToBytes());
                writer.Write(BitConverter.GetBytes(CommandId).ReverseToBytes());
            }
            writer.Flush();
            return stream.ToArray();
        }
    }

    public void ExceptHead(ref byte[] buffer)
    {
        var buf = GetBuffer();
        var newBuffer = new byte[buffer.Length - buf.Length];
        Buffer.BlockCopy(buffer, buf.Length, newBuffer, 0, newBuffer.Length);
        buffer = newBuffer;
    }
}

/// <summary>
/// 相应消息头
/// </summary>
public class ResponseMessageHead : DefaultMessageHead
{
    public double Time { get; set; }

    public int Status { get; set; }

    public override byte[] GetBuffer()
    {
        return base.GetBuffer();
    }
}


public class MessageFactory
{
    public static IMessage CreateMessage(IMessageHead head, object body, MessageFormat format = MessageFormat.Binary)
    {
        switch (format)
        {
            case MessageFormat.Json: return new JsonMessage(body);
            case MessageFormat.ProtoBuf: return new ProtoBufMessage(head, body);
            default: return new BinaryMessage(head, body);
        }
    }

    public static IMessage CreateMessage(IMessageHead head, byte[] buffer, MessageFormat format = MessageFormat.Binary)
    {
        switch (format)
        {
            case MessageFormat.Json: return new JsonMessage(buffer);
            case MessageFormat.ProtoBuf: return new ProtoBufMessage(head, buffer);
            default: return new BinaryMessage(head, buffer);
        }
    }
}
