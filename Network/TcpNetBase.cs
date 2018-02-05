//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace we_admin.net.socket
//{
//    public abstract class TcpNetBase
//    {
//        protected Socket socket = null;//异步socket//
//        protected int _cacheSize = 1024;//接收缓冲大小//
//        protected string _IP = "127.0.0.1";
//        protected int _Port = 8080;
//        //protected int headSize = 6;//包头长度//

//        //Socket连接状态//
//        public enum ConnectionState
//        {
//            Succ,//成功//
//            Fail,//失败//
//            ConnectionRefused,//不能连接到服务器//
//            NetworkUnreachable//本地网络不能用//
//        }

//        public enum SendMsgState {
//            Succ,
//            Fail
//        }

//        //连接事件//
//        /*public delegate void ConnectionEvent(ConnectionState state);
//        public abstract event ConnectionEvent connEvent;*/


//        //--------------------事件-----------------------//
//        //连接//
//        public delegate void ConnectEvent(SocketError socketError);
//        public abstract event ConnectEvent connectEvent;
//        //接收//
//        public delegate void ReciveEvent(SocketError socketError);
//        public abstract event ReciveEvent reciveEvent;
//        //发送//
//        public delegate void SendEvent(SocketError socketError);
//        public abstract event SendEvent sendEvent;
//        //--------------------事件-----------------------//

//        //接收到数据 //
//        public delegate void ReciveDataEvent(byte[] data);
//        public abstract event ReciveDataEvent reciveDataEvent;

//        //连接//
//        public abstract void Connect(string IP, int Port, int size);

//        //发送数据//
//        public abstract void Send(byte[] data);

//        //关闭连接//
//        public abstract void Close();
//    }
//}
