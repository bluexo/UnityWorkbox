using System;
using System.Collections;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using UnityEngine;

namespace Arthas.Network
{
    /// <summary>
    /// 抽象网络连接
    /// </summary>

    public abstract class Connection
    {
        /// <summary>
        /// 是否使用SSL加密
        /// </summary>
        protected virtual bool UseSSL { get; set; }
        public bool IsConnected { get; protected set; }

        protected readonly Socket Socket;
        protected readonly SocketAsyncEventArgs ConnectArgs = new SocketAsyncEventArgs(),
            SendArgs = new SocketAsyncEventArgs(),
            ReceiveArgs = new SocketAsyncEventArgs();

        public Connection() :
            this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {

        }

        protected Connection(Socket socket)
        {
            Socket = socket;
            ConnectArgs.Completed += OnConnectedCompleted;
            SendArgs.Completed += OnSendCompleted;
            ReceiveArgs.Completed += OnMessageReceived;
        }

        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        public virtual void Connect(IPAddress iPAddress, int port)
        {
            var ep = new IPEndPoint(iPAddress, port);
            Connect(ep);
        }

        public virtual void Connect(string ip, int port)
        {
            IPAddress iPAddress;
            if (IPAddress.TryParse(ip, out iPAddress)) Connect(iPAddress, port);
            else Debug.LogError("Invalid ip format" + ip);
        }

        protected void Connect(IPEndPoint endPoint)
        {
            try
            {
                ConnectArgs.RemoteEndPoint = endPoint;
                Socket.ConnectAsync(ConnectArgs);
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.Message);
            }
        }

        public virtual void Send(byte[] buffer)
        {
            SendArgs.SetBuffer(buffer, 0, buffer.Length);
            Socket.SendAsync(SendArgs);
        }

        private void OnMessageReceived(object sender, SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError != SocketError.Success)
            {
                IsConnected = false;
                Debug.LogError(args.SocketError.ToString());
                return;
            }
            IsConnected = true;
        }
    }
}
