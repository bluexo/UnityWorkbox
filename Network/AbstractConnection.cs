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
        protected abstract bool UseSSL { get; }

        protected readonly Socket Socket;

        public Connection() :
            this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {

        }

        public Connection(Socket socket)
        {
            Socket = socket;
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
                Socket.ConnectAsync(new SocketAsyncEventArgs
                {

                });
            }
            catch
            {

            }
        }
    }
}
