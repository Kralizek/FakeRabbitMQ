using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FakeRabbitMQ.Internal
{
    public class Exchange
    {
        public Exchange(string name, string typeName, bool durable, bool autoDelete, IDictionary<string, object> arguments = null) : this(name, (ExchangeType)typeName, durable, autoDelete, arguments) { }

        public Exchange(string name, ExchangeType type, bool durable, bool autoDelete, IDictionary<string, object> arguments = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsDurable = durable;
            AutoDelete = autoDelete;
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        public string Name { get; }

        public ExchangeType Type { get; }

        public bool IsDurable { get; }

        public bool AutoDelete { get; }

        public IDictionary<string, object> Arguments { get; }
        
        public ConcurrentQueue<Message> Messages { get; } = new ConcurrentQueue<Message>();

        public ConcurrentDictionary<string, Binding> Bindings { get; } = new ConcurrentDictionary<string, Binding>();

        public void PublishMessage(Message message)
        {
            Messages.Enqueue(message);

            if (string.IsNullOrWhiteSpace(message.RoutingKey))
            {
                foreach (var binding in Bindings)
                {
                    binding.Value.Queue.PublishMessage(message);
                }
            }
            else
            {
                var matchingBindings = Bindings
                                       .Values
                                       .Where(b => b.RoutingKey == message.RoutingKey);

                foreach (var binding in matchingBindings)
                {
                    binding.Queue.PublishMessage(message);
                }

            }

        }

        public void BindToQueue(Queue queue, string routingKey, IDictionary<string, object> arguments)
        {
            var binding = new Binding(this, queue, routingKey, arguments);

            binding.AttachTo(Bindings);
            binding.AttachTo(queue.Bindings);
        }

        public void UnbindFromQueue(Queue queue, string routingKey)
        {
            var binding = new Binding(this, queue, routingKey);

            binding.RemoveFrom(Bindings);
            binding.RemoveFrom(queue.Bindings);
        }
    }

    public class ExchangeType
    {
        private ExchangeType(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Type { get; }

        public static implicit operator ExchangeType(string name) => new ExchangeType(name);

        public static ExchangeType Direct = "direct";
        public static ExchangeType Topic = "topic";
        public static ExchangeType FanOut = "fanout";
        public static ExchangeType Headers = "headers";
    }
}