﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;

using Sulakore.Network;
using Sulakore.Network.Protocol;

namespace HabboGallery.Desktop.Habbo.Network
{
    public class HConnection : IHConnection
    {
        private readonly object _disconnectLock;

        private bool _isIntercepting;
        private int _inSteps, _outSteps;

        /// <summary>
        /// Occurs when the connection between the client, and server have been intercepted.
        /// </summary>
        public event EventHandler<ConnectedEventArgs> Connected;
        protected virtual void OnConnected(ConnectedEventArgs e)
        {
            Connected?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when either the game client, or server have disconnected.
        /// </summary>
        public event EventHandler Disconnected;
        protected virtual void OnDisconnected(EventArgs e)
        {
            Disconnected?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when the client's outgoing data has been intercepted.
        /// </summary>
        public event EventHandler<DataInterceptedEventArgs> DataOutgoing;
        protected virtual void OnDataOutgoing(DataInterceptedEventArgs e)
        {
            DataOutgoing?.Invoke(this, e);
        }

        /// <summary>
        /// Occrus when the server's incoming data has been intercepted.
        /// </summary>
        public event EventHandler<DataInterceptedEventArgs> DataIncoming;
        protected virtual void OnDataIncoming(DataInterceptedEventArgs e)
        {
            DataIncoming?.Invoke(this, e);
        }

        public int ListenPort { get; set; } = 9567;
        public bool IsConnected { get; private set; }

        public HNode Local { get; private set; }
        public HNode Remote { get; private set; }
        public X509Certificate Certificate { get; set; }

        public HConnection()
        {
            _disconnectLock = new object();
        }

        public Task InterceptAsync(IPEndPoint endpoint)
        {
            return InterceptAsync(new HotelEndPoint(endpoint));
        }
        public Task InterceptAsync(string host, int port)
        {
            return InterceptAsync(HotelEndPoint.Parse(host, port));
        }
        public async Task InterceptAsync(HotelEndPoint endpoint)
        {
            // TODO: Implement the usage of a CancellationToken instead of constantly checking the _isIntercepting field.
            _isIntercepting = true;
            while (!IsConnected && _isIntercepting)
            {
                try
                {
                    Local = await HNode.AcceptAsync(ListenPort).ConfigureAwait(false);
                    if (!_isIntercepting) break;

                    bool wasDetermined = await Local.DetermineFormatsAsync().ConfigureAwait(false);
                    if (!_isIntercepting) break;

                    if (Local.IsWebSocket)
                    {
                        await Local.UpgradeWebSocketAsServerAsync(Certificate).ConfigureAwait(false);
                    }
                    else if (wasDetermined) throw new NotSupportedException();

                    var args = new ConnectedEventArgs(endpoint);
                    OnConnected(args);

                    endpoint = args.HotelServer ?? endpoint;
                    if (endpoint == null)
                    {
                        endpoint = await args.HotelServerSource.Task.ConfigureAwait(false);
                    }

                    Remote = await HNode.ConnectAsync(endpoint).ConfigureAwait(false);
                    Remote.ReflectFormats(Local);
                    if (Local.IsWebSocket)
                    {
                        IsConnected = await Remote.UpgradeWebSocketAsClientAsync().ConfigureAwait(false);
                    }
                    else IsConnected = true;

                    _inSteps = 0;
                    _outSteps = 0;
                    Task interceptOutgoingTask = InterceptOutgoingAsync();
                    Task interceptIncomingTask = InterceptIncomingAsync();
                }
                finally
                {
                    if (!IsConnected)
                    {
                        Local?.Dispose();
                        Remote?.Dispose();
                    }
                }
            }
            _isIntercepting = false;
        }

        public ValueTask<int> SendToServerAsync(byte[] data)
        {
            return Remote.SendAsync(data);
        }
        public ValueTask<int> SendToServerAsync(HPacket packet)
        {
            return Remote.SendAsync(packet);
        }
        public ValueTask<int> SendToServerAsync(ushort id, params object[] values)
        {
            return Remote.SendAsync(id, values);
        }

        public ValueTask<int> SendToClientAsync(byte[] data)
        {
            return Local.SendAsync(data);
        }
        public ValueTask<int> SendToClientAsync(HPacket packet)
        {
            return Local.SendAsync(packet);
        }
        public ValueTask<int> SendToClientAsync(ushort id, params object[] values)
        {
            return Local.SendAsync(id, values);
        }

        private ValueTask<int> ClientRelayer(DataInterceptedEventArgs relayedFrom)
        {
            return SendToClientAsync(relayedFrom.Packet);
        }
        private ValueTask<int> ServerRelayer(DataInterceptedEventArgs relayedFrom)
        {
            return SendToServerAsync(relayedFrom.Packet);
        }
        private async Task InterceptOutgoingAsync(DataInterceptedEventArgs continuedFrom = null)
        {
            HPacket packet = await Local.ReceiveAsync().ConfigureAwait(false);
            if (packet != null)
            {
                var args = new DataInterceptedEventArgs(packet, ++_outSteps, true, InterceptOutgoingAsync, ServerRelayer);
                try { OnDataOutgoing(args); }
                catch { args.Restore(); }

                if (!args.IsBlocked && !args.WasRelayed)
                {
                    await SendToServerAsync(args.Packet).ConfigureAwait(false);
                }
                if (!args.HasContinued)
                {
                    if (args.WaitUntil != null)
                    {
                        await args.WaitUntil.ConfigureAwait(false);
                    }
                    args.Continue();
                }
            }
            else Disconnect();
        }
        private async Task InterceptIncomingAsync(DataInterceptedEventArgs continuedFrom = null)
        {
            HPacket packet = await Remote.ReceiveAsync().ConfigureAwait(false);
            if (packet != null)
            {
                var args = new DataInterceptedEventArgs(packet, ++_inSteps, false, InterceptIncomingAsync, ClientRelayer);
                try { OnDataIncoming(args); }
                catch { args.Restore(); }

                if (!args.IsBlocked && !args.WasRelayed)
                {
                    await SendToClientAsync(args.Packet).ConfigureAwait(false);
                }
                if (!args.HasContinued)
                {
                    args.Continue();
                }
            }
            else Disconnect();
        }

        public void Disconnect()
        {
            if (Monitor.TryEnter(_disconnectLock))
            {
                try
                {
                    _isIntercepting = false;
                    if (Local != null)
                    {
                        Local.Dispose();
                        Local = null;
                    }
                    if (Remote != null)
                    {
                        Remote.Dispose();
                        Remote = null;
                    }
                    if (IsConnected)
                    {
                        IsConnected = false;
                        OnDisconnected(EventArgs.Empty);
                    }
                }
                finally { Monitor.Exit(_disconnectLock); }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
            }
        }
    }
}