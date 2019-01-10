using System;
using System.Collections.Concurrent;
using FakeRabbitMQ.Internal;
using RabbitMQ.Client;

namespace FakeRabbitMQ
{
    public class FakeServer
    {
        private readonly ConcurrentDictionary<string, Exchange> _exchanges = new ConcurrentDictionary<string, Exchange>();
        private readonly ConcurrentDictionary<string, Queue> _queues = new ConcurrentDictionary<string, Queue>();

        public void Reset()
        {
            _exchanges.Clear();
            _queues.Clear();
        }

        public IConnectionFactory CreateConnectionFactory() => new FakeConnectionFactory(this);

        public void AddExchange(Exchange exchange)
        {
            if (exchange == null)
            {
                throw new ArgumentNullException(nameof(exchange));
            }
            _exchanges.AddOrUpdate(exchange.Name, exchange, (s, e) => e);
        }

        public void AddOrUseExchange(string exchangeName, Func<string, Exchange> onAdd, Func<string, Exchange, Exchange> onUpdate)
        {
            if (exchangeName == null)
            {
                throw new ArgumentNullException(nameof(exchangeName));
            }

            if (onAdd == null)
            {
                throw new ArgumentNullException(nameof(onAdd));
            }

            if (onUpdate == null)
            {
                throw new ArgumentNullException(nameof(onUpdate));
            }

            _exchanges.AddOrUpdate(exchangeName, onAdd, onUpdate);
        }

        public bool TryRemoveExchange(string name, out Exchange exchange)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return _exchanges.TryRemove(name, out exchange);
        }

        public bool TryGetExchangeByName(string name, out Exchange exchange)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _exchanges.TryGetValue(name, out exchange);
        }

        public void AddQueue(Queue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }
            _queues.AddOrUpdate(queue.Name, queue, (s, q) => q);
        }

        public bool TryRemoveQueue(string name, out Queue queue)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            return _queues.TryRemove(name, out queue);
        }

        public bool TryGetQueueByName(string name, out Queue queue)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return _queues.TryGetValue(name, out queue);
        }
    }
}