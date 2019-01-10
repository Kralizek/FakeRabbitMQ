using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace FakeRabbitMQ.Internal
{
    public class Message
    {
        public Message(string exchange, bool mandatory, bool immediate, byte[] body, IBasicProperties basicProperties = null, string routingKey = "")
        {
            Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
            RoutingKey = routingKey ?? throw new ArgumentNullException(nameof(routingKey));
            Mandatory = mandatory;
            Immediate = immediate;
            BasicProperties = basicProperties ?? new BasicProperties();
            Body = body ?? throw new ArgumentNullException(nameof(body));
        }

        public string Exchange { get; }

        public string RoutingKey { get; }

        public string Queue { get; set; }

        public bool Mandatory { get; }

        public bool Immediate { get; }

        public IBasicProperties BasicProperties { get; set; }

        public byte[] Body { get; set; }

        public Message Clone(string name = null)
        {
            return new Message(Exchange, Mandatory, Immediate, Body, BasicProperties, RoutingKey) { Queue = name ?? Queue };
        }
    }
}
