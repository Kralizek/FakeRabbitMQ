using System.Collections.Concurrent;
using FakeRabbitMQ.Internal;

namespace FakeRabbitMQ
{
    public class FakeServer
    {
        public ConcurrentDictionary<string, Exchange> Exchanges { get; } = new ConcurrentDictionary<string, Exchange>();
        public ConcurrentDictionary<string, Queue> Queues { get; } = new ConcurrentDictionary<string, Queue>();
        
        public void Reset()
        {
            Exchanges.Clear();
            Queues.Clear();
        }
    }
}