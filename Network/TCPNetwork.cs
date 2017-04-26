using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Arthas.Network
{
    /// <summary>
    /// TCP网络
    /// </summary>
    public class TCPNetwork : SingletonBehaviour<TCPNetwork>
    {
        /// <summary>
        /// 推送事件
        /// </summary>
        public static event Action<IMessage> PushEvent;
        /// <summary>
        /// 网络连接事件
        /// </summary>
        public static event Action ConnectedEvent;
        /// <summary>
        /// 网络断开事件
        /// </summary>
        public static event Action DisconnectedEvent;
        /// <summary>
        /// 错误回调
        /// </summary>
        private static Action<string> ErrorCallback;

        public static IMessageWrapper MessageWrapper { get; private set; }

        public static bool IsLittleEndian
        {
            get { return Instance.isLittleEndian; }
            set { Instance.isLittleEndian = value; }
        }

        public static bool IsConnected { get { return connector.IsConnected; } }

        private readonly static TCPConnect connector = new TCPConnect();
        private readonly static Queue<IMessage> msgQueue = new Queue<IMessage>();
        private readonly static Dictionary<int, Action<IMessage>> responseActions = new Dictionary<int, Action<IMessage>>();

        private WaitForSeconds heartbeatWaiter, timeoutWaiter;
        private Coroutine checkTimeout;
        private float currentTime, connectCheckDuration = .1f;
        private static object enterLock = new object();

        [SerializeField]
        private float connectTimeout = 10f, heartbeatDuration = 8f;
        [SerializeField]
        private bool isLittleEndian = true;
        private static int messageSerialNumber = 1;

        protected override void Awake()
        {
            base.Awake();
            timeoutWaiter = new WaitForSeconds(connectCheckDuration);
            heartbeatWaiter = new WaitForSeconds(heartbeatDuration);
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect(string ip, int port, IMessageWrapper wrapper = null)
        {
            if (connector.IsConnected) connector.Close();
            MessageWrapper = wrapper ?? new DefaultMessageWrapper(isLittleEndian);
            connector.Connect(ip, port);
            checkTimeout = StartCoroutine(CheckeTimeout());
#if UNITY_EDITOR
            Debug.LogFormat("Connect to server , <color=cyan>Addr:[{0}:{1}] ,wrapper:{2}.</color>", ip, port, MessageWrapper.GetType());
#endif
        }

        /// <summary>
        /// 根据网络配置连接到服务器
        /// </summary>
        /// <param name="configuration"></param>
        public static void Connect(Action callback = null,
            Action<string> error = null,
            IMessageWrapper wrapper = null)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (Application.internetReachability == NetworkReachability.NotReachable
                && error != null)
            {
                error.Invoke("Network not reachable , please check you network setting!");
                return;
            }
#endif
            ErrorCallback = error;
            ConnectedEvent = callback;
            Instance.Connect(NetworkConfiguration.Current.ip, NetworkConfiguration.Current.port, wrapper);
        }

        private IEnumerator CheckeTimeout()
        {
            while (true)
            {
                yield return timeoutWaiter;
                if (connector.IsConnected)
                {
                    OnConnected();
                    StopCoroutine(checkTimeout);
                    yield break;
                }
                if ((currentTime += Time.deltaTime) > connectTimeout)
                {
                    currentTime = 0;
                    StopCoroutine(checkTimeout);
                    if (ErrorCallback != null)
                    {
                        ErrorCallback("Cannot connect to server , please check your network!");
                    }
                }
            }
        }

        private void OnConnected()
        {
            if (ConnectedEvent != null)
                ConnectedEvent();
            //StartCoroutine(Heartbeat());
            HandleEvent(true);
            Debug.LogFormat("Connect server success, Addr : {0} .", connector.Address);
        }

        private void OnDisconnected()
        {
            if (DisconnectedEvent != null)
                DisconnectedEvent();
            HandleEvent(false);
        }

        private void OnMessageRespond(byte[] buffer)
        {
            lock (enterLock)
            {
                var lenthbytes = new byte[sizeof(int)];
                Buffer.BlockCopy(buffer, 0, lenthbytes, 0, sizeof(int));
                var lenth = BitConverter.ToInt32(isLittleEndian ? lenthbytes : lenthbytes.Reverse(), 0);
                var dest = new byte[lenth - sizeof(int)];
                Buffer.BlockCopy(buffer, sizeof(int), dest, 0, lenth - sizeof(int));
                var msg = MessageWrapper.FromBuffer(dest, true);
                msgQueue.Enqueue(msg);
#if UNITY_EDITOR
                Debug.LogFormat("<color=green>[TCPNetwork]</color> [Receive] << SN:{0} , Descriptor:{1} , CMD:{2} , BUF_SIZE:{3}",
                    msg.Header.SerialNumber,
                    msg.Header.Descriptor,
                    msg.Header.Command,
                    buffer.Length);
#endif
            }
        }

        private void Update()
        {
            if (msgQueue.Count > 0)
            {
                var message = msgQueue.Dequeue();
                if (responseActions.Count > 0
                    && responseActions.ContainsKey(message.Header.Command))
                {
                    var action = responseActions[message.Header.Command];
                    if (action != null)
                    {
                        action.Invoke(message);
                        responseActions.Remove(message.Header.Command);
                        return;
                    }
                }
                else if (PushEvent != null)
                {
                    PushEvent(message);
                }
            }
        }

        public IEnumerator Heartbeat()
        {
            while (connector.IsConnected)
            {
                yield return heartbeatWaiter;
                //connector.Send(1, -1, string.Empty);
            }
        }

        public static void Send(short cmd, byte[] buf, Action<IMessage> callback)
        {
            var header = MessageWrapper.RequestHeader;
            header.SerialNumber = messageSerialNumber;
            header.Descriptor = (short)(cmd == 10001 ? 10 : 33);
            header.Command = cmd;
            responseActions.Replace(MessageWrapper.RequestHeader.Command, callback);
            try
            {
                var message = MessageWrapper.FromBuffer(buf);
                var buffer = message.GetBufferWithLength();
                connector.Send(buffer);
#if UNITY_EDITOR
                Debug.LogFormat("<color=cyan>[TCPNetwork]</color> [Send] >> SN:{0} , Descriptor:{1} , CMD:{2} , BUF_SIZE:{3}",
                    header.SerialNumber,
                    header.Descriptor,
                    header.Command,
                    buffer.Length);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public static void Send(short cmd, object obj, Action<IMessage> callback)
        {
            var header = MessageWrapper.RequestHeader;
            header.SerialNumber = messageSerialNumber;
            header.Descriptor = (short)(cmd == 10001 ? 10 : 33);
            header.Command = cmd;
            responseActions.Add(MessageWrapper.RequestHeader.Command, callback);
            try
            {
                var message = MessageWrapper.FromObject(obj);
                var buffer = message.GetBufferWithLength();
                connector.Send(buffer);
#if UNITY_EDITOR
                Debug.LogFormat("<color=cyan>[TCPNetwork]</color> [Send] >> SN:{0} , Descriptor:{1} , CMD:{2} , BUF_SIZE:{3}",
                    header.SerialNumber,
                    header.Descriptor,
                    header.Command,
                    buffer.Length);
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private void HandleEvent(bool bind)
        {
            if (bind)
            {
                connector.MessageRespondEvent += OnMessageRespond;
                connector.DisconnectEvent += OnDisconnected;
            }
            else
            {
                connector.MessageRespondEvent -= OnMessageRespond;
                connector.DisconnectEvent -= OnDisconnected;
            }
        }

        public static void Close()
        {
            connector.Close();
        }
    }
}