using System;
using System.Collections.Generic;
using System.Net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FakeRabbitMQ {
    public class FakeConnection : IConnection
    {
        private readonly FakeServer _server;
        public List<FakeChannel> Channels { get; } = new List<FakeChannel>();

        public FakeConnection(FakeServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public EndPoint LocalEndPoint { get; set; }

        public EndPoint RemoteEndPoint { get; set; }

        public int LocalPort { get; set; }

        public int RemotePort { get; set; }

        public void Dispose() { }

        public void Abort() => Abort(1, null, 0);

        public void Abort(ushort reasonCode, string reasonText) => Abort(reasonCode, reasonText, 0);

        public void Abort(int timeout) => Abort(1, null, timeout);

        public void Abort(ushort reasonCode, string reasonText, int timeout)
        {
            IsOpen = false;

            CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, reasonCode, reasonText);
        }

        public void Close() => Close(1, null, 0);

        public void Close(ushort reasonCode, string reasonText) => Close(reasonCode, reasonText, 0);

        public void Close(int timeout) => Close(1, null, timeout);

        public void Close(ushort reasonCode, string reasonText, int timeout)
        {
            IsOpen = false;
            CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, reasonCode, reasonText);

            Channels.ForEach(c => c.Close());
        }

        public IModel CreateModel()
        {
            var channel = new FakeChannel(_server);

            Channels.Add(channel);

            return channel;
        }

        public void HandleConnectionBlocked(string reason)
        {
            
        }

        public void HandleConnectionUnblocked()
        {
            
        }

        public bool AutoClose { get; set; }

        public ushort ChannelMax { get; }

        public IDictionary<string, object> ClientProperties { get; }

        public ShutdownEventArgs CloseReason { get; set; }

        public AmqpTcpEndpoint Endpoint { get; }

        public uint FrameMax { get; }

        public ushort Heartbeat { get; }

        public bool IsOpen { get; set; }

        public AmqpTcpEndpoint[] KnownHosts { get; }

        public IProtocol Protocol { get; }

        public IDictionary<string, object> ServerProperties { get; }

        public IList<ShutdownReportEntry> ShutdownReport { get; }

        public string ClientProvidedName { get; }

        public ConsumerWorkService ConsumerWorkService { get; }
        public event EventHandler<CallbackExceptionEventArgs> CallbackException;
        public event EventHandler<EventArgs> RecoverySucceeded;
        public event EventHandler<ConnectionRecoveryErrorEventArgs> ConnectionRecoveryError;
        public event EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
        public event EventHandler<ShutdownEventArgs> ConnectionShutdown;
        public event EventHandler<EventArgs> ConnectionUnblocked;
    }
}