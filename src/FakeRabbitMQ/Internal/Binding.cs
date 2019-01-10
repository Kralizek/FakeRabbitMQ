using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FakeRabbitMQ.Internal
{
    public class Binding
    {
        public Binding(Exchange exchange, Queue queue, string routingKey = null, IDictionary<string, object> arguments = null)
        {
            Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            Arguments = arguments ?? new Dictionary<string, object>();
            RoutingKey = routingKey;
        }

        public string RoutingKey { get; }

        public Exchange Exchange { get; }

        public Queue Queue { get; }

        public IDictionary<string, object> Arguments { get; }

        public string Key => $"{Exchange.Name}|{RoutingKey}|{Queue.Name}";

        public void AttachTo(ConcurrentDictionary<string, Binding> bindings)
        {
            bindings.AddOrUpdate(Key, this, (s, b) => b);
        }

        public bool RemoveFrom(ConcurrentDictionary<string, Binding> bindings) => bindings.TryRemove(Key, out _);
    }
}