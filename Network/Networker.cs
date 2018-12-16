using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Arthas.Network
{
    public enum NetworkStatus
    {
        Connecting = 1,            //正在连接
        Connected = 2,             //网络已经连接
        Disconnected = 3,          //网络断开
        NetworkNotReachbility = 4, //无网络
        ConnectTimeout = 5,        //超时
        HighLatency = 6            //高延迟
    }

    public class NetworkStatusEventArgs
    {
        public NetworkStatus Status { get; set; }
        public string Description { get; set; }
        public int StateCode { get; set; }
    }

    /// <summary>
    /// 网络
    /// </summary>
    public class Networker : SingletonBehaviour<Networker>
    {
        /// <summary>
        /// 推送和响应事件
        /// </summary>
        public static event Action<INetworkMessage> ResponseEvent;

        public static event Action ConnectedEvent, DisconnectedEvent;

        /// <summary>
        /// 网络状态事件
        /// </summary>
        [Obsolete("Use NetworkStatusEventHandler instead!")]
        public static event Action<NetworkStatus> NetworkStatusEvent;

        /// <summary>
        /// 网络状态事件
        /// </summary>
        public static event EventHandler<NetworkStatusEventArgs> NetworkStatusEventHandler;

        /// <summary>
        /// 错误回调
        /// </summary>
        public static event Action<string> ConnectErrorEvent;

        public static NetworkStatus NetworkStatus { get; private set; }
        public static int PingTime { get; private set; }
        public static Func<object> HeartbeatCommandGetter { get; set; }

        public const short DefaultHeartbeatCommand = 0, MaxBuffSize = 4096;

        public static bool IsLittleEndian
        {
            get { return Instance.isLittleEndian; }
            set { Instance.isLittleEndian = value; }
        }

        public static bool IsConnected { get { return connection != null && connection.IsConnected; } }
        private static object enterLock = new object();
        private static Ping ping;

        private readonly static ConcurrentQueue<INetworkMessage> msgQueue = new ConcurrentQueue<INetworkMessage>();
        private readonly static Dictionary<object, Queue<Action<INetworkMessage>>> responseActions = new Dictionary<object, Queue<Action<INetworkMessage>>>();

        private Coroutine timeoutCor, connectCor, heartbeatCor;
        private WaitForSeconds heartbeatWaitFor, timeoutWaiter, connectPollWaiter = new WaitForSeconds(.5f);
        private DateTime prevHeartbeatDateTime = DateTime.MinValue;
        private float prevConnectTime, connectCheckDuration = .1f;
        private bool isPaused = false;
        private int retryCount = 0;

        [SerializeField]
        private float connectTimeout = 10f,
            heartbeatInterval = 12f,
            pingInterval = 1f,
            heartbeatTimeout = 8f,
            delayTimeThreshold = 150;

        [SerializeField]
        private bool isLittleEndian = true,
            useSSL = false,
            enableLogging = true;

        [SerializeField]
        private int maxRetryCount = 3;

        [SerializeField, HideInInspector]
        private string connectorTypeName, messageHandlerName;
        private static IConnection connection;
        private static INetworkMessageHandler messageHandler;
        public NetworkAddress NetworkAddress { get; private set; }
        private static float prevSendTime = 0,
            prevSendPingTime = 0;
        private static bool isConnecting = false;

        protected override void Awake()
        {
            base.Awake();
            timeoutWaiter = new WaitForSeconds(connectCheckDuration);
            heartbeatWaitFor = new WaitForSeconds(heartbeatInterval);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        protected void ConnectInternal(string ip, short port, IConnection conn, INetworkMessageHandler handler = null)
        {
            if (connection != null && connection.IsConnected) connection.Close();
            if (!string.IsNullOrEmpty(connectorTypeName))
                connection = (IConnection)Activator.CreateInstance(Type.GetType(connectorTypeName, true, true));
            if (!string.IsNullOrEmpty(messageHandlerName))
                messageHandler = (INetworkMessageHandler)Activator.CreateInstance(Type.GetType(messageHandlerName, true, true));
            if (messageHandler == null)
                messageHandler = handler ?? new DefaultMessageHandler();
            if (connection == null)
                connection = conn ?? new TCPConnection();
            NetworkAddress = new NetworkAddress { ip = ip, port = port };
            ping = new Ping(ip);
            Connect();
#if UNITY_EDITOR
            Debug.LogFormat("Connect to server , <color=cyan>Addr:[{0}:{1}] ,connector:{2} ,wrapper:{3}.</color>", ip, port, connection.GetType(), messageHandler.GetType());
#endif
        }

        /// <summary>
        /// 根据网络配置连接到服务器
        /// </summary>
        /// <param name="configuration"></param>
        public static void Connect(Action callback = null,
           Action<string> error = null,
           IConnection conn = null,
           INetworkMessageHandler handler = null)
        {
            Connect(NetworkConfiguration.Current.ip,
                NetworkConfiguration.Current.port,
                callback,
                error,
                conn,
                handler);
        }

        /// <summary>
        /// 根据IP连接到服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callback"></param>
        /// <param name="error"></param>
        /// <param name="handler"></param>
        public static void Connect(string ip,
            short port,
            Action callback = null,
            Action<string> error = null,
            IConnection connector = null,
            INetworkMessageHandler handler = null)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (NetworkStatusEvent != null) NetworkStatusEvent(NetworkStatus.NetworkNotReachbility);
                if(error != null) error.Invoke("Network not reachable , please check you network setting!");
                return;
            }
#endif
            ConnectedEvent = callback;
            ConnectErrorEvent = error;
            Instance.ConnectInternal(ip, port, connector, handler);
        }

        protected void Connect()
        {
            if (isConnecting || IsConnected) return;
            isConnecting = true;
            prevConnectTime = Time.time;
            if (connection != null) connection.Connect(NetworkAddress.ip, NetworkAddress.port);
            InvokeStatusEvent(NetworkStatus.Connecting);
            timeoutCor = StartCoroutine(TimeoutDetectAsync());
        }

        protected IEnumerator TimeoutDetectAsync()
        {
            while (true)
            {
                yield return timeoutWaiter;
                if (connection.IsConnected)
                {
                    OnConnected();
                    StopCoroutine(timeoutCor);
                    yield break;
                }
                if (Time.time - prevConnectTime > connectTimeout)
                {
                    prevConnectTime = 0;
                    OnDisconnected();
                    StopCoroutine(timeoutCor);
                    if (ConnectErrorEvent != null) ConnectErrorEvent("Cannot connect to server , please check your network!");
                }
            }
        }

        private void InvokeStatusEvent(NetworkStatus status, string description = null)
        {
            if (NetworkStatus == status) return;
            NetworkStatus = status;
            if (NetworkStatusEvent != null) NetworkStatusEvent(NetworkStatus);
            if (NetworkStatusEventHandler != null) NetworkStatusEventHandler(this, new NetworkStatusEventArgs
            {
                Status = status,
                Description = description
            });
            Debug.LogFormat("Network status event={0} invoked!", status);
        }

        protected IEnumerator HeartbeatDetectAsync()
        {
            while (true)
            {
                yield return heartbeatWaitFor;
                var cmd = (HeartbeatCommandGetter != null)
                    ? HeartbeatCommandGetter()
                    : DefaultHeartbeatCommand;
                SendHeartbeat(cmd);
            }
        }

        public void SendHeartbeat(object command)
        {
            if (!connection.IsConnected) return;
            if (prevHeartbeatDateTime == DateTime.MinValue)
                prevHeartbeatDateTime = DateTime.Now;
            var diff = DateTime.Now - prevHeartbeatDateTime;
            if (diff.TotalSeconds > heartbeatTimeout
                && IsConnected)
            {
                Debug.LogError($"Heatbeat timeout!");
                StopCoroutine(heartbeatCor);
                Close();
                return;
            }
            Send(command, callback: r => prevHeartbeatDateTime = DateTime.Now);
        }

        protected IEnumerator ConnectionDetectAsync()
        {
            while (true)
            {
                yield return connectPollWaiter;
                if (Application.internetReachability == NetworkReachability.NotReachable
                    || !connection.IsConnected)
                {
                    OnDisconnected();
                    StopCoroutine(connectCor);
                    var status = Application.internetReachability == NetworkReachability.NotReachable
                        ? NetworkStatus.NetworkNotReachbility
                        : NetworkStatus.Disconnected;
                    InvokeStatusEvent(status);
                }
            }
        }

        protected void OnConnected()
        {
            retryCount = 0;
            isConnecting = false;
            if (connection != null)
            {
                connection.MessageRespondEvent -= OnMessageRespond;
                connection.MessageRespondEvent += OnMessageRespond;
            }
            ConnectedEvent?.Invoke();
            InvokeStatusEvent(NetworkStatus.Connected);
            prevHeartbeatDateTime = DateTime.Now;
            connectCor = StartCoroutine(ConnectionDetectAsync());
            heartbeatCor = StartCoroutine(HeartbeatDetectAsync());
        }

        protected void OnDisconnected()
        {
            connection.MessageRespondEvent -= OnMessageRespond;
            isConnecting = false;
            if (connection.IsConnected) return;
            DisconnectedEvent?.Invoke();
            InvokeStatusEvent(NetworkStatus.Disconnected);

            //if (retryCount == maxRetryCount)
            //{
            //    InvokeStatusEvent(NetworkStatus.Disconnected);
            //    if (DisconnectedEvent != null) DisconnectedEvent();
            //    retryCount = 0;
            //}
            //else
            //{
            //    retryCount++;
            //    this.Invoke(Connect, 1f);
            //}
        }

        protected void OnMessageRespond(byte[] buffer)
        {
            lock (msgQueue)
            {
                // buf.WriteBytes(buffer);
                var msgs = messageHandler.ParseMessage(buffer);
                for (var i = 0; i < msgs.Count; i++)
                {
                    msgQueue.Enqueue(msgs[i]);
                    INetworkMessage message;
                    if (msgQueue.Count > byte.MaxValue) msgQueue.TryDequeue(out message);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (Instance.enableLogging)
                        Debug.LogFormat("<color=blue>[TCPNetwork]</color> [Receive] << CMD:{0},TIME:{1}", msgs[i].Command, DateTime.Now);
#endif
                }
            }
        }

        private void Update()
        {
            while (msgQueue.Count > 0)
            {
                INetworkMessage message;
                if (!msgQueue.TryDequeue(out message)) continue;
                if (responseActions.Count > 0 && responseActions.ContainsKey(message.Command))
                {
                    var action = responseActions[message.Command].Dequeue();
                    if (action != null) action.Invoke(message);
                }
                ResponseEvent?.Invoke(message);
            }
        }

        private void Ping()
        {
            if (connectCor != null && connection.IsConnected
               && Time.time - prevSendPingTime > pingInterval
               && ping != null)
            {
                if (ping.time <= 0)
                    return;

                if (ping.time > delayTimeThreshold && PingTime > delayTimeThreshold)
                {
                    var closeThreshold = delayTimeThreshold * 2;
                    if (ping.time > closeThreshold && PingTime > closeThreshold)
                    {
                        PingTime = 0;
                        Close();
                        return;
                    }
                    InvokeStatusEvent(NetworkStatus.HighLatency);
                }

                PingTime = ping.time;
                ping.DestroyPing();
                prevSendPingTime = Time.time;
                ping = new Ping(NetworkAddress.ip);
            }
        }

        public static void Send(object cmd, object buf = null, Action<INetworkMessage> callback = null, params object[] parameters)
        {
            if (!IsConnected)
                return;

            try
            {
                var message = messageHandler.PackMessage(cmd, buf, parameters);
                var buffer = message.GetBuffer(IsLittleEndian);
                if (!responseActions.ContainsKey(message.Command))
                    responseActions.Add(message.Command, new Queue<Action<INetworkMessage>>());
                responseActions[message.Command].Enqueue(callback);
                connection.Send(buffer);
                prevSendTime = Time.time;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (Instance.enableLogging)
                    Debug.LogFormat("<color=cyan>[TCPNetwork]</color> [Send] >> CMD:{0},TIME:{1}", cmd, DateTime.Now);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static void Close()
        {
            if (connection != null) connection.Close();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            isPaused = !hasFocus;
#endif
        }

        private void OnApplicationPause(bool pause)
        {
            isPaused = pause;
        }

        private void OnApplicationQuit()
        {
            Close();
        }
    }
}