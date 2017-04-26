using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 默认的请求头部格式
    /// </summary>
    public class RequestHeader : IMessageHeader
    {
        public bool IsLittleEndian { get; set; }
        public int SerialNumber { get; set; }
        public short Descriptor { get; set; }
        public short Command { get; set; }

        public virtual void Overwrite(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public virtual byte[] GetBuffer()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (IsLittleEndian && BitConverter.IsLittleEndian)
                {
                    writer.Write(SerialNumber);
                    writer.Write(Descriptor);
                    writer.Write(Command);
                }
                else
                {
                    writer.Write(BitConverter.GetBytes(SerialNumber).Reverse());
                    writer.Write(BitConverter.GetBytes(Descriptor).Reverse());
                    writer.Write(BitConverter.GetBytes(Command).Reverse());
                }
                writer.Flush();
                return stream.ToArray();
            }
        }

        public void ExceptHeader(ref byte[] buffer)
        {
            var headerBuffer = GetBuffer();
            var newBuffer = new byte[buffer.Length - headerBuffer.Length];
            Buffer.BlockCopy(buffer, headerBuffer.Length, newBuffer, 0, newBuffer.Length);
            buffer = newBuffer;
        }
    }

    /// <summary>
    /// 默认的响应头部格式
    /// </summary>
    public class ResponseHeader : RequestHeader
    {
        public double Time { get; set; }

        public int Status { get; set; }

        public override void Overwrite(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            using (var reader = new BinaryReader(stream))
            {
                if (IsLittleEndian && BitConverter.IsLittleEndian)
                {
                    SerialNumber = reader.ReadInt32();
                    Descriptor = reader.ReadInt16();
                    Command = reader.ReadInt16();
                    Time = reader.ReadDouble();
                    Status = reader.ReadInt32();
                }
                else
                {
                    SerialNumber = BitConverter.ToInt32(reader.ReadBytes(sizeof(int)).Reverse(), 0);
                    Descriptor = BitConverter.ToInt16(reader.ReadBytes(sizeof(short)).Reverse(), 0);
                    Command = BitConverter.ToInt16(reader.ReadBytes(sizeof(short)).Reverse(), 0);
                    Time = BitConverter.ToDouble(reader.ReadBytes(sizeof(double)).Reverse(), 0);
                    Status = BitConverter.ToInt32(reader.ReadBytes(sizeof(int)).Reverse(), 0);
                }
            }
        }

        public override byte[] GetBuffer()
        {
            using (var stream = new MemoryStream())
            using (var writer = new BinaryWriter(stream))
            {
                if (IsLittleEndian && BitConverter.IsLittleEndian)
                {
                    writer.Write(SerialNumber);
                    writer.Write(Descriptor);
                    writer.Write(Command);
                    writer.Write(Time);
                    writer.Write(Status);
                }
                else
                {
                    writer.Write(BitConverter.GetBytes(SerialNumber).Reverse());
                    writer.Write(BitConverter.GetBytes(Descriptor).Reverse());
                    writer.Write(BitConverter.GetBytes(Command).Reverse());
                    writer.Write(BitConverter.GetBytes(Time).Reverse());
                    writer.Write(BitConverter.GetBytes(Status).Reverse());
                }
                writer.Flush();
                return stream.ToArray();
            }
        }
    }

    /// <summary>
    /// 默认消息格式
    /// </summary>
    public class DefaultMessage : IMessage
    {
        public IMessageHeader Header { get; protected set; }

        protected byte[] bodyBuffer;

        protected DefaultMessage() { }

        public DefaultMessage(IMessageHeader header, byte[] buffer, bool containsHeader = false)
        {
            Header = header;
            if (containsHeader) header.ExceptHeader(ref buffer);
            bodyBuffer = buffer;
        }

        public virtual byte[] GetBuffer(bool containsHead = false)
        {
            if (containsHead)
            {
                var headBuf = Header.GetBuffer();
                var newBuf = new byte[bodyBuffer.Length + headBuf.Length];
                Buffer.BlockCopy(headBuf, 0, newBuf, 0, headBuf.Length);
                Buffer.BlockCopy(bodyBuffer, 0, newBuf, headBuf.Length, bodyBuffer.Length);
                return newBuf;
            }
            return bodyBuffer;
        }

        public virtual byte[] GetBufferWithLength()
        {
            var buffer = GetBuffer(true);
            var length = buffer.Length + sizeof(int);
            var lengthBuffer = Header.IsLittleEndian
                ? BitConverter.GetBytes(length)
                : BitConverter.GetBytes(length).Reverse();
            var newBuffer = new byte[length];
            Buffer.BlockCopy(lengthBuffer, 0, newBuffer, 0, lengthBuffer.Length);
            Buffer.BlockCopy(buffer, 0, newBuffer, lengthBuffer.Length, buffer.Length);
            return newBuffer;
        }

        public virtual T GetValue<T>()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 默认的消息包装器
    /// </summary>
    public class DefaultMessageWrapper : IMessageWrapper
    {
        public IMessageHeader RequestHeader { get; private set; }

        public IMessageHeader ResponseHeader { get; private set; }

        public DefaultMessageWrapper(bool littleEndian = true)
        {
            RequestHeader = new RequestHeader() { IsLittleEndian = littleEndian };
            ResponseHeader = new ResponseHeader() { IsLittleEndian = littleEndian };
        }

        public IMessage FromObject(object obj)
        {
            throw new NotImplementedException();
        }

        public IMessage FromString(string str)
        {
            throw new NotImplementedException();
        }

        public IMessage FromBuffer(byte[] buffer, bool containHeader = false) 
        {
            if (containHeader)
            {
                ResponseHeader.Overwrite(buffer);
                return new DefaultMessage(ResponseHeader, buffer, containHeader);
            }
            else
            {
                return new DefaultMessage(RequestHeader, buffer, containHeader);
            }
        }
    }
}
