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
using System.Threading;

namespace UnityWorkbox.Network
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

        private readonly AutoResetEvent connectHandle = new AutoResetEvent(false),
            receiveHandle = new AutoResetEvent(false),
            sendHandle = new AutoResetEvent(false);
        private readonly WaitHandle[] readWriteHandles;

        protected readonly SocketAsyncEventArgs ConnectArgs = new SocketAsyncEventArgs(),
            ReadWriteArgs = new SocketAsyncEventArgs();

        private readonly ByteBuffer buffer = new ByteBuffer(short.MaxValue);

        public Connection() :
            this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
        {
            readWriteHandles = new WaitHandle[] { sendHandle, receiveHandle };
        }

        protected Connection(Socket socket)
        {
            Socket = socket;
        }

        public void Connect(IPAddress iPAddress, int port)
        {
            var ep = new IPEndPoint(iPAddress, port);
            Connect(ep);
        }

        public virtual void Connect(string ip, int port)
        {
            IPAddress iPAddress;
            if (string.Equals(ip, "localhost", StringComparison.CurrentCultureIgnoreCase))
            {
                iPAddress = IPAddress.Loopback;
            }
            else if (!IPAddress.TryParse(ip, out iPAddress))
            {
                Debug.LogError("Invalid ip format " + ip);
                return;
            }
            Connect(iPAddress, port);
        }

        protected void Connect(IPEndPoint endPoint)
        {
            try
            {
                ConnectArgs.UserToken = Socket;
                ConnectArgs.RemoteEndPoint = endPoint;
                ConnectArgs.Completed += OnConnectedCompleted;
                Socket.ConnectAsync(ConnectArgs);
                connectHandle.WaitOne();
                if (ConnectArgs.SocketError != SocketError.Success)
                    throw new SocketException((int)ConnectArgs.SocketError);
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.Message);
            }
        }

        public virtual void Disconnect()
        {
            Socket.Disconnect(false);
        }

        public virtual void Send(byte[] buffer)
        {
            if (!IsConnected) return;
            ReadWriteArgs.SetBuffer(buffer, 0, buffer.Length);
            ReadWriteArgs.UserToken = Socket;
            ReadWriteArgs.RemoteEndPoint = Socket.RemoteEndPoint;
            ReadWriteArgs.Completed += OnMessageSend;
            Socket.SendAsync(ReadWriteArgs);

            WaitHandle.WaitAll(readWriteHandles);

            ProcessMessage(ReadWriteArgs.Buffer, ReadWriteArgs.Offset, ReadWriteArgs.BytesTransferred);
        }

        private void ProcessMessage(byte[] buffer, int offset, int bytesTransferred)
        {
            var msg = Encoding.UTF8.GetString(buffer, offset, bytesTransferred);
            Debug.Log(msg);
        }

        private void OnMessageSend(object sender, SocketAsyncEventArgs e)
        {
            sendHandle.Set();
            if (e.SocketError != SocketError.Success)
            {
                ProcessError(e);
                return;
            }
            if (e.LastOperation == SocketAsyncOperation.Send)
            {
                var s = e.UserToken as Socket;
                byte[] receiveBuffer = new byte[255];
                e.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);
                e.Completed += OnMessageReceived;
                s.ReceiveAsync(e);
            }
        }

        private void OnMessageReceived(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                Debug.LogError("Message receive error:" + e.SocketError.ToString());
            }
            if (e.BytesTransferred == 0)
            {
                Debug.LogFormat("Disconnected from server");
                ProcessError(e);
            }

            receiveHandle.Set();
        }

        private void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
        {
            connectHandle.Set();
            IsConnected = (args.SocketError == SocketError.Success);
        }

        /// <summary>
        /// Close socket in case of failure and throws a SockeException according to the SocketError.
        /// </summary>
        /// <param name="e">SocketAsyncEventArg associated with the failed operation.</param>
        private void ProcessError(SocketAsyncEventArgs e)
        {
            Socket s = e.UserToken as Socket;
            if (s.Connected)
            {
                try
                {
                    s.Shutdown(SocketShutdown.Both);
                }
                finally
                {
                    if (s.Connected) s.Close();
                }
            }
            throw new SocketException((int)e.SocketError);
        }

        public void Dispose()
        {
            connectHandle.Close();
            sendHandle.Close();
            receiveHandle.Close();
            if (Socket.Connected)
                Socket.Close();
        }
    }
}
