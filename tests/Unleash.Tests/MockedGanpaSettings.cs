using System;
using System.Collections.Generic;
using Unleash.Internal;
using Unleash.Tests.Mock;
using System.Text;

namespace Unleash.Tests
{
    public class MockedGanpaSettings : GanpaSettings
    {
        public MockedGanpaSettings(bool mockFileSystem = true, string instanceTag = "test instance 1")
        {
            AppName = "test";
            InstanceTag = instanceTag;
            UnleashApi = new Uri("http://localhost:4242/");

            GanpaApiClient = new MockApiClient();
            FileSystem = new MockFileSystem();
            
            if (!mockFileSystem)
            {
                FileSystem = new FileSystem(Encoding.UTF8);
            }

            GanpaContextProvider = new DefaultGanpaContextProvider(new GanpaContext
            {
                UserId = "userA",
                SessionId = "sessionId",
                RemoteAddress = "remoteAddress",
                Properties = new Dictionary<string, string>()
            });
        }
    }
}