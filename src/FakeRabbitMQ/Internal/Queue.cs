using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FakeRabbitMQ.Internal
{
    public class Queue
    {
        public Queue(string name, bool isDurable, bool isExclusive, bool isAutoDelete, IDictionary<string, object> arguments = null)
        {
            Name = name ?? GenerateQueueName();
            IsDurable = isDurable;
            IsExclusive = isExclusive;
            IsAutoDelete = isAutoDelete;
            Arguments = arguments ?? new Dictionary<string, object>();
        }

        private static string GenerateQueueName() => Guid.NewGuid().ToString("N");

        public string Name { get; }

        public bool IsDurable { get; }

        public bool IsExclusive { get; }

        public bool IsAutoDelete { get; }

        public IDictionary<string, object> Arguments { get; }

        public ConcurrentQueue<Message> Messages { get; } = new ConcurrentQueue<Message>();

        public ConcurrentDictionary<string, Binding> Bindings { get; } = new ConcurrentDictionary<string, Binding>();

        public event EventHandler<Message> MessagePublished = (sender, message) => { };

        public void PublishMessage(Message message)
        {
            var queueMessage = message.Clone(name: Name);

            Messages.Enqueue(queueMessage);

            MessagePublished(this, queueMessage);
        }
    }
}