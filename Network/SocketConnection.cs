using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UnityWorkbox.Network
{

    public class SocketConnection : IConnection
    {
        private class StateObject
        {
            internal byte[] sBuffer;
            internal Socket sSocket;
        }

        private byte[] DataBuff = new byte[1024 * 64];  //数据包的存储缓冲数组
        private int currentDataLength = 0;              // 数据长度
        public Socket clientSocket;
        private bool isConnected = false;

        public event Action<byte[]> MessageRespondEvent;

        public void Connect(string hostName, int port, Action<object> callback)
        {
            IPAddress[] addresses = Dns.GetHostAddresses(hostName);
            AddressFamily _family = AddressFamily.Unknown;

            try
            {
                foreach (IPAddress address in addresses)
                {
                    if (clientSocket == null)
                    {
                        Debug.Assert(address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6);
                        if ((address.AddressFamily == AddressFamily.InterNetwork && Socket.OSSupportsIPv4) || Socket.OSSupportsIPv6)
                        {
                            Connect(new IPEndPoint(address, port));
                        }

                        _family = address.AddressFamily;
                        break;
                    }
                    if (address.AddressFamily == _family || _family == AddressFamily.Unknown)
                    {
                        Connect(new IPEndPoint(address, port));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                callback?.Invoke(ex);
            }
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            try
            {
                if (IsConnected)
                    Close();

                currentDataLength = 0;
                Array.Clear(DataBuff, 0, DataBuff.Length);
                clientSocket = Socket.OSSupportsIPv6
                    ? new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp)
                    : new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(ConnectCallBack), clientSocket);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message},{ex.StackTrace}");
                isConnected = false;
                throw ex;
            }
        }

        /// <summary>
        /// 连接结果回调
        /// </summary>
        /// <param name="asyncConnect"></param>
        private void ConnectCallBack(IAsyncResult asyncConnect)
        {
            try
            {
                clientSocket = (Socket)asyncConnect.AsyncState;
                clientSocket.EndConnect(asyncConnect);
                Debug.Log($"Beginning connectCallback IPEndPoint:{clientSocket.LocalEndPoint}");
                if (clientSocket != null && clientSocket.Connected)
                    IsConnected = true;
                Receive();
            }
            catch (SocketException ex)
            {
                isConnected = false;
                Debug.Log(" SocketException ErrorCode:" + ex.ErrorCode + "  SocketErrorCode:" + ex.SocketErrorCode + " ex:" + ex.ToString());
            }
            catch (Exception ex)
            {
                isConnected = false;
                Debug.Log("Exception ex:" + ex.ToString());
            }

        }

        public void DismantleData()
        {
            var headBytes = new byte[sizeof(short)];
            Buffer.BlockCopy(DataBuff, 0, headBytes, 0, headBytes.Length);
            var length = BitConverter.ToInt16(headBytes, 0);
            //var length = BitConverter.ToInt16(DataBuff, 0);
            var len = currentDataLength - length;
            if (len < 0)
                return;
            if (length <= 0 || length > DataBuff.Length)
            {
                Array.Clear(DataBuff, 0, DataBuff.Length);
                currentDataLength = 0;
                return;
            }
            byte[] msgData = new byte[length];
            Array.Copy(DataBuff, headBytes.Length, msgData, 0, length);

            MessageRespondEvent?.Invoke(msgData);

            Array.Clear(DataBuff, 0, length);
            if (len > 0)
            {
                byte[] DataBuffCopy = (byte[])DataBuff.Clone();
                Array.Clear(DataBuff, length, len);
                Array.Copy(DataBuffCopy, length, DataBuff, 0, len);
            }
            currentDataLength = len;
        }

        public void Send(byte[] SendBuf)
        {
            if (clientSocket == null)
                return;

            if (!clientSocket.Connected)
                return;

            try
            {
                if (SendBuf == null
                    || SendBuf.Length <= 0)
                    return;
                var asyncSend = clientSocket.BeginSend(SendBuf,
                    0,
                    SendBuf.Length,
                    SocketFlags.None,
                    new AsyncCallback(SendCallback), clientSocket);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message},{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 发送消息回调
        /// </summary>
        /// <param name="asyncSend"></param>
        private void SendCallback(IAsyncResult asyncSend)
        {
            try
            {
                var clientSocket = (Socket)asyncSend.AsyncState;
                if (clientSocket == null)
                    return;
                clientSocket.EndSend(asyncSend);
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message},{ex.StackTrace}");
            }

        }

        /// <summary>
        /// 接受数据
        /// </summary>
        private void Receive()
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                StateObject stateObject = new StateObject
                {
                    sBuffer = new byte[2048],
                    sSocket = clientSocket
                };
                clientSocket.BeginReceive(stateObject.sBuffer,
                    0,
                    stateObject.sBuffer.Length,
                    SocketFlags.None,
                    new AsyncCallback(ReceiveCallback),
                    stateObject);
            }
        }

        /// <summary>
        /// 获取消息回调
        /// </summary>
        /// <param name="asyncReceive"></param>
        private void ReceiveCallback(IAsyncResult asyncReceive)
        {
            try
            {
                StateObject stateObject = (StateObject)asyncReceive.AsyncState;
                if (stateObject.sSocket == null)
                    return;
                else if (!stateObject.sSocket.Connected)
                    return;
                int length = stateObject.sSocket.EndReceive(asyncReceive);
                if (length <= 0)
                    return;
                if (length + currentDataLength > DataBuff.Length)
                    return;
                Array.Copy(stateObject.sBuffer, 0, DataBuff, currentDataLength, length);
                currentDataLength += length;
                DismantleData();
            }
            catch (Exception e)
            {
                Close();
                Debug.LogError("   ReceiveCallback:" + e.ToString());
            }

            Receive();
        }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (clientSocket == null)
                    return false;
                return isConnected && clientSocket.Connected;
            }
            set { isConnected = value; }
        }

        /// <summary>
        /// 关闭socket
        /// </summary>
        /// <param name="isHandClose">是否是手动关闭</param>
        public void Close()
        {
            if (clientSocket == null)
                return;
            if (clientSocket != null && clientSocket.Connected)
            {
                clientSocket.Shutdown(SocketShutdown.Both);

                if (clientSocket != null && clientSocket.Connected)
                    clientSocket.Close();
            }

            clientSocket = null;
        }
    }
}
