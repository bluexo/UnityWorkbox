using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;

namespace Arthas.Network
{
    public class UDPConnection : MonoBehaviour, IConnection
    {
        public readonly int MaxHostCount = 16;
        public bool IsConnected { get; private set; }
        public event Action<byte[]> MessageRespondEvent;

        [SerializeField] List<QosType> qosTypes = new List<QosType> { QosType.Reliable, QosType.Unreliable, QosType.StateUpdate };
        [SerializeField] ConnectionConfig connectionConfig = new ConnectionConfig();
        [SerializeField] GlobalConfig globalConfig = new GlobalConfig();

        private readonly HashSet<byte> channelIds = new HashSet<byte>();
        private readonly HashSet<int> hosts = new HashSet<int>();

        private HostTopology hostTopology;

        public void Start()
        {
            NetworkTransport.Init(globalConfig);
            for (var i = 0; i < qosTypes.Count; i++)
                channelIds.Add(connectionConfig.AddChannel(qosTypes[i]));
        }

        public void Connect(string ip, int port, Action<object> action = null)
        {
            byte error;
            var connId = NetworkTransport.Connect(0, ip, port, 0, out error);
            if (error > 0)
                Debug.LogErrorFormat("Connect error , code = {0}", error);
        }

        public void Send(byte[] buffer)
        {
            byte error;
            NetworkTransport.Send(0, 0, 0, buffer, buffer.Length, out error);
        }

        protected void Update()
        {
            int recHostId;
            int connectionId;
            int channelId;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            byte error;
            NetworkEventType recData = NetworkTransport.Receive(out recHostId,
                out connectionId,
                out channelId,
                recBuffer,
                bufferSize,
                out dataSize,
                out error);
            switch (recData)
            {
                case NetworkEventType.Nothing:         //1
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    break;
                case NetworkEventType.DataEvent:       //3
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    break;
            }
        }


        public void Close()
        {
            byte error;
            NetworkTransport.Disconnect(0, 0, out error);
        }
    }
}
