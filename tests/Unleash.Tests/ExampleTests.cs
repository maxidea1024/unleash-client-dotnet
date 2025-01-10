using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Unleash.ClientFactory;

namespace Unleash.Tests
{

    public class ExampleTests
    {
        private IGanpa _ganpa;

        [SetUp]
        public async Task Setup()
        {
            var factory = new GanpaClientFactory();
            _ganpa = await factory.CreateClientAsync(new MockedGanpaSettings(instanceTag: "test instance ExampleTests"), true);
        }

        [Test]
        public void UserAEnabled()
        {
            _ganpa.IsEnabled("one-enabled")
                .Should().BeTrue();
        }

        [Test]
        public void DisabledFeature()
        {
            _ganpa.IsEnabled("one-disabled")
                .Should().BeFalse();
        }

        [TearDown]
        public void Dispose()
        {
            _ganpa?.Dispose();
        }
    }
}