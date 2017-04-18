using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading;

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
        public static event Action DisconnectedEvent;
        public static bool IsConnected { get { return connector.IsConnected; } }
        private static Action<string> ErrorCallback;
        private static TCPConnect connector = new TCPConnect();

        private static Queue<IMessage> msgQueue = new Queue<IMessage>();
        private static Queue<KeyValuePair<int, Action<IMessage>>> actionQueue = new Queue<KeyValuePair<int, Action<IMessage>>>();
        private WaitForSeconds heartbeatDuration = new WaitForSeconds(15f);
        private Coroutine pollCoroutine;
        private static object enterLock = new object();

        /// <summary>
        /// 处理响应事件
        /// </summary>
        private const float connectTimeout = 5f;
        private bool checkTimeout = false;
        private float timeout = 0;

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect(string ip, int port, bool poll = false)
        {
            if (connector.IsConnected)
            {
                OnConnected();
                return;
            }
            connector.Connect(ip, port);
            checkTimeout = true;
            if (poll && pollCoroutine == null)
                pollCoroutine = StartCoroutine(PollConnect());
        }

        /// <summary>
        /// 根据网络配置连接到服务器
        /// </summary>
        /// <param name="configuration"></param>
        public static void Connect(Action onSuccess = null, Action<string> onError = null)
        {
#if !UNITY_EDITOR && !UNITY_STANDALONE
            if (Application.internetReachability == NetworkReachability.NotReachable
                && onError != null)
            {
                onError.Invoke("Network not reachable , please check you network setting!");
                return;
            }
#endif
            ErrorCallback = onError;
            ConnectedEvent = onSuccess;
            Instance.Connect(NetworkConfiguration.Current.ip, NetworkConfiguration.Current.port);
        }

        private IEnumerator PollConnect()
        {
            while (true)
            {
                yield return new WaitForSeconds(3f);
                if (!connector.IsConnected)
                {
                    Connect();
#if UNITY_EDITOR
                    Debug.LogError("Connect server fail , try reconnecting!");
#endif
                }
            }
        }

        private void CheckeTimeout()
        {
            if (connector.IsConnected)
            {
                checkTimeout = false;
                OnConnected();
                return;
            }
            if ((timeout += Time.deltaTime) > connectTimeout)
            {
                timeout = 0;
                checkTimeout = false;
                if (ErrorCallback != null)
                    ErrorCallback("Cannot connect to server , please check your network!");
            }
        }

        private void OnConnected()
        {
            if (ConnectedEvent != null)
                ConnectedEvent();
            StartCoroutine(Heartbeat());
            HandleEvent(true);
            Debug.LogFormat("Connect server success, Addr : {0} .", connector.Address);
        }

        private void OnDisconnected()
        {
            if (DisconnectedEvent != null)
                DisconnectedEvent();
            HandleEvent(false);
        }

        private void OnMessageRespond(IMessage wrapper)
        {
            lock (enterLock)
            {
                msgQueue.Enqueue(wrapper);
            }
        }

        private void Update()
        {
            if (checkTimeout)
                CheckeTimeout();
            ReciveMessage();
        }

        private void ReciveMessage()
        {
            if (msgQueue.Count > 0)
            {
                var wrapper = msgQueue.Dequeue();
                var msgId = -1;
                Action<IMessage> action = null;
                if (actionQueue.Count > 0)
                {
                    var pair = actionQueue.Dequeue();
                    msgId = pair.Key;
                    action = pair.Value;
                }
                if (wrapper.Head.CommandId == msgId && wrapper.Head.CommandId > 0)
                {
                    if (action != null)
                    {
                        action.Invoke(wrapper);
                        return;
                    }
                }
                else if (PushEvent != null)
                {
                    PushEvent(wrapper);
                    return;
                }
            }
        }

        public IEnumerator Heartbeat()
        {
            while (connector.IsConnected)
            {
                yield return heartbeatDuration;
                connector.SendData(1, -1, string.Empty);
            }
        }

        /// <summary>
        /// 发送一个带返回值的消息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="msgId"></param>
        /// <param name="t"></param>
        /// <param name="callback"></param>
        public static void Send<T>(CommandType cmdType, T t, Action<IMessage> callback = null)
        {
            var msgId = (short)cmdType;
            var pair = new KeyValuePair<int, Action<IMessage>>(msgId, callback);
            actionQueue.Enqueue(pair);
            connector.Send(msgId, t);
        }

        public static void Send(CommandType cmdType, Action<IMessage> callback = null)
        {
            Send(cmdType, new object(), callback);
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
    }
}