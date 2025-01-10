using FluentAssertions;
using NUnit.Framework;

namespace Unleash.Tests
{
    public class GanpaSettingsTests
    {
        [Test]
        public void Should_set_environment_to_default()
        {
            // Act
            var settings = new GanpaSettings();

            // Assert
            settings.Environment.Should().Be("default");
        }

        [Test]
        public void Should_set_sdk_name()
        {
            // Act
            var settings = new GanpaSettings();

            // Assert
            settings.SdkVersion.Should().StartWith("_ganpa-client-dotnet:v");
        }
    }
}