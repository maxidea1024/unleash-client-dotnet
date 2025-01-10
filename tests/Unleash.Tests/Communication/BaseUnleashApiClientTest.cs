using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Unleash.Communication;
using Unleash.Events;
using Unleash.Internal;
using Unleash.Serialization;

namespace Unleash.Tests.Communication
{
    public abstract class BaseUnleashApiClientTest
    {
        private static IGanpaApiClient CreateApiClient()
        {
            var apiUri = new Uri("http://_ganpa.herokuapp.com/api/");

            var jsonSerializer = new DynamicNewtonsoftJsonSerializer();
            jsonSerializer.TryLoad();

            var httpClientFactory = new DefaultHttpClientFactory();

            var requestHeaders = new GanpaApiClientRequestHeaders
            {
                AppName = "api-test-client",
                InstanceTag = "instance1",
                CustomHttpHeaders = new Dictionary<string, string>()
                {
                    // "Test" token from 21.10.2021
                    { "Authorization", "*:default.77c45b703a681983b714fee87e575a823bfb1fd0ab282d9399647243" }
                },
                CustomHttpHeaderProvider = null
            };

            var httpClient = httpClientFactory.Create(apiUri);
            var client = new GanpaApiClient(httpClient, jsonSerializer, requestHeaders, new EventCallbackConfig());
            return client;
        }

        internal IGanpaApiClient api
        {
            get => TestExecutionContext.CurrentContext.CurrentTest.Properties.Get("api") as IGanpaApiClient;
            set => TestExecutionContext.CurrentContext.CurrentTest.Properties.Set("api", value);
        }

        [SetUp]
        public void SetupTest()
        {
            api = CreateApiClient();
        }
    }
}