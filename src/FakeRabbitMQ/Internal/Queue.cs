using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace FakeRabbitMQ.Internal
{
    public class Queue
    {
        public string Name { get; set; }

        public bool IsDurable { get; set; }

        public bool IsExclusive { get; set; }

        public bool IsAutoDelete { get; set; }

        public IDictionary Arguments = new Dictionary<string, object>();

        public ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();
        public ConcurrentDictionary<string, Binding> Bindings = new ConcurrentDictionary<string, Binding>();

        public event EventHandler<Message> MessagePublished = (sender, message) => { };

        public void PublishMessage(Message message)
        {
            var queueMessage = message.Clone();
            queueMessage.Queue = Name;

            Messages.Enqueue(queueMessage);

            MessagePublished(this, queueMessage);
        }
    }
}