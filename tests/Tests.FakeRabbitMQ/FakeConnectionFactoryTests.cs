using AutoFixture.Idioms;
using FakeRabbitMQ;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class FakeConnectionFactoryTests
    {
        [Test, AutoMoqData]
        public void Constructor_is_guarded(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(FakeConnectionFactory).GetConstructors());
        }

        [Test, AutoMoqData]
        public void CreateConnection_returns_FakeConnection(FakeConnectionFactory sut)
        {
            var connection = sut.CreateConnection();

            Assert.That(connection, Is.InstanceOf<FakeConnection>());
        }
    }
}
