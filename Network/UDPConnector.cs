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

#if !UNITY_WSA
        public UdpClient client;

        public event Action MessageRespondEvent;
#endif

        public void Connect(string ip, int port, Action<object> action = null)
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
    }
}
