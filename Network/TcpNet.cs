//using ClientCache;
//using FluentScheduler;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace we_admin.net.socket
//{
//    public class TcpNet : TcpNetBase
//    {
//        //事件//
//        public override event ConnectEvent connectEvent;
//        public override event ReciveEvent reciveEvent;
//        public override event SendEvent sendEvent;

//        //分析事件 data为接收到的原始数据//
//        public override event ReciveDataEvent reciveDataEvent;

//        // 初始化接收缓冲//
//        SocketAsyncEventArgs receiver = null;
//        // 初始化发送缓冲//
//        SocketAsyncEventArgs sender = null;

//        private byte[] resendCache = null;

//        public void Connect()
//        {
//            Connect(_IP, _Port);
//        }

//        public void Connect(string IP, int Port)
//        {
//            Connect(IP, Port, _cacheSize);
//        }

//        /*Registry registry = new Registry();
//        public TcpNet() {
//            registry.Schedule(() => {
//                Connect();
//            }).WithName("reconnect").ToRunNow().AndEvery(3).Seconds();
//        }*/

//        //连接//
//        public override void Connect(string IP, int Port, int size)
//        {
//            try
//            {
//                if (socket == null || !socket.Connected)
//                {
//                    _IP = IP;//外部传递进来的服务器ip地址，主要用于重连//
//                    _Port = Port;//外部传递进来的服务器端口号，主要用于重连//
//                    _cacheSize = size;

//                    IPAddress ip = IPAddress.Parse(IP);

//                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//                    SocketAsyncEventArgs connArg = new SocketAsyncEventArgs();
//                    // 要连接的远程服务器  //
//                    connArg.RemoteEndPoint = new IPEndPoint(ip, Port);
//                    // 操作完成后的回调  //
//                    connArg.Completed += (sendObj, arg) => {
//                        if (arg.SocketError == SocketError.Success)
//                        {//连接成功//
//                            //connEvent?.Invoke(ConnectionState.Succ);//通知上层，连接成功//

//                            sender = new SocketAsyncEventArgs();
//                            sender.Completed += sendDataEvent;
//                            receiver = new SocketAsyncEventArgs();
//                            // 操作完成后的回调  //
//                            receiver.Completed += receiveDataEvent;
//                            Receive();//启动接收器//

//                            if (resendCache != null)
//                            {
//                                Send(resendCache);// 重新发送//
//                                resendCache = null;
//                            }
//                        }
//                        connectEvent?.Invoke(arg.SocketError);
//                    };
//                    // 开始连接  //
//                    socket?.ConnectAsync(connArg);
//                }
//            }
//            catch (Exception)
//            {
//                connectEvent?.Invoke(SocketError.SocketError);
//            }
//        }

//        //接收方法//
//        public void Receive()
//        {
//            byte[] bytes = new byte[_cacheSize];
//            receiver.SetBuffer(bytes, 0, bytes.Length);
//            socket?.ReceiveAsync(receiver);
//        }

//        public void receiveDataEvent(object data, SocketAsyncEventArgs args) {
//            if (args.SocketError == SocketError.Success)
//            {//接收完成//
//                if (receiver?.BytesTransferred > 0)
//                {
//                    byte[] receiveData = new byte[receiver.BytesTransferred];
//                    Array.Copy(receiver.Buffer, receiver.Offset, receiveData, 0, receiver.BytesTransferred);
//                    reciveDataEvent(receiveData);
//                }
//                socket?.ReceiveAsync(receiver);
//            }
//            reciveEvent?.Invoke(receiver.SocketError);
//        }

//        //发送数据//
//        public override void Send(byte[] data)
//        {
//            if (socket == null)
//            {//没有连接//
//                Connect();
//            }
//            else
//            {
//                if (!socket.Connected)
//                {//已经断开，重新连接//
//                    Close();
//                    Connect();
//                    resendCache = data;
//                }
//                else if (socket.Connected)
//                {//处于连接状态//
//                    sender.SetBuffer(data, 0, data.Length);
//                    socket.SendAsync(sender);
//                }
//            }
//        }

//        public void sendDataEvent(object data, SocketAsyncEventArgs args) {
//            sendEvent?.Invoke(args.SocketError);
//        }

//        //关闭unitysocket，释放资源//
//        public override void Close()
//        {
//            if (socket != null)
//            {
//                if (receiver != null)
//                {
//                    receiver.Completed -= receiveDataEvent;
//                }
//                if (sender != null)
//                {
//                    sender.Completed -= sendDataEvent;
//                }
//                socket.Close();
//            }
//        }
//    }
//}
