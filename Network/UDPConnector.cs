using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Arthas.Network
{
    public class UDPConnector : IConnector
    {
        public bool IsConnected { get; private set; }

        public UdpClient client;

        public event Action<byte[]> MessageRespondEvent;

        public void Connect(string ip, int port)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }
    }
}
