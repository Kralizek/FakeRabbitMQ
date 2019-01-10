using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using FakeRabbitMQ;
using FakeRabbitMQ.Internal;

namespace Tests
{
    public class AutoMoqDataAttribute : AutoDataAttribute
    {
        public AutoMoqDataAttribute(bool configureMembers = true, bool generateDelegates = true) : base(() => CreateFixture(generateDelegates, configureMembers)) { }

        static IFixture CreateFixture(bool generateDelegates, bool configureMembers)
        {
            var fixture = new Fixture();

            fixture.Customize(new AutoMoqCustomization
            {
                GenerateDelegates = generateDelegates,
                ConfigureMembers = configureMembers
            });

            fixture.Customize<FakeConnectionFactory>(o => o.OmitAutoProperties());

            fixture.Customize<FakeConnection>(o => o.OmitAutoProperties());

            fixture.Customize<FakeChannel>(o => o.OmitAutoProperties());

            fixture.Customizations.Add(new TypeRelay(typeof(ExchangeType), typeof(string)));

            return fixture;
        }
    }
}