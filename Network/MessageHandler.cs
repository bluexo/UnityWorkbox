using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 默认消息
    /// </summary>
    public class DefaultMessage : INetworkMessage
    {
        protected byte[] buffer = { };

        public object[] Parameters { get; private set; }

        public object Command { get; private set; }

        public DefaultMessage(object command, byte[] buf, params object[] parameters)
        {
            Command = command;
            buffer = buf;
            Parameters = parameters;
        }

        public byte[] GetBuffer(bool withLength = false, bool littleEndian = false)
        {
            if (withLength)
            {
                var lenBytes = littleEndian && BitConverter.IsLittleEndian
                    ? BitConverter.GetBytes(buffer.Length)
                    : BitConverter.GetBytes(buffer.Length).Reverse();
                var newBuffer = new byte[buffer.Length + lenBytes.Length];
                Buffer.BlockCopy(lenBytes, 0, newBuffer, 0, lenBytes.Length);
                Buffer.BlockCopy(buffer, 0, newBuffer, lenBytes.Length, buffer.Length);
                return newBuffer;
            }
            return buffer;
        }

        public T GetValue<T>()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 默认的消息包装器
    /// </summary>
    public class DefaultMessageHandler : INetworkMessageHandler
    {
        public INetworkMessage PackMessage(object command, object obj, params object[] parameters)
        {
            if (obj is byte[])
            {
                var bodyBuffer = obj as byte[];
                var cmdBytes = BitConverter.GetBytes((short)command);
                var buffer = new byte[bodyBuffer.Length + cmdBytes.Length];
                Buffer.BlockCopy(cmdBytes, 0, buffer, 0, cmdBytes.Length);
                Buffer.BlockCopy(bodyBuffer, 0, buffer, cmdBytes.Length, bodyBuffer.Length);
                return new DefaultMessage(command, buffer, parameters);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public INetworkMessage ParseMessage(byte[] buffer)
        {
            var len = BitConverter.ToInt16(buffer, 0);
            var command = BitConverter.ToInt16(buffer, sizeof(short));
            var msgBuffer = new byte[buffer.Length - 4];
            Buffer.BlockCopy(buffer, 4, msgBuffer, 0, msgBuffer.Length);
            return new DefaultMessage(command, msgBuffer);
        }
    }
}
