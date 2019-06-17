using Hazel;
using Hazel.Udp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// Obviously this isn't a real Unity project, but we'll just pretend.
// I don't want to create the external buttons and stuff, so that's totally up to you.
namespace Arthas.Network
{
    // Usually this kind of class should be a singleton, but everyone has 
    // their own way of doing that. So I leave it up to you.
    internal class HazelNetworkManager : MonoBehaviour, IConnection
    {
        private const int ServerPort = 23456;

        // Unity gets very grumpy if you start messing with GameObjects on threads
        // other than the main one. So while sending/receiving messages can be multithreaded,
        // we need a queue to hold events until a Update/FixedUpdate method can handle them.
        public List<Action> EventQueue = new List<Action>();

        // How many seconds between batched messages
        public float MinSendInterval = .1f;

        private UdpClientConnection connection;

        // This will hold a reliable and an unreliable "channel", so you can batch 
        // messages to the server every MinSendInterval seconds.
        private readonly MessageWriter[] Streams = new MessageWriter[] {
            MessageWriter.Get(SendOption.None),
            MessageWriter.Get(SendOption.Reliable)
        };

        private float timer = 0;

        public event Action<byte[]> MessageRespondEvent;

        public bool IsConnected => throw new NotImplementedException();

        public void Update()
        {
            lock (EventQueue)
            {
                foreach (var evt in EventQueue)
                {
                    evt();
                }

                EventQueue.Clear();
            }

            timer += Time.fixedDeltaTime;

            if (timer < MinSendInterval)
            {
                // Unless you are making a highly competitive action game, you don't need updates
                // every frame. And many network connections cannot handle that kind of traffic.
                return;
            }

            timer = 0;

            foreach (var msg in Streams)
            {
                try
                {
                    // TODO: In hazel, I need to change this so it makes sense
                    // Right now:
                    // 7 = Tag (1) + MessageLength (2) + GameId (4)
                    // Ideally, no magic calculation, just msg.HasMessages
                    if (!msg.HasBytes(7)) continue;
                    msg.EndMessage();
                    connection.Send(msg);
                }
                catch
                {
                    // Logging, probably
                }

                msg.Clear(msg.SendOption);
                msg.StartMessage(0);
                msg.Write(1);
            }
        }

        public void Send(byte[] buffer)
        {
            if (connection == null) return;

            var msg = MessageWriter.Get(SendOption.Reliable);
            msg.WriteBytesAndSize(buffer);

            try { connection.Send(msg); }
            catch
            {
            }
            msg.Recycle();
        }


        public IEnumerator ConnectAsync(string ip , int port , Action<object> callback)
        {
            // Don't leak connections!
            if (connection != null) yield break;

            Streams[0].Clear(SendOption.None);
            Streams[1].Clear(SendOption.Reliable);

            connection = new UdpClientConnection(new IPEndPoint(IPAddress.Parse(ip), port));
            connection.DataReceived += HandleMessage;
            connection.Disconnected += HandleDisconnect;

            // If you block in a Unity Coroutine, it'll hang the game!
            connection.ConnectAsync(new byte[] { 1, 0, 0, 0 });

            while (connection != null && connection.State != ConnectionState.Connected)
            {
                yield return null;
            }

            callback?.Invoke(null);
        }

        // Remember this is on a new thread.
        private void HandleDisconnect(object sender, DisconnectedEventArgs e)
        {
            lock (EventQueue)
            {
                EventQueue.Clear();
                // Maybe something like:
                // this.EventQueue.Add(ChangeToMainMenuSceneWithError(e.Reason));
            }
        }

        private void HandleMessage(DataReceivedEventArgs obj)
        {
            try
            {
                while (obj.Message.Position < obj.Message.Length)
                {
                    // Remember from the server code that sub-messages aren't pooled,
                    // they share the parent message's buffer. So don't recycle them!
                    var msg = obj.Message.ReadMessage();
                    int idOrError = msg.ReadInt32();
                }
            }
            catch
            {
                // Error logging
            }
            finally
            {

            }
        }

        public void Connect(string ip, int port, Action<object> callback = null)
        {
            StartCoroutine(ConnectAsync(ip, port, callback);
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}