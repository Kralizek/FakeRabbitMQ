using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;

namespace FakeRabbitMQ.Internal
{
    public class Message
    {
        public string Exchange { get; set; }

        public string RoutingKey { get; set; }

        public string Queue { get; set; }

        public bool Mandatory { get; set; }

        public bool Immediate { get; set; }

        public IBasicProperties BasicProperties { get; set; }

        public byte[] Body { get; set; }

        public Message Clone()
        {
            return new Message
            {
                Exchange = Exchange,
                RoutingKey = RoutingKey,
                Queue = Queue,
                Mandatory = Mandatory,
                Immediate = Immediate,
                BasicProperties = BasicProperties,
                Body = Body
            };

        }
    }
}
