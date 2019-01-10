using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using FakeRabbitMQ.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using BasicProperties = RabbitMQ.Client.Framing.BasicProperties;
using ExchangeType = FakeRabbitMQ.Internal.ExchangeType;
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
            if (!Server.TryGetExchangeByName(exchange, out var instance))
            {
                return Array.Empty<Message>();
            }

            return instance.Messages;
        }

        public IEnumerable<Message> GetMessagesOnQueue(string queue)
        {
            if (!Server.TryGetQueueByName(queue, out var instance))
            {
                return Array.Empty<Message>();
            }

            return instance.Messages;
        }

        public bool ApplyPrefetchToAllChannels { get; private set; }
        public ushort PrefetchCount { get; private set; }
        public uint PrefetchSize { get; private set; }
        public bool IsChannelFlowActive { get; private set; }

        public void Dispose() { }

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
            var exchangeInstance = new Exchange(exchange, type, durable, autoDelete, arguments);

            Server.AddExchange(exchangeInstance);
        }

        public void ExchangeDeclarePassive(string exchange) => ExchangeDeclare(exchange, type: null, durable: false, autoDelete: false, arguments: null);

        public void ExchangeDeclareNoWait(string exchange, string type, bool durable, bool autoDelete, IDictionary<string, object> arguments) => ExchangeDeclare(exchange, type, durable, autoDelete: false, arguments: arguments);

        public void ExchangeDelete(string exchange, bool ifUnused) => Server.TryRemoveExchange(exchange, out _);

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
            if (Server.TryGetExchangeByName(source, out var exchange) && Server.TryGetQueueByName(destination, out var queue))
            {
                exchange.BindToQueue(queue, routingKey, arguments);
            }
        }

        public void ExchangeUnbind(string destination, string source, string routingKey, IDictionary<string, object> arguments = null)
        {
            if (Server.TryGetExchangeByName(source, out var exchange) && Server.TryGetQueueByName(destination, out var queue))
            {
                exchange.UnbindFromQueue(queue, routingKey);
            }
        }

        public QueueDeclareOk QueueDeclarePassive(string queue) => QueueDeclare(queue);

        public uint MessageCount(string queue)
        {
            if (Server.TryGetQueueByName(queue, out var q))
            {
                return (uint)q.Messages.Count;
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
            var queueInstance = new Queue(queue, durable, exclusive, autoDelete, arguments);

            Server.AddQueue(queueInstance);

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
            Server.TryGetQueueByName(queue, out var instance);

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
            Server.TryRemoveQueue(queue, out var instance);

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
            if (Server.TryGetQueueByName(queue, out var queueInstance))
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
            if (!Server.TryGetQueueByName(queue, out var instance) || !instance.Messages.TryDequeue(out var message))
            {
                return null;
            }

            Interlocked.Increment(ref _lastDeliveryTag);
            var deliveryTag = Convert.ToUInt64(_lastDeliveryTag);
            const bool redelivered = false;
            var exchange = message.Exchange;
            var routingKey = message.RoutingKey;
            var messageCount = Convert.ToUInt32(instance.Messages.Count);
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
            var message = new Message(exchange, mandatory, immediate, body, basicProperties, routingKey);

            Server.AddOrUseExchange(exchange, CreateExchange, UseExisting);

            NextPublishSeqNo++;

            Exchange CreateExchange(string exchangeName)
            {
                var newExchange = new Exchange(exchangeName, ExchangeType.Direct, false, false);
                newExchange.PublishMessage(message);

                return newExchange;
            }

            Exchange UseExisting(string _, Exchange existingExchange)
            {
                existingExchange.PublishMessage(message);

                return existingExchange;
            }
        }

        public void BasicAck(ulong deliveryTag, bool multiple)
        {
            if (_workingMessages.TryRemove(deliveryTag, out var message) && Server.TryGetQueueByName(message.Queue, out var queue))
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
            if (_workingMessages.TryRemove(deliveryTag, out var message) && requeue && Server.TryGetQueueByName(message.Queue, out var queue))
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
                    if (Server.TryGetQueueByName(message.Value.Queue, out var queueInstance))
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