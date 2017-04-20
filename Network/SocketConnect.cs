using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace Arthas.Network
{
    public class SocketConnect : MonoBehaviour
    {
        public class ResponseContext
        {
            public int Id { get; set; }
            public Action<IMessage> Response { get; set; }
        }

        public static SocketConnect Instance;
        private Socket socket;
        private Thread thread;                  //接收消息的线程
        private int messageSerialNumber = 1;    //消息序列号
        private readonly Dictionary<int, ResponseContext> responses = new Dictionary<int, ResponseContext>();

        private void Awake()
        {
            Instance = this;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var address = IPAddress.Parse(NetworkConfiguration.Current.ip);
            var endpoint = new IPEndPoint(address, NetworkConfiguration.Current.port);
            var result = socket.BeginConnect(endpoint, new AsyncCallback(OnConnected), socket);
            if (result.AsyncWaitHandle.WaitOne(5000, true))
            {
                thread = new Thread(new ThreadStart(Receive));
                thread.Start();
            }
            else
            {
                Close();
                Debug.Log("Connect timeout!");
            }
        }

        private void OnConnected(IAsyncResult asyncConnect)
        {
            Debug.Log("Connect success");
        }

        private void Receive()
        {
            while (true)
            {
                if (!socket.Connected)
                {
                    Debug.Log("Failed to clientSocket server.");
                    socket.Close();
                    break;
                }
                try
                {
                    byte[] bytes = new byte[4096];
                    int i = socket.Receive(bytes);
                    if (i <= 0)
                    {
                        socket.Close();
                        break;
                    }
                    Debug.Log("Server say :" + Encoding.Default.GetString(bytes));
                }
                catch (Exception e)
                {
                    Debug.Log("Disconnected from server , info :" + e);
                    socket.Close();
                    break;
                }
            }
        }

        public void Close()
        {
            if (socket != null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            socket = null;
        }

        public void Send(IMessage message, Action<IMessage> callback = null)
        {
            if (!socket.Connected)
            {
                socket.Close();
                return;
            }
            try
            {
                if (messageSerialNumber >= int.MaxValue) messageSerialNumber = 1;
                message.Header.SerialNumber = messageSerialNumber++;
                var buffer = message.GetBufferWithLength();
                var pair = new ResponseContext() { Id = message.Header.SerialNumber, Response = callback };
                var asyncSend = socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), pair);
                if (asyncSend.AsyncWaitHandle.WaitOne(5000, true))
                {
                    Debug.LogError("Send data to server fail or timeout!");
                    socket.Close();
                }
            }
            catch (Exception ex)
            {
                socket.Close();
                Debug.LogErrorFormat("Send message error , detail:", ex.Message);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            var context = ar.AsyncState as ResponseContext;
            responses.Add(context.Id, context);
        }

        void OnApplicationQuit()
        {
            thread.Abort();
            Close();
        }
    }
}