using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;

namespace Arthas.Network
{
    /// <summary>
    /// Websocket 封装
    /// </summary>
    public class WSConnector : IConnector
    {
        public bool IsConnected { get; private set; }

        public event Action<byte[]> MessageRespondEvent;

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Connect(string ip, int port)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
