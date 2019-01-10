using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FakeRabbitMQ.Internal
{
    public class Exchange
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public bool IsDurable { get; set; }

        public bool AutoDelete { get; set; }

        public IDictionary Arguments = new Dictionary<string, object>();
        
        public readonly ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();

        public readonly ConcurrentDictionary<string, Binding> Bindings = new ConcurrentDictionary<string, Binding>();

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
    }
}