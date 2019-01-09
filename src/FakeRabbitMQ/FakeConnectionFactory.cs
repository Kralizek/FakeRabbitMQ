using System;
using RabbitMQ.Client;

namespace FakeRabbitMQ
{
    public class FakeConnectionFactory : ConnectionFactory
    {
        public FakeServer Server { get; }

        public FakeConnectionFactory() : this(new FakeServer()) { }

        public FakeConnectionFactory(FakeServer server)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public override IConnection CreateConnection()
        {
            return new FakeConnection(Server);
        }
    }
}