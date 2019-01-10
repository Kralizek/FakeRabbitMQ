using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FakeRabbitMQ.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Impl;
using BasicProperties = RabbitMQ.Client.Framing.BasicProperties;
using Queue = FakeRabbitMQ.Internal.Queue;

namespace FakeRabbitMQ
{
    public class FakeChannel : IModel
    {       
        public FakeServer Server { get; }

        public FakeChannel(FakeServer server)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
        }
        
        public IEnumerable<Message> GetMessagesPublishedToExchange(string exchange)
        {
            Server.Exchanges.TryGetValue(exchange, out var exchangeInstance);

            if (exchangeInstance == null)
            {
                return new List<Message>();
            }

            return exchangeInstance.Messages;
        }

        public IEnumerable<Message> GetMessagesOnQueue(string queueName)
        {
            Server.Queues.TryGetValue(queueName, out var queueInstance);

            if (queueInstance == null)
            {
                return new List<Message>();
            }

            return queueInstance.Messages;
        }

        public bool ApplyPrefetchToAllChannels { get; private set; }
        public ushort PrefetchCount { get; private set; }
        public uint PrefetchSize { get; private set; }
        public bool IsChannelFlowActive { get; private set; }

        public void Dispose()
        {

        }

        public IBasicPublishBatch CreateBasicPublishBatch()
        {
            throw new NotImplementedException();
        }

        public IBasicProperties CreateBasicProperties()
        {
            return new BasicProperties();
        }

        public void ExchangeBindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }

        public void ChannelFlow(bool active)
        {
            IsChannelFlowActive = active;
        }

        public void ExchangeDeclare(string exchange, string type, bool durable, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            var exchangeInstance = new Exchange
            {
                Name = exchange,
                Type = type,
                IsDurable = durable,
                AutoDelete = autoDelete,
                Arguments = arguments as IDictionary
            };
            Server.Exchanges.AddOrUpdate(exchange, exchangeInstance, (name, existing) => existing);
        }

        public void ExchangeDeclarePassive(string exchange) => ExchangeDeclare(exchange, type: null, durable: false, autoDelete: false, arguments: null);

        public void ExchangeDeclareNoWait(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => ExchangeDeclare(exchange, type, durable, autoDelete: false, arguments: arguments);

        public void ExchangeDelete(string exchange, bool ifUnused) => Server.Exchanges.TryRemove(exchange, out _);

        public void ExchangeDeleteNoWait(string exchange, bool ifUnused) => ExchangeDelete(exchange, ifUnused: false);

        public void ExchangeUnbindNoWait(string destination, string source, string routingKey, IDictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }

        public void QueueBindNoWait(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            throw new NotImplementedException();
        }

        public void ExchangeBind(string destination, string source, string routingKey, IDictionary<string, object> arguments = null)
        {
            Server.Exchanges.TryGetValue(source, out var exchange);

            Server.Queues.TryGetValue(destination, out var queue);

            var binding = new Binding { Exchange = exchange, Queue = queue, RoutingKey = routingKey };

            exchange?.Bindings.AddOrUpdate(binding.Key, binding, (k, v) => binding);
            queue?.Bindings.AddOrUpdate(binding.Key, binding, (k, v) => binding);
        }

        public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary<string, object> arguments = null)
        {
            Server.Exchanges.TryGetValue(source, out var exchange);

            Server.Queues.TryGetValue(destination, out var queue);

            var binding = new Binding { Exchange = exchange, Queue = queue, RoutingKey = routingKey };

            if (exchange != null)
            {
                exchange.Bindings.TryRemove(binding.Key, out _);
            }

            if (queue != null)
            {
                queue.Bindings.TryRemove(binding.Key, out _);
            }
        }

        public QueueDeclareOk QueueDeclarePassive(string queue) => QueueDeclare(queue, durable: false, exclusive: false, autoDelete: false, arguments: null);

        public uint MessageCount(string queue)
        {
            if (Server.Queues.TryGetValue(queue, out var q))
            {
                return (uint) q.Messages.Count;
            }

            return 0u;
        }

        public uint ConsumerCount(string queue)
        {
            throw new NotImplementedException();
        }

        public void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments = null)
        {
            ExchangeBind(queue, exchange, routingKey, arguments);
        }

        public QueueDeclareOk QueueDeclare(string queue = null, bool durable = false, bool exclusive = false, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            queue = queue ?? Guid.NewGuid().ToString("N");

            var queueInstance = new Queue
            {
                Name = queue,
                IsDurable = durable,
                IsExclusive = exclusive,
                IsAutoDelete = autoDelete,
                Arguments = arguments as IDictionary
            };

            Server.Queues.AddOrUpdate(queue, queueInstance, (name, existing) => existing);

            return new QueueDeclareOk(queue, 0, 0);
        }

        public void QueueDeclareNoWait(string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments)
        {
            QueueDeclare(queue, durable, exclusive, autoDelete, arguments);
        }

        public void QueueUnbind(string queue, string exchange, string routingKey, IDictionary<string, object> arguments)
        {
            ExchangeUnbind(queue, exchange, routingKey);
        }

        public uint QueuePurge(string queue)
        {
            Server.Queues.TryGetValue(queue, out var instance);

            if (instance == null)
            {
                return 0u;
            }

            while (!instance.Messages.IsEmpty)
            {
                instance.Messages.TryDequeue(out _);
            }

            return 1u;
        }

        public uint QueueDelete(string queue, bool ifUnused = false, bool ifEmpty = false)
        {
            Server.Queues.TryRemove(queue, out var instance);

            return instance != null ? 1u : 0u;
        }

        public void QueueDeleteNoWait(string queue, bool ifUnused, bool ifEmpty) => QueueDelete(queue);

        public void ConfirmSelect()
        {
            throw new NotImplementedException();
        }

        public bool WaitForConfirms()
        {
            throw new NotImplementedException();
        }

        public bool WaitForConfirms(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public bool WaitForConfirms(TimeSpan timeout, out bool timedOut)
        {
            throw new NotImplementedException();
        }

        public void WaitForConfirmsOrDie()
        {
            throw new NotImplementedException();
        }

        public void WaitForConfirmsOrDie(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public int ChannelNumber { get; }

        public string BasicConsume(string queue, bool noAck, IBasicConsumer consumer)
        {
            return BasicConsume(queue: queue, noAck: noAck, consumerTag: Guid.NewGuid().ToString(), noLocal: true, exclusive: false, arguments: null, consumer: consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, IBasicConsumer consumer)
        {
            return BasicConsume(queue: queue, noAck: noAck, consumerTag: consumerTag, noLocal: true, exclusive: false, arguments: null, consumer: consumer);
        }

        public string BasicConsume(string queue, bool noAck, string consumerTag, IDictionary<string, object> arguments, IBasicConsumer consumer)
        {
            return BasicConsume(queue: queue, noAck: noAck, consumerTag: consumerTag, noLocal: true, exclusive: false, arguments: arguments, consumer: consumer);
        }

        private readonly ConcurrentDictionary<string, IBasicConsumer> _consumers = new ConcurrentDictionary<string, IBasicConsumer>();

        public string BasicConsume(string queue, bool noAck, string consumerTag, bool noLocal, bool exclusive, IDictionary<string, object> arguments, IBasicConsumer consumer)
        {
            Server.Queues.TryGetValue(queue, out var queueInstance);

            if (queueInstance != null)
            {
                _consumers.AddOrUpdate(consumerTag, consumer, (s, basicConsumer) => basicConsumer);

                NotifyConsumerOfExistingMessages(consumerTag, consumer, queueInstance);
                NotifyConsumerWhenMessagesAreReceived(consumerTag, consumer, queueInstance);
            }

            return consumerTag;
        }

        private void NotifyConsumerWhenMessagesAreReceived(string consumerTag, IBasicConsumer consumer, Queue queueInstance)
        {
            queueInstance.MessagePublished += (sender, message) => { NotifyConsumerOfMessage(consumerTag, consumer, message); };
        }

        private void NotifyConsumerOfExistingMessages(string consumerTag, IBasicConsumer consumer, Queue queueInstance)
        {
            foreach (var message in queueInstance.Messages)
            {
                NotifyConsumerOfMessage(consumerTag, consumer, message);
            }
        }

        private void NotifyConsumerOfMessage(string consumerTag, IBasicConsumer consumer, Message message)
        {
            Interlocked.Increment(ref _lastDeliveryTag);
            var deliveryTag = Convert.ToUInt64(_lastDeliveryTag);
            const bool redelivered = false;
            var exchange = message.Exchange;
            var routingKey = message.RoutingKey;
            var basicProperties = message.BasicProperties ?? CreateBasicProperties();
            var body = message.Body;

            _workingMessages.AddOrUpdate(deliveryTag, message, (key, existingMessage) => existingMessage);

            consumer.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, basicProperties, body);
        }

        public void BasicCancel(string consumerTag)
        {
            _consumers.TryRemove(consumerTag, out var consumer);

            consumer?.HandleBasicCancelOk(consumerTag);
        }

        private long _lastDeliveryTag;

        private readonly ConcurrentDictionary<ulong, Message> _workingMessages = new ConcurrentDictionary<ulong, Message>();

        public BasicGetResult BasicGet(string queue, bool noAck)
        {
            if (!Server.Queues.TryGetValue(queue, out var queueInstance))
            {
                return null;
            }

            if (!queueInstance.Messages.TryDequeue(out var message))
            {
                return null;
            }
            
            Interlocked.Increment(ref _lastDeliveryTag);
            var deliveryTag = Convert.ToUInt64(_lastDeliveryTag);
            const bool redelivered = false;
            var exchange = message.Exchange;
            var routingKey = message.RoutingKey;
            var messageCount = Convert.ToUInt32(queueInstance.Messages.Count);
            var basicProperties = message.BasicProperties ?? CreateBasicProperties();
            var body = message.Body;

            _workingMessages.AddOrUpdate(deliveryTag, message, (key, existingMessage) => existingMessage);

            return new BasicGetResult(deliveryTag, redelivered, exchange, routingKey, messageCount, basicProperties, body);
        }

        public void BasicQos(uint prefetchSize, ushort prefetchCount, bool global)
        {
            PrefetchSize = prefetchSize;
            PrefetchCount = prefetchCount;
            ApplyPrefetchToAllChannels = global;
        }

        public void BasicPublish(PublicationAddress addr, IBasicProperties basicProperties, byte[] body)
        {
            BasicPublish(exchange: addr.ExchangeName, routingKey: addr.RoutingKey, mandatory: true, immediate: true, basicProperties: basicProperties, body: body);
        }

        public void BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body)
        {
            BasicPublish(exchange: exchange, routingKey: routingKey, mandatory: true, immediate: true, basicProperties: basicProperties, body: body);
        }

        public void BasicPublish(string exchange, string routingKey, bool mandatory, IBasicProperties basicProperties, byte[] body)
        {
            BasicPublish(exchange: exchange, routingKey: routingKey, mandatory: mandatory, immediate: true, basicProperties: basicProperties, body: body);
        }

        public void BasicPublish(string exchange, string routingKey, bool mandatory, bool immediate, IBasicProperties basicProperties, byte[] body)
        {
            var parameters = new Message
            {
                Exchange = exchange,
                RoutingKey = routingKey,
                Mandatory = mandatory,
                Immediate = immediate,
                BasicProperties = basicProperties,
                Body = body
            };

            Func<string, Exchange> addExchange = s =>
            {
                var newExchange = new Exchange
                {
                    Name = exchange,
                    Arguments = null,
                    AutoDelete = false,
                    IsDurable = false,
                    Type = "direct"
                };
                newExchange.PublishMessage(parameters);

                return newExchange;
            };
            Func<string, Exchange, Exchange> updateExchange = (s, existingExchange) =>
            {
                existingExchange.PublishMessage(parameters);

                return existingExchange;
            };
            Server.Exchanges.AddOrUpdate(exchange, addExchange, updateExchange);

            NextPublishSeqNo++;
        }


        public void BasicAck(ulong deliveryTag, bool multiple)
        {
            if (_workingMessages.TryRemove(deliveryTag, out var message) && Server.Queues.TryGetValue(message.Queue, out var queue))
            {
                queue.Messages.TryDequeue(out message);
            }
        }

        public void BasicReject(ulong deliveryTag, bool requeue)
        {
            BasicNack(deliveryTag: deliveryTag, multiple: false, requeue: requeue);
        }

        public void BasicNack(ulong deliveryTag, bool multiple, bool requeue)
        {
            if (_workingMessages.TryRemove(deliveryTag, out var message) && requeue && Server.Queues.TryGetValue(message.Queue, out var queue))
            {
                queue.PublishMessage(message);
            }
        }

        public void BasicRecover(bool requeue)
        {
            if (requeue)
            {
                foreach (var message in _workingMessages)
                {
                    if (Server.Queues.TryGetValue(message.Value.Queue, out var queueInstance))
                    {
                        queueInstance.PublishMessage(message.Value);
                    }
                }
            }

            _workingMessages.Clear();
        }

        public void BasicRecoverAsync(bool requeue)
        {
            BasicRecover(requeue);
        }

        public void TxSelect()
        {
            throw new NotImplementedException();
        }

        public void TxCommit()
        {
            throw new NotImplementedException();
        }

        public void TxRollback()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            Close(ushort.MaxValue, string.Empty);
        }

        public void Close(ushort replyCode, string replyText)
        {
            IsClosed = true;
            IsOpen = false;
            CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, replyCode, replyText);
        }

        public void Abort()
        {
            Abort(ushort.MaxValue, string.Empty);
        }

        public void Abort(ushort replyCode, string replyText)
        {
            IsClosed = true;
            IsOpen = false;
            CloseReason = new ShutdownEventArgs(ShutdownInitiator.Library, replyCode, replyText);
        }

        public IBasicConsumer DefaultConsumer { get; set; }

        public ShutdownEventArgs CloseReason { get; set; }

        public bool IsOpen { get; set; }

        public bool IsClosed { get; set; }
        
        public ulong NextPublishSeqNo { get; set; }

        public TimeSpan ContinuationTimeout { get; set; }

        event EventHandler<BasicAckEventArgs> IModel.BasicAcks
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<BasicNackEventArgs> IModel.BasicNacks
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<EventArgs> IModel.BasicRecoverOk
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<BasicReturnEventArgs> IModel.BasicReturn
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<CallbackExceptionEventArgs> IModel.CallbackException
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<FlowControlEventArgs> IModel.FlowControl
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ShutdownEventArgs> IModel.ModelShutdown
        {
            add => AddedModelShutDownEvent += value;
            remove => AddedModelShutDownEvent -= value;
        }

        public EventHandler<ShutdownEventArgs> AddedModelShutDownEvent { get; set; }
    }
}