using System;
using AutoFixture.Idioms;
using AutoFixture.NUnit3;
using FakeRabbitMQ;
using FakeRabbitMQ.Internal;
using Moq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class FakeServerTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(FakeServer).GetConstructors());
        }

        [Test, AutoMoqData]
        public void CreateConnectionFactory_returns_new_connectionFactory_connected_to_server(FakeServer sut)
        {
            var connectionFactory = sut.CreateConnectionFactory() as FakeConnectionFactory;

            Assert.That(connectionFactory, Is.Not.Null);
            Assert.That(connectionFactory.Server, Is.SameAs(sut));
        }

        [Test, AutoMoqData]
        public void AddExchange_is_guarded(FakeServer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddExchange(null));
        }

        [Test, AutoMoqData]
        public void AddExchange_adds_exchange(FakeServer sut, Exchange exchange)
        {
            sut.AddExchange(exchange);

            Assert.That(sut.TryGetExchangeByName(exchange.Name, out _), Is.True);
        }

        [Test, AutoMoqData]
        public void AddExchange_reuses_existing_item_if_duplicate(FakeServer sut,[Frozen(Matching.ParameterName)] string name, Exchange firstExchange, Exchange secondExchange)
        {
            Assume.That(firstExchange.Name, Is.EqualTo(name));
            Assume.That(secondExchange.Name, Is.EqualTo(name));

            sut.AddExchange(firstExchange);

            sut.AddExchange(secondExchange);

            sut.TryGetExchangeByName(name, out var result);

            Assert.That(result, Is.SameAs(firstExchange));
        }

        [Test, AutoMoqData]
        public void AddOrUseExchange_is_guarded(FakeServer sut, string exchangeName)
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddOrUseExchange(null, Mock.Of<Func<string, Exchange>>(), Mock.Of<Func<string, Exchange, Exchange>>()));

            Assert.Throws<ArgumentNullException>(() => sut.AddOrUseExchange(exchangeName, null, Mock.Of<Func<string, Exchange, Exchange>>()));

            Assert.Throws<ArgumentNullException>(() => sut.AddOrUseExchange(exchangeName, Mock.Of<Func<string, Exchange>>(), null));
        }

        [Test, AutoMoqData()]
        public void AddOrUseExchange_adds_exchange_if_not_found(FakeServer sut, string exchangeName)
        {
            var createExchange = Mock.Of<Func<string, Exchange>>();

            var useExchange = Mock.Of<Func<string, Exchange, Exchange>>();

            sut.AddOrUseExchange(exchangeName, createExchange, useExchange);

            Mock.Get(createExchange).Verify(p => p(exchangeName));
        }

        [Test, AutoMoqData]
        public void AddOrUseExchange_uses_existing_exchange(FakeServer sut, Exchange exchange)
        {
            sut.AddExchange(exchange);

            var createExchange = Mock.Of<Func<string, Exchange>>();

            var useExchange = Mock.Of<Func<string, Exchange, Exchange>>();

            sut.AddOrUseExchange(exchange.Name, createExchange, useExchange);

            Mock.Get(useExchange).Verify(p => p(exchange.Name, exchange));
        }

        [Test, AutoMoqData]
        public void TryRemoveExchange_is_guarded(FakeServer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.TryRemoveExchange(null, out _));
        }

        [Test, AutoMoqData]
        public void TryRemoveExchange_return_false_if_exchange_not_found(FakeServer sut, string exchangeName)
        {
            var result = sut.TryRemoveExchange(exchangeName, out var exchange);

            Assert.That(result, Is.False);
        }

        [Test, AutoMoqData]
        public void TryRemoveExchange_return_no_item_if_exchange_not_found(FakeServer sut, string exchangeName)
        {
            var result = sut.TryRemoveExchange(exchangeName, out var exchange);

            Assert.That(exchange, Is.Null);
        }

        [Test, AutoMoqData]
        public void TryGetExchangeByName_is_guarded(FakeServer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.TryGetExchangeByName(null, out _));
        }

        [Test, AutoMoqData]
        public void TryGetExchangeByName_return_false_if_exchange_not_found(FakeServer sut, string exchangeName)
        {
            var result = sut.TryGetExchangeByName(exchangeName, out var exchange);

            Assert.That(result, Is.False);
        }

        [Test, AutoMoqData]
        public void TryGetExchangeByName_return_no_item_if_exchange_not_found(FakeServer sut, string exchangeName)
        {
            var result = sut.TryGetExchangeByName(exchangeName, out var exchange);

            Assert.That(exchange, Is.Null);
        }

        [Test, AutoMoqData]
        public void AddQueue_is_guarded(FakeServer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.AddQueue(null));
        }

        [Test, AutoMoqData]
        public void AddQueue_adds_queue(FakeServer sut, Queue queue)
        {
            sut.AddQueue(queue);

            var result = sut.TryGetQueueByName(queue.Name, out _);

            Assert.That(result, Is.True);
        }

        [Test, AutoMoqData]
        public void AddQueue_reuses_existing_item_if_duplicate(FakeServer sut, [Frozen(Matching.ParameterName)] string name, Queue firstQueue, Queue secondQueue)
        {
            Assume.That(firstQueue.Name, Is.EqualTo(name));
            Assume.That(secondQueue.Name, Is.EqualTo(name));

            sut.AddQueue(firstQueue);

            sut.AddQueue(secondQueue);

            sut.TryGetQueueByName(name, out var result);

            Assert.That(result, Is.SameAs(firstQueue));
        }

        [Test, AutoMoqData]
        public void TryRemoveQueue_is_guarded(FakeServer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.TryRemoveQueue(null, out _));
        }

        [Test, AutoMoqData]
        public void TryRemoveQueue_return_false_if_exchange_not_found(FakeServer sut, string queueName)
        {
            var result = sut.TryRemoveQueue(queueName, out var queue);

            Assert.That(result, Is.False);
        }

        [Test, AutoMoqData]
        public void TryRemoveQueue_return_no_item_if_exchange_not_found(FakeServer sut, string queueName)
        {
            var result = sut.TryRemoveQueue(queueName, out var queue);

            Assert.That(queue, Is.Null);
        }

        [Test, AutoMoqData]
        public void TryGetQueueByName_is_guarded(FakeServer sut)
        {
            Assert.Throws<ArgumentNullException>(() => sut.TryGetQueueByName(null, out _));
        }

        [Test, AutoMoqData]
        public void TryGetQueueByName_return_false_if_Queue_not_found(FakeServer sut, string queueName)
        {
            var result = sut.TryGetQueueByName(queueName, out var queue);

            Assert.That(result, Is.False);
        }

        [Test, AutoMoqData]
        public void TryGetQueueByName_return_no_item_if_Queue_not_found(FakeServer sut, string queueName)
        {
            var result = sut.TryGetQueueByName(queueName, out var queue);

            Assert.That(queue, Is.Null);
        }

        [Test, AutoMoqData]
        public void Reset_clears_queues(FakeServer sut, Queue queue)
        {
            sut.AddQueue(queue);

            Assume.That(sut.TryGetQueueByName(queue.Name, out _), Is.True);

            sut.Reset();

            Assert.That(sut.TryGetQueueByName(queue.Name, out _), Is.False);
        }

        [Test, AutoMoqData]
        public void Reset_clears_exchanges(FakeServer sut, Exchange exchange)
        {
            sut.AddExchange(exchange);

            Assume.That(sut.TryGetExchangeByName(exchange.Name, out _), Is.True);

            sut.Reset();

            Assert.That(sut.TryGetExchangeByName(exchange.Name, out _), Is.False);
        }
    }
}